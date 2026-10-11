---
title: "ADR-0010: The engine is layered namespaces in one assembly, with I/O at the edge, one rule per id and a mediator in front"
description: "Item 0154's record of the owner's architecture session. One engine assembly keeps its name and gains boundaries: namespaces that follow folders and depend downward only, held by a test. I/O is confined to four stores and one embedded-resource reader; the model is an immutable snapshot built before any rule runs; each of the 21 rule ids is one rule, so a rule set off is never run; a pure evaluator selects, runs, stamps and sorts; and a host reaches the engine by sending a request through a minimal mediator the repository owns until the owner's source-generated port exists. Every static holding logic, I/O or loaded state goes. Supersedes named clauses of ADR-0001, ADR-0004, ADR-0005 and ADR-0008, and carries the staged order of work."
type: adr
---

# ADR-0010: The engine is layered namespaces in one assembly, with I/O at the edge, one rule per id and a mediator in front

## Status

accepted - 2026-10-11. Decider: the repository owner. Item `0154`
(`.issue/0154-engine-boundaries.yml`) quotes the owner's request and records
each decision the owner made in the session of 2026-10-11; this record was
accepted on those answers, as
[ADR-0007](0007-each-part-registers-itself-and-one-factory-composes-the-host.md)
was. Seven points were left open. Each is listed at the end with a default,
and each default stands unless the owner overrules it in review.

Supersedes in part, each record's body unedited and its Status gaining one
line pointing here:
[ADR-0001](0001-resolve-the-engine-from-the-container.md),
[ADR-0004](0004-per-rule-settings-are-selection-and-a-map.md),
[ADR-0005](0005-typed-injected-stages-not-a-chain.md) and
[ADR-0008](0008-evaluate-takes-the-rule-set-and-the-model-carries-the-vocabulary.md).
The clauses are named under Decision, "What this supersedes". The ADR template
says a new record supersedes an accepted one entirely; these partial
supersessions are deliberate, as ADR-0004's and ADR-0008's were.

## Context

### How to read the evidence

Three kinds of statement appear below, and each is marked where it is not
obvious:

- **Read from source**: every path, line and quotation, read at commit
  `eb7c161` when this record was written. Pull request #118 was read at its
  head, `d7d8762`, which contains `eb7c161`. The lines cited in the four
  records this one supersedes are theirs at `eb7c161`; the Status line each
  gains with this record moves every line below it down by two.
- **Decided**: an owner decision of 2026-10-11, recorded in item `0154`.
- **Judgement** or **inference**: labelled where it stands. A C# block below
  is a shape, not a signature; the stage that builds it writes the code.

### What the owner asked for

Item `0154` holds the words. In short: SOLID; Gang of Four patterns where they
fit; an abstraction only where it is earned; a rule the manifest turns off is
not run; I/O at the boundary of the application, with an in-memory
representation between the read and the write; no logic that takes a loader;
and structure in place of "statics and loose cohesion and highly coupled
concrete statics all over the place".

### What the source shows

All **read from source**.

**A disabled rule can still run.** Eight rule classes report the 21 rule ids:

| Class                   | Ids it reports                     | Declared at                                    |
| ----------------------- | ---------------------------------- | ---------------------------------------------- |
| `FrontmatterSchemaRule` | SPEC001, SPEC002, SPEC003, SPEC004 | `src/specht/Rules/FrontmatterSchemaRule.cs:24` |
| `SectionStructureRule`  | SPEC010, SPEC013                   | `src/specht/Rules/SectionStructureRule.cs:16`  |
| `IdentityRule`          | SPEC011, SPEC012                   | `src/specht/Rules/IdentityRule.cs:22`          |
| `FeatureFileRule`       | SPEC020, SPEC021                   | `src/specht/Rules/FeatureFileRule.cs:17`       |
| `ClaimRule`             | SPEC030, SPEC031                   | `src/specht/Rules/ClaimRule.cs:22`             |
| `ChildItemRule`         | SPEC040, SPEC041, SPEC043, SPEC044 | `src/specht/Rules/ChildItemRule.cs:22`         |
| `DependencyRule`        | SPEC050, SPEC051, SPEC052          | `src/specht/Rules/DependencyRule.cs:20`        |
| `ApprovalRule`          | SPEC060, SPEC061                   | `src/specht/Rules/ApprovalRule.cs:21`          |

`SpechtRunner.Evaluate` skips a class only when none of its ids is enabled
(`src/specht/SpechtRunner.cs:30`,
`rules.Where(rule => rule.ReportedIds.Any(enabled.Contains))`), then drops the
violations of the disabled ids after the class has run (`:37`). With SPEC044
off and SPEC043 on, the whole of `ChildItemRule` runs. ADR-0004's selection
clause - "a disabled rule is not evaluated"
(`.spec/adr/0004-per-rule-settings-are-selection-and-a-map.md:167-169`) - holds
for a class and not for an id.

**A rule reads the disk while it evaluates.** `FeatureFileRule` takes a
`FeatureFileReader` (`src/specht/Rules/FeatureFileRule.cs:11`) and calls
`reader.ReadTags(path)` inside `Evaluate` (`:43`); the reader calls
`fileSystem.File.ReadAllLines` (`src/specht/Rules/FeatureFileReader.cs:19`).
The rule also maps an absolute path back to a relative one
(`FeatureFileRule.cs:41`, `SpecPath.Relative(model.Root, path)`).

