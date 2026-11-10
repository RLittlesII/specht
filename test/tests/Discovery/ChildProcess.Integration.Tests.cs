using AwesomeAssertions;
using Specht.Discovery;

namespace Specht.Tests.Discovery;

/// <summary>
/// Holds <see cref="ChildProcess"/> against real programs: a program that starts gives back its exit code and its standard
/// output, decoded as UTF-8 with every NUL character kept, from the working directory it was given; a program that does not
/// exist is none (<c>0001-F6</c> B-005, B-006; decision 0002).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class ChildProcessIntegrationTests
{
    public static bool IsWindows => OperatingSystem.IsWindows();

    [Fact(Skip = "The fixture program is a shell script.", SkipWhen = nameof(IsWindows))]
    public void AProgramThatWritesAndExits_WhenRunInADirectory_ShouldGiveBackItsExitCodeAndItsStandardOutputWithEveryNulCharacter()
    {
        // Given
        var directory = Directory.CreateTempSubdirectory("specht-child-process-");
        File.WriteAllText(Path.Combine(directory.FullName, "marker"), "spé");

        // When
        ChildProcessResult? result;
        try
        {
            result = new ChildProcess().Run(
                "sh",
                ["-c", "printf '%s\\000' \"$(cat marker)\" second; printf ignored >&2; exit 3"],
                directory.FullName);
        }
        finally
        {
            directory.Delete(recursive: true);
        }

        // Then
        result.Should().Be(new ChildProcessResult(3, "spé\0second\0"));
    }

    [Fact]
    public void AProgramThatDoesNotExist_WhenRun_ShouldGiveBackNothing()
    {
        // When
        var result = new ChildProcess().Run("specht-program-that-does-not-exist", [], Directory.GetCurrentDirectory());

        // Then
        result.Should().BeNull();
    }
}
