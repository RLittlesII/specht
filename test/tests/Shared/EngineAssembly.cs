using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Specht.Tool;

namespace Specht.Tests.Shared;

/// <summary>
/// The library <c>src/specht</c> builds, read from its metadata rather than loaded (<c>0001-F1</c> B-005, B-006), and the
/// tool's own assembly beside it, read the same way for the references a network call needs (<c>0001-F7</c> B-025). It
/// reads the file the build wrote, never the loaded assembly: a coverage run instruments the copy the test process loads
/// (<c>0055-F3</c> B-001) and adds references of its own to it.
/// </summary>
public static class EngineAssembly
{
    /// <summary>
    /// Gets the members and types whose reference would let the machine reach a verdict (decision 0006): the machine
    /// name, the environment, the working directory, the clock, the local time zone and the current culture.
    /// </summary>
    public static IReadOnlyList<string> Forbidden { get; } =
    [
        "System.Environment.MachineName",
        "System.Environment.GetEnvironmentVariable",
        "System.Environment.GetEnvironmentVariables",
        "System.Environment.CurrentDirectory",
        "System.IO.Directory.GetCurrentDirectory",
        "System.DateTime.Now",
        "System.DateTime.UtcNow",
        "System.DateTime.Today",
        "System.DateTimeOffset.Now",
        "System.DateTimeOffset.UtcNow",
        "System.TimeProvider",
        "System.Diagnostics.Stopwatch",
        "System.TimeZoneInfo.Local",
        "System.Globalization.CultureInfo.CurrentCulture",
        "System.Globalization.CultureInfo.CurrentUICulture",
    ];

    /// <summary>
    /// Gets the one allowed reference: the frontmatter schema check saves, sets and restores the current culture around its schema
    /// evaluation, so a dependency's culture-sensitive lowercasing cannot reach a message.
    /// </summary>
    public static Reference Exemption { get; } =
        new("System.Globalization.CultureInfo.CurrentCulture", "Specht.Rules.FrontmatterSchemaCheck.EvaluateInvariant");

    /// <summary>Gets the path of the library <c>src/specht</c> built for this configuration and target framework.</summary>
    public static string Built => BuiltUnder("specht", typeof(Specht.Check.SpechtCheck).Assembly);

    /// <summary>
    /// Gets the path of the assembly <c>src/tool</c> built for this configuration and target framework: the tool's own
    /// code, not the libraries it is published beside.
    /// </summary>
    public static string BuiltTool => BuiltUnder("tool", typeof(TypeRegistrar).Assembly);

    /// <summary>
    /// The entries of <see cref="Forbidden"/> the assembly at <paramref name="path"/> references, each with the method whose
    /// body references it. A forbidden type, and a forbidden member with no method body references, has an empty site.
    /// </summary>
    /// <param name="path">A compiled assembly.</param>
    /// <returns>Each forbidden reference and its site, once, in ordinal order.</returns>
    public static IReadOnlyList<Reference> ForbiddenReferencesIn(string path) =>
        [.. ReferencesIn(path).Where(static reference => Forbidden.Contains(reference.Member))];

    /// <summary>
    /// The types and members of a <c>System.Net</c> namespace the assembly at <paramref name="path"/> references, each with
    /// the method whose body references it: what a network call needs (<c>0001-F7</c> B-025, OQ-39).
    /// </summary>
    /// <param name="path">A compiled assembly.</param>
    /// <returns>Each network reference and its site, once, in ordinal order.</returns>
    public static IReadOnlyList<Reference> NetworkReferencesIn(string path) =>
        [.. ReferencesIn(path).Where(static reference => reference.Member.StartsWith(NetworkNamespace, StringComparison.Ordinal))];

    /// <summary>Gets network references outside the fetch adapter and its credential-free host configuration (ADR-0013).</summary>
    /// <param name="path">A compiled assembly.</param>
    /// <returns>Each network reference the check must not carry.</returns>
    public static IReadOnlyList<Reference> CheckNetworkReferencesIn(string path) =>
        [.. NetworkReferencesIn(path).Where(static reference => !AllowedNetworkReference(reference))];

    /// <summary>The references in <paramref name="references"/> other than <see cref="Exemption"/>.</summary>
    /// <param name="references">Forbidden references and their sites.</param>
    /// <returns>Every reference the exemption does not allow.</returns>
    public static IReadOnlyList<Reference> Unexempted(IEnumerable<Reference> references) =>
        [.. references.Where(static reference => reference != Exemption)];