**Reading and building are one method.** `SpecModelLoader.Load`
(`src/specht/Model/SpecModelLoader.cs:41-75`) loads the schemas (`:43`), walks
the tree (`:45`), reads and parses each specification in one expression
(`:52`), finds companions and items (`:53`, `:58`), and reads each
frontmatter (`:63`, `:71`). The model it returns carries absolute paths:
`SpecModel.Root` is documented "Absolute path to the repository root"
(`src/specht/Model/SpecModel.cs:21-22`), `SpecLocation.AbsolutePath`
(`src/specht/Discovery/SpecLocation.cs:8`), and `FeatureSpec.FeatureFiles`
(`src/specht/Model/FeatureSpec.cs:24`).

**Logic, I/O and loaded state sit in statics.**

| Static                            | At                                           | What it holds                                                                                         |
| --------------------------------- | -------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| `SpecManifest`, a static class    | `src/specht/Manifest/SpecManifest.cs:18`     | The manifest read (`Load`, `:41`, I/O at `:43`, `:50`, `:57`), its parse and its checks               |
| `SpecManifest.Defaults`           | `src/specht/Manifest/SpecManifest.cs:275`    | Loaded state: the embedded default manifest, read by `ReadDefaults` (`:249`)                          |
| `SpecSchemas.Load`, two overloads | `src/specht/Model/SpecSchemas.cs:72`, `:100` | A data type that loads itself, through `SpecManifest.Load` (`:74`, `:102`)                            |
| `SpecSchemas.Options`             | `src/specht/Model/SpecSchemas.cs:31`         | Process-wide evaluation options, read by a rule (`src/specht/Rules/FrontmatterSchemaRule.cs:99`)      |
| `SchemaVersions.Embedded`         | `src/specht/Versioning/SchemaVersions.cs:15` | Loaded state: every shipped version, read from resources by `ReadEmbedded` (`:30-62`)                 |
| `SpecDocument.Parse`              | `src/specht/Model/SpecDocument.cs:37`        | A data type that parses itself                                                                        |
| `FrontmatterReader.Parse`         | `src/specht/Model/FrontmatterReader.cs:28`   | The frontmatter parse, beside an instance `Read` that does the I/O (`:26`)                            |
| `SpecPath`, a static class        | `src/specht/Model/SpecPath.cs:4`             | The absolute-to-relative mapping                                                                      |
| `SpecReportDocument.From`         | `src/specht/Report/SpecReportDocument.cs:33` | The mapping from a report to its document                                                             |
| `SpechtRunner.Evaluate`, `Order`  | `src/specht/SpechtRunner.cs:25`, `:60`       | Selection, grading (`Enabled` `:87`, `Graded` `:90`, over raw level strings), ordering and the report |
| `InitWriter.ShippingCopy`         | `src/tool/Features/Init/InitWriter.cs:25`    | The embedded shipping copy, read for `src/tool/Program.cs:15`                                         |

`SchemaVersions.Embedded` is read from three instance types
(`src/specht/Model/SpecModelLoader.cs:26`,
`src/specht/Model/SpecSchemasLoader.cs:18`, `src/specht/SpechtRunner.cs:69`).
The rule settings are raw strings: `SpecStructure.Rules` is an
`IReadOnlyDictionary<string, string>` (`src/specht/Manifest/SpecStructure.cs:30`).

**The namespaces depend on each other in three cycles.**

1. `Specht.Manifest` and `Specht.Versioning`. The manifest uses
   `SemanticVersion` (`src/specht/Manifest/SpecStructure.cs:2`,
   `src/specht/Manifest/SpecManifest.cs:5`); `SchemaVersions.Select` throws
   `SpechtManifestException` and names `SpecManifest.RelativePath`
   (`src/specht/Versioning/SchemaVersions.cs:2`, `:24-28`).
2. `Specht.Report` and the root namespace. `SpechtReport` holds
   `SpecReportLayout` (`src/specht/SpechtReport.cs:2`, `:17`);
   `SpecReportDocument.From` takes `SpechtReport`
   (`src/specht/Report/SpecReportDocument.cs:33`).
3. `Specht.Manifest`, the root namespace and `Specht.Model`. The manifest
   throws the root's `SpechtRootNotFoundException`
   (`src/specht/Manifest/SpecManifest.cs:45`); the root uses the model
   (`src/specht/SpechtReport.cs:1`, `src/specht/SpechtRunner.cs:2`); the model
   uses the manifest (`src/specht/Model/SpecSchemas.cs:3`,
   `src/specht/Model/SpecSchemasLoader.cs:2`,
   `src/specht/Model/SpecModelLoader.cs:3`).

**The host reaches the engine through a delegate.** `src/tool/Program.cs:14`
registers a `Func<string, SpechtReport>` over the resolved runner, and
`CheckCommand` takes it (`src/tool/Features/Check/CheckCommand.cs:20`).

**The two test projects compile each other's source.**
`test/acceptance/acceptance.csproj:47-67` links seven files from `test/tests`
and patches their `System*` usings (`:73-76`); `test/tests/tests.csproj:67-69`
links one back. A ninth link, the tier guard, is keyed on the project's name
(`test/Directory.Build.props:20-22`), as is the implicit-usings switch
(`Directory.Build.props:15`).

