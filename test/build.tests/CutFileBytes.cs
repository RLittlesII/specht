using System.Text;

namespace Specht.Build.Tests;

public sealed class CutFileBytes : TheoryData<string, string, byte[], byte[]>
{
    public CutFileBytes()
    {
        Add(
            "src/tool/schema/v0.2.0/frontmatter/feature.schema.json",
            "src/tool/schema/v0.3.0/frontmatter/feature.schema.json",
            [.. Bom, .. Encoding.UTF8.GetBytes(
                "{\r\n  \"$id\": \"https://github.com/rlittlesii/specht/schema/v0.2.0/frontmatter/feature.schema.json\",\r\n"
                + "  \"description\": \"Since 0.2.0.\"\r\n}\r\n")],
            [.. Bom, .. Encoding.UTF8.GetBytes(
                "{\r\n  \"$id\": \"https://github.com/rlittlesii/specht/schema/v0.3.0/frontmatter/feature.schema.json\",\r\n"
                + "  \"description\": \"Since 0.2.0.\"\r\n}\r\n")]);
        Add(
            "docs/rules/v0.2.0/SPEC001.md",
            "docs/rules/v0.3.0/SPEC001.md",
            [.. Bom, .. Encoding.UTF8.GetBytes(
                "# SPEC001\r\n\r\nIntroduced in 0.2.0.\r\n\r\n| Property         | Value       |\r\n"
                + "| ---------------- | ----------- |\r\n| Schema version   | 0.2.0       |")],
            [.. Bom, .. Encoding.UTF8.GetBytes(
                "# SPEC001\r\n\r\nIntroduced in 0.2.0.\r\n\r\n| Property         | Value       |\r\n"
                + "| ---------------- | ----------- |\r\n| Schema version   | 0.3.0       |")]);
        Add(
            "src/tool/manifest/v0.2.0/manifest.json",
            "src/tool/manifest/v0.3.0/manifest.json",
            Encoding.UTF8.GetBytes("{\n  \"schemaVersion\": \"0.2.0\"\n}\n"),
            Encoding.UTF8.GetBytes("{\n  \"schemaVersion\": \"0.2.0\"\n}\n"));
        Add(
            "src/tool/templates/v0.2.0/feature.md",
            "src/tool/templates/v0.3.0/feature.md",
            [0, 255, 13, 10, .. Encoding.UTF8.GetBytes("0.2.0")],
            [0, 255, 13, 10, .. Encoding.UTF8.GetBytes("0.2.0")]);
    }

    private static readonly byte[] Bom = [0xEF, 0xBB, 0xBF];
}
