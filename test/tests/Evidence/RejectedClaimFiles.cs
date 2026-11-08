using Xunit;

namespace Specht.Tests.Evidence;

public sealed class RejectedClaimFiles : TheoryData<string, bool>
{
    public RejectedClaimFiles()
    {
        Add("Feature: Read\n\n  @need/7\n  Scenario: Reads a value\n    Then it is read\n", true);
        Add("Scenario: Orphan\n  Given a value\n", false);
        Add("# language: fr\nFonctionnalité: Lire\n\n  @need/7\n  Scénario: Lit une valeur\n    Alors elle est lue\n", false);
    }
}
