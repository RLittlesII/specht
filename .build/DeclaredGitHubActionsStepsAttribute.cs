using System;
using System.Collections.Generic;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Rocket.Surgery.Nuke.GithubActions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class DeclaredGitHubActionsStepsAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images)
    : GitHubActionsStepsAttribute(name, image, images)
{
    public override AbsolutePath ConfigurationFile => _creatingConfiguration
        ? base.ConfigurationFile / "action-versions"
        : base.ConfigurationFile;

    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        _creatingConfiguration = true;
        try
        {
            return base.GetConfiguration(relevantTargets);
        }
        finally
        {
            _creatingConfiguration = false;
        }
    }

    private bool _creatingConfiguration;
}
