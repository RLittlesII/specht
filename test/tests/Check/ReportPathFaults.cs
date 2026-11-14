using System.Text.Json;
using Specht.Manifest;
using Specht.Tree;

namespace Specht.Tests.Check;

/// <summary>
/// What a check run throws when it checks nothing, and the exit code the command folds it into: a root that is not a
/// directory and a root with no manifest exit <c>2</c>, a manifest that does not parse and one the check rejects exit
/// <c>3</c> (<c>0001-F3</c> B-044, B-045; <c>0001-F2</c> B-005 to B-007).
/// </summary>
public sealed class ReportPathFaults : TheoryData<Exception, int>
{
    public ReportPathFaults()
    {
        Add(new SpechtRootNotFoundException(), 2);
        Add(new SpechtManifestNotFoundException(retiredManifestPresent: false), 2);
        Add(new SpechtManifestUnreadableException(new JsonException("The manifest is not well-formed JSON.")), 3);
        Add(new SpechtManifestException("The manifest carries a key the tool does not know."), 3);
    }
}
