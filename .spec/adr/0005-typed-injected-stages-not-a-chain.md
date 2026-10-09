---
title: "ADR-0005: Typed injected stages, not a chain; a rule wrapper earned by fault isolation"
description: "The engine's statics become one coordinator, SpecCheckRunner, that calls typed stages injected from the container in an order the compiler checks: concrete manifest and model loaders, ISpecDiscovery, and the rule set. Chain of responsibility is shown against the code and rejected for the stages, for first-handler-wins and for concat-all on structural grounds; a generic IStage pipeline is a repository-convention judgement, not chosen. The first decorator around ISpecRule is fault isolation, reporting under the failing rule's own id at a fixed Error severity the severity map skips, owed a claim and a scenario before code. Records the owner's 2026-10-09 decision that a test fake is not the second implementation."
type: adr
---

# ADR-0005: Typed injected stages, not a chain; a rule wrapper earned by fault isolation

## Status

proposed - 2026-10-09. Asked for by the owner, who found that ADR-0002 and
ADR-0004 rejected chain of responsibility without showing why against the
code. Revised the same day against the-architect's assessment and four owner
decisions of 2026-10-09, each recorded under Decision as decided: a test fake
is not the second implementation ((a), (d)); the fault violation's id and
severity ((c)); a generic stage interface is a convention judgement, not
chosen (option 8); the coordinator keeps the name `SpecCheckRunner` ((a)).
Acceptance is the owner's.

Relation to earlier records, each stated in full under Decision:

