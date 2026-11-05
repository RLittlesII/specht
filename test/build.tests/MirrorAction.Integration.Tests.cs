using System.Diagnostics;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class MirrorActionIntegrationTests
{
    [Fact]
    public async Task B066_DefaultScript_WhenRun_ShouldMirrorTheConsumerCheckoutWithoutChangingItsFiles()
    {
        // Given
        using var checkout = new ActionCheckout();
        await checkout.Initialize();

        // When
        var (ExitCode, Output, Error, Calls, Summary) = await checkout.Run();

        // Then
        ExitCode.Should().Be(0, Error);
        Calls.Should().Contain("0041: Consumer task").And.Contain("sample/consumer").And.Contain("0.8.0");
        Calls.Should().Contain("repos/sample/consumer/milestones\n").And.Contain("POST\n");
        Summary.Should().Contain("Work-item mirror").And.Contain("created").And.Contain("milestones");
    }

    [Fact]
    public async Task B068_DryRun_WhenRun_ShouldPrintThePlanWithoutWritingToGitHub()
    {
        // Given
        using var checkout = new ActionCheckout();
        await checkout.Initialize();

        // When
        var (ExitCode, Output, Error, Calls, Summary) = await checkout.Run(("MIRROR_DRY_RUN", "true"));

        // Then
        ExitCode.Should().Be(0, Error);
        Output.Should().Contain("create_issue").And.Contain("Consumer task");
        Calls.Should().NotContain("POST\n").And.NotContain("PATCH\n").And.NotContain("@@\nissue\ncreate\n");
        Summary.Should().Contain("Work-item mirror");
    }

    [Fact]
    public async Task B069_RepositoryScript_WhenSelected_ShouldRunTheExistingMirrorWithoutChangingTheCheckout()
    {
        // Given
        using var checkout = new ActionCheckout();
        await checkout.Initialize();

        // When
        var (ExitCode, Output, Error, Calls, Summary) = await checkout.Run(
            ("MIRROR_SCRIPT", Path.Combine(checkout.Repository, ".github/mirror/mirror_items.py")));

        // Then
        ExitCode.Should().Be(0, Error);
        Calls.Should().Contain("0041: Consumer task").And.Contain("sample/consumer");
        Summary.Should().Contain("Work-item mirror");
    }

    [Theory]
    [ClassData(typeof(MirrorActionExitCodes))]
    public async Task B067_B070_B071_ConsumerScript_WhenRun_ShouldForwardInputsAndReturnItsExitCodeWithoutFallback(int exitCode)
    {
        // Given
        using var checkout = new ActionCheckout();
        await checkout.Initialize();
        const string script = "scripts/custom $(touch injected).sh";
        await checkout.WriteScript(script, executable: true);

        // When
        var (ExitCode, Output, Error, Calls, Summary) = await checkout.Run(
            ("MIRROR_ROOT", "consumer checkout"), ("MIRROR_SCRIPT", script),
            ("MIRROR_BEFORE", new string('a', 40)), ("MIRROR_AFTER", new string('b', 40)),
            ("MIRROR_BACKFILL", "true"), ("MIRROR_DRY_RUN", "true"), ("MIRROR_TEST_EXIT", exitCode.ToString()));

        // Then
        ExitCode.Should().Be(exitCode, Error);
        Output.Should().Contain("root matches cwd").And.Contain("token inherited");
        Output.Should().Contain("<--dry-run>\n<--root>\n");
        Output.Should().Contain("<--repo>\n<sample/consumer>\n<--before>\n<" + new string('a', 40) + ">\n");
        Output.Should().Contain("<--after>\n<" + new string('b', 40) + ">\n<--backfill>\n");
        Summary.Should().Be("Consumer summary\n");
        Calls.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(MirrorActionInvalidInputs))]
    public async Task B067_InvalidInput_WhenRun_ShouldFailBeforeStartingTheMirror(string name, string value)
    {
        // Given
        using var checkout = new ActionCheckout();
        await checkout.Initialize();

        // When
        var (ExitCode, Output, Error, Calls, Summary) = await checkout.Run((name, value));

        // Then
        ExitCode.Should().Be(2, Error);
        Error.Should().NotBeEmpty().And.NotContain("Traceback");
        Calls.Should().BeEmpty();
        Summary.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(MirrorActionUnstartableScripts))]
    public async Task B071_UnstartableScript_WhenRun_ShouldFailWithoutFallback(string script)
    {
        // Given
        using var checkout = new ActionCheckout();
        await checkout.Initialize();
        await checkout.WriteScript("not-executable.sh", executable: false);

        // When
        var (ExitCode, Output, Error, Calls, Summary) = await checkout.Run(("MIRROR_SCRIPT", script));

        // Then
        ExitCode.Should().Be(2, Error);
        Error.Should().Contain("script could not be started").And.NotContain("Traceback");
        Calls.Should().BeEmpty();
        Summary.Should().BeEmpty();
    }

    private sealed class ActionCheckout : IDisposable
    {
        public ActionCheckout()
        {
            Repository = FindRepository();
            _scratch = Path.Combine(Repository, ".artifacts", "mirror-action-tests", Guid.NewGuid().ToString("N"));
            Root = Path.Combine(_scratch, "consumer checkout");
            Directory.CreateDirectory(Path.Combine(Root, ".issue"));
            Directory.CreateDirectory(Path.Combine(_scratch, "bin"));
        }

        public string Repository { get; }

        public string Root { get; }

        public async Task Initialize()
        {
            await File.WriteAllTextAsync(Path.Combine(Root, ".issue/0041-task.yml"), """
                id: "0041"
                type: task
                title: Consumer task
                status: ready
                risk: 1
                parent: null
                depends_on: []
                summary: |
                  Consumer work.
                """, TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(Root, ".issue/.goal"), "0.8.0\n0041\n", TestContext.Current.CancellationToken);
            var gh = Path.Combine(_scratch, "bin/gh");
            await File.WriteAllTextAsync(gh, """
                #!/bin/sh
                printf '@@\n' >> "$MIRROR_TEST_CALLS"
                printf '%s\n' "$@" >> "$MIRROR_TEST_CALLS"
                case "$1:$2" in
                  issue:create) printf 'https://github.com/sample/consumer/issues/1\n' ;;
                  label:list|label:create) ;;
                  api:*)
                    case " $* " in
                      *' POST '*) printf '1\n' ;;
                      *) printf '[]\n' ;;
                    esac ;;
                  *) printf 'unexpected gh command\n' >&2; exit 1 ;;
                esac
                """ + "\n", TestContext.Current.CancellationToken);
            MakeExecutable(gh);
            foreach (var args in new[]
            {
                new[] { "init", "-b", "main" }, new[] { "add", "." },
                new[] { "-c", "core.hooksPath=disabled-hooks", "-c", "commit.gpgsign=false", "-c", "user.name=Tester",
                    "-c", "user.email=tester@example.invalid", "commit", "-m", "Initial consumer tree" },
            })
            {
                var (ExitCode, Output, Error) = await Start("git", args, Root, []);
                ExitCode.Should().Be(0, Error);
            }
        }

        public async Task WriteScript(string path, bool executable)
        {
            var target = Path.Combine(Root, path);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            await File.WriteAllTextAsync(target, """
                #!/bin/sh
                [ "$PWD" = "$3" ] && printf 'root matches cwd\n'
                [ "$GH_TOKEN" = 'synthetic-token' ] && printf 'token inherited\n'
                printf '<%s>\n' "$@"
                printf 'Consumer summary\n' >> "$GITHUB_STEP_SUMMARY"
                exit "$MIRROR_TEST_EXIT"
                """ + "\n", TestContext.Current.CancellationToken);
            if (executable)
            {
                MakeExecutable(target);
            }
        }

        public async Task<(int ExitCode, string Output, string Error, string Calls, string Summary)> Run(
            params (string Name, string Value)[] overrides)
        {
            var action = Path.Combine(Repository, ".github/actions/mirror");
            var metadata = await File.ReadAllTextAsync(Path.Combine(action, "action.yml"), TestContext.Current.CancellationToken);
            var shell = string.Join('\n', metadata.Split("      run: |\n", 2)[1].Split('\n')
                .Select(static line => line.StartsWith("        ", StringComparison.Ordinal) ? line[8..] : line));
            var calls = Path.Combine(_scratch, "calls");
            var summary = Path.Combine(_scratch, "summary");
            var environment = new Dictionary<string, string>
            {
                ["PATH"] = Path.Combine(_scratch, "bin") + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH"),
                ["MIRROR_ACTION_PATH"] = action,
                ["MIRROR_ROOT"] = Root,
                ["MIRROR_REPOSITORY"] = "sample/consumer",
                ["MIRROR_SCRIPT"] = string.Empty,
                ["MIRROR_BEFORE"] = string.Empty,
                ["MIRROR_AFTER"] = string.Empty,
                ["MIRROR_BACKFILL"] = "false",
                ["MIRROR_DRY_RUN"] = "false",
                ["GH_TOKEN"] = "synthetic-token",
                ["PYTHONPYCACHEPREFIX"] = Path.Combine(Root, "bytecode"),
                ["MIRROR_TEST_CALLS"] = calls,
                ["MIRROR_TEST_EXIT"] = "0",
                ["GITHUB_STEP_SUMMARY"] = summary,
                ["GITHUB_ENV"] = Path.Combine(_scratch, "environment"),
                ["GITHUB_OUTPUT"] = Path.Combine(_scratch, "output"),
                ["GITHUB_PATH"] = Path.Combine(_scratch, "path"),
            };
            foreach (var (name, value) in overrides)
            {
                environment[name] = value;
            }

            var before = Snapshot();
            var (ExitCode, Output, Error) = await Start("bash", ["-e", "-o", "pipefail", "-c", shell], _scratch, environment);
            Snapshot().Should().BeEquivalentTo(before, "the action must not change the consumer checkout");
            return (ExitCode, Output, Error,
                File.Exists(calls) ? await File.ReadAllTextAsync(calls, TestContext.Current.CancellationToken) : string.Empty,
                File.Exists(summary) ? await File.ReadAllTextAsync(summary, TestContext.Current.CancellationToken) : string.Empty);
        }

        public void Dispose() => Directory.Delete(_scratch, recursive: true);

        private static void MakeExecutable(string path)
        {
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                    UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
            }
        }

        private static async Task<(int ExitCode, string Output, string Error)> Start(
            string command, string[] args, string directory, IEnumerable<KeyValuePair<string, string>> environment)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo(command)
            {
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            foreach (var arg in args)
            {
                process.StartInfo.ArgumentList.Add(arg);
            }

            foreach (var (name, value) in environment)
            {
                process.StartInfo.Environment[name] = value;
            }

            process.Start();
            var output = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            var error = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            return (process.ExitCode, await output, await error);
        }

        private static string FindRepository()
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "specht.slnx")))
                {
                    return directory.FullName;
                }
            }

            throw new DirectoryNotFoundException("Could not locate the repository root.");
        }

        private Dictionary<string, byte[]> Snapshot() => Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(Root, path), File.ReadAllBytes);

        private readonly string _scratch;
    }
}