**Pull request #118 adds a third caller of the whole run.**
`.performance/benchmarks/Check/CheckBenchmarks.cs` saves a generated tree with
`new SpechtTreeStore(new FileSystem()).Save(SpechtTree.Of(Specifications), _root)`
and measures `SpechtRunner.Run(_root)`, in `[GlobalSetup]` and in the
`[Benchmark]` method, over `[Params(1, 10, 100, 1000)]`. It calls `Run` on the
type; at `eb7c161`, which that branch contains, `Run` is an instance member
(`src/specht/SpechtRunner.cs:69`). **Inference:** that is why the pull
request's build fails; its log was not read for this record. Reaching the
runner through `AddSpechtEngine()` is that pull request's change.

### What earlier records decided, and where the source has outgrown them

- ADR-0001 and ADR-0005 left `SpecManifest.Defaults` and `SpecSchemas.Options`
  as process-wide statics and kept a list of "statics that stay"
  (`.spec/adr/0001-resolve-the-engine-from-the-container.md:241-244`,
  `:249-258`; `.spec/adr/0005-typed-injected-stages-not-a-chain.md:469-481`).
  **Judgement:** each entry was defensible alone. Together they are the
  coupling the owner named.
- ADR-0005 (a) made the coordinator take its stages as loaders and call them
  from `Run` (`:405-461`). The owner's request rules that shape out in so many
  words: logic does not take a loader.
- Item `0107` decided on 2026-10-11 that "`SchemaVersions` stays a `Run`
  argument" and that "`Evaluate(model, rules)` and `Order` stay static"
  (`src/specht/.issue/0107-resolve-the-engine-from-the-container.yml:87-104`).
- ADR-0003 weighed a source-generated container, option 7
  (`.spec/adr/0003-microsoft-extensions-dependency-injection-is-the-container.md:125-132`),
  on the generator, a second composition model and Spectre's run-time
  resolution. It weighed neither start-up time nor ahead-of-time compilation.

## Decision drivers

In priority order.

1. **SOLID.** One reason to change per type; a rule set that grows without
   editing the evaluator; dependencies pointing at the stable side.
2. **I/O at the boundary.** Read, build an in-memory model, work on it, write.
   Nothing between the read and the write touches the file system.
3. **A rule that is off does not run.** For an id, not for a class.
4. **No static holds logic, I/O or loaded state.**
5. **An abstraction only where it is earned** (`coding-conventions` § Design;
   ADR-0005 (d)). Where this record takes one early, it says so.
6. **One assembly, with boundaries the build enforces.** The compiler does not
   stop a namespace cycle inside an assembly, so a test does.
7. **Readiness for ahead-of-time compilation, and start-up time.** The tool
   runs in a pre-commit hook on every operating system. New code uses no
   reflection.
8. **No structural stage changes a verdict** (`0001-F1` C-9, C-10). The golden
   report stays byte-identical through every stage but the renumbering.

## Considered options

1. **Layered namespaces in one assembly, I/O in stores, one rule per id, a
   pure evaluator, a mediator in front** - chosen.
2. **Keep ADR-0005 (a): one coordinator taking its loaders, and the eight rule
   classes.** No new type, and every citation of `SpechtRunner` stays true.
   Rejected: it is the shape the owner asked to leave. Logic keeps taking
   loaders, a rule keeps a reader, and a disabled id still runs.
3. **A use-case object in front of the engine** - one class per operation that
   the command constructs and calls. Fewer types than a mediator and no
   dispatcher. Rejected by the owner: "use-case is essentially clean
   architecture, which we explicitly prefer mediator over."
4. **Several assemblies, one per layer.** The compiler would then enforce the
   direction. Not proposed and not built: `0001-F1` C-7 counts the engine's
   packages for one assembly, `src/tool` is the one project that packs, and
   each added assembly is a load at start-up (driver 7). A test over one
   assembly's metadata gives the same guarantee.
5. **A third-party mediator now.** MediatR dispatches by run-time reflection
   over open generic types, against driver 7, and its current releases carry a
   commercial licence. A source-generated mediator package has neither
   defect, and every handler would implement that package's interfaces, then
   move again when the owner's source-generated cqrsz port arrives. Not
   chosen: the dispatcher this record needs is one lookup.
6. **Keep the eight classes and pass each the enabled ids.** Each rule then
   skips its own disabled branches. Rejected by the owner: selection leaks
   into every rule, each rule's branches are what a test has to arrange, and
   "is this id off" becomes eight implementations of one question. Leaving
   today's run-and-drop filter was rejected with it.
7. **Adopt a source-generated container now** (ADR-0003 option 7). Not chosen:
   nothing has been measured. ADR-0003 stands until track P's evidence.

## Decision

### The owner's eight decisions

Recorded in item `0154`, each dated 2026-10-11.

1. One rule per rule id: 21 rules, each reporting only its own id.
2. A mediator, not a use-case object. Requests and handlers take the shape of
   the owner's cqrsz library and are dispatched through a minimal `ICqrs` in
   this repository that uses no reflection, to be replaced by the owner's
   source-generated port without touching a handler.
3. Requests and handlers live in the engine, so the tool, the benchmarks and
   any later host send the same request.
4. A handler returns a plain result; an input failure stays an exception, as
   [ADR-0009](0009-input-failures-stay-exceptions-until-a-native-union-can-carry-them.md)
   proposes.