- **ADR-0001** (accepted on `main`, pull request #21): not reordered, and its
  driver 3 holds unchanged. This record names the shape its stage D lands,
  reached through stages A-C, and adds two pieces of work stage D did not name:
  a manifest loader class and a home for the pure path mapping (Decision (a)).
- **ADR-0002** (accepted): re-affirmed for the stages and for
  first-handler-wins, now with the code that shows why. Decision (c) carries
  out its Decision bullet 4
  (`.spec/adr/0002-no-chain-of-responsibility-for-the-check.md:104-107`).
- **ADR-0004** (proposed; on `main` since pull request #24): re-affirmed whole, option 8's
  rejection included. Decision (c) is the trigger it re-cut
  (`.spec/adr/0004-per-rule-settings-are-selection-and-a-map.md:191-195`).
  Disable and re-grade stay its two functions inside `Evaluate`; its severity
  map skips the fault violation (Decision (c)).

## Context

### What the code is today

The engine composes itself with static classes and `new`:

- `SpecCheckRunner` is a `static class` (`src/specht/SpecCheckRunner.cs:9`).
  `Run(root)` (`:12-37`) calls `SpecModel.Load(root)` (`:14`), finds the rules
  by reflection (`Discover()`, `:52-58`: `GetTypes()` and
  `Activator.CreateInstance`), evaluates each in a plain `foreach` with no
  `try` (`:18-21`), sorts once (`:23-28`), and counts rules evaluated as
  `rules.Sum(static rule => rule.ReportedIds.Count)` (`:35`).
- `SpecModel.Load(root)` (`src/specht/SpecModel.cs:48-87`) creates its own
  file system, `SpecSchemas.Load(new FileSystem(), root)` (`:50`), then calls
  the static `SpecDiscovery.FindSpecifications` (`:51`), `SpecDocument.Parse`
  (`:58`), `Directory.EnumerateFiles` (`:59`, `:78`), `FrontmatterReader.Read`
  (`:69`, `:82`) and `Directory.Exists` (`:76`).
- `SpecDiscovery` is a `static class` over `Directory` and `File`
  (`src/specht/SpecDiscovery.cs:12`). Its pure `Relative(root, path)` is
  called from inside a rule: `FeatureFileRule` maps its `.feature` path with
  it (`src/specht/Rules/FeatureFileRule.cs:39`).
- `ISpecRule` (`src/specht/ISpecRule.cs:8-18`) is the one interface: `Id`,
  `ReportedIds`, and `Evaluate(SpecModel)` returning a lazy
  `IEnumerable<SpecViolation>`. It takes no cancellation token (`:17`).
- The host reaches the engine through one registration,
  `Func<string, SpecCheckReport>` bound to `SpecCheckRunner.Run`
  (`src/specht.tool/Program.cs:9`).

The owner wants a sturdy application, and tests alone will not get there: a
coupled static pipeline can be covered by tests and still have no seam to
substitute a stage, no place to put behaviour around a rule, and a rule
exception that ends the run.

### A rule that throws ends the whole run

Nothing between a rule and the command catches. `CheckCommand` catches three
exception types, all from loading (`SpechtRootNotFoundException`,
`SpechtManifestNotFoundException`, `SpechtManifestUnreadableException`;
`src/specht.tool/Features/Check/CheckCommand.cs:28-39`). An exception from any
`Evaluate` - `FeatureFileRule` reads the disk inside `Evaluate`, through
`FeatureFileReader.ReadTags(path)` (`src/specht/Rules/FeatureFileRule.cs:41`) -
leaves `SpecCheckRunner.Run`, then `CheckCommand`, and reaches Spectre, which
returns `-1`. That breaks three things at once: the seven other rules report
nothing; `0001-F2` C-2 (exit codes are returned, never thrown) is violated, the
anti-pattern its row names (`src/specht.tool/Features/Check/.spec/README.md:84`);
and the rendered exception can carry an absolute path, which brief § 9 forbids.

### What ADR-0002 and ADR-0004 left unshown

Both records reject chain of responsibility (CoR), and both state the verdict
without the code that shows it. They also ran two separate questions together:

1. **The shape of the check** - a chain of handlers, or a fixed sequence.
2. **Static coupling** - classes that `new` their dependencies and call each
   other's statics.

The second is fixed by dependency inversion, which is ADR-0001's decision, not
by choosing a chain. **Rejecting CoR never meant keeping the statics.** Both
can be true: no chain, and every stage injected.

### Constraints at stake

- `0001-F2` B-001: the check evaluates every rule `0001-F1` evaluates and
  prints each violation as `<path>(<line>): <severity> SPEC###: <message>`.
- `0001-F1` B-001: violations are reported under exactly the twenty-one ids,
  "and under no other id" (`src/specht/.spec/README.md:61`); `0001-F1` C-5:
  no new `SPEC###` id before `0001-F7` (`:83`); `0001-F5` C-3: a manifest may
  disable a rule or lower its severity, and may not add one.
- `0001-F1` C-3: deterministic; no clock, environment variable, locale or
  machine name reaches the output. `0001-F1` C-4 and brief § 9: every emitted
  path is repository-relative.
- `0001-F5` C-5: the manifest is validated whole before any rule runs;
  `0001-F5` B-018 is its observable form.
- `0001-F5` B-010 and B-011: a re-graded rule reports at the manifest's
  severity; a disabled rule reports nothing and leaves the count of rules
  evaluated.
- `0001-F1` C-9: no edit changes a verdict, proven against the golden report
  (`0001-F1` B-004, C-10).
- `0001-F2` C-2: exit codes are returned, never thrown.

## Decision drivers

1. Every rule runs and every violation is reported (`0001-F2` B-001).
2. Stage order is checked by the compiler, not by registration order, so
   `0001-F5` C-5 cannot be broken by a misplaced line.
3. Every stage can be substituted in a test without a tree on disk (ADR-0001
   driver 1).
4. A behaviour wrapped around a rule must keep the rule's identity - its `Id`
   and its `ReportedIds` - or it is a membership change, not a wrap
   (ADR-0004).
5. A seam is earned (`coding-conventions` § Design,
   `.claude/skills/coding-conventions/SKILL.md:37-47`, as Decision (d)
   resolves it) - and where that bar is a repository convention rather than a
   fact about the code, this record says so.

## Considered options

Each option says whether its rejection is **structural** - the code cannot
satisfy a constraint in that shape - or **judgement** - a repository convention
the owner can change.

### 1. Keep the statics, no change

What runs today (Context). Cost: the coupling the owner named. `SpecModel.Load`
creates its own `FileSystem` (`SpecModel.cs:50`), so no test substitutes it;
the rule set is reflection-built (`SpecCheckRunner.cs:52-58`), so no test
injects a fake rule; a rule exception ends the run. Rejected: it is the problem
ADR-0001 was accepted to solve.

### 2. CoR over the rules, first handler wins

The classic form: each handler either handles the request or passes it on.

```csharp
public abstract class RuleHandler
{
    private RuleHandler? next;

    public RuleHandler SetNext(RuleHandler handler) => next = handler;

    public IEnumerable<SpecViolation> Handle(SpecModel model)
    {
        var violations = Evaluate(model).ToList();

        return violations.Count > 0 || next is null
            ? violations
            : next.Handle(model);
    }

    protected abstract IEnumerable<SpecViolation> Evaluate(SpecModel model);
}
```

On a tree that breaks `SPEC010` and `SPEC031`, the first handler to report
stops the walk, so one of the two is never evaluated.

**Rejected - structural.** A checker must run every rule (`0001-F2` B-001;
`0001-F1` B-001), and first-handler-wins stops at the first handler by
definition, so it under-reports by design. No registration or configuration
repairs this; it is the pattern's contract.

### 3. Middleware over the stages, sharing a `CheckContext`

The handler-calls-next form, each stage a handler over one shared context:

```csharp
public sealed class CheckContext
{
    public required string Root { get; init; }
    public SpecStructure? Structure { get; set; }
    public IReadOnlyList<SpecLocation>? Locations { get; set; }
    public SpecModel? Model { get; set; }
    public List<SpecViolation> Violations { get; } = [];
}

public interface ICheckHandler
{
    void Handle(CheckContext context, Action<CheckContext> next);
}

public sealed class DiscoveryHandler(ISpecDiscovery discovery) : ICheckHandler
{
    public void Handle(CheckContext context, Action<CheckContext> next)
    {
        context.Locations = discovery.Find(context.Root, context.Structure!);
        next(context);
    }
}
```

```csharp
services.AddSingleton<ICheckHandler, ManifestHandler>();
services.AddSingleton<ICheckHandler, DiscoveryHandler>();
services.AddSingleton<ICheckHandler, ModelHandler>();
services.AddSingleton<ICheckHandler, RulesHandler>();
```

The same check as a typed sequence:

```csharp
var schemas = manifest.Load(root);
var locations = discovery.Find(root, schemas.Structure);
var model = builder.Build(root, locations, schemas);
```

(`discovery.Find(root, structure)` is the shape after `0001-F6`'s manifest
inputs land, item `0005`; today `SpecDiscovery.FindSpecifications(root)` takes
no structure, `SpecDiscovery.cs:15`.)

**Rejected - structural.** Three facts, each visible in the sketches:

- **The types are erased into a nullable bag.** `Structure`, `Locations` and
  `Model` are nullable because each is empty until its stage runs.
  `DiscoveryHandler` reads `context.Structure!`; swap the first two
  `AddSingleton` lines and it compiles and throws `NullReferenceException` at
  run time. In the typed sequence, `discovery.Find` cannot be called before
  `schemas` exists - the compiler refuses. Stage order moves from the compiler
  to registration order.
- **`0001-F5` C-5 becomes a registration line.** "The manifest is validated
  whole before any rule runs" is today a consequence of `SpecModel.Load`
  calling `SpecSchemas.Load` first (`SpecModel.cs:50`); in the typed sequence
  it is a data dependency, because the model needs `schemas` and the rules
  need the model. In the middleware, it holds only while `ManifestHandler` is
  registered before `RulesHandler`.
- **Reordering buys nothing.** The order is fixed by data: each stage consumes
  the one before it. The flexibility middleware sells - insert, remove,
  reorder - is flexibility no claim asks for and no reordering could use.

### 4. Decorators around each `ISpecRule`, for per-rule settings

A decorator implements `ISpecRule` and wraps another:

```csharp
public sealed class DisableableRule(ISpecRule inner, ISet<string> disabled) : ISpecRule
{
    public string Id => inner.Id;

    public IReadOnlyList<string> ReportedIds =>
        disabled.Contains(inner.Id) ? [] : inner.ReportedIds;

    public IEnumerable<SpecViolation> Evaluate(SpecModel model) =>
        disabled.Contains(inner.Id) ? [] : inner.Evaluate(model);
}
```

Facts, then judgement, labelled:

- **Fact - disable changes identity.** `0001-F5` B-011 takes a disabled rule
  out of the count of rules evaluated, and the count is
  `rules.Sum(static rule => rule.ReportedIds.Count)` (`SpecCheckRunner.cs:35`).
  So the decorator must rewrite `ReportedIds` too, as above. A wrapper that
  changes what the rule reports it is is not a wrap; it is a membership change
  in disguise. Selection over the rule set (ADR-0004) is the direct form.
- **Fact - re-grade is a value map.** `0001-F5` B-010 changes each violation's
  `Severity`; that is `violations.Select(v => v with { Severity = ... })`, a
  map, not behaviour around a call.
- **Fact - no built-in decorator in the container.**
  `Microsoft.Extensions.DependencyInjection` has no decorator registration for
  `IEnumerable<ISpecRule>`; Scrutor, which adds one, was rejected in ADR-0003
  (option 8). So each of the eight registrations is wrapped by hand, and a
  rule registered unwrapped is silent unless a decoration-coverage test
  exists (ADR-0004).
- **Fact - link order is a new place for a verdict to change.** With disable
  and re-grade as two decorators, which one is outermost decides whether a
  re-graded disabled rule reports anything; the order is a registration
  detail, and `0001-F1` C-9 must then be guarded against it.

**Rejected for settings - partly structural, partly judgement.** That disable
needs to rewrite identity is structural: it follows from B-011 and `:35`.
That two hand-wrapped decorators cost more than two functions is judgement.
The convention bar ADR-0004 used against a selector with one implementation
(its option 8) is now settled by the owner (Decision (d)): a test fake is not
the second implementation, so that rejection stands.

### 5. Fault isolation as a `try`/`catch` in the evaluation loop

The smallest form of Decision (c): a `try` around `rule.Evaluate(model)` in
the loop at `SpecCheckRunner.cs:18-21`, materialising each rule's output
inside it. Cost: none in types; but each later around-the-call behaviour
(timing, tracing) is another edit to the same loop, and the loop is the
coordinator's, not the rule's. **Rejected - judgement.** It is behaviourally
equivalent; the wrapper is chosen because it gives the around-the-call
behaviours one home. If the owner prefers the loop, this is the fallback, and
nothing else in this record changes.

### 6. Typed, injected stages under one coordinator (chosen)

Each stage is an injected type with a typed signature; one coordinator calls
them in an order the compiler checks; the rules arrive as
`IEnumerable<ISpecRule>`. Pipes-and-filters with typed stages. Sketched under
Decision (a). Cost: every engine file changes shape, one ADR-0001 stage per
pull request under `0001-F1` C-9; the constructor of the coordinator lists
its stages, so adding a stage is an edit to it - which is the point.

### 7. A chain over the rules that always calls next (concat-all)

The CoR form that does not stop: every link evaluates, then hands on, and the
results are concatenated.

```csharp
public abstract class RuleLink(RuleLink? next) : ISpecRule
{
    public abstract string Id { get; }

    public abstract IReadOnlyList<string> ReportedIds { get; }

    public IEnumerable<SpecViolation> Handle(SpecModel model) =>
        next is null ? Evaluate(model) : Evaluate(model).Concat(next.Handle(model));

    public abstract IEnumerable<SpecViolation> Evaluate(SpecModel model);
}
```

It repairs option 2's under-reporting: every rule runs. What it does not
repair:

- **Structural - the check does arithmetic over a set, and a chain exposes
  only its head.** The count of rules evaluated is
  `rules.Sum(static rule => rule.ReportedIds.Count)` (`SpecCheckRunner.cs:35`),
  and `0001-F5` B-011 takes a disabled rule out of that sum; both need every
  member at once. A chain hands the coordinator one link, so the count needs a
  walk the pattern does not offer, or a second list beside the chain - which
  is the set again.
- **Structural - the chain's order carries nothing.** The report's order is
  one sort over all violations (`SpecCheckRunner.cs:23-28`, item `0103`'s
  order function after it), which discards the order the links produced. The
  one property a chain adds over a set is thrown away by the next line.
- **Judgement - rules stop being stateless singletons.** Each rule gains a
  `next` field, set at composition. ADR-0001 § Decision, Lifetimes, registers
  engine types as singletons because every one is stateless; a rule holding
  its successor is not. Stage C's hand-written list (item `0106`) becomes a
  linked list built by hand.

**Rejected - structural**, with the judgement cost on top.

### 8. A generic stage interface, `IStage<TIn, TOut>`, composed with `Then`

Each stage implements one generic interface, and a typed builder composes
them:

```csharp
public interface IStage<in TIn, out TOut>
{
    TOut Run(TIn input);
}

public sealed record ManifestResult(string Root, SpecSchemas Schemas);

public sealed record DiscoveryResult(string Root, SpecSchemas Schemas, IReadOnlyList<SpecLocation> Locations);

var check = manifest.Then(discovery).Then(builder).Then(rules);
```

```csharp
services.AddSingleton<IStage<string, ManifestResult>, ManifestStage>();
services.AddSingleton<IStage<ManifestResult, DiscoveryResult>, DiscoveryStage>();
services.AddSingleton<IStage<DiscoveryResult, SpecModel>, ModelStage>();
services.AddSingleton<IStage<SpecModel, SpecCheckReport>, RulesStage>();
```

**None of option 3's structural facts apply.** Nothing is nullable, no type
is erased, and `Then` still refuses a stage whose input is not the previous
stage's output, so order stays compile-checked and `0001-F5` C-5 stays a data
dependency.

**Not chosen - REPO-CONVENTION JUDGEMENT, not structural.** The owner can
change the convention; the code does not forbid the shape.

- **Cost - one record type per stage boundary.** A stage takes one input, but
  the model stage needs three: `builder.Build(root, locations, schemas)` has
  arity 3, so a `DiscoveryResult(Root, Schemas, Locations)` is forced, and
  `ManifestResult` with it, to carry the root forward. Those records exist
  only to fit the interface, against "keep it direct" and "no speculative
  abstraction" (`coding-conventions` SKILL.md:30-32, :37-41).
- **Cost - the composition is harder to audit.** Four closed-generic
  registrations say less, read top to bottom, than one constructor naming its
  stages, against ADR-0001 driver 5 (a readable, explicit list).
- **What it buys - one insertion point for stage-level timing and tracing.**
  A wrapper around `IStage<TIn, TOut>` times or traces every stage once; that
  is the gap ADR-0002 records ("timing, tracing - has no insertion point",
  `.spec/adr/0002-no-chain-of-responsibility-for-the-check.md:121-122`).
- **What would earn it.** A claim asking for stage-level timing or tracing -
  the same kind of condition ADR-0004 named for its option 8 (`0101-F5`
  OQ-3). Until then option 6 is kept.

## Decision

Proposed:

### (a) The target architecture: typed, injected stages under one coordinator

The statics are replaced by one instance coordinator, resolved from the
container, that takes its stages by constructor and calls them in
compile-checked order:

```csharp
public sealed class SpecCheckRunner(
    SpecSchemasLoader manifest,
    ISpecDiscovery discovery,
    SpecModelLoader builder,
    IEnumerable<ISpecRule> rules)
{
    public SpecCheckReport Run(string root)
    {
        var schemas = manifest.Load(root);
        var locations = discovery.Find(root, schemas.Structure);
        var model = builder.Build(root, locations, schemas);

        return Evaluate(model);
    }

    public SpecCheckReport Evaluate(SpecModel model)
    {
        var selected = Select(rules, model.Schemas.Structure);
        var violations = selected.SelectMany(rule => rule.Evaluate(model)).ToList();
        var graded = Grade(violations, model.Schemas.Structure);
        var ordered = Order(graded);

        return SpecCheckReport.From(model, selected, ordered);
    }
}
```

The sketch is the shape, not the signatures; item `0107` and the items after
it write the code. What it fixes:

- **`Evaluate` runs in this order:** ADR-0004's selection over the rule set
  (`Select`); every selected rule evaluated and the violations materialised
  **once**, as a list; ADR-0004's severity map (`Grade`), which skips fault
  violations (Decision (c)); item `0103`'s order function (`Order`); the
  report, counted over the selected set. The list matters: a lazy `SelectMany`
  enumerated by both the map and the count runs every rule twice, and
  `FeatureFileRule` re-reads the disk each time (`FeatureFileRule.cs:41`).
- **The model stage owns its readers and parser.** `FrontmatterReader.Read`
  (`SpecModel.cs:69`, `:82`) and `SpecDocument.Parse` (`:58`) are the model
  loader's dependencies, not the coordinator's: the readers as constructor
  parameters of `SpecModelLoader` once stage D makes them instances, beside its
  `IFileSystem`; the parser as the pure
  `Parse(text, relativePath)` stage A makes it, called by the loader with text
  it read through `IFileSystem`.
- **The manifest and model stages are concrete, unconditionally.**
  `SpecSchemasLoader` and `SpecModelLoader` are sealed classes; tests
  substitute them through `IFileSystem` and through `Evaluate(SpecModel)`, not
  through an interface (Decision (d)). `ISpecDiscovery` stays an interface,
  because its second mode and the git listing behind it are real seams
  (ADR-0001, stage E). The rule set is `IEnumerable<ISpecRule>`.
- **`SpecSchemasLoader` is new work beyond ADR-0001 stage D.** Stage D threads
  `IFileSystem` through the readers, the parser's caller, the walk and the
  loader, and leaves `SpecSchemas.Load(IFileSystem, root)` (`SpecSchemas.cs:55`)
  the static loader of the manifest and the frontmatter schemas, as the names
  table below records. An instance class around it is one more class in item
  `0107`'s scope; that is owed to the item.

**The statics (a) does not replace.** Not every static is a stage:

- `SpecManifest.Defaults` (`SpecManifest.cs:113`, read at `:79-80`) and
  `SpecSchemas.Options` (`SpecSchemas.cs:26`) stay process-wide, as ADR-0001
  § Decision, Lifetimes, already says.
- ADR-0001's "statics that stay" list stands: each is a pure function or a
  constant with no I/O to substitute.
- **One of them collides with this shape.** `SpecDiscovery.Relative` is on
  that list, but `SpecDiscovery` becomes the `ISpecDiscovery` stage, and
  `FeatureFileRule` calls `SpecDiscovery.Relative` from inside a rule
  (`FeatureFileRule.cs:39`). A rule reaching into the discovery stage for a
  path mapping couples the two. The pure root-relative mapping moves to a type
  of its own, still static; the type's name is owed to item `0107`.

**How it relates to ADR-0001.** It is the shape ADR-0001's stage D lands,
reached through stages A-C in ADR-0001's order: A makes the model
constructible, B splits `Run` from `Evaluate`, C replaces `Discover()` with a
hand-written rule list, D turns the stages into instance classes registered by
`AddSpechtEngine()` (item `0107`), and E adds discovery's interface and
git-listing seam. Nothing is reordered, and ADR-0001's driver 3 - an interface
only where a substitution exists today - is followed as written.

**Names, reconciled with ADR-0001 rather than duplicated.** The discussion
used `SpecChecker`, `IManifestLoader` and `ISpecModelBuilder`. ADR-0001
already names some of these, and ADR-0004, `0001-F5` § 7 and items
`0105`-`0107` cite them:

| Stage       | Discussion name     | ADR-0001's name                                          | Today                                           | Owed to item `0107`                           |
| ----------- | ------------------- | -------------------------------------------------------- | ----------------------------------------------- | --------------------------------------------- |
| Coordinator | `SpecChecker`       | `SpecCheckRunner`, split into `Run` and `Evaluate`       | `SpecCheckRunner.Run` (`SpecCheckRunner.cs:12`) | Nothing; the name stays                       |
| Manifest    | `IManifestLoader`   | none; `SpecSchemas.Load` stays the loader of both        | `SpecSchemas.Load` (`SpecSchemas.cs:55`)        | The class, and its name                       |
| Discovery   | `ISpecDiscovery`    | `ISpecDiscovery` (stage E)                               | `SpecDiscovery` (`SpecDiscovery.cs:12`)         | The path mapping's own type, and its name     |
| Model       | `ISpecModelBuilder` | "the loader"; `ISpecModelLoader` only if B's split fails | `SpecModel.Load` (`SpecModel.cs:48`)            | `SpecModelLoader`'s name, sealed and concrete |
| Rules       | `ISpecRule`         | `ISpecRule`, a hand-written list (stage C)               | `Discover()` (`SpecCheckRunner.cs:52-58`)       | Registration, with the fault wrapper's item   |

`SpecSchemasLoader` and `SpecModelLoader` are placeholders. **The coordinator
keeps the name `SpecCheckRunner`** (owner decision, 2026-10-09): a rename to
`SpecChecker` touches every citation of `SpecCheckRunner.Evaluate` - ADR-0004,
`0001-F5` § 7, items `0105`-`0107` - in the middle of a migration whose every
stage is guarded by `0001-F1` C-9, and that churn buys a reader nothing.

### (b) No chain over the stages or the rules - structural

Options 2, 3 and 7 are rejected on structural grounds: first-handler-wins
breaks `0001-F2` B-001; a context bag erases the stage types and turns
`0001-F5` C-5 into a registration line; and a concat-all chain exposes only
its head to a count that is arithmetic over a set (`0001-F5` B-011), while one
sort discards the order it adds. ADR-0002's verdict stands, now with the code
that shows it. Option 8, the generic stage interface, is not rejected on
structure; it waits on a claim (option 8).

### (c) The first decorator around `ISpecRule` is fault isolation

A rule that throws today ends the whole run (Context). A wrapper catches the
exception, reports it as a violation for that rule, and lets the other rules
run. This carries out records already accepted, not a new direction: ADR-0002
named a decorator around `ISpecRule` as the move once a behaviour that wraps a
rule's evaluation appears (its Decision bullet 4,
`.spec/adr/0002-no-chain-of-responsibility-for-the-check.md:104-107`), and
ADR-0004 re-cut that trigger to behaviour that wraps evaluation and preserves
identity, naming exception-wrapping
(`.spec/adr/0004-per-rule-settings-are-selection-and-a-map.md:191-195`).

```csharp
public sealed class FaultIsolatedRule(ISpecRule inner) : ISpecRule
{
    public string Id => inner.Id;

    public IReadOnlyList<string> ReportedIds => inner.ReportedIds;

    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        try
        {
            return inner.Evaluate(model).ToList();
        }
        catch (Exception exception)
        {
            return [RuleFailure(inner, exception)];
        }
    }
}
```

```csharp
services.AddSingleton<ISpecRule>(new FaultIsolatedRule(new ClaimRule()));
services.AddSingleton<ISpecRule>(new FaultIsolatedRule(new FeatureFileRule()));
```

Facts the sketch carries:

- **It keeps identity.** `Id` and `ReportedIds` pass through, so the count at
  `SpecCheckRunner.cs:35` and `0001-F5` B-011 are untouched.
- **It must materialise.** `Evaluate` returns a lazy sequence (`ISpecRule.cs:17`;
  the rules `yield`), so without `.ToList()` inside the `try` the exception
  surfaces later, in the coordinator, outside the wrapper.
- **A throwing rule's partial output is discarded, deliberately.** An iterator
  rule may have yielded violations before it threw (`FeatureFileRule.cs:26`,
  `:45`); `.ToList()` throws before returning them, and the wrapper reports
  only the fault. All-or-nothing per rule keeps the report a function of the
  tree, not of how far a fault got (`0001-F1` C-3).
- **No exception is filtered.** `ISpecRule.Evaluate` takes no cancellation
  token (`ISpecRule.cs:17`), and `CheckCommand`'s token never reaches `run`
  (`CheckCommand.cs:26`), so no `OperationCanceledException` can come from a
  rule for the wrapper to let through.
- **It is registered by hand**, one line per rule (ADR-0003 option 8; the
  rules are parameterless), with the decoration-coverage test ADR-0004
  requires, because a rule registered unwrapped is silent.
- **Its message carries no absolute path** (brief § 9) and nothing
  locale-dependent (`0001-F1` C-3: no "locale ... reaching the output"), so
  `exception.Message`, which the runtime localises and which can carry an
  absolute path, is not copied into it as is.

**Decided (owner, 2026-10-09): the fault violation carries the failing rule's
own id, is exempt from the severity map, and is fixed at `Error`.**

- **Own id.** No vocabulary change, and the violation sits beside that rule's
  others.
- **Exempt from the map, fixed at `Error`.** The violation carries a
  provenance marker - a fact on the violation that says the rule failed rather
  than found - and ADR-0004's severity map (`0001-F5` B-010) skips any
  violation that carries it. The shape is decided here; the marker's name and
  form are owed to the item.

Both alternatives are rejected, each on a fact:

- **A new `SPEC###` id - rejected.** `0001-F1` B-001 reports under the
  twenty-one ids "and under no other id" (`src/specht/.spec/README.md:61`),
  and `0001-F1` C-5 rules out "A new `SPEC###` id before `0001-F7`" (`:83`).
  A new id is a new schema version, after brief § 8 step 6.
- **The own id, mappable like any other - rejected.** A consumer who lowers
  that rule to warning (`0001-F5` B-010, applied by ADR-0004's map) would lower
  the fault with it, and a non-strict run fails only on errors
  (`CheckCommand.cs:46`): a crash would exit `0`. A setting meant for a rule's
  findings must not silence the rule's failure.

**What fixes `RuleFailure`'s fields.** `RuleId` is the inner rule's `Id`;
`Severity` is `Error`, fixed by this record; `File`, `Line` and the message are
the claim's, which `spec-author` writes. `File` is not optional:
`SpecViolation.File` is non-nullable and rendered first
(`SpecViolation.cs:27`). `spec-author`'s choice is bounded by `0001-F2` B-001's
line shape (`<path>(<line>): <severity> SPEC###: <message>`), by `0001-F1` C-4
and by brief § 9: a repository-relative path, never an absolute one.

**It needs a claim and a scenario before code** (AGENTS.md, rule 1): a new
claim in `0001-F1` - the engine, whose claims already cover what the engine
reports (`0001-F1` B-001, B-010) - with a scenario in
`src/specht/.spec/engine.feature`; and a scenario in
`src/specht.tool/Features/Check/.spec/check.feature`, "a rule that throws
exits 1, not -1", proving `0001-F2` C-2. All listed under Owed work.

Once the wrapper exists, per-rule timing and tracing join it as further
around-the-call behaviour, each under its own claim. Disable and re-grade stay
ADR-0004's selection and map; they never become wrappers.

### (d) A test fake is not the second implementation - decided

**Decided (owner, 2026-10-09).** A test double earns an interface only where no
production substitution exists - the file system, a child process, a second
provider - not where `IFileSystem` or an argument the test can construct
already supplies one. The facts that make the substitution already exist here:

- The manifest stage reads through an injected file system:
  `SpecSchemas.Load(IFileSystem, root)` (`SpecSchemas.cs:55`).
- Stage B's `Evaluate(SpecModel)` takes a constructible model, so a unit test
  of the rules, the selection, the map and the order skips the loaders
  entirely.
- `0001-F5` B-018 is provable without a loader interface: a `MockFileSystem`
  holding an invalid manifest, and a fake rule whose `Evaluate` is never
  called.

Consequences, all applied in this record:

- (a) is unconditional: `SpecSchemasLoader` and `SpecModelLoader` are concrete;
  `ISpecDiscovery` stays an interface for its second mode and git listing.
- `coding-conventions` § Design is edited in the same commit as this revision,
  so its "seam" bullet no longer names a test double as a substitution in its
  own right (`.claude/skills/coding-conventions/SKILL.md:42-47`); the "second
  real caller or a substitution that exists today" bar (`:37-41`) is
  unchanged.
- ADR-0004 option 8's rejection stands. ADR-0001's driver 3 needs no status
  line: it already says this.

## Consequences

Buys:

- The rejections of CoR are shown in code, and each is labelled structural or
  judgement, so the owner can see which they can overrule.
- One coordinator whose constructor lists its stages and whose body calls them
  in compile-checked order; `0001-F5` C-5 stays a data dependency.
- Every stage substitutable in a test, through an injected type, through
  `IFileSystem`, or through `Evaluate(SpecModel)`.
- A rule that throws no longer ends the run: the other rules report, the
  command returns an exit code (`0001-F2` C-2), no stack trace reaches stdout,
  and no manifest setting can turn the failure into a pass.
- A home for per-rule timing and tracing when a claim asks for them.

Costs:

- The engine changes shape across every file, staged as ADR-0001 already
  staged it, each stage proven against the golden report (`0001-F1` C-9).
- Item `0107` grows: the manifest loader class and the path mapping's own
  type.
- A decorator registered by hand per rule, and a decoration-coverage test
  with it.
- Fault isolation adds one violation to a report in a run where a rule fails,
  and drops what that rule yielded before it failed.
- A transient disk fault - `FeatureFileRule` reads the disk inside `Evaluate`
  (`FeatureFileRule.cs:41`) - now produces a violation instead of a crash, so
  the report of a run that hit one differs from the next run's, as the crash
  did.

Owed work:

- **The fault-isolation claim and its engine scenario**, by `spec-author`, in
  `0001-F1` § 3 and `src/specht/.spec/engine.feature` (Decision (c)), with the
  `0001-F1` § 9 row the new claim needs: `0001-F1` is approved, so a row left
  `Missing` is a `SPEC060` violation, and the scenario's tag must resolve
  (`SPEC021`).
- **The command scenario**, by `spec-author`, in
  `src/specht.tool/Features/Check/.spec/check.feature`: "a rule that throws
  exits 1, not -1", proving `0001-F2` C-2 (its anti-pattern,
  `src/specht.tool/Features/Check/.spec/README.md:84`).
- **The `0001-F1` C-9 reading, in `0001-F1` § 10**, by `spec-author`: the
  baseline tree is synthetic and breaks rules, not the engine (C-10), so
  B-004's proof is untouched; C-9 binds on trees the engine completed at
  `e7dba24`, and a tree on which it threw had no verdict to preserve.
- **A work item cut from that claim**, depending on item `0106` (the
  hand-written rule list it wraps) and item `0107` (`AddSpechtEngine()`, where
  it is registered). The provenance marker's name and form are the item's.
- **Item `0107`'s scope** (`src/specht/.issue/0107-resolve-the-engine-from-the-container.yml`):
  the manifest loader class and its name; the path mapping's own type and its
  name; the model loader's name. Its `out_of_scope` line "A decorator or chain
  around ISpecRule — ADR-0002" moves to cite ADR-0005 (c).
