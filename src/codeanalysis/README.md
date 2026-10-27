---
title: "Specht.CodeAnalysis package"
description: "Install the Specht analyzer and code fixes into a consumer project"
type: reference
---

# Specht.CodeAnalysis

Install `Specht.CodeAnalysis` as a private `PackageReference` in projects that
should load the analyzer and code fixes. The package includes MSBuild integration
that finds the nearest `.specht/manifest.json` above the project directory and
supplies consumer analysis inputs to Roslyn.

```xml
<PackageReference Include="Specht.CodeAnalysis" Version="$(SpechtVersion)" PrivateAssets="all" />
```

Set `SpechtRoot` when the project is outside the repository tree or when the
nearest manifest is not the intended repository. Add extra input files through
`SpechtAdditionalRecords`; each item must set `SpechtInputKind` to the input role
and may set `SpechtRelativePath` when the path cannot be derived from the root.
Add `SpechtTestEvidence` items for explicit test evidence; the package delivers
them under the `test-evidence` input kind. Declare both item types in the project
body.

On a clean checkout no evidence exists yet. The first build recognises test methods
provisionally and succeeds with mandatory enforcement and warnings as errors. Run
`dotnet test --list-tests`, then `dotnet test`; the logger writes
`obj/specht-test-evidence/<project>-<framework>.json`. Supply that file as
`SpechtTestEvidence` to rebuild with authoritative recognition, and run `specht
evidence obj/specht-test-evidence` to reconcile. An artifact that does
not match the build's project, framework, configuration, sources or policy is
ignored and recognition stays provisional.

For an xUnit v2/VSTest test project the package registers the `specht` VSTest
logger: it adds its `build/vstest/<tfm>` folder to `VSTestTestAdapterPath` and
`specht` to `VSTestLogger`. Passing `--logger` or `--settings` on `dotnet test`
replaces the registered logger, so no evidence is written unless the command also
names `--logger specht`.

For tagged `.feature` files as the claim catalogue, see
[native Gherkin claim input](https://github.com/RLittlesII/specht/blob/main/docs/native-gherkin-claims.md)
for manifest settings and the clean-checkout evidence sequence.

Diagnostic severity and suppression use standard Roslyn configuration. The
package adds no application runtime dependency.

See the [diagnostic reference](docs/README.md) for each published diagnostic,
its reporting conditions, and available code fixes.