    /// <summary>
    /// The types of the assembly at <paramref name="path"/> that reference the file system abstraction (<c>0001-F1</c>
    /// C-12): a type of <c>System.IO.Abstractions</c>, <c>IFileSystem</c> and the members it hands out, named by a base
    /// type, an interface, a field, a method's signature, a constructor's included, a method's locals, or an operand of a
    /// method's body. A compiler-generated nested type counts as the type that declares it.
    /// </summary>
    /// <param name="path">A compiled assembly.</param>
    /// <returns>Each referencing type, as <c>Namespace.Type</c> with <c>+</c> before a nested type, once, in ordinal order.</returns>
    public static IReadOnlyList<string> FileSystemTypesIn(string path) =>
        TypesReferencing(path, static type => type.StartsWith(FileSystemNamespace, StringComparison.Ordinal), static _ => false);

    /// <summary>
    /// The types of the assembly at <paramref name="path"/> whose methods read an assembly's embedded resources
    /// (<c>0001-F1</c> C-12): a call to <c>Assembly.GetManifestResourceStream</c>, <c>GetManifestResourceNames</c> or
    /// <c>GetManifestResourceInfo</c>. A compiler-generated nested type counts as the type that declares it.
    /// </summary>
    /// <param name="path">A compiled assembly.</param>
    /// <returns>Each reading type, as <c>Namespace.Type</c> with <c>+</c> before a nested type, once, in ordinal order.</returns>
    public static IReadOnlyList<string> ResourceReadingTypesIn(string path) =>
        TypesReferencing(path, static _ => false, static member => member.StartsWith(ResourceRead, StringComparison.Ordinal));

    /// <summary>A referenced type or member, and the method whose body references it, or empty where no body does.</summary>
    /// <param name="Member">The type or member, as <see cref="Forbidden"/> names it.</param>
    /// <param name="Site">The referencing method, as <c>Namespace.Type.Method</c>, with <c>+</c> before a nested type.</param>
    public sealed record Reference(string Member, string Site);

    private static bool AllowedNetworkReference(Reference reference) =>
        (reference.Site.Length == 0 && NetworkConfigurationTypes.Contains(reference.Member))
        || (reference.Member == "System.Net.Http.HttpClient.GetByteArrayAsync"
            && reference.Site.StartsWith("Specht.Manifest.UpstreamSchemaFetcher+<Fetch>", StringComparison.Ordinal))
        || (NetworkConfigurationMembers.Contains(reference.Member)
            && reference.Site.StartsWith("Specht.Tool.Features.UpstreamFetchRegistration+<>c.<AddUpstreamSchemaFetch>", StringComparison.Ordinal));

    private static string BuiltUnder(string project, Assembly assembly)
    {
        var output = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var configuration = typeof(EngineAssembly).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration
                            ?? throw new InvalidOperationException("The test assembly names no configuration.");

        return Path.Combine(
            FindRepository(output),
            "src",
            project,
            "bin",
            configuration,
            Path.GetFileName(output),
            Path.GetFileName(assembly.Location));
    }

    private static List<Reference> ReferencesIn(string path)
    {
        using var file = new PEReader(File.OpenRead(path));
        var metadata = file.GetMetadataReader();
        var members = metadata.MemberReferences.ToDictionary(handle => handle, handle => MemberName(metadata, handle));
        var sited = new HashSet<MemberReferenceHandle>();
        var references = new List<Reference>();

        foreach (var handle in metadata.MethodDefinitions)
        {
            var method = metadata.GetMethodDefinition(handle);

            if (method.RelativeVirtualAddress == 0)
            {
                continue;
            }

            var site = $"{TypeName(metadata, method.GetDeclaringType())}.{metadata.GetString(method.Name)}";

            foreach (var member in Operands(metadata, file.GetMethodBody(method.RelativeVirtualAddress)))
            {
                sited.Add(member);
                references.Add(new Reference(members[member], site));
            }
        }

        references.AddRange(members.Where(member => !sited.Contains(member.Key)).Select(static member => new Reference(member.Value, string.Empty)));
        references.AddRange(metadata.TypeReferences.Select(type => new Reference(TypeName(metadata, type), string.Empty)));

        return [.. references
            .Distinct()
            .OrderBy(static reference => reference.Member, StringComparer.Ordinal)
            .ThenBy(static reference => reference.Site, StringComparer.Ordinal)];
    }

