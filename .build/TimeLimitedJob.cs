using Nuke.Common.Utilities;
using Rocket.Surgery.Nuke.GithubActions;

namespace Specht.Build;

/// <summary>A workflow job that GitHub Actions cancels once it has run for a stated number of minutes.</summary>
internal sealed class TimeLimitedJob : RocketSurgeonsGithubActionsJob
{
    /// <summary>Takes over every setting of a generated job and adds its time limit.</summary>
    /// <param name="job">The generated job.</param>
    /// <param name="minutes">The minutes the job may run.</param>
    public TimeLimitedJob(RocketSurgeonsGithubActionsJob job, int minutes)
        : base(job.Name)
    {
        Needs = job.Needs;
        Outputs = job.Outputs;
        Environment = job.Environment;
        Concurrency = job.Concurrency;
        Defaults = job.Defaults;
        If = job.If;
        Permissions = job.Permissions;
        FailFast = job.FailFast;
        Matrix = job.Matrix;
        RunsOn = job.RunsOn;
        Steps = job.Steps;
        Minutes = minutes;
    }

    /// <summary>Gets the minutes the job may run.</summary>
    public int Minutes { get; }

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        base.Write(writer);
        using (writer.Indent())
        {
            writer.WriteLine($"timeout-minutes: {Minutes}");
        }
    }
}
