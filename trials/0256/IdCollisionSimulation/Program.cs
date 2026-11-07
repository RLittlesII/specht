// Item 0256 trial, never merged: criterion 1's collision simulation over the Strategy/Decorator design in src/specht/IdGeneration.
// Modes:
//   stats [--root <repo>] [--feature <dir>] [--repetitions R] [--seed S]   N parallel branches, in memory, repeated
//   git   [--root <repo>] [--seed S]                                       N = 5 real git branches, merged, scanned
//   held  [--root <repo>] [--feature <dir>]                                ids the collision guard reads, per type
using System.Diagnostics;
using System.Globalization;
using System.IO.Abstractions;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Specht.IdGeneration;

var mode = args.Length > 0 ? args[0] : "stats";
string Option(string name, string fallback) => Array.IndexOf(args, name) is var i and >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
var root = Path.GetFullPath(Option("--root", "."));
var feature = Option("--feature", "src/specht/Manifest");
var repetitions = int.Parse(Option("--repetitions", "1000"), CultureInfo.InvariantCulture);
var seed = int.Parse(Option("--seed", "256"), CultureInfo.InvariantCulture);
var fileSystem = new FileSystem();
var reader = new HeldIds(fileSystem);

// Ids minted on one branch per type: one pull request's worth, every per-Feature type in one shared Feature (lesson 0005's 0001-F5).
var profile = new (string Type, int PerBranch)[]
{
    ("epic", 1), ("feature", 1), ("task", 2), ("adr", 1), ("lesson", 1),
    ("claim", 3), ("constraint", 1), ("openQuestion", 2), ("decision", 1), ("scenario", 3),
};

var prefixes = new Dictionary<string, string>
{
    ["epic"] = "", ["task"] = "", ["adr"] = "", ["lesson"] = "", ["decision"] = "",
    ["feature"] = "F", ["claim"] = "B-", ["constraint"] = "C-", ["openQuestion"] = "OQ-", ["scenario"] = "S-",
};

const string Base32 = NanoIdSettings.Base32;
var candidates = new List<Candidate>
{
    new("(a) sequence", null),
    new("(b) Snowflake as supplied", Snowflake("{ }")),
    new("(b) Snowflake, worker by worktree path", Snowflake("""{ "datacenter": "machine", "worker": "path" }""")),
    new("(b) Snowflake, random fields and sequence", Snowflake("""{ "datacenter": "random", "worker": "random", "sequenceStart": "random" }""")),
    new("(g) Nano ID base32, size 4", Nano(Base32, 4)),
    new("(g) Nano ID base32, size 5", Nano(Base32, 5)),
    new("(g) Nano ID base32, size 6", Nano(Base32, 6)),
    new("(g) Nano ID base32, size 8", Nano(Base32, 8)),
    new("(g) Nano ID url-safe, size 21", Nano(NanoIdSettings.UrlSafe, 21)),
};

switch (mode)
{
    case "held":
        Held();
        break;
    case "git":
        Git();
        break;
    default:
        Stats();
        break;
}

return;

JsonObject Snowflake(string settings) =>
    new() { ["strategy"] = "snowflake", ["snowflake"] = JsonNode.Parse(settings), ["encoding"] = "base32" };

JsonObject Nano(string alphabet, int size) =>
    new() { ["strategy"] = "nanoid", ["nanoid"] = new JsonObject { ["alphabet"] = alphabet, ["size"] = size } };

JsonObject Manifest(Candidate candidate)
{
    var mint = new JsonObject();
    var identifiers = new JsonObject();
    foreach (var (type, _) in profile)
    {
        var settings = candidate.Settings!.DeepClone().AsObject();
        settings["prefix"] = prefixes[type];
        settings["attempts"] = 8;
        mint[type] = settings;
        identifiers[type] = $"^{Regex.Escape(prefixes[type])}[0-9A-Za-z_-]+$";
    }

    return new JsonObject { ["identifiers"] = identifiers, ["mint"] = mint };
}

IReadOnlySet<string> MainHeld(string at, string type) => reader.Read(at, type, HeldIds.PerFeature(type) ? feature : null);

