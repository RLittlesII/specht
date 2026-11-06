---
title: "Releasing specht"
description: "The maintainer checklist for publishing a version and closing its completed milestone"
type: reference
---

# Releasing

Follow the [clone setup](../AGENTS.md#setup) and authenticate the `gh` CLI.
Publish the intended release commit before advancing to the next development
minor. Replace `<run-id>` and `<tag>` below with the values GitHub and `nbgv` print.

1. **Prepare and merge the release changes.** Confirm the selected work items
   are done and the PR checks pass. `version.json` selects the release line,
   such as `0.5-preview`; `nbgv` computes the patch number. Every `0.x` version
   keeps `-preview`. If a new schema version is included, run
   `./build.sh LockSchemaVersion` and commit its lock before publishing.

2. **Dry-run the release on `main`.** Start from a clean checkout:

   ```sh
   git switch main
   git pull --ff-only
   dotnet tool restore
   gh workflow run release.yml --ref main
   gh run list --workflow release.yml --limit 3
   gh run watch <run-id> --exit-status
   ```

   Select the manual run for the intended commit and require success. It
   builds, tests and packs without publishing; tag-specific guards run on
   the pushed tag. Keep the tested commit checked out for the next step.

3. **Review, create and push its computed tag.**

   ```sh
   dotnet nbgv get-version --variable NuGetPackageVersion
   dotnet nbgv tag --what-if
   dotnet nbgv tag
   git push origin <tag>
   ```

   Use the exact tag `nbgv` prints. Pushing it publishes: the workflow checks
   that the version matches the tag, the commit is reachable from `main`,
   every shipped schema is locked and a `0.x` version is a prerelease.

4. **Verify publication.** Watch the tag's `release` workflow to completion.
   Confirm `specht.tool`, `Specht.CodeAnalysis` and `specht.mcp` are on nuget.org
   at the same computed version, and the GitHub Release has generated notes and the correct
   prerelease flag. Packages are on nuget.org; the GitHub Release attaches
   neither package. Review the advisory release-comparison report; an
   incomplete comparison is retried by rerunning all jobs.

5. **Close a completed milestone in one PR.** After its final version is
   published, make these edits together:

   | File              | Change                                                                             |
   | ----------------- | ---------------------------------------------------------------------------------- |
   | `version.json`    | Advance the development minor, for example `0.5-preview` to `0.6-preview`.         |
   | `.issue/.goal`    | Remove the completed milestone's entire section; the next section becomes current. |
   | `docs/roadmap.md` | Mark the delivered slice complete and identify the next milestone as current.      |

   Run `./build.sh`, merge after the PR checks pass, and watch the `mirror`
   workflow on the merge commit. Verify the released milestone is closed with
   zero open issues and the next milestone remains open. Record the release,
   workflow and milestone links in the PR. GitHub milestones follow the goal
   file; closing one by hand will be undone while its section remains.

For a patch release during an unfinished milestone, keep its goal section and
development minor. The closeout step applies when the selected milestone work
is complete. Package versions and schema versions are independent; publishing
a package does not require a schema-version bump.
