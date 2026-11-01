using System.IO;
using System.Reflection.Metadata;

namespace Specht.Comparison;

/// <summary>Names the argument types of a custom attribute blob from metadata alone, refusing enumerations it cannot resolve.</summary>
internal sealed class AttributeTypeNames : ICustomAttributeTypeProvider<string>
{
    public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode.ToString();

    public string GetSystemType() => SystemType;

    public string GetSZArrayType(string elementType) => elementType + "[]";

    public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
    {
        var type = reader.GetTypeDefinition(handle);
        return $"{reader.GetString(type.Namespace)}.{reader.GetString(type.Name)}";
    }

    public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
    {
        var type = reader.GetTypeReference(handle);
        return $"{reader.GetString(type.Namespace)}.{reader.GetString(type.Name)}";
    }

    public string GetTypeFromSerializedName(string name) => name;

    public PrimitiveTypeCode GetUnderlyingEnumType(string type) =>
        throw new InvalidDataException("An enumeration argument cannot be decoded without loading its assembly.");

    public bool IsSystemType(string type) => type == SystemType;

    private const string SystemType = "System.Type";
}