5. Every static holding logic, I/O or loaded state goes; a private
   `static readonly` constant stays.
6. One engine assembly, with boundaries inside it.
7. Readiness for ahead-of-time compilation and start-up time are drivers, and
   a spike measures them.
8. Rule ids become four digits with the thousands digit naming a group, in a
   track of their own after the rule split, as a clean break.

A chain of responsibility stays rejected
([ADR-0002](0002-no-chain-of-responsibility-for-the-check.md)).

### The layout

No production assembly is added. `src/specht` stays one assembly and
`src/tool` stays the host; the engine keeps its five packages (`0001-F1` C-7).
Each name below is a namespace and a folder under `src/specht/`. A namespace
references only namespaces below it in this list.

| Namespace           | Holds                                                                                                |
| ------------------- | ---------------------------------------------------------------------------------------------------- |
| `Specht.Check`      | `Check.Query` and `Check.Handler`; later the handlers of `--report`, `upgrade`, the pin and `format` |
| `Specht.Report`     | `SpecReportDocumentMapper`, `SpecReportSerializer`, `SpecReportStore`, the report document           |
| `Specht.Evaluation` | `SpecEvaluator`, `SpechtReport`, `SpecViolation`, `SpecViolationOrder`                               |
| `Specht.Rules`      | `ISpecRule`, `SpecFinding`, the 21 rules in a folder per group, `FrontmatterSchemaCheck`             |
| `Specht.Model`      | `SpecModel`, the parsers, `SpecModelBuilder`, `SpecModelReader`                                      |
| `Specht.Tree`       | `SpechtTree`, `SpechtTreeFile`, `SpechtTreeStore`, `SpechtRootNotFoundException` (item `0153`)       |
| `Specht.Manifest`   | `SpecStructure`, `SpecRuleSettings`, `SpecManifestParser`, `SpecManifestStore`                       |
| `Specht.Discovery`  | `SpecDiscovery`, its inputs and its locations                                                        |
| `Specht.Versioning` | `SemanticVersion`, `SchemaVersion`, `SchemaVersions`                                                 |

Two namespaces stand outside the order:

- `Specht.Cqrs` - `ICqrs`, `IQuery<T>`, `ICommand<T>`, the two handler
  contracts and the dispatcher. It references no engine namespace, and
  `Specht.Check` references it.
- `Specht.Composition` - `AddSpechtEngine()`. It registers every part, so it
  references every namespace and nothing references it.

Nothing is left in the root namespace. Where `SpechtResources` sits is owed
to the stage that builds it; it references no engine namespace.

### I/O is confined

Four types touch the file system, each through `IFileSystem`:

| Store               | Reads or writes                                    |
| ------------------- | -------------------------------------------------- |
| `SpecManifestStore` | The manifest's text, and whether it exists         |
| `SpecDiscovery`     | The listing of files under a root                  |
| `SpechtTreeStore`   | The files of a tree: their text in, and a tree out |
| `SpecReportStore`   | The report document, to the path a caller names    |

One more type, `SpechtResources`, reads the assembly's embedded resources -
the default manifest and the shipped schema versions - once, at composition.

`SpecModelReader` is the one type that sequences the stores: it takes a root,
asks each store for what it holds, and hands the text to `SpecModelBuilder`,
which touches no file system. No other type holds a store. A store parses
nothing and decides nothing; a parser, a builder, a rule, the evaluator, the
mapper and the serializer hold no `IFileSystem`.

### The model is an immutable snapshot

`SpecModel` is built once per run and never changed. It carries:

- paths relative to the root, and no other kind - the root is the reader's
  and the stores', and is not in the model;
- the tags of every companion `.feature` file, read at load;
- the derivations several rules compute for themselves today - a Feature's
  claims, its matrix section, the Features that have an identity, and the
  dependency graph - each computed once at build;
- the rule settings as `SpecRuleSettings`, mapping a rule id to a
  `SpecRuleLevel`, in place of `"error"`, `"warning"` and `"off"` strings.

### One rule per id

```csharp
public interface ISpecRule
{
    string Id { get; }

    SpecSeverity DefaultSeverity { get; }

    IEnumerable<SpecFinding> Evaluate(SpecModel model);
}
```

`ReportedIds` goes. A `SpecFinding` carries a file, a line, an identifier, a
message and what was expected; it carries no rule id and no severity, because
the evaluator stamps both. A rule takes no constructor argument that reads or
loads. The work two rules share - the JSON Schema evaluation beneath SPEC002,
SPEC003 and SPEC004 - is a class the three take, `FrontmatterSchemaCheck`, not
a base class.

### The evaluator is pure

```csharp
public sealed class SpecEvaluator(IEnumerable<ISpecRule> rules, SpecViolationOrder order)
{
    public SpechtReport Evaluate(SpecModel model);
}
```

`Evaluate` does four things in this order:

1. **Select** the rules whose id is in the pinned version's vocabulary and
   whose level is not off.
2. **Run** each selected rule once and materialise its findings.
3. **Stamp** each finding with the rule's id and a severity: the manifest's
   level for that id, or the rule's `DefaultSeverity` where the manifest gives
   none.
4. **Sort** with `SpecViolationOrder`: severity descending, file, line, rule
   id, stable - today's order (`src/specht/SpechtRunner.cs:60-66`).

