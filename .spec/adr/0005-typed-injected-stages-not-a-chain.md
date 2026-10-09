---
title: "ADR-0005: Typed injected stages, not a chain; a rule wrapper earned by fault isolation"
description: "The engine's statics become one coordinator that calls typed stages injected from the container, in an order the compiler checks; chain of responsibility is shown against the code and rejected for the stages and for first-handler-wins on structural grounds, and the first decorator around ISpecRule is fault isolation, owed a claim and a scenario before code. Separates the structural rejections from the repository-convention ones the owner can overrule."
type: adr
---

# ADR-0005: Typed injected stages, not a chain; a rule wrapper earned by fault isolation

## Status

proposed - 2026-10-09. Asked for by the owner, who found that ADR-0002 and
ADR-0004 rejected chain of responsibility without showing why against the
code. Acceptance is the owner's.

Relation to earlier records, each stated in full under Decision:

- **ADR-0001** (accepted on `main`, pull request #21): not reordered. This
  record names the shape its stage D lands, reached through stages A-C. Where
  that shape and ADR-0001's interface rule differ, the difference is put to
  the owner (Decision (a) and (d)), not settled here.
- **ADR-0002** (accepted): re-affirmed for the stages and for
  first-handler-wins, now with the code that shows why.
- **ADR-0004** (proposed, pull request #24): re-affirmed, with one exception.
  Its rejection of the rule-set selector on the "one implementation" bar
  (ADR-0004 option 8) is re-opened to the owner as a convention question
  (Decision (d)). Nothing else in it is superseded. Disable and re-grade stay
  its two functions inside `Evaluate`.

## Context

### What the code is today

The engine composes itself with static classes and `new`:

- `SpecCheckRunner` is a `static class` (`src/specht/SpecCheckRunner.cs:9`).
  `Run(root)` (`:12-37`) calls `SpecModel.Load(root)` (`:14`), finds the rules
  by reflection (`Discover()`, `:52-58`: `GetTypes()` and
  `Activator.CreateInstance`), evaluates each in a plain `foreach` with no
  `try` (`:18-21`), sorts once (`:23-28`), and counts rules evaluated as
  `rules.Sum(rule => rule.ReportedIds.Count)` (`:35`).
- `SpecModel.Load(root)` (`src/specht/SpecModel.cs:48-87`) creates its own
  file system, `SpecSchemas.Load(new FileSystem(), root)` (`:50`), then calls
  the static `SpecDiscovery.FindSpecifications` (`:51`), `SpecDocument.Parse`
  (`:58`), `Directory.EnumerateFiles` (`:59`, `:78`), `FrontmatterReader.Read`
  (`:69`, `:82`) and `Directory.Exists` (`:76`).
- `SpecDiscovery` is a `static class` over `Directory` and `File`
  (`src/specht/SpecDiscovery.cs:12`).
- `ISpecRule` (`src/specht/ISpecRule.cs:8-18`) is the one interface: `Id`,
  `ReportedIds`, and `Evaluate(SpecModel)` returning a lazy
  `IEnumerable<SpecViolation>`.
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
nothing; `0001-F2` C-2 (exit codes are returned, never thrown) is violated; and
the rendered exception can carry an absolute path, which brief § 9 forbids.

### What ADR-0002 and ADR-0004 left unshown

Both records reject chain of responsibility (CoR), and both state the verdict
without a line of code. They also ran two separate questions together:

1. **The shape of the check** - a chain of handlers, or a fixed sequence.
2. **Static coupling** - classes that `new` their dependencies and call each
   other's statics.

The second is fixed by dependency inversion, which is ADR-0001's decision, not
by choosing a chain. **Rejecting CoR never meant keeping the statics.** Both
can be true: no chain, and every stage injected.

### Constraints at stake

- `0001-F2` B-001: the check evaluates every rule `0001-F1` evaluates;
  `0001-F1` B-001: violations are reported under exactly the twenty-one ids.
- `0001-F5` C-5: the manifest is validated whole before any rule runs; `0001-F5`
  B-018 is its observable form.
- `0001-F5` B-011: a disabled rule reports nothing and leaves the count of
  rules evaluated.
- `0001-F1` C-9: no edit changes a verdict, proven against the golden report
  (`0001-F1` B-004, C-10).
- `0001-F1` C-5 and `0001-F5` C-3: the rule vocabulary is fixed per schema
  version; a new `SPEC###` is a new schema version (AGENTS.md § Invariants).
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
   `.claude/skills/coding-conventions/SKILL.md:37-43`) - and where that bar is
   a repository convention rather than a fact about the code, this record says
   so.

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
  `rules.Sum(rule => rule.ReportedIds.Count)` (`SpecCheckRunner.cs:35`). So
  the decorator must rewrite `ReportedIds` too, as above. A wrapper that
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

**Where this rejection is weak.** The bar ADR-0004 used to reject a seam with
one implementation - "a second real caller or a substitution that exists
today" (`coding-conventions` § Design, `SKILL.md:37-41`) - is a repository
convention, not a fact about the code, and the owner can change it. The same
section says a seam is "where something is substituted: a test double, a
second provider, a second host" (`SKILL.md:42-43`), so a test fake arguably
is the second implementation. This record does not decide that; Decision (d)
puts it to the owner.

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
        var violations = rules.SelectMany(rule => rule.Evaluate(model));

        return SpecCheckReport.From(model, rules, violations);
    }
}
```

The sketch is the shape, not the signatures; item `0107` and the items after
it write the code. `Evaluate` carries ADR-0004's selection and severity map
and item `0103`'s order function, unchanged by this record.

**How it relates to ADR-0001.** It is the shape ADR-0001's stage D lands,
reached through stages A-C in ADR-0001's order: A makes the model
constructible, B splits `Run` from `Evaluate`, C replaces `Discover()` with a
hand-written rule list, D turns the stages into instance classes registered by
`AddSpechtEngine()` (item `0107`), and E adds discovery's git-listing seam.
Nothing is reordered.

**Names, reconciled with ADR-0001 rather than duplicated.** The discussion
used `SpecChecker`, `IManifestLoader` and `ISpecModelBuilder`. ADR-0001
already names some of these, and ADR-0004, `0001-F5` § 7 and items
`0105`-`0107` cite them:

| Stage       | Discussion name     | ADR-0001's name                                          | Today                                           |
| ----------- | ------------------- | -------------------------------------------------------- | ----------------------------------------------- |
| Coordinator | `SpecChecker`       | `SpecCheckRunner`, split into `Run` and `Evaluate`       | `SpecCheckRunner.Run` (`SpecCheckRunner.cs:12`) |
| Manifest    | `IManifestLoader`   | none; `SpecSchemas.Load` stays the loader of both        | `SpecSchemas.Load` (`SpecSchemas.cs:55`)        |
| Discovery   | `ISpecDiscovery`    | `ISpecDiscovery` (stage E)                               | `SpecDiscovery` (`SpecDiscovery.cs:12`)         |
| Model       | `ISpecModelBuilder` | "the loader"; `ISpecModelLoader` only if B's split fails | `SpecModel.Load` (`SpecModel.cs:48`)            |
| Rules       | `ISpecRule`         | `ISpecRule`, a hand-written list (stage C)               | `Discover()` (`SpecCheckRunner.cs:52-58`)       |

This record keeps ADR-0001's names: the coordinator stays `SpecCheckRunner`,
discovery is `ISpecDiscovery`, the model stage is the loader. The manifest
stage has no ADR-0001 name; `SpecSchemasLoader` above is a placeholder, and
the name is owed to item `0107`. A rename to `SpecChecker` is the owner's to
ask for; it touches every citation of `SpecCheckRunner.Evaluate`.

**Where this differs from ADR-0001.** ADR-0001 puts an interface only at a
seam with a substitution today (its driver 3) and rejects an interface per
reader (its option 2, which lists `ISpecModelLoader`). Under that rule the
coordinator above takes the manifest and model stages as sealed concrete
types, substituted in tests through `IFileSystem`, and takes interfaces only
for discovery (from stage E) and the rule set. The owner's "typed injected
stage interfaces" makes all three stages interfaces. **Which of the two
holds is the owner's decision (d), not this record's.** The shape - typed
stages, injected, one coordinator, compile-checked order - is the same under
either answer, and so is everything else in this Decision.

### (b) No chain over the stages, and no first-handler-wins - structural

Option 2 and option 3 are rejected on structural grounds: first-handler-wins
breaks `0001-F2` B-001, and a context bag erases the stage types and turns
`0001-F5` C-5 into a registration line. ADR-0002's verdict on both stands,
now with the code that shows it.

### (c) The first decorator around `ISpecRule` is fault isolation

A rule that throws today ends the whole run (Context). A wrapper catches the
exception, reports it as a violation for that rule, and lets the other rules
run:

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
        catch (Exception exception) when (exception is not OperationCanceledException)
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
  `SpecCheckRunner.cs:35` and `0001-F5` B-011 are untouched. It is a genuine
  around-the-call behaviour, and it meets ADR-0004's trigger for a decorator:
  behaviour that wraps a rule's evaluation and preserves its identity
  (`.spec/adr/0004-per-rule-settings-are-selection-and-a-map.md:191-195`).
- **It must materialise.** `Evaluate` returns a lazy sequence (`ISpecRule.cs:17`;
  the rules `yield`), so without `.ToList()` inside the `try` the exception
  surfaces later, in the coordinator, outside the wrapper.
- **It is registered by hand**, one line per rule (ADR-0003 option 8; the
  rules are parameterless), with the decoration-coverage test ADR-0004
  requires, because a rule registered unwrapped is silent.
- **Its message must carry no absolute path** (brief § 9) and must be
  deterministic (`0001-F1` C-3), so `exception.Message` is not copied into it
  as is.

**It needs a claim and a scenario before code** (AGENTS.md, rule 1). Owed: a
new claim, written by `spec-author`, in `0001-F1` - the engine, whose claims
already cover what the engine reports (`0001-F1` B-001, B-010) - with a
scenario in `src/specht/.spec/engine.feature`. `0001-F2` is the command, and
under the wrapper the command sees an ordinary report, so it needs no new
claim; its C-2 is what the wrapper stops being broken.

**Open question, for the owner, not decided here: which rule id does the
internal-failure violation carry?**

- **A new `SPEC###`.** Clear in the report, but the rule vocabulary is fixed
  per schema version (AGENTS.md § Invariants; `0001-F1` C-5; `0001-F5` C-3),
  so a new id is a new schema version, after brief § 8 step 6.
