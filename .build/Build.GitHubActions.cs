using Nuke.Common.CI.GitHubActions;

// 0055-F2: the integration workflow, generated into .github/workflows/ci.yml and committed (C-1).
// B-001, B-002: a pull request to main and a push to main. B-003, B-010: one job per image, so each operating
// system reports as its own check, named by its image alone (B-007). B-004: every gate through the entry script.
// B-008: no target here pushes a package, no package is kept as a run artifact (§ 5 #7), and the token is read-only.
[IntegrationWorkflow(
    "ci",
    GitHubActionsImage.UbuntuLatest,
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.MacOsLatest,
    OnPullRequestBranches = ["main"],
    OnPushBranches = ["main"],
    InvokedTargets = [nameof(Format), nameof(Compile), nameof(Test), nameof(SpecCheck), nameof(Pack)],
    ReadPermissions = [GitHubActionsPermissions.Contents],
    PublishArtifacts = false,
    AutoGenerate = true)]
internal partial class Build;
