using AwesomeAssertions;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Tests.Mcp;

/// <summary>
/// Holds how the parent of a process is read from one <c>/proc/&lt;pid&gt;/stat</c> line on Linux, the link a cancelled run's
/// descendants are found through: the field after the last <c>)</c>, since a command name may hold spaces and parentheses, and no
/// parent for a line that is empty or cut short (0132-F3 B-037).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ProcStatLineUnitTests
{
    [Theory]
    [ClassData(typeof(StatLinesWithAParent))]
    public void AStatLine_WhenItsParentIsRead_ShouldReadTheParentAfterTheLastParenthesis(string text, int parent)
    {
        // Given
        var line = new ProcStatLine(text);

        // When
        var read = line.Parent;

        // Then
        read.Should().Be(parent);
    }

    [Theory]
    [ClassData(typeof(StatLinesWithNoParent))]
    public void AStatLineThatIsEmptyOrCutShort_WhenItsParentIsRead_ShouldBeNoParent(string text)
    {
        // Given
        var line = new ProcStatLine(text);

        // When
        var read = line.Parent;

        // Then
        read.Should().BeNull();
    }
}