- **The failing rule's own id.** No vocabulary change, and the violation sits
  beside that rule's others; but a consumer cannot tell a rule's finding from
  the rule's failure by id alone, and the message carries the difference.

The claim also fixes the violation's file, line and severity; those are
`spec-author`'s to write once the id is decided.

Once the wrapper exists, per-rule timing and tracing join it as further
around-the-call behaviour, each under its own claim. Disable and re-grade stay
ADR-0004's selection and map; they never become wrappers.

### (d) A partial supersession of ADR-0004, put to the owner

Only one part of ADR-0004 is re-opened: its rejection of the rule-set
selector (ADR-0004 option 8,
`.spec/adr/0004-per-rule-settings-are-selection-and-a-map.md:147-155`) on the bar "one
implementation and no second caller today". That bar is the repository
convention in `coding-conventions` § Design (`SKILL.md:37-43`), and the same
convention decides whether the manifest and model stages in (a) are
interfaces. **Owner decision, not decided here:** does a test fake count as
the second implementation that earns an interface?

- **If yes:** the convention text changes to say so; the three stages in (a)
  become interfaces; and ADR-0004 option 8's selector is no longer rejected on
  that bar (whether it is then wanted is a separate call).
- **If no:** the convention stands; (a) takes sealed stage types beside
  `ISpecDiscovery` and the rule set, exactly as ADR-0001 says; and ADR-0004
  stands whole.