The count of rules evaluated is the count selected. Selection is a `Where`
in the evaluator, as ADR-0004 decided; no selector type exists.
`SpecEvaluator` and `SpecViolationOrder` are instances, registered by
`AddSpechtEngine()`.

### A host sends a request

```csharp
public static class Check
{
    public sealed record Query(string Root) : IQuery<SpechtReport>;

    public sealed class Handler(SpecModelReader reader, SpecEvaluator evaluator)
        : IQueryHandler<Query, SpechtReport>;
}
```

The conventions are cqrsz's:

- one static class per operation, nesting the message record and its
  `Handler`; the static class holds no member but the two types;
- a message is a `Query` or a `Command`;
- a handler is registered by its concrete type;
- a handler never takes the mediator.

`Check.Handler` reads the model and evaluates it, and does nothing else.
`ICqrs` is the mediator: a host sends a message and receives its result. The
dispatcher that implements it resolves the handler from registrations
`AddSpechtEngine()` writes by hand, one per message, with no assembly scan and
no reflection.

**The departure from cqrsz.** Its handlers return `Either<Error, T>`. These
return the result and let the four input-failure exceptions cross, to the
catch ladder in `src/tool/Features/Check/CheckCommand.cs:31-46`, until a
native C# union can carry them (owner decision 4; ADR-0009, proposed, with
which this record is consistent and on which it does not depend).

### The patterns, present and absent

| Pattern   | Where                                                                                    |
| --------- | ---------------------------------------------------------------------------------------- |
| Strategy  | `ISpecRule`: the evaluator runs whichever rules it is given                              |
| Builder   | `SpecModelBuilder`: text in, one immutable model out                                     |
| Mediator  | `ICqrs`: a host knows a message, not a handler                                           |
| Decorator | Reserved for the fault isolation around a rule (ADR-0005 (c)); nothing else wraps a rule |

Deliberately absent:

- a chain of responsibility (ADR-0002, ADR-0005 (b));
- a generic stage interface (ADR-0005 option 8);
- a rule-selector type (ADR-0004 option 8);
- a report-renderer strategy - two formats and one consumer.

### Each interface, and what earns it

| Interface                                                | What earns it                                                                    |
| -------------------------------------------------------- | -------------------------------------------------------------------------------- |
| `ISpecRule`                                              | 21 implementations                                                               |
| `IFileSystem`                                            | Exists; the substitution for every store (ADR-0001)                              |
| `IQuery<T>`, `ICommand<T>` and the two handler contracts | The dispatcher is written against them; a second handler arrives with `--report` |
| `ICqrs`                                                  | Owner decision 2                                                                 |

`ICqrs` has one implementation the day it lands. It does not meet the bar of
ADR-0005 (d) and `coding-conventions` § Design - "a second real caller or a
substitution that exists today"
(`.spec/adr/0005-typed-injected-stages-not-a-chain.md:646-648`). It is taken
on the owner's decision, for a named second implementation: the generated
port. This is an exception to that rule, made once and written here; it is
not a precedent for another interface.

Everything else is a sealed class. `ISpecDiscovery` still arrives with items
`0007` and `0008`, as ADR-0001 decided, and not before.

### What "no static" covers

Gone: every static class but the two kinds named below, every non-private
static member that computes, reads or loads, and every static field or
property that holds loaded state. Staying:

- a private `static readonly` constant - a compiled regular expression, the
  Markdig pipeline, the serializer options, a list of known keys;
- `AddSpechtEngine()`, an extension method, which the language makes static;
- a cqrsz operation class such as `Check`, which nests two types and holds
  no member.

**Judgement:** a private static helper method inside an instance class, and a
`static` lambda, are not what the owner's decision removes. Neither is a
coupling another type can take, and the analyzers ask for both. Two public
statics are left to open points 4 and 5.

### How each cycle is cut

1. **Manifest and Versioning.** `SchemaVersions` gains a lookup that returns
   no version where `Select` throws today. The manifest parser asks, and it
   throws `SpechtManifestException` with the manifest's path. `Versioning`
   then references nothing.
2. **Report and the root.** `SpechtReport` moves to `Specht.Evaluation`, with
   the layout counts it holds. `Specht.Report` references `Evaluation`, and
   not the reverse.
3. **Manifest, the root and Model.** `SpechtRootNotFoundException` moves to
   `Specht.Tree`, beside the type that owns a root, and the reader makes that
   check before it asks for the manifest, so a missing root is still reported
   before a missing manifest. `SpecSchemas` becomes data, built by a pure
   `SpecSchemaCompiler` from the pinned version's schema text; it loads
   nothing. `SpecPath` is deleted: no absolute path reaches the model, so
   nothing maps one back.

### The guard

A test asserts, from the engine assembly's metadata:

- that no namespace references one above it in the layout;
- that only the four stores reference `IFileSystem`, and only
  `SpechtResources` reads an embedded resource.

It is built on the reference guard the tests already have
(`test/tests/Shared/EngineAssembly.cs`), with no new package, and lands with
stage 7, when the last upward reference is gone.

### The test-support project

One project is added, and it is the only one: a test-support library that
`test/tests` and `test/acceptance` both reference. It holds the eight files
the two projects link from each other today. With it go the eight
`<Compile Include>` links, the `System*` `<Using>` patch, and the conditions
keyed on a project's name. This is track T.

### Rule ids

Rule ids become `SPEC` and four digits. The thousands digit is the group:

