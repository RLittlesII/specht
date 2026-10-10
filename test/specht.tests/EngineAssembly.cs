using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace specht.tests;

/// <summary>
/// The library <c>src/specht</c> builds, read from its metadata rather than loaded (<c>0001-F1</c> B-005, B-006). It reads
/// the file the build wrote, never the loaded assembly: a coverage run instruments the copy the test process loads
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
    /// Gets the one allowed reference: the frontmatter rule saves, sets and restores the current culture around its schema
    /// evaluation, so a dependency's culture-sensitive lowercasing cannot reach a message.
    /// </summary>
    public static Reference Exemption { get; } =
        new("System.Globalization.CultureInfo.CurrentCulture", "specht.Rules.FrontmatterSchemaRule.EvaluateInvariant");

    /// <summary>Gets the path of the library <c>src/specht</c> built for this configuration and target framework.</summary>
    public static string Built
    {
        get
        {
            var output = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            var configuration = typeof(EngineAssembly).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration
                ?? throw new InvalidOperationException("The test assembly names no configuration.");

            return Path.Combine(
                FindRepository(output),
                "src",
                "specht",
                "bin",
                configuration,
                Path.GetFileName(output),
                Path.GetFileName(typeof(SpechtRunner).Assembly.Location));
        }
    }

    /// <summary>
    /// The entries of <see cref="Forbidden"/> the assembly at <paramref name="path"/> references, each with the method whose
    /// body references it. A forbidden type, and a forbidden member no method body references, has an empty site.
    /// </summary>
    /// <param name="path">A compiled assembly.</param>
    /// <returns>Each forbidden reference and its site, once, in ordinal order.</returns>
    public static IReadOnlyList<Reference> ForbiddenReferencesIn(string path)
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
            .Where(static reference => Forbidden.Contains(reference.Member))
            .Distinct()
            .OrderBy(static reference => reference.Member, StringComparer.Ordinal)
            .ThenBy(static reference => reference.Site, StringComparer.Ordinal)];
    }

    /// <summary>The references in <paramref name="references"/> other than <see cref="Exemption"/>.</summary>
    /// <param name="references">Forbidden references and their sites.</param>
    /// <returns>Every reference the exemption does not allow.</returns>
    public static IReadOnlyList<Reference> Unexempted(IEnumerable<Reference> references) =>
        [.. references.Where(static reference => reference != Exemption)];

    /// <summary>A forbidden type or member, and the method whose body references it, or empty where no body does.</summary>
    /// <param name="Member">The type or member, as <see cref="Forbidden"/> names it.</param>
    /// <param name="Site">The referencing method, as <c>Namespace.Type.Method</c>, with <c>+</c> before a nested type.</param>
    public sealed record Reference(string Member, string Site);

    private static IEnumerable<MemberReferenceHandle> Operands(MetadataReader metadata, MethodBodyBlock body)
    {
        var il = body.GetILReader();

        while (il.RemainingBytes > 0)
        {
            var value = il.ReadByte();
            var opCode = OpCodesByValue[value == 0xFE ? (short)(0xFE00 | il.ReadByte()) : value];

            switch (opCode.OperandType)
            {
                case OperandType.InlineField or OperandType.InlineMethod or OperandType.InlineTok or OperandType.InlineType:
                    var token = MetadataTokens.EntityHandle(il.ReadInt32());

                    if (token.Kind == HandleKind.MethodSpecification)
                    {
                        token = metadata.GetMethodSpecification((MethodSpecificationHandle)token).Method;
                    }

                    if (token.Kind == HandleKind.MemberReference)
                    {
                        yield return (MemberReferenceHandle)token;
                    }

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
            ? $"{TypeName(metadata, (TypeReferenceHandle)member.Parent)}.{accessed}"
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

    private static readonly Dictionary<short, OpCode> OpCodesByValue = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(static field => (OpCode)field.GetValue(null)!)
        .ToDictionary(static opCode => opCode.Value);
}
