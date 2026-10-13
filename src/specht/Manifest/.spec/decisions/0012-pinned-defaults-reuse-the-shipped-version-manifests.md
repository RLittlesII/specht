---
title: "Decision 0012: pinned defaults reuse the shipped version manifests"
description: "Technical design for B-048 using the existing versioned manifest sources, an instance catalog and a parse-local selected default, while preserving collection merge boundaries and rule-setting behavior"
type: decision
---

# Decision 0012: pinned defaults reuse the shipped version manifests

**Date:** 2026-10-13
**Decided by:** implementer for the internal design; the product rule is the
owner's decision 0010, ratified 2026-10-13

## Status

Designed, not built. This record chooses the mechanism for B-048 and adds
no new consumer behavior or interpretation beyond decision 0010.
Implementation follows ADR-0010 stage D1, item `0165`.

## The call

1. Link the existing `src/tool/manifest/v<version>/manifest.json` files into
   the engine's embedded resources. Use the version folder as identity;
   no second manifest source or package-to-schema mapping is created.
2. Carry each version's manifest text on its existing version contract.
   `SpechtResources` reads raw resources once at composition; the instance
   version catalog is supplied to the pure parser. Versioning depends on
   no other engine namespace.
3. Read the consumer's exact pin before filling settings. A missing pin
   means `0.1.0`, not the newest default's pin. An unavailable pin keeps
   the existing invalid-manifest failure and gains no fallback.
4. Select defaults locally for each parse; never change a singleton
   parser's shared default when a different root pins another version.
5. Preserve existing merge boundaries: whole declared collections replace
   their defaults; named maps fill missing names and keep explicit values.
   `rules` is never filled, and no default upstream source is adopted.
6. Repoint all parser construction paths, including the host's injected
   parser and optional fallback, to the same versioned catalog. This
   changes no `init` write policy, `ShippingCopy` behavior or schema source.

## Why

The engine currently embeds the repository's live manifest, while the host
already ships a manifest per schema version. B-048 names the pinned
version's shipped manifest as the source of omission defaults; the
existing versioned files therefore supply it without a second copy.

`0165` supplies the required instance catalog, pure lookup and resource
boundary. Waiting for that stage keeps this behavioral change out of its
verdict-preserving refactor. The dependency is on those delivered seams,
not simply on shared files.

Two shipped versions currently have equal values for every setting that
is filled. A real-resource baseline stays useful, but it cannot prove
which version supplied the value. Synthetic versions with different
defaults must make the wrong-version choice fail in memory and acceptance.

## Rejected alternatives

- **Keep the live manifest as a global default.** Smaller change, but a
  later working version can change an older pin's result and the parser
  cannot prove B-048.
- **Copy defaults into an engine-owned directory.** A second source can
  drift from what `init` writes; resource links already solve that problem.
- **Select the newest default or inherit its pin.** It silently moves the
  contract of a manifest whose pin is absent or older.
- **Mutate a singleton parser's selected default.** Interleaved or
  concurrent parses of different roots can affect one another.
- **Deep-merge every object or concatenate lists.** It changes the existing
  explicit-value and whole-collection behavior; B-048 changes the version
  supplying a default, not those semantics.
- **Introduce a default-provider interface or parallel version catalog.**
  The existing instance catalog supplies the data and is directly
  constructible in tests; another abstraction has no second real caller
  or missing substitution to justify it.

## Affects

Manifest § 6, § 7 and § 8; B-048's scenarios and Missing traceability row;
item `0191`'s dependency on `0165`. The earlier `0016` design remains
blocked on `0191` as well as its other prerequisites.

The later implementation changes the engine's resource links, the version
contract, parser default selection and callers. No such code, resource
bytes, schema version or test binding is changed by this design pass.

## Reversal

None.
