using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace Specht.Comparison;

/// <summary>Reads the workload identity a revision's own benchmarks declare from its built assembly's metadata, without loading or executing it.</summary>
public static class DeclaredIdentity
{
    public static ImmutableArray<IdentityMapping> Read(Stream assembly, string productCommit, string checkoutRoot,
        Func<string, byte[]?> readSource)
    {
        var root = Normalise(checkoutRoot).TrimEnd('/') + "/";
        var digests = new Dictionary<string, string?>(StringComparer.Ordinal);
        var mappings = ImmutableArray.CreateBuilder<IdentityMapping>();
        try
        {
            using var image = new PEReader(assembly, PEStreamOptions.LeaveOpen);
            if (!image.HasMetadata)
            {
                return [];
            }

            var reader = image.GetMetadataReader();
            var names = new AttributeTypeNames();
            foreach (var type in reader.TypeDefinitions.Select(reader.GetTypeDefinition))
            {
                if (!type.GetDeclaringType().IsNil || type.GetGenericParameters().Count > 0)
                {
                    continue;
                }

                var declaring = $"{reader.GetString(type.Namespace)}.{reader.GetString(type.Name)}";
                foreach (var method in type.GetMethods().Select(reader.GetMethodDefinition))
                {
                    if (Declaration(reader, names, method) is not { } declared || HasUnreadArguments(reader, type, method))
                    {
                        continue;
                    }

                    var relative = Normalise(declared.SourceFile) is var compiled && compiled.StartsWith(root, StringComparison.Ordinal)
                        ? compiled[root.Length..] : null;
                    if (!EvidencePaths.IsRelative(relative))
                    {
                        continue;
                    }

                    if (!digests.TryGetValue(relative!, out var digest))
                    {
                        digest = readSource(relative!) is { } bytes ? Convert.ToHexStringLower(SHA256.HashData(bytes)) : null;
                        digests[relative!] = digest;
                    }

                    if (digest is null || Parameters(reader, names, type) is not { } parameters)
                    {
                        continue;
                    }

                    var qualified = $"{declaring}.{reader.GetString(method.Name)}";
                    foreach (var combination in Combinations(parameters))
                    {
                        var fullName = combination.IsEmpty
                            ? qualified
                            : $"{qualified}({string.Join(", ", combination.Select(static p => $"{p.Name}: {p.Value}"))})";
                        var workload = new WorkloadCase(declared.OperationId, declared.OperationRevision, declared.FixtureId,
                            declared.FixtureRevision,
                            combination.ToImmutableSortedDictionary(static p => p.Name, static p => new ParameterValue("number", p.Value),
                                StringComparer.Ordinal),
                            declared.Boundary, [MetricKind.Time, MetricKind.ManagedAllocation]);
                        mappings.Add(new IdentityMapping(fullName, productCommit, relative!, digest, workload));
                    }
                }
            }
        }
        catch (BadImageFormatException)
        {
            return [];
        }

        return mappings.ToImmutable();
    }

    private static (string OperationId, string OperationRevision, string FixtureId, string FixtureRevision, string Boundary, string SourceFile)?
        Declaration(MetadataReader reader, AttributeTypeNames names, MethodDefinition method)
    {
        foreach (var attribute in method.GetCustomAttributes().Select(reader.GetCustomAttribute))
        {
            if (AttributeName(reader, attribute) != WorkloadIdentityAttribute)
            {
                continue;
            }

            var arguments = Decode(attribute, names)?.FixedArguments;
            if (arguments is not { Length: 6 } || arguments.Value.Any(static argument => argument.Value is not string { Length: > 0 }))
            {
                return null;
            }

            var text = arguments.Value.Select(static argument => (string) argument.Value!).ToArray();
            return (text[0], text[1], text[2], text[3], text[4], text[5]);
        }

        return null;
    }

    private static bool HasUnreadArguments(MetadataReader reader, TypeDefinition type, MethodDefinition method) =>
        method.GetCustomAttributes().Select(reader.GetCustomAttribute).Any(attribute => AttributeName(reader, attribute)
            is "BenchmarkDotNet.Attributes.ArgumentsAttribute" or "BenchmarkDotNet.Attributes.ArgumentsSourceAttribute")
        || type.GetProperties().SelectMany(handle => reader.GetPropertyDefinition(handle).GetCustomAttributes())
            .Concat(type.GetFields().SelectMany(handle => reader.GetFieldDefinition(handle).GetCustomAttributes()))
            .Select(reader.GetCustomAttribute).Any(attribute => AttributeName(reader, attribute)
                is "BenchmarkDotNet.Attributes.ParamsSourceAttribute" or "BenchmarkDotNet.Attributes.ParamsAllValuesAttribute");