| Ids        | Group                    | Today's ids in it                  |
| ---------- | ------------------------ | ---------------------------------- |
| `SPEC0xxx` | Faults of the tool       | None                               |
| `SPEC1xxx` | Frontmatter              | SPEC001, SPEC002, SPEC003, SPEC004 |
| `SPEC2xxx` | Structure                | SPEC010, SPEC013                   |
| `SPEC3xxx` | Identity                 | SPEC011, SPEC012                   |
| `SPEC4xxx` | Claims and feature files | SPEC020, SPEC021, SPEC030, SPEC031 |
| `SPEC5xxx` | Work items               | SPEC040, SPEC041, SPEC043, SPEC044 |
| `SPEC6xxx` | Dependencies             | SPEC050, SPEC051, SPEC052          |
| `SPEC7xxx` | Approval                 | SPEC060, SPEC061                   |

This record assigns no new id. The map from each old id to its new one is
track R's, and the owner approves it there.

The renumbering rewrites the vocabulary of schema version `0.1.0` in place
and replaces the golden baseline once. As written, `0001-F7` C-11 makes "any
change under which a tree that passed can fail - a new `SPEC###` included" a
major version, and `0001-F1` C-10 fixes the golden report as the engine's
output at commit `e7dba24`. The owner decided a clean break with no new major
version: "we haven't shipped anything". So track R opens with a decision
record of that ruling and the deltas to brief § 4, `0001-F7` C-11 and
`0001-F1` C-10, before any id changes. It is the one track that is not a
refactor.

### Ahead-of-time compilation

Track P is a spike. On trial branches that never merge, it publishes the tool
ahead of time, records the trim and compilation warnings library by library -
`Spectre.Console.Cli` first - and measures start-up through `0109-F4`.
[ADR-0003](0003-microsoft-extensions-dependency-injection-is-the-container.md)
is open to revisit on that evidence. It is not superseded, and nothing here
adopts a source-generated container.

### What this supersedes

| Record   | Superseded                                                                                                                                                                                                                                                                                                       | Stands                                                                                                                                                                                                                                                  |
| -------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ADR-0001 | In "Lifetimes": the two process-wide statics and `SpecSchemas.Load` as the loader (`:241-246`). In "The statics that stay" (`:249-258`): `SpecDiscovery.Relative` and `SpecReportDocument.From`.                                                                                                                 | Container resolution and `AddSpechtEngine()` as one hand-written list (`:213-221`); `IFileSystem` as the substitution (`:228-229`); `ISpecDiscovery` with items `0007` and `0008` (`:230-235`); the constants on the list; driver 3, but for `ICqrs`.   |
| ADR-0004 | That `DefaultSeverity` lands with epic `0101`'s first item and not before (`:180-185`): it lands with the collapsed `ISpecRule`. The cost bullet keeping `SpecSeverity.Error` in each rule's yields until then (`:218-220`).                                                                                     | Selection and the severity map as two functions inside evaluation (`:165-176`), now true for an id; `MinimumSchemaVersion`'s timing; the rejected stage pipeline and rule-set filter (`:177-179`); the decorator trigger (`:193-199`).                  |
| ADR-0005 | In Decision (a): the coordinator that takes its stages and calls them from `Run`, and its sketch (`:405-437`); the model stage owning its readers (`:449-455`); the loaders as the manifest and model stages (`:456-461`); "The statics (a) does not replace" (`:469-481`); the coordinator's name (`:504-508`). | The order inside `Evaluate` (`:442-448`), with the severity map applied as the stamp; (b), no chain; (c), fault isolation by a decorator; (d), a test fake is not a second implementation, with `ICqrs` the one stated exception; option 8's rejection. |
| ADR-0008 | The rule set as `Evaluate`'s second argument (`:98-101`): the evaluator takes its rules by constructor. The filter "over each rule's violations" (`:104-106`): a rule reports one id, so nothing is filtered after it runs. This also settles the question `:115-118` left open.                                 | The model carries the pinned vocabulary and the evaluator reads it there (`:102-106`); ADR-0004's clause as narrowed (`:107-110`).                                                                                                                      |

ADR-0002, ADR-0003 and ADR-0007 stand whole. ADR-0006 stays rejected.
ADR-0009 is proposed, and this record neither accepts nor depends on it.

Item `0107`'s two decisions of 2026-10-11 are reversed: `Evaluate` and `Order`
become instance members of `SpecEvaluator` and `SpecViolationOrder`, and the
set of shipped versions is a constructor dependency, read once at
composition, in place of an argument to `Run`.

### The order of work

Every code stage is proven by the golden report
(`test/tests/Engine/Baseline/engine-e7dba24.json`) byte-identical and the
whole suite green. `spec-author` cuts the items; the letters are the plan's,
and no item id is taken here.

