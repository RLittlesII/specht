---
title: "Lesson 0001: a tree kill that stops first stalls the host"
description: "On .NET 11 under Unix, Kill(entireProcessTree: true) stops the tree before it kills it and can stall every later start; specht-mcp kills descendants itself"
type: lesson
kind: product
---

# Lesson 0001: a tree kill that stops first stalls the host

**Date:** 2026-11-08
**Kind:** product

## Symptom

Under `net11.0` the Integration Test step stalled and never ended: runs
42526206109 and 42530364175 on pull request #792, and 1 of 11 full `net11.0`
Integration-tier runs locally. Stuck test hosts ran for up to two days at
high CPU, the cancel test's `sh` stopped (state `T`) and the hosts' children
left unreaped as zombies. `net10.0` passed.

## Root cause

The reading the evidence supports; no managed stacks were obtained. On Unix,
.NET 11's `Process.Kill(entireProcessTree: true)` stops the whole tree with
`SIGSTOP` before it kills it (dotnet/runtime#128598), and it can get stuck
between the two steps, after which every later process start in the host
blocks. The upstream reorder (dotnet/runtime#135624, issue #131944) is in
the 12.0.0 milestone. Bounding the tests and removing the FIFO (`5099c78a`)
did not stop the stall: it still reproduced 1 of 11 locally.

## Spec delta

`0132-F3` § 3: B-036, a cancelled call ends cancelled, and B-037, it leaves
no program it started running, with their scenarios and § 9 rows; § 5 row 20
excludes a program started after the cancellation.

`0132-F3` § 6 and § 7 (`bb378585`): on macOS and Linux a cancelled run takes
one snapshot of the program's descendants and kills them, children before
parent, then the program, with no step that stops the tree. Windows and other
systems keep the tree kill.

Gap, recorded in § 7: a program forked after the snapshot is not killed, and
the cancel test's tree is one level deep, a shell and its `sleep`.

## Claim

`0132-F3` B-036 and B-037 — § 9, through the process boundary beneath them,
`AProgramThatIsRunning_WhenTheRunIsCancelled_ShouldThrowCancelledAndLeaveNoProgramAlive`.
The test is bounded, uses no FIFO, and fails if a descendant is not killed.