Nothing else in ADR-0004 is superseded: per-rule settings remain selection and
a map inside `Evaluate`, and the stage pipeline after evaluation (its option 4) stays rejected.

## Consequences

Buys:

- The rejections of CoR are shown in code, and each is labelled structural or
  judgement, so the owner can see which they can overrule.
- One coordinator whose constructor lists its stages and whose body calls them
  in compile-checked order; `0001-F5` C-5 stays a data dependency.
- Every stage substitutable in a test, through an injected type or through
  `IFileSystem`.
- A rule that throws no longer ends the run: the other rules report, the
  command returns an exit code (`0001-F2` C-2), and no stack trace reaches
  stdout.
- A home for per-rule timing and tracing when a claim asks for them.

Costs:

- The engine changes shape across every file, staged as ADR-0001 already
  staged it, each stage proven against the golden report (`0001-F1` C-9).
- A decorator registered by hand per rule, and a decoration-coverage test
  with it.
- Fault isolation adds one violation to a report in a run where a rule fails;
  until the rule-id question is answered, that violation cannot be written.

Owed work:

- **The fault-isolation claim and scenario**, by `spec-author`, in `0001-F1`
  (Decision (c)), then a work item cut from it, depending on item `0106` (the
  hand-written rule list it wraps) and item `0107` (`AddSpechtEngine()`, where
  it is registered).
- **The open question on the rule id** (Decision (c)), recorded as an `OQ` in
  `0001-F1` § 11 by `spec-author`, for the owner.
- **The stage names reconciled with ADR-0001 stage D**, in item `0107`
  (`src/specht/.issue/0107-resolve-the-engine-from-the-container.yml` on
  `main`): the manifest stage's name, and whether the coordinator is renamed.
- **The owner decision on the `coding-conventions` bar** (Decision (d)). If it
  changes, `coding-conventions` § Design is edited in the same pull request as
  the decision, and ADR-0001's driver 3 and ADR-0004's option 8 each gain a
  status line pointing here.