    private static List<string> TypesReferencing(string path, Func<string, bool> matchesType, Func<string, bool> matchesMember)
    {
        using var file = new PEReader(File.OpenRead(path));
        var metadata = file.GetMetadataReader();
        var mentions = new Mentions(metadata, matchesType, matchesMember);

        return [.. metadata.TypeDefinitions
            .Where(handle => mentions.In(file, metadata.GetTypeDefinition(handle)))
            .Select(handle => TypeName(metadata, Declaring(metadata, handle)))
            .Distinct()
            .Order(StringComparer.Ordinal)];
    }

    private static TypeDefinitionHandle Declaring(MetadataReader metadata, TypeDefinitionHandle handle)
    {
        var type = metadata.GetTypeDefinition(handle);
        var declaring = type.GetDeclaringType();

        return !declaring.IsNil && metadata.GetString(type.Name).StartsWith('<') ? Declaring(metadata, declaring) : handle;
    }

    private static IEnumerable<EntityHandle> Tokens(MethodBodyBlock body)
    {
        var il = body.GetILReader();

        while (il.RemainingBytes > 0)
        {
            var value = il.ReadByte();
            var opCode = OpCodesByValue[value == 0xFE ? (short) (0xFE00 | il.ReadByte()) : value];

            switch (opCode.OperandType)
            {
                case OperandType.InlineField or OperandType.InlineMethod or OperandType.InlineTok or OperandType.InlineType:
                    yield return MetadataTokens.EntityHandle(il.ReadInt32());
                    break;
                case OperandType.InlineSwitch:
                    var targets = il.ReadInt32();
                    il.Offset += 4 * targets;
                    break;
                default:
                    il.Offset += OperandSize(opCode.OperandType);
                    break;
            }
        }
    }

    private static IEnumerable<MemberReferenceHandle> Operands(MetadataReader metadata, MethodBodyBlock body) =>
        Tokens(body)
            .Select(token => token.Kind == HandleKind.MethodSpecification
                ? metadata.GetMethodSpecification((MethodSpecificationHandle) token).Method
                : token)
            .Where(static token => token.Kind == HandleKind.MemberReference)
            .Select(static token => (MemberReferenceHandle) token);

    private static int OperandSize(OperandType operand) => operand switch
    {
        OperandType.InlineNone => 0,
        OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
        OperandType.InlineVar => 2,
        OperandType.InlineI8 or OperandType.InlineR => 8,
        _ => 4,
    };

    private static string MemberName(MetadataReader metadata, MemberReferenceHandle handle)
    {
        var member = metadata.GetMemberReference(handle);
        var name = metadata.GetString(member.Name);
        var accessed = name.StartsWith("get_", StringComparison.Ordinal) || name.StartsWith("set_", StringComparison.Ordinal) ? name[4..] : name;

        return member.Parent.Kind == HandleKind.TypeReference
            ? $"{TypeName(metadata, (TypeReferenceHandle) member.Parent)}.{accessed}"
            : accessed;
    }

    private static string TypeName(MetadataReader metadata, TypeReferenceHandle handle)
    {
        var type = metadata.GetTypeReference(handle);

        return $"{metadata.GetString(type.Namespace)}.{metadata.GetString(type.Name)}";
    }

    private static string TypeName(MetadataReader metadata, TypeDefinitionHandle handle)
    {
        var type = metadata.GetTypeDefinition(handle);
        var declaring = type.GetDeclaringType();

        return declaring.IsNil
            ? $"{metadata.GetString(type.Namespace)}.{metadata.GetString(type.Name)}"
            : $"{TypeName(metadata, declaring)}+{metadata.GetString(type.Name)}";
    }

    private static string FindRepository(string directory) =>
        File.Exists(Path.Combine(directory, "build.sh")) && Directory.Exists(Path.Combine(directory, ".nuke"))
            ? directory
            : FindRepository(Path.GetDirectoryName(directory.TrimEnd(Path.DirectorySeparatorChar))
                             ?? throw new InvalidOperationException("No repository root above the test assembly."));