| Stage | Work                                                                                                                                                                                                    | Kind                    | After    |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------- | -------- |
| 1     | Item `0100`: the `Spec*` types take the `Specht*` prefix                                                                                                                                                | refactor                | #118     |
| 2     | A: the shared derivations move onto the model; `SpecRuleSettings` and `SpecRuleLevel`; companion tags read at load; `FeatureFileRule` loses its reader; `SpecPath` deleted                              | refactor                | 1        |
| 3     | B: each multi-id rule class is split into rules of one id, with the evaluator untouched                                                                                                                 | refactor, one per class | 2        |
| 4     | C: `ISpecRule` collapses; `SpecFinding` and `DefaultSeverity`; `SpecEvaluator` and `SpecViolationOrder` as instances                                                                                    | refactor                | all of 3 |
| 5     | D1: `SpecManifestStore`, `SpecManifestParser`, the defaults; `SchemaVersions` an instance; `SpechtResources`; the Manifest and Versioning cycle cut                                                     | refactor                | 1        |
| 6     | D2, with item `0153`: the tree and its store move into the engine; `SpecModelBuilder`, the parsers, `SpecModelReader`; the loaders and the statics of `Model` go                                        | refactor                | 2, 5     |
| 7     | E: `Specht.Cqrs`; `Check.Query` and `Check.Handler`; the report mapper, serializer and store; `SpechtRunner` deleted; the guard                                                                         | refactor                | 4, 6     |
| 8     | Item `0144`, reshaped: ADR-0007's factory; `CheckCommand` sends `Check.Query` through `ICqrs`; the `Func<string, SpechtReport>` seam goes; `ShippingCopy` an instance                                   | refactor                | 7        |
| T     | The test-support project; the eight links, the `System*` patch and the name-keyed conditions go; the root `Content` globs nothing reads go, once the package's contents are confirmed unchanged         | refactor                | nothing  |
| R     | The renumbering: the owner's decision record and the specification deltas first, then the approved id map, the rule pages, the vocabulary, the manifest keys, a new golden report and the group folders | task                    | 4        |
| P     | The ahead-of-time spike                                                                                                                                                                                 | spike                   | 7        |

**What runs in parallel.** Stage 5 beside stages 2 to 4. The parts of stage 3
beside each other, where they touch different classes. Track T beside
everything. Track R once stage 4 lands.

**Where the source differs from the plan.** The plan cut stage 3 into seven
parts, "one per old class". There are eight classes. `FeatureFileRule` is
rewritten in stage 2; whether its split rides with stage 2 or is an eighth
part is `spec-author`'s to cut. The plan also listed `Specht.Composition`
inside the downward order, below `Specht.Check`; a composition root registers
every part, so it stands outside the order, as above.

**Items reshaped.**

- Item `0152` becomes one unit-test class per single-id rule, 21 of them, and
  depends on stage 3.
- Item `0036` becomes `--report` and a `SaveReport.Command` handler over
  `SpecReportStore`, and depends on stage 7.
- Items `0007` and `0008` gain a dependency on stage 6.
- Still owed, and not reshaped: the fault-isolation decorator of ADR-0005 (c)
  (`0001-F1` B-012 to B-016, `0001-F1` OQ-2).

**Specification deltas owed, each written by its owning role before the code
of its stage.** This record edits no specification.

| Stage | Delta                                                                                                                                                                                                                                                                                                   |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 2     | `0109-F2` C-6 and C-7, and its A-2 and decision 0001, which say a rule and evaluation read the generated tree's `.feature` files; after stage 2 neither reads the disk. `0001-F5` § 7 "Where rule settings apply" and "What loads a rule".                                                              |
| 4     | Any § 9 row that names a test of a multi-id rule class or of `SpechtRunner.Evaluate`. `0001-F5` § 7 and § 8 where they name `Enabled`, `Graded` and `SpechtRunner.Order`. `0109-F2` A-1, which names the static `SpechtRunner.Evaluate`.                                                                |
| 6     | `0109-F2` B-003 and C-3, the frontmatter read as a unit. `0109-F2` A-1, which names `SpecSchemasLoader` and `SpecModelLoader`. `0109-F1` § 7 and § 8, which place the tree types in the benchmark project.                                                                                              |
| 7     | `0001-F2` C-4, "calls the runner", and its § 7 on the injected runner. `0001-F3` § 7 on `SpecReportDocument.From` and `SpechtRunner.WriteReport`. `0109-F3` A-2, § 6 row 2, § 7, § 8 and § 9 as pull request #118 leaves them, which name `SpechtRunner.Run`. `0109-F1` § 7 and § 8 where they name it. |
| R     | Brief § 4, `0001-F7` C-11, `0001-F1` C-10, and every rule page.                                                                                                                                                                                                                                         |

`0109-F3` C-2 needs no delta: "the engine's whole run from a root on disk -
manifest load, discovery, model build, evaluation and the report - and
nothing of the host" names no type and stays true.

**Why no structural stage changes a verdict.**

- The rule id is the last key of the sort, so splitting a class changes
  nothing the first three keys decide.
- The sort is stable, so two violations equal on all four keys keep the order
  they were emitted in. Within one id that order is the old class's, and each
  split rule keeps it. Four ids are emitted from more than one place:

  | Id      | Emitted, in this order                   | Read at                                                     |
  | ------- | ---------------------------------------- | ----------------------------------------------------------- |
  | SPEC001 | Features, then items, then epics         | `src/specht/Rules/FrontmatterSchemaRule.cs:29-51`           |
  | SPEC010 | Missing or duplicated, then out of order | `src/specht/Rules/SectionStructureRule.cs:33`, `:43`, `:62` |
  | SPEC044 | Reused ids, then gaps in a sequence      | `src/specht/Rules/ChildItemRule.cs:138`, `:184`             |
  | SPEC052 | Self-dependencies, then cycles           | `src/specht/Rules/DependencyRule.cs:96`, `:147`             |

