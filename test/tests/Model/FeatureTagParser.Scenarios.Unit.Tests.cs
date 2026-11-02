using AwesomeAssertions;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>The scenarios a companion parse records, with the claim tags Gherkin gives each (0001-F12 B-008, B-009).</summary>
[Trait("Tier", "Unit")]
public sealed class FeatureTagParserScenariosUnitTests
{
    [Fact]
    public void ACompanion_WhenRead_ShouldRecordEachScenarioAndScenarioOutlineWithTheClaimTagsGherkinGivesIt()
    {
        // Given
        var structure = EngineServices.Parser().Parse("""{"schemaVersion":"0.3.0"}""");
        const string text = """
            # @B-099 is in a comment
            @B-001
            Feature: Sample

              Background:
                Given a thing

              @B-002
              # a comment between

              Scenario: Own tag
                Given a thing

              @B-003
              Scenario Outline: An outline
                Given <thing>

                @B-004
                Examples:
                  | thing |
                  | one   |

                @B-005
                Scenarios:
                  | thing |
                  | two   |

              @B-006
              Example: Not recorded
                Given a thing

              @B-007
              Scenario Template: Not recorded either
                Given <thing>

                @B-008
                Examples:
                  | thing |
                  | one   |

              @B-009
              Rule: A rule

                Scenario:   Padded title
                  Given a thing
            """;

        // When
        var file = new FeatureTagParser().Read("src/sample/.spec/sample.feature", text, structure);

        // Then
        file.RelativePath.Should().Be("src/sample/.spec/sample.feature");
        file.Scenarios.Select(static scenario => (scenario.Title, scenario.Line, string.Join(' ', scenario.Tags.Select(static tag => tag.Id))))
            .Should().Equal(
                ("Own tag", 11, "B-001 B-002"),
                ("An outline", 15, "B-001 B-003 B-004 B-005"),
                ("Padded title", 44, "B-001 B-009"));
    }
}
