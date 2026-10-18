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
Set `SpechtTestEvidence` for explicit collector inputs.

Diagnostic severity and suppression use standard Roslyn configuration. The
package adds no application runtime dependency.