    private static ImmutableArray<(string Name, ImmutableArray<string> Values)>? Parameters(MetadataReader reader, AttributeTypeNames names,
        TypeDefinition type)
    {
        var members = type.GetFields().Select(handle => reader.GetFieldDefinition(handle))
            .Select(field => (Name: reader.GetString(field.Name), Attributes: field.GetCustomAttributes()))
            .Concat(type.GetProperties().Select(handle => reader.GetPropertyDefinition(handle))
                .Select(property => (Name: reader.GetString(property.Name), Attributes: property.GetCustomAttributes())));
        var parameters = ImmutableArray.CreateBuilder<(string Name, ImmutableArray<string> Values)>();
        foreach (var (name, attributes) in members)
        {
            foreach (var attribute in attributes.Select(reader.GetCustomAttribute))
            {
                if (AttributeName(reader, attribute) != ParamsAttribute)
                {
                    continue;
                }

                if (Decode(attribute, names)?.FixedArguments is not [{ Value: ImmutableArray<CustomAttributeTypedArgument<string>> values }]
                    || values.IsEmpty || values.Any(static value => !IsWholeNumber(value.Type) || value.Value is null))
                {
                    return null;
                }

                parameters.Add((name, [.. values.Select(static value => Convert.ToString(value.Value, CultureInfo.InvariantCulture)!)]));
            }
        }

        return parameters.ToImmutable();
    }

    private static IEnumerable<ImmutableArray<(string Name, string Value)>> Combinations(
        ImmutableArray<(string Name, ImmutableArray<string> Values)> parameters) =>
        parameters.Aggregate(new[] { ImmutableArray<(string Name, string Value)>.Empty }.AsEnumerable(),
            static (combinations, parameter) => combinations.SelectMany(_ => parameter.Values, (combination, value) =>
                combination.Add((parameter.Name, value))));

    private static CustomAttributeValue<string>? Decode(CustomAttribute attribute, AttributeTypeNames names)
    {
        try
        {
            return attribute.DecodeValue(names);
        }
        catch (Exception exception) when (exception is InvalidDataException or BadImageFormatException or ArgumentException)
        {
            return null;
        }
    }

    private static string? AttributeName(MetadataReader reader, CustomAttribute attribute)
    {
        var parent = attribute.Constructor.Kind switch
        {
            HandleKind.MethodDefinition => (EntityHandle) reader.GetMethodDefinition((MethodDefinitionHandle) attribute.Constructor).GetDeclaringType(),
            HandleKind.MemberReference => reader.GetMemberReference((MemberReferenceHandle) attribute.Constructor).Parent,
            _ => default,
        };
        return parent.Kind switch
        {
            HandleKind.TypeDefinition => reader.GetTypeDefinition((TypeDefinitionHandle) parent) is var type
                ? $"{reader.GetString(type.Namespace)}.{reader.GetString(type.Name)}" : null,
            HandleKind.TypeReference => reader.GetTypeReference((TypeReferenceHandle) parent) is var reference
                ? $"{reader.GetString(reference.Namespace)}.{reader.GetString(reference.Name)}" : null,
            _ => null,
        };
    }

    private static bool IsWholeNumber(string type) => type is nameof(PrimitiveTypeCode.SByte) or nameof(PrimitiveTypeCode.Byte)
        or nameof(PrimitiveTypeCode.Int16) or nameof(PrimitiveTypeCode.UInt16) or nameof(PrimitiveTypeCode.Int32)
        or nameof(PrimitiveTypeCode.UInt32) or nameof(PrimitiveTypeCode.Int64) or nameof(PrimitiveTypeCode.UInt64);

    private static string Normalise(string path) => path.Replace('\\', '/');

    private const string WorkloadIdentityAttribute = "Specht.Benchmarks.WorkloadIdentityAttribute";
    private const string ParamsAttribute = "BenchmarkDotNet.Attributes.ParamsAttribute";
}
