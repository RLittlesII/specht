---
title: "ADR-0024: Scrutor decorates explicit rule registrations"
description: "Keep the Microsoft dependency-injection container and explicit rule list, and apply fault isolation with Scrutor Decorate"
type: adr
---

# ADR-0024: Scrutor decorates explicit rule registrations

## Status

Accepted, 2026-10-27, by the repository owner while addressing review
5675863896 on PR 587. The owner confirmed adoption in the delivery session.
Supersedes ADR-0005 (c)'s per-registration wrapper construction only.

## Context

The fault-isolation implementation repeats wrapper construction in every
`ISpecRule` registration, manually supplying the matrix rules' dependencies.
The review proposed Scrutor. `0001-F1` C-7 requires an ADR for an additional
engine dependency. ADR-0003 keeps the existing container and explicit service
list; ADR-0014 continues to gate the mediator and architecture migration.

## Considered options

1. Keep each manually constructed wrapper. No new dependency, but every rule
   registration repeats decoration and constructor wiring.
2. Keep the explicit typed registrations and use Scrutor's `Decorate` once.
   Adds a package but delegates constructor activation and decoration to the
   existing container extension.
3. Scan assemblies with Scrutor. Removes the explicit list and changes which
   rules run when a type is added; rejected under ADR-0003 and `0001-F1` C-5.

## Decision

Choose option 2. Pin Scrutor 7.0.0 centrally and reference it from the engine.
Register each rule explicitly with `AddSingleton<ISpecRule, T>()`, then call
`Decorate<ISpecRule, FaultIsolatedRule>()`. Retain the composition test that
every concrete rule is present exactly once, wrapped, in ordinal id order.
No assembly scanning, container replacement or mediator is introduced.

The [package](https://www.nuget.org/packages/Scrutor/7.0.0) supports the engine's
targets and depends on dependency-injection abstractions and DependencyModel
10.0.0 or later. The existing central abstractions pin remains authoritative.
The [documented decoration API](https://github.com/khellang/Scrutor#decoration)
wraps already registered services.

## Consequences

Rule names and order remain visible, and the container supplies constructor
dependencies. Fault isolation is applied in one place. The packed tool gains
Scrutor and its DependencyModel dependency, and their maintenance cost. The
ordinary verdict, default levels, singleton lifetime and error handling remain
unchanged. This adoption makes no performance claim and does not waive ADR-0014.
