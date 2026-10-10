---
title: "SPEC012: A Feature identity is specified in one place"
description: Two or more Feature specifications declare the same epic and id.
type: rule
---

# SPEC012: A Feature identity is specified in one place

## Metadata

| Property         | Value    |
| ---------------- | -------- |
| Rule ID          | SPEC012  |
| Family           | Identity |
| Default severity | Error    |
| Schema version   | 1        |

## Cause

Two or more Feature specifications, in either layout, declare the same `epic` and `id`. Reported once for each such file, at its `id` key:

`'{identity}' is specified in {count} places ({paths}) - a migration removes the old tree in the same change`

## Rule description

One file declares each identity, `<epic>-<id>`. A Feature moved to the co-located layout leaves no copy under `epics/`.

### Example violation

`epics/0007-orders/F1-checkout/spec.md` and `src/orders/.spec/README.md` both open with:

```yaml
---
id: "F1"
epic: "0007"
```

```text
epics/0007-orders/F1-checkout/spec.md(2): error SPEC012: '0007-F1' is specified in 2 places (epics/0007-orders/F1-checkout/spec.md, src/orders/.spec/README.md) - a migration removes the old tree in the same change
src/orders/.spec/README.md(2): error SPEC012: '0007-F1' is specified in 2 places (epics/0007-orders/F1-checkout/spec.md, src/orders/.spec/README.md) - a migration removes the old tree in the same change
```

### Corrected

`epics/0007-orders/F1-checkout/` is deleted; one file declares `0007-F1`:

```text
src/orders/.spec/README.md
```

## How to fix violations

- **One Feature in two places.** Keep one file and delete the other with its folder.
- **Two Features with one identity.** Give the newer one the next unused `id` in its epic; the older one keeps its identity.