- A composition test asserts that the registered rule ids equal the pinned
  version's vocabulary, each once, so the count of rules evaluated stays 21.
- Every rule returns error severity today, so `DefaultSeverity` is error for
  all 21 and stamping changes no severity.

### The whole-check benchmark

Pull request #118 lands before stage 1, and the staged items assume it has.
This change is documents only and touches none of its files.

- **It is a named caller.** `CheckBenchmarks` measures "the engine's whole run
  from a root on disk ... and nothing of the host" (`0109-F3` C-2). It is one
  of the two callers decision 3 puts the request and its handler in the engine
  for: a benchmark cannot reach a handler that lives in the host without
  measuring the host.
- **What it measures after stage 7.** `[GlobalSetup]` builds a provider from
  `AddSpechtEngine()` and resolves `Check.Handler`; the measured method calls
  that handler with a `Check.Query` of the root. Building the container and
  resolving stay outside the measured call, as `0109-F2` C-9 requires of a
  stage and `0109-F2` decision 0001 gives the reason for: "Container
  resolution is not a unit."
- **The handler, not a send.** **Judgement.** The handler is the whole run
  C-2 describes; the dispatcher is how a host reaches it. Measuring the
  handler keeps the finding comparable when the dispatcher is replaced by the
  generated port, and it needs nothing from `src/` that production does not
  already have, because a handler is registered by its concrete type. No
  production type changes shape for the benchmark (`0109-F2` C-11). What a
  send adds is `0109-F4`'s to show, inside the tool's start.
- **Which stage re-points it.** Stage 6, with item `0153`, re-points its use
  of `SpechtTree` and `SpechtTreeStore`. Stage 7 re-points the measured call.
  Both are in those stages' scope.
- **The benchmark project compiles after every stage.** `specht.slnx` lists
  it and `test/tests/tests.csproj:25` references it, so a stage that changes
  an entry point a benchmark calls changes the benchmark in the same pull
  request. Stage 4 does so for any `0109-F2` benchmark that calls
  `SpechtRunner.Evaluate` by then.

## Consequences

Buys:

- A rule the manifest turns off is not run, for every id.
- Rules, the evaluator, the builder, the parsers and the report mapping run
  on values in memory. Each is tested with no file system and no fixture
  tree, which is what item `0152` needs.
- One place sequences the read, one place evaluates, one place writes.
- A new host - the benchmark today, `--report`, `upgrade` and `format` next -
  sends a message and composes nothing.
- The namespace direction is a failing test, not a convention.
- No type reads a static another type filled.
- New code is free of reflection, so track P measures the libraries and not
  this repository's own code.

Costs:

- About thirty new types, an estimate from the layout and not a count of
  code, and 21 rule classes and 21 registrations where there were eight.
- A dispatcher this repository owns and tests until the port replaces it,
  behind an interface with one implementation.
- Handlers that depart from cqrsz's return type, to be revisited with
  ADR-0009.
- Companion `.feature` files are read at load. A file that cannot be read
  fails the load, where today it fails only when SPEC020 or SPEC021 runs
  (open point 1).
- `DefaultSeverity` has one value across all 21 rules the day it lands. It is
  taken so a finding carries no severity; ADR-0004 called such a member "a
  configuration point nothing configures", and that stays true until a rule
  defaults to warning.
- Every specification, skill and item that names `SpechtRunner`, a loader or
  a multi-id rule class is owed a delta, listed above; the benchmark
  specifications are rewritten at three stages.
- A benchmark finding recorded before stage 7 and one recorded after it are
  measured through different entry points: `SpechtRunner.Run`, then
  `Check.Handler`. The two compare the check. Neither measures dispatch, and
  a difference between them is not evidence about the mediator. The
  `benchmarker` records the entry point beside each finding.
- Eight stages and one track of work in which, by design, no verdict
  changes; the renumbering then replaces the golden report once.

## Open points

Each default stands unless the owner overrules it in review.

1. **A companion file that cannot be read.** Reading every `.feature` file at
   load makes an unreadable one a load failure even with SPEC020 and SPEC021
   off. Default: accept it, and record the changed timing in `0001-F1` § 10
   with stage 2.
2. **Where `init`'s handler lives.** Its data is the tool's embedded shipping
   copy. Default: in the tool's `Init` slice, dispatched through the same
   `ICqrs`, until `upgrade` pulls it into the engine.
3. **Namespace casing.** The layout above writes `Specht.*`; the repository's
   naming is lowercase `specht` for projects and packages. Default: settled
   inside item `0100`, which renames the types.
4. **Two pure statics.** Default: `SemanticVersion.TryParse`
   (`src/specht/Versioning/SemanticVersion.cs:15`) stays static, the base
   class library's own parse idiom; the glob helpers of `SpecDiscovery`
   (`src/specht/Discovery/SpecDiscovery.cs:71-91`) become a value object.
5. **`Console.Error` in commands**
   (`src/tool/Features/Check/CheckCommand.cs:93`,
   `src/tool/Features/Init/InitCommand.cs:32`). Default: handled under item
   `0144`.
6. **Synchronous I/O.** Default: it stays through this series; an
   asynchronous boundary is a later item.
7. **The vocabulary is written twice** - in `SchemaVersions`
   (`src/specht/Versioning/SchemaVersions.cs:71-77`) and as the 21 rules'
   ids. Default: keep both, held equal by the composition test, and revisit
   in track R.