    /// <summary>Whether a type definition, a signature or a token names a matching type or a matching member.</summary>
    private sealed class Mentions(MetadataReader metadata, Func<string, bool> matchesType, Func<string, bool> matchesMember)
        : ISignatureTypeProvider<bool, object?>
    {
        public bool In(PEReader file, TypeDefinition type) =>
            In(type.BaseType)
            || type.GetInterfaceImplementations().Any(handle => In(metadata.GetInterfaceImplementation(handle).Interface))
            || type.GetFields().Any(handle => metadata.GetFieldDefinition(handle).DecodeSignature(this, null))
            || type.GetMethods().Select(metadata.GetMethodDefinition).Any(method => In(method.DecodeSignature(this, null)) || In(file, method));

        public bool GetArrayType(bool elementType, ArrayShape shape) => elementType;

        public bool GetByReferenceType(bool elementType) => elementType;

        public bool GetFunctionPointerType(MethodSignature<bool> signature) => In(signature);

        public bool GetGenericInstantiation(bool genericType, ImmutableArray<bool> typeArguments) =>
            genericType || typeArguments.Any(static argument => argument);

        public bool GetGenericMethodParameter(object? genericContext, int index) => false;

        public bool GetGenericTypeParameter(object? genericContext, int index) => false;

        public bool GetModifiedType(bool modifier, bool unmodifiedType, bool isRequired) => unmodifiedType;

        public bool GetPinnedType(bool elementType) => elementType;

        public bool GetPointerType(bool elementType) => elementType;

        public bool GetPrimitiveType(PrimitiveTypeCode typeCode) => false;

        public bool GetSZArrayType(bool elementType) => elementType;

        public bool GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => false;

        public bool GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) =>
            matchesType(TypeName(reader, handle));

        public bool GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) =>
            reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);

        private static bool In(MethodSignature<bool> signature) =>
            signature.ReturnType || signature.ParameterTypes.Any(static parameter => parameter);

        private bool In(PEReader file, MethodDefinition method)
        {
            if (method.RelativeVirtualAddress == 0)
            {
                return false;
            }

            var body = file.GetMethodBody(method.RelativeVirtualAddress);

            return (!body.LocalSignature.IsNil
                    && metadata.GetStandaloneSignature(body.LocalSignature).DecodeLocalSignature(this, null).Any(static local => local))
                   || Tokens(body).Any(token => In(token));
        }

        private bool In(EntityHandle handle) => handle.Kind switch
        {
            _ when handle.IsNil => false,
            HandleKind.TypeReference => matchesType(TypeName(metadata, (TypeReferenceHandle) handle)),
            HandleKind.TypeSpecification => metadata.GetTypeSpecification((TypeSpecificationHandle) handle).DecodeSignature(this, null),
            HandleKind.FieldDefinition => metadata.GetFieldDefinition((FieldDefinitionHandle) handle).DecodeSignature(this, null),
            HandleKind.MethodDefinition => In(metadata.GetMethodDefinition((MethodDefinitionHandle) handle).DecodeSignature(this, null)),
            HandleKind.MethodSpecification => In(metadata.GetMethodSpecification((MethodSpecificationHandle) handle)),
            HandleKind.MemberReference => In((MemberReferenceHandle) handle),
            _ => false,
        };

        private bool In(MethodSpecification specification) =>
            In(specification.Method) || specification.DecodeSignature(this, null).Any(static argument => argument);

        private bool In(MemberReferenceHandle handle)
        {
            var member = metadata.GetMemberReference(handle);

            return matchesMember(MemberName(metadata, handle))
                   || In(member.Parent)
                   || (member.GetKind() == MemberReferenceKind.Method
                       ? In(member.DecodeMethodSignature(this, null))
                       : member.DecodeFieldSignature(this, null));
        }
    }

    private const string NetworkNamespace = "System.Net.";

    private const string FileSystemNamespace = "System.IO.Abstractions.";

    private const string ResourceRead = "System.Reflection.Assembly.GetManifestResource";

    private static readonly HashSet<string> NetworkConfigurationTypes = new(StringComparer.Ordinal)
    {
        "System.Net.Http.HttpClient",
        "System.Net.Http.HttpClientHandler",
        "System.Net.Http.HttpMessageHandler",
        "System.Net.Http.HttpRequestException",
        "System.Net.ICredentials",
    };

    private static readonly HashSet<string> NetworkConfigurationMembers = new(StringComparer.Ordinal)
    {
        "System.Net.Http.HttpClient..ctor",
        "System.Net.Http.HttpClientHandler..ctor",
        "System.Net.Http.HttpClientHandler.AllowAutoRedirect",
        "System.Net.Http.HttpClientHandler.UseCookies",
        "System.Net.Http.HttpClientHandler.UseDefaultCredentials",
        "System.Net.Http.HttpClientHandler.Credentials",
        "System.Net.Http.HttpClientHandler.DefaultProxyCredentials",
    };

    private static readonly Dictionary<short, OpCode> OpCodesByValue = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(static field => (OpCode) field.GetValue(null)!)
        .ToDictionary(static opCode => opCode.Value);
}