IIdGenerator Sequence(string type, ICollection<string> held)
{
    var prefix = prefixes[type];
    var numbers = held.Select(id => Regex.Match(id, $"^{Regex.Escape(prefix)}([0-9]+)[a-z]?$"))
        .Where(static match => match.Success)
        .Select(static match => int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
    var next = numbers.DefaultIfEmpty(0).Max() + 1;
    var width = prefix switch { "" => 4, "B-" or "S-" => 3, _ => 1 };
    return new Fixed(prefix + next.ToString(CultureInfo.InvariantCulture).PadLeft(width, '0'));
}

string MintOne(Candidate candidate, JsonObject? manifest, string type, ICollection<string> held, Clock clock, IRandomSource random, string branch, ref int refused)
{
    if (candidate.Settings is null)
    {
        return Sequence(type, held).Next();
    }

    string Identity(string source) => source == "path" ? $"worktrees/{branch}" : "host-1";
    var guard = (CollisionGuardDecorator)IdMinter.Create(manifest!, type, Identity, clock, random, held.Contains);
    var id = guard.Next();
    refused += guard.Refused;
    return id;
}

void Stats()
{
    var mainHeld = profile.ToDictionary(static p => p.Type, p => MainHeld(root, p.Type));
    Console.WriteLine($"Held on main, read by the collision guard (feature scope `{feature}`):");
    Console.WriteLine();
    Console.WriteLine("| Type | Held |");
    Console.WriteLine("| --- | --- |");
    foreach (var (type, _) in profile)
    {
        Console.WriteLine($"| {type} | {mainHeld[type].Count} |");
    }

    foreach (var windowSeconds in new[] { 1, 60, 3600 })
    {
        foreach (var branches in new[] { 5, 16, 64 })
        {
            Console.WriteLine();
            Console.WriteLine($"## {branches} branches from one main, mints spread over {windowSeconds} s, {repetitions} repetitions, seed {seed}");
            Console.WriteLine();
            Console.WriteLine("| Candidate | Runs with a collision | Collisions per run | Guard refusals per run | " + string.Join(" | ", profile.Select(static p => p.Type)) + " |");
            Console.WriteLine("| --- | --- | --- | --- | " + string.Join(" | ", profile.Select(static _ => "---")) + " |");
            foreach (var candidate in candidates)
            {
                if (windowSeconds != 1 && candidate.Settings?["strategy"]?.GetValue<string>() != "snowflake")
                {
                    continue;
                }

                var manifest = candidate.Settings is null ? null : Manifest(candidate);
                var random = new SeededRandomSource(new Random(seed));
                var byType = profile.ToDictionary(static p => p.Type, static _ => 0L);
                long runsWithCollision = 0, refusedTotal = 0;
                for (var run = 0; run < repetitions; run++)
                {
                    var minted = profile.ToDictionary(static p => p.Type, static _ => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
                    var start = new DateTimeOffset(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);
                    for (var b = 0; b < branches; b++)
                    {
                        var branch = $"b{b:00}";
                        var clock = new Clock(start.AddMilliseconds(random.Next(windowSeconds * 1000)));
                        var refused = 0;
                        foreach (var (type, count) in profile)
                        {
                            var held = new HashSet<string>(mainHeld[type], StringComparer.OrdinalIgnoreCase);
                            for (var k = 0; k < count; k++)
                            {
                                clock.Now = clock.Now.AddMilliseconds(random.Next(1000));
                                var id = MintOne(candidate, manifest, type, held, clock, random, branch, ref refused);
                                held.Add(id);
                                minted[type][id] = minted[type].GetValueOrDefault(id) + 1;
                            }
                        }

                        refusedTotal += refused;
                    }

                    var collided = false;
                    foreach (var (type, _) in profile)
                    {
                        var extra = minted[type].Values.Sum(static n => n - 1);
                        byType[type] += extra;
                        collided |= extra > 0;
                    }

                    runsWithCollision += collided ? 1 : 0;
                }

                var perRun = byType.Values.Sum() / (double)repetitions;
                Console.WriteLine(
                    $"| {candidate.Name} | {runsWithCollision}/{repetitions} | {perRun:0.####} | {refusedTotal / (double)repetitions:0.####} | "
                    + string.Join(" | ", profile.Select(p => (byType[p.Type] / (double)repetitions).ToString("0.####", CultureInfo.InvariantCulture))) + " |");
            }
        }
    }
}

void Held()
{
    foreach (var (type, _) in profile)
    {
        var held = MainHeld(root, type);
        Console.WriteLine($"{type}\t{held.Count}\t{string.Join(" ", held.Order(StringComparer.Ordinal).Take(5))}");
    }
}

void Git()
{
    const int branches = 5;
    Console.WriteLine($"## {branches} real git branches cut from one main, merged in order, seed {seed}");
    Console.WriteLine();
    Console.WriteLine("| Candidate | Merges git stopped on | Files git reported in conflict | Duplicates after merge | " + string.Join(" | ", profile.Select(static p => p.Type)) + " |");
    Console.WriteLine("| --- | --- | --- | --- | " + string.Join(" | ", profile.Select(static _ => "---")) + " |");
    foreach (var candidate in candidates.Where(static c => c.Name is "(a) sequence" or "(b) Snowflake as supplied" or "(g) Nano ID base32, size 6"))
    {
        var repo = Path.Combine(Path.GetTempPath(), $"specht-0256-{Regex.Replace(candidate.Name, "[^a-z0-9]+", "-")}");
        if (Directory.Exists(repo))
        {
            Directory.Delete(repo, true);
        }

        Directory.CreateDirectory(repo);
        Run(repo, "init", "-q", "-b", "main");
        Seed(repo);
        Run(repo, "add", "-A");
        Run(repo, "commit", "-q", "-m", "main");

        var manifest = candidate.Settings is null ? null : Manifest(candidate);
        var random = new SeededRandomSource(new Random(seed));
        var start = new DateTimeOffset(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);
        for (var b = 0; b < branches; b++)
        {
            var branch = $"b{b:00}";
            Run(repo, "checkout", "-q", "-b", branch, "main");
            var clock = new Clock(start.AddMilliseconds(random.Next(1000)));
            var refused = 0;
            foreach (var (type, count) in profile)
            {
                for (var k = 0; k < count; k++)
                {
                    clock.Now = clock.Now.AddMilliseconds(random.Next(1000));
                    var held = new HashSet<string>(reader.Read(repo, type, HeldIds.PerFeature(type) ? "f" : null), StringComparer.OrdinalIgnoreCase);
                    var id = MintOne(candidate, manifest, type, held, clock, random, branch, ref refused);
                    Write(repo, type, id, $"{branch}-{k}");
                }
            }

            Run(repo, "add", "-A");
            Run(repo, "commit", "-q", "-m", branch);
        }

        Run(repo, "checkout", "-q", "main");
        int stopped = 0, conflicted = 0;
        for (var b = 0; b < branches; b++)
        {
            if (Run(repo, "merge", "-q", "--no-edit", $"b{b:00}").Code == 0)
            {
                continue;
            }

            stopped++;
            foreach (var path in Run(repo, "diff", "--name-only", "--diff-filter=U").Output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                conflicted++;
                var full = Path.Combine(repo, path);
                File.WriteAllText(full + ".base", Run(repo, "show", $":1:{path}").Output);
                File.WriteAllText(full + ".theirs", Run(repo, "show", $":3:{path}").Output);
                File.WriteAllText(full, Run(repo, "show", $":2:{path}").Output);
                Run(repo, "merge-file", "--union", path, path + ".base", path + ".theirs");
                File.Delete(full + ".base");
                File.Delete(full + ".theirs");
                Run(repo, "add", path);
            }

            Run(repo, "commit", "-q", "--no-edit");
        }

        var duplicates = profile.ToDictionary(static p => p.Type, p => Ids(repo, p.Type).GroupBy(static id => id, StringComparer.OrdinalIgnoreCase).Sum(static g => g.Count() - 1));
        Console.WriteLine(
            $"| {candidate.Name} | {stopped}/{branches} | {conflicted} | {duplicates.Values.Sum()} | "
            + string.Join(" | ", profile.Select(p => duplicates[p.Type])) + " |");
        Directory.Delete(repo, true);
    }
}

void Seed(string repo)
{
    Directory.CreateDirectory(Path.Combine(repo, ".issue"));
    Directory.CreateDirectory(Path.Combine(repo, ".spec", "adr"));
    Directory.CreateDirectory(Path.Combine(repo, ".spec", "lessons"));
    Directory.CreateDirectory(Path.Combine(repo, "f", ".spec", "decisions"));
    Directory.CreateDirectory(Path.Combine(repo, "epics", "0001-seed"));
    File.WriteAllText(Path.Combine(repo, ".issue", "0002-seed.yml"), "id: \"0002\"\n");
    File.WriteAllText(Path.Combine(repo, ".spec", "adr", "0001-seed.md"), "seed\n");
    File.WriteAllText(Path.Combine(repo, ".spec", "lessons", "0001-seed.md"), "seed\n");
    File.WriteAllText(Path.Combine(repo, "f", ".spec", "decisions", "0001-seed.md"), "seed\n");
    File.WriteAllText(Path.Combine(repo, "epics", "0001-seed", "epic.md"), "---\nid: \"0001\"\n---\n");
    File.WriteAllText(Path.Combine(repo, "f", ".spec", "README.md"), "---\nid: \"F1\"\n---\n\n| ID | Row |\n| --- | --- |\n| B-001 | seed |\n| C-1 | seed |\n| OQ-1 | seed |\n");
    File.WriteAllText(Path.Combine(repo, "f", ".spec", "f.feature"), "Feature: f\n\n  @S-001\n  Scenario: seed\n");
}

void Write(string repo, string type, string id, string tag)
{
    switch (type)
    {
        case "task":
            File.WriteAllText(Path.Combine(repo, ".issue", $"{id}-{tag}.yml"), $"id: \"{id}\"\n");
            break;
        case "adr":
            File.WriteAllText(Path.Combine(repo, ".spec", "adr", $"{id}-{tag}.md"), "x\n");
            break;
        case "lesson":
            File.WriteAllText(Path.Combine(repo, ".spec", "lessons", $"{id}-{tag}.md"), "x\n");
            break;
        case "decision":
            File.WriteAllText(Path.Combine(repo, "f", ".spec", "decisions", $"{id}-{tag}.md"), "x\n");
            break;
        case "epic":
            Directory.CreateDirectory(Path.Combine(repo, "epics", $"{id}-{tag}"));
            File.WriteAllText(Path.Combine(repo, "epics", $"{id}-{tag}", "epic.md"), $"---\nid: \"{id}\"\n---\n");
            break;
        case "feature":
            Directory.CreateDirectory(Path.Combine(repo, $"g-{tag}", ".spec"));
            File.WriteAllText(Path.Combine(repo, $"g-{tag}", ".spec", "README.md"), $"---\nid: \"{id}\"\n---\n");
            break;
        case "scenario":
            File.AppendAllText(Path.Combine(repo, "f", ".spec", "f.feature"), $"\n  @{id}\n  Scenario: {tag}\n");
            break;
        default:
            File.AppendAllText(Path.Combine(repo, "f", ".spec", "README.md"), $"| {id} | {tag} |\n");
            break;
    }
}

IEnumerable<string> Ids(string repo, string type)
{
    string[] Names(string directory) => Directory.GetFiles(directory).Select(static f => Path.GetFileName(f).Split('-')[0]).ToArray();
    IEnumerable<string> Matches(string file, string pattern) => Regex.Matches(File.ReadAllText(file), pattern, RegexOptions.Multiline).Select(static m => m.Groups[1].Value);
    var readme = Path.Combine(repo, "f", ".spec", "README.md");
    return type switch
    {
        "task" => Names(Path.Combine(repo, ".issue")),
        "adr" => Names(Path.Combine(repo, ".spec", "adr")),
        "lesson" => Names(Path.Combine(repo, ".spec", "lessons")),
        "decision" => Names(Path.Combine(repo, "f", ".spec", "decisions")),
        "epic" => Directory.GetDirectories(Path.Combine(repo, "epics")).Select(static d => Path.GetFileName(d).Split('-')[0]),
        "feature" => Directory.GetDirectories(repo, "g-*").Select(d => Matches(Path.Combine(d, ".spec", "README.md"), @"^id: ""([^""]+)""$").Single()).Append("F1"),
        "scenario" => Matches(Path.Combine(repo, "f", ".spec", "f.feature"), @"@(S-\S+)"),
        "claim" => Matches(readme, @"^\| (B-[^ |]+) "),
        "constraint" => Matches(readme, @"^\| (C-[^ |]+) "),
        _ => Matches(readme, @"^\| (OQ-[^ |]+) "),
    };
}

(int Code, string Output) Run(string repo, params string[] arguments)
{
    var info = new ProcessStartInfo("git") { WorkingDirectory = repo, RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (var setting in new[] { "-c", "user.name=simulation", "-c", "user.email=simulation@example.invalid", "-c", "commit.gpgsign=false", "-c", "core.hooksPath=/dev/null" })
    {
        info.ArgumentList.Add(setting);
    }

    foreach (var argument in arguments)
    {
        info.ArgumentList.Add(argument);
    }

    using var process = Process.Start(info)!;
    var output = process.StandardOutput.ReadToEnd();
    process.StandardError.ReadToEnd();
    process.WaitForExit();
    return (process.ExitCode, output);
}

internal sealed record Candidate(string Name, JsonObject? Settings);

internal sealed class Clock(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}

internal sealed class Fixed(string id) : IIdGenerator
{
    public string Next() => id;
}
