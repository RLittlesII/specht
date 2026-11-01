---
title: "ADR-0027: Files stay the reading path; an index, if built, is keyed by path; online allocation waits"
description: "Item 0252's proposal, carrying its own evidence. Filesystem search, a keyed index followed by source reads, and complete documents stored in a database answered one question set over one corpus; no path improved on filesystem search, which used the fewest tokens and tool calls, and the id-keyed store lost a record. Proposed: keep Markdown as the reading path and decision 0001 as it stands, build no index on answer-quality grounds, key any index by path, and do not adopt online allocation now. The record adopts nothing."
type: adr
---

# ADR-0027: Files stay the reading path; an index, if built, is keyed by path; online allocation waits

## Status

proposed - 2026-11-01. Decider: the repository owner. Item `0252`
([`.issue/0252-evaluate-mcp-record-storage.yml`](../../.issue/0252-evaluate-mcp-record-storage.yml)),
the spike epic [`0132`](../../epics/0132-model-context-protocol/epic.md)
§ "Related records" and epic [`0324`](../../epics/0324-specht-service/epic.md)
§ "What this epic waits on" name. The record adopts nothing, amends no claim
and edits no accepted record, `0132-F1`
[decision 0001](../../src/specht/Index/.spec/decisions/0001-markdown-authority-and-current-answers.md)
included. It leaves the owner two questions: `0132-F1` OQ-4, which access and
storage option that Feature takes, and epic `0324`'s existing question
"Should online mode be adopted, and which offline, permitted-write and CLI
parity rules would need amendment?", which names this spike.

This record is the evidence. The trial's corpus definition, questions, answer
key, rubric, agent setup, adapter, results, storage survey and allocation
analysis are written here, in the Context and the appendices, so that a
reader with only `main` can re-run the trial. No branch holds anything this
record needs.

## Context

### How to read the evidence

- **Read from a commit**: the corpus and specification text. The corpus is
  the tree at `51c00ffd` (`main`, 2026-10-31).
- **Measured by a trial**: an agent's answers, their scores, and the tokens,
  tool calls and elapsed time the harness's task notification reported for
  each participant. Run on 2026-11-01 and not re-run for this record.
- **Surveyed**: storage facts read from vendor and project pages on
  2026-11-01 by a participant with web access, not re-read for this record.
  The pages are listed in Appendix D.
- **Judgement** or **inference**: labelled where it stands.

### The question

Item `0252`, from the owner's clarification of 2026-10-20 (epic `0132`
§ Summary): does an agent answer questions about the records around a
specification - items, Features, epics, ADRs, lessons and decisions - better
from complete documents held in a database, or from an index that leads it to
the authoritative files? And, if an online mode is adopted, how does one
project sequence stay unique across machines (epic `0324` § "Document
sequence allocation", owner, 2026-10-17 and 2026-10-20)?

What stands today: `0132-F1` decision 0001 makes Markdown authoritative and
the database "a rebuildable index of its files and facts", every answer
current with the tree (C-1, C-8). Its B-003 and OQ-3 rest the index on the
cost of repeated lookups, which no benchmark has measured. `0132-F1` § 2 also
records the owner's request of 2026-10-30 for an opt-in database authority
over all managed documents, now epic `0324` § "Service mode".

### The trial

**Corpus.** The repository's tree at `51c00ffd`, read in place. The adapter
(Appendix C) finds 617 record files - 287 items, 54 Features, 13 epics, 33
ADRs (26 of them root ADRs, 7 in Features), 203 decisions and 27 lessons -
and stores 616 rows and 4,996 links; the one file lost is the duplicate id
below. These counts were re-taken for this record by running the adapter's
discovery and build read-only against an in-memory database. The trial's own
scoring sheet gave "616 records" with "26 ADRs": its total was the row count,
and its breakdown counted root ADRs only and summed to 610.

**Question set.** Twelve questions, each crossing record types and asking for
a source path; Q05 tests a missing link, Q06 a missing record, Q10 a stale
statement and Q11, after the key correction, a false premise. Verbatim, with
the answer key, in Appendix A.

**Rubric.** Per question: correct 0 to 2, complete 0 to 2, citation 0 to 2,
and a stale penalty of 1. Six a question, 72 in all. Verbatim in Appendix A.

**Agent setup.** One general-purpose participant per path, every one the
same model and agent type, with the same question text and output format, run
in parallel on 2026-11-01. The access rules were stated in each prompt and not
enforced by the harness. Appendix B gives the rules per path, and Appendix E
each prompt and each raw answer verbatim.

**Versions.** Corpus `51c00ffd`; Python 3.15.2; SQLite 3.54.1 with FTS5.

**The adapter.** A disposable command-line script over SQLite with FTS5, not
an MCP server, with two modes: `index`, which returns keys, paths, status and
links and never a body, and `docs`, which returns complete stored bodies.
Appendix C describes it fully enough to rebuild.

### The results

Measured. Tokens are the participant's whole consumption, its fixed prompt
overhead included.

| Path                                  | Score /72  | Wrong facts | Tokens  | Tool calls | Elapsed s | Protocol                                                      |
| ------------------------------------- | ---------- | ----------- | ------- | ---------- | --------- | ------------------------------------------------------------- |
| 1 filesystem search and reads (rerun) | 72         | 0           | 117,339 | 21         | 136.0     | Clean                                                         |
| 2 index lookup, then source reads     | 71         | 0           | 191,199 | 33         | 137.9     | One self-reported breach: piped the adapter's output via grep |
| 3 complete stored documents           | 71         | 0           | 156,494 | 42         | 248.7     | Clean                                                         |
| 1, first run (void)                   | not scored | -           | 258,208 | 65         | 246.4     | Search tools unavailable                                      |

Every deduction, all of them Q03's citation:

- **Path 2, Q03, citation 1.** It cited only the lesson for the skill file;
  the authoritative `delivery.md` came back as a file link and was not read.
- **Path 3, Q03, citation 1.** It named `delivery.md` from the lesson's link
  and could not read it: files outside the record kinds are not stored.

No path gave a wrong fact, invented ADR-0031, or repeated decision 0008's
stale statement about epic `0194`. Each path's answers, question by question,
are in Appendix B.

Observations, as the trial recorded them:

1. Every scored path answered every question correctly. No access path
   improved correctness or citation over filesystem search.
2. Path 1 used 61% of path 2's tokens and 75% of path 3's, and the fewest
   tool calls. Its elapsed time and path 2's are within two seconds, on one
   run each; path 3 took about 80% longer.
3. Without a search tool (the void run) path 1 cost the most and failed four
   questions. An inference: the search tool, not the file layout, carries
   path 1.
4. A store of complete documents covers only the records it ingests. The
   skill reference file Q03 needs was outside the record kinds and
   unreachable on path 3: a coverage gap by construction.
5. Path 2's inbound links answered Q05 and Q07 in few calls; path 1 answered
   each with one `rg`.

**The void first run.** Path 1's first participant reported Grep and Glob
unavailable in its session and the shell forbidden by its prompt, so it could
read only paths it already knew or guessed: 258,208 tokens, 65 tool calls,
246.4 s, with Q06, Q07, Q10 and Q11 unanswered or partial and Q01's status
inferred rather than read. Paths 2 and 3 each had a search command, so the
run was not comparable. It was declared void, kept unscored, and path 1 was
rerun with read-only shell search allowed (Appendix B).

**The answer-key correction on Q11.** The key first said `0001-F7` OQ-69
records the owner's ruling on ADR-0025, repeating the wording of spike
`0220`'s commit message. OQ-69 reads "Open. Owner not asked." Path 2's
participant rejected the premise; the key was corrected after its answer, and
all three scored paths were scored against the corrected key. Every path then
answered that OQ-69 is open.

**The duplicate item id `0220`.** At `51c00ffd` two items are numbered
`0220`: the rule page spike `.issue/0220-rule-page-versioning-spike.yml`
(added by `3ec5090b`, pull request #194) and the bug
`.performance/benchmarks/Startup/.issue/0220-cold-start-baseline-bundled-framework.yml`
(added by `2b48200c`, pull request #195), both merged on 2026-10-14.
ADR-0025 noted the pair in passing. Paths 1 and 2 reported it. The adapter
keys an item by the first four characters of its file name and writes each
row with insert-or-replace into a table whose key is the primary key, so the
second write silently replaced the first: the re-run above keeps the bug's
row, and the spike's file is in no row. Path 3 answered Q11 from ADR-0025 and
reported that the spike's own record "cannot be fetched on its own". A store
keyed by a record id loses a record exactly where the corpus breaks id
uniqueness; a path cannot collide.

Item `0128` ([`.issue/0128-detect-an-id-claimed-twice.yml`](../../.issue/0128-detect-an-id-claimed-twice.yml),
`blocked` until its specifications exist) is the existing item for this
defect: its case (b) reports two `.issue/` items sharing an id. Its decision
of 2026-10-10 says no id is claimed twice on trunk, which held then; the pair
above arrived four days later. This record fixes neither, renumbers nothing
(AGENTS.md § Stable IDs) and edits neither item.

### The storage candidates

Surveyed on 2026-11-01 from the vendor and project pages in Appendix D, by a
participant whose prompt is in Appendix E; "unverified" marks what no vendor
page confirmed. The survey's matrix is condensed here into two tables, one row
per candidate in each, every column the survey had kept.

**Reach, cost and licence.** Prices are a month unless stated, as read on
2026-11-01.

| Candidate                     | Offline                                                | Shared online sequence                                       | Concurrent access                                      | Local/online sync                                    | Cost                                                                 | Ops burden          | Licence                   |
| ----------------------------- | ------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------ | ---------------------------------------------------- | -------------------------------------------------------------------- | ------------------- | ------------------------- |
| SQLite, Microsoft.Data.Sqlite | Full, in process                                       | None; a local `UPDATE ... RETURNING` on one machine          | Processes on one host (WAL, one writer); no network FS | None                                                 | Free                                                                 | None                | MIT; SQLite public domain |
| LiteDB                        | Full, in process                                       | None                                                         | Shared mode by named mutexes, one host                 | None                                                 | Free                                                                 | None                | MIT                       |
| RavenDB Embedded              | Full, but starts a server process                      | None in embedded mode                                        | One embedded server per process                        | Replication to hosted (not checked)                  | Community licence free, renewed yearly, 3 cores and 6 GB             | High                | AGPLv3 or commercial      |
| DuckDB (DuckDB.NET)           | Full, in process                                       | None                                                         | One read-write process; others read-only               | None                                                 | Free                                                                 | None                | MIT                       |
| Turso / libSQL                | Local reads; writes go to the primary unless `offline` | Online only, on the primary; offline writes "last push wins" | Many machines through the primary                      | Embedded replicas (page level); Turso Sync (logical) | Free 5 GB, 10M rows written; Developer $4.99 as displayed            | Low                 | MIT                       |
| Azure Cosmos DB               | No; the emulator is for development only               | Patch `incr`, with a conditional predicate                   | Many machines                                          | None                                                 | Free tier 1000 RU/s and 25 GB; serverless price unverified           | Medium              | MIT (SDK)                 |
| Supabase (Postgres)           | No                                                     | `nextval`, atomic, with gaps                                 | Many machines                                          | None                                                 | Free 500 MB, paused after a week idle; Pro from $25                  | Low to medium       | Postgres, Npgsql          |
| Cloudflare D1                 | No                                                     | `UPDATE ... RETURNING`; `batch()` is one transaction         | Many machines, through Workers                         | None                                                 | Free 100k rows written a day; Workers Paid $5 minimum                | Medium (own Worker) | Proprietary               |
| MongoDB Atlas                 | No                                                     | `findOneAndUpdate` with `$inc`, atomic per document          | Many machines                                          | None (Device Sync not checked)                       | M0 free; Flex $8 to $30; M10 from $56.94; Serverless, M2, M5 retired | Low                 | Apache-2.0 (driver)       |
| A git remote ref (added)      | Reads the last fetched value                           | Compare-and-swap by push rejection or `--force-with-lease`   | Many machines                                          | The git transport                                    | Free on existing hosting                                             | Very low            | n/a                       |
| Neon (Postgres, added)        | No                                                     | `nextval`                                                    | Many machines                                          | None                                                 | Free 1 GB, suspends after 5 minutes idle; pay as you go              | Low                 | Postgres                  |
| Upstash Redis (added)         | No                                                     | `INCR` over REST (not re-read)                               | Many machines                                          | None                                                 | Free 500k commands; $0.20 per 100k or from $10                       | Low                 | n/a                       |

**Durability, migration, security and fit.**

| Candidate                     | Durability                              | Schema migration                                                       | Security and auth for a CLI                                                                               | .NET client and `dotnet tool` fit                                                                                      |
| ----------------------------- | --------------------------------------- | ---------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| SQLite, Microsoft.Data.Sqlite | ACID file; WAL mode recommended         | DDL with `PRAGMA user_version`, or drop and rebuild a disposable index | File permissions only                                                                                     | Official; 10.0.14 bundles e_sqlite3 with FTS5 on three OSes; SQLitePCLRaw 3.0 moves the native package; AOT unverified |
| LiteDB                        | Its own log file; ACID claim unverified | Schemaless                                                             | File only                                                                                                 | Community; stable 5.0.21 (2024-07-05), v6 prerelease since 2025; managed, no native binaries; AOT unverified           |
| RavenDB Embedded              | ACID document store                     | Schemaless                                                             | A licence key in the server options                                                                       | 229.88 MB package; needs a .NET runtime it does not bundle                                                             |
| DuckDB (DuckDB.NET)           | ACID                                    | DDL                                                                    | File only                                                                                                 | Community; 1.5.6.1 (2026-10-15) bundles the native library; RIDs and AOT unverified                                    |
| Turso / libSQL                | Managed cloud primary plus a local file | SQLite DDL                                                             | A database token passed as an environment variable                                                        | Official provider 0.8.3, pre-1.0; AOT packages for six RIDs; partial sync not on Windows; sync docs list no .NET       |
| Azure Cosmos DB               | Managed, replicated                     | Schemaless                                                             | An account key or role-based access (not re-read)                                                         | Official SDK 3.63.3 (2026-10-27); AOT unverified                                                                       |
| Supabase (Postgres)           | Managed Postgres                        | SQL migrations                                                         | A database password over Npgsql, or API keys; legacy keys deprecated by the end of 2026                   | Npgsql, AOT and trimming supported; the direct connection is IPv6 by default                                           |
| Cloudflare D1                 | Managed                                 | `wrangler` migrations (not re-read)                                    | An API token; the REST API is for administration, 1,200 requests in 5 minutes; a proxy Worker recommended | No .NET driver; HTTP only                                                                                              |
| MongoDB Atlas                 | Managed replica set                     | Schemaless                                                             | A connection string with a database user (not re-read)                                                    | Official driver 3.13.0 (2026-10-22); Native AOT "Won't Fix", trimming open                                             |
| A git remote ref              | As the git host                         | n/a                                                                    | The developer's existing git credentials                                                                  | No new dependency; shells out to git                                                                                   |
| Neon (Postgres)               | Managed                                 | SQL                                                                    | A database password                                                                                       | Npgsql, AOT-capable                                                                                                    |
| Upstash Redis                 | Managed                                 | n/a                                                                    | A REST token                                                                                              | HTTP, or StackExchange.Redis (not checked)                                                                             |

The survey's findings, condensed:

1. Microsoft.Data.Sqlite with FTS5 fits a disposable offline index best. Its
   open risks are SQLitePCLRaw 3.0's change of native package, which 10.0.x
   does not yet take, and AOT, which is unverified.
2. SQLite serves an MCP host and a command side by side on one host, not over
   a shared network drive.
3. RavenDB Embedded does not fit a `dotnet tool`: 230 MB, a server process, a
   runtime it does not bundle, and a licence renewed yearly.
4. DuckDB allows one read-write process and adds nothing over SQLite for about
   600 documents and keyword search.
5. No sync product gives an offline-safe shared counter; Turso Sync is "last
   push wins". The survey concludes that reserving an id needs the service,
   and that offline either fails or hands out a provisional id; the allocation
   analysis below rejects the provisional id.
6. Every hosted candidate gives a correct online counter in one atomic call;
   Postgres sequences leave gaps, acceptable because ids are never reused.
7. Turso is the only candidate with one official .NET package covering local
   file, remote and sync; it is pre-1.0, without partial sync on Windows.
8. A git ref as the sequence needs no new service, account or credential, and
   the survey suggests spiking it before any vendor. This record reads it as
   conflicting with epic `0324`'s requirement (owner, 2026-10-17) that
   allocation not depend on a git repository or a tracked counter file, and
   lists it for completeness only.
9. Recent changes: Atlas Serverless, M2 and M5 retired (support ended
   2026-01-22); Supabase legacy keys go by the end of 2026 and free projects
   pause after a week idle; D1's REST API is for administration and
   rate-limited; MongoDB.Driver will not support Native AOT.
10. .NET 10 packs RID-specific and AOT tool packages through
    `ToolPackageRuntimeIdentifiers`. Whether a portable framework-dependent
    tool package loads `runtimes/<rid>/native` assets such as `e_sqlite3` on
    all three OSes is unverified; a pack-and-install check settles it.

Durability, schema migration and security are the survey's columns above.
**CLI and MCP parity** was not among the survey's criteria and it has no such
column; what follows is this record's reasoning, not a survey finding. A
store is reached by the same tool through either door, so parity is a
property of the command design, not of the store; for a local store, finding
2 says both doors can hold it open at once, and a hosted store is reached the
same way from both.

### The allocation analysis

The requirement: concurrent allocations within one project and document-type
scope never return the same value, without git (epic `0324`, owner,
2026-10-17); if online mode is adopted, one sequence across machines (owner,
2026-10-20).

1. **Uniqueness across machines** needs one linearizable authority per
   project and document type: a single counter row advanced by an atomic
   increment-and-return. Every hosted candidate provides one (Postgres
   `nextval`, Cosmos Patch `incr`, Mongo `$inc`, D1 or Turso
   `UPDATE ... RETURNING` on the primary). Gaps are acceptable, because ids
   are never reused or renumbered.
2. **Independent local counters are not one sequence**, nor is any sync that
   merges by last writer (Turso Sync): two offline machines take the same
   value.
3. **An unreachable authority** leaves four options:
   - (a) fail closed: no allocation while offline; unique, and offline
     allocation is impossible;
   - (b) leased blocks: while online a machine leases a block of values, and
     offline it allocates only from its unexpired lease; unique; numbers are
     not chronological, unused values become gaps, and a lost machine strands
     its block;
   - (c) provisional local ids renamed on reconnect: violates "ids are never
     reused and never renumbered"; rejected;
   - (d) machine-partitioned ids: unique without coordination but not one
     sequence; spike `0256`'s directions, answered there, not here.
4. **A mode change.** Offline to online: the online counter is seeded once at
   the greater of the highest id in the tree and the highest local
   reservation, plus one, and recorded with the project identity; a seed lower
   than any issued value is refused. Online to offline: the local authority
   starts above the online high-water mark it last saw and every lease it
   holds; without a known high-water mark it refuses to allocate.
5. **A retry after an uncertain response** carries an idempotency key per
   request; the authority returns the same value for the same key.
6. **Project identity** must not come from git (owner, 2026-10-17). A
   generated project id recorded in the manifest is a write into the
   consumer's tree and needs the permitted-write list amended.
7. **What an online mode would amend**: "Deterministic and offline" (only
   `init` and `upgrade` may fetch), "Never a write into a consumer's tree"
   (reservation state and the project id), and CLI parity for the allocation
   command. Machine-local reservation state outside the tree needs its own
   permission in the owning records.

Two **inferences** this record adds. Returning to offline mode returns the
guarantee to one machine, the scope the owner resolved for offline mode on
2026-10-17: two machines that each start above the same high-water mark then
collide, as point 2 predicts. And for the machine-local offline authority
epic `0324` already requires, survey findings 1 and 2 make a SQLite file
outside the tree a candidate for reservations across worktrees and processes
on one host, not across a network drive; its durability was not read.

For item `0256`'s sixth criterion: this record states what that spike's
direction (f), online allocation from one shared sequence, requires, and
recommends against adopting it now. Whether (f) counts as answered here is for
`0256`'s own record to state.

### Limits

- One run per path: no variance estimate. One model, one agent type. A
  one-point difference in score is one citation.
- The access rules were instructions, not enforced; one breach was
  self-reported.
- The adapters are command-line scripts, not an MCP server: protocol overhead
  and the cost of tool descriptions are not measured. Each prompt carried its
  own adapter's instructions, which the token totals include.
- The question set, the answer key and the adapter were written by the same
  coordinator; the key held one error, found by a participant.
- One repository of 616 rows. A larger or multi-repository corpus, or
  questions needing many records at once, were not tested.
- The raw answers in Appendix E are model output. Nothing verified them
  beyond the scoring against the answer key.
- The survey is a reading of vendor pages on one day by one participant;
  CLI and MCP parity is this record's reasoning, not a survey column.
- No benchmark measured repeated-query cost, `0132-F1` OQ-3's question. This
  trial measured answer quality and agent cost, not lookup latency.

## Decision drivers

1. Answer quality is measured, not inferred from lookup speed (item `0252`).
2. Markdown stays authoritative unless a consumer opts into the service mode,
   and file-based use stays supported (`0132-F1` decision 0001 and C-1; epic
   `0324` § Constraints).
3. Every answer reflects the current tree (`0132-F1` C-8; `0132-F4` C-4).
4. Deterministic and offline: only `init` and `upgrade` may fetch, and the MCP
   host makes no outbound call and writes nothing (AGENTS.md § Invariants;
   `0132-F3` C-1, C-3).
5. Ids are never reused or renumbered, and allocation does not depend on git
   (AGENTS.md § Stable IDs; owner, 2026-10-17).
6. Persistent state needs a reason that exists today (`coding-conventions`
   § Design), and an index does not waive ADR-0014's gate (`0132-F1` C-7).

## Considered options

Two questions, ruled apart. The trial added no option beyond the item's three
paths; options (d) and (A) are the not-yet choices.

### Access and storage

**(a) Files stay the reading path; no index on answer-quality grounds.**
Markdown is read by filesystem search, as path 1 did, and decision 0001 stands.
`0132-F1` stays draft and proceeds only if OQ-3's benchmark shows repeated
lookups cost enough to justify persistent state. If an index is built, it
keys each record by its root-relative path; a stable id stays a query key
(C-5) that may resolve to more than one record, and a collision is reported,
not resolved. The candidate engine is Microsoft.Data.Sqlite with FTS5, in
state outside the tree, disposable and rebuilt.

- Evidence: path 1 scored highest with the fewest tokens and tool calls; the
  id-keyed store lost a record; the index path's inbound links saved no call
  path 1 needed.
- Cost: none now. An index, when OQ-3 justifies one, still meets `0132-F3` C-3
  and B-008.

**(b) Build `0132-F1`'s index now, in path 2's shape.**

- Evidence against: on this corpus it cost 63% more tokens and 12 more tool
  calls than path 1 for one point less, and its participant breached the
  access rule once to filter output.
- Cost: persistent state, freshness validation on every answer (C-8), and an
  amendment to `0132-F3` C-3 and B-008 for state the host writes. OQ-3's
  benchmark and ADR-0014's gate are still owed (C-7, § 7).

**(c) Serve complete stored documents, path 3's shape.**

- Evidence against: one point less than path 1, 33% more tokens, twice the
  tool calls and about 80% more elapsed time; it could not reach a file
  outside the record kinds it ingests (Q03), and it lost the spike numbered
  `0220`.
- Cost: as the answer path in file mode it makes database content a second
  copy of every document, which decision 0001 rejected and C-1 rules out. The
  opt-in service mode of epic `0324` asks for complete documents for other
  reasons - authority, lifetime keys, projection - and is not decided by this
  evidence.

**(d) Re-run before ruling.** Several runs per path, an MCP server in place of
the scripts, a larger or multi-repository corpus, a key written by someone
other than the adapter's author, raw answers and versions kept. Cost: another
trial; nothing waits on it but `0132-F1`, which waits on OQ-3 regardless.

### Allocation

**(A) No online allocation now.** Epic `0324` candidate F stays machine-local
and offline, the scope the owner resolved on 2026-10-17. No invariant changes.

**(B) Online allocation, one hosted counter per scope.** If adopted, it needs:

- one linearizable counter per project and document type, on a hosted
  candidate's atomic increment;
- fail closed by default when the authority is unreachable, with leased blocks
  as the opt-in offline option;
- an idempotency key on every allocation request;
- the mode-change seeding of allocation point 4, refusing a seed below an
  issued value;
- a project identity that does not come from git;
- the amendments listed under Consequences.

Machine-partitioned ids, the other way to avoid coordination, are spike
`0256`'s and are not an option here.

## Decision

Proposed. Each part is a judgement resting on the evidence above; the owner
decides. The recommended default is **(a) and (A)**.

- **Access and storage: (a) - judgement.** On this corpus no path improved
  correctness or citation over filesystem search, which cost least in tokens
  and tool calls. One run per path cannot show a small advantage, but it gives
  no answer-quality case for a persistent index at this size, and none for
  serving stored documents. `0132-F1` therefore should not proceed on
  retrieval grounds; its own ground, the cost of repeated lookups (B-003,
  OQ-3), is unmeasured and stays the test. Decision 0001 stands as written.
- **If an index is built - judgement.** Key records by path and report id
  collisions, because the trial showed an id-keyed store losing a record where
  ids collide, and the tree breaks id uniqueness today. Microsoft.Data.Sqlite
  with FTS5 is the candidate the survey favours; `0132-F1` § 5 row 6 keeps the
  engine unchosen until the query and freshness contracts are settled, and
  § 7 is where an implementer chooses it, after the pack-and-install check.
- **(b), (c) and (d) are not recommended - judgement.** (b) spends measured
  agent cost for no measured gain, before the evidence its own Feature asks
  for. (c) contradicts decision 0001 in file mode and has a coverage gap by
  construction. (d) is the choice for an owner who reads one run as too thin;
  it changes nothing `0132-F1` needs first.
- **Allocation: (A) - judgement.** Online allocation needs a hosted service,
  a credential on every machine and outbound calls from a tool whose invariant
  is offline, and amends three invariants and two Features' constraints, for a
  mode the owner raised conditionally. Spike `0256` and item `0128` are still
  weighing coordination-free and detecting answers to the same collisions. If
  the owner adopts it, (B)'s list is the minimum.

## Consequences

### Under the recommended default, (a) and (A)

No code changes, and nothing is amended: not decision 0001, not a claim, not
an invariant. The owner's ruling is recorded in `0132-F1` OQ-4 and in epic
`0324`'s online-mode question; after it, `spec-author` brings these
statements up to date, none made here:

- Epic `0132`: the "Questions for related records" row "Does an MCP index over
  authoritative files improve agent answers enough, or does serving complete
  database documents help more?" takes the answer; § Summary's "Spike `0252`
  will test" and the success-criteria row "spike `0252` will evaluate how
  their content is served" become past tense.
- `0132-F1`: OQ-4 resolved. OQ-2 gains an input: a first query covering
  record ids must say how a collision is reported. Nothing else changes; the
  § 2 note on the requested database authority stands for epic `0324`.
- Epic `0324`: the "What this epic waits on" row lists spike `0252` as
  `ready`; the online-mode question takes the answer. The machine-local
  authority's storage question gains the SQLite inference above as an input.
- Item `0256`: its sixth criterion can cite this record's allocation analysis.

### Under (b)

`0132-F1` proceeds as specified, still owing OQ-3's benchmark (§ 7, C-7) and
OQ-2. `0132-F3` C-3 ("the host writes nothing"), B-008 and its § 5 row 8 change
for index state the host writes outside the tree. Decision 0001 and the
invariants stand.

### Under (c)

A new decision answers `0132-F1` decision 0001, reversing its first rejected
option, as `0132-F1` § 2's requirements revision anticipates; `0132-F1` A-1,
B-001 to B-006, C-1, C-2, C-3 and C-8 are reconciled; epic `0132`'s constraint
"the index writes only derived state outside that tree" is revisited;
`0132-F4` C-4 holds only if the store is current with the tree. If the
database also projects files, AGENTS.md "Never a write into a consumer's
tree" is amended. That is epic `0324`'s service mode, which owns the question.

### Under (B)

Each is owed before implementation, and none is made here:

| Record                                              | Change owed                                                                                                                                   |
| --------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------- |
| AGENTS.md § Invariants, "Deterministic and offline" | The allocation command joins `init` and `upgrade` as a caller that may reach the network, its authority named; the check itself stays offline |
| AGENTS.md § Invariants, "Never a write ..."         | The project id in the manifest, and a narrow permission for reservation and lease state outside the tree                                      |
| `0001-F7` decision 0004, `0001-F4` decision 0003    | The fetch boundary AGENTS.md cites for "only `init` and `upgrade`"                                                                            |
| Epic `0132` § Constraints and § "Out of this epic"  | "makes no outbound call" and "Remote or hosted access ... Never", if allocation is reachable through MCP                                      |
| `0132-F3` C-1, C-3, B-004, B-008                    | Outbound calls and lease state, if the host serves allocation                                                                                 |
| Epic `0324` § Constraints and § "Out of this epic"  | The offline boundary row and "Cross-machine coordination is outside the current offline scope"                                                |
| CLI parity (`specht-conventions`, questions)        | An allocation command beside the MCP operation, its name still open (epic `0324`)                                                             |

Untouched by every option: Markdown as this repository's authoring format,
the check's verdict (`0132-F1` C-6), the rule vocabulary, and the two items
numbered `0220`, which stay as they are.

## Appendix A: the questions and the answer key

The corpus is the repository at `51c00ffd`. Each question is verbatim; the
required facts and the authoritative sources are the corrected key's.

**Rubric**, verbatim:

- correct: "0 wrong fact, 1 partly right, 2 every required fact right";
- complete: "share of required facts present, scored 0, 1 or 2 (all = 2, at
  least half = 1)";
- citation: "0 no source path, 1 a path that is not the authoritative
  record, 2 the authoritative record's path for every fact";
- stale: "1 when the answer repeats a statement the corpus itself
  contradicts, or invents a record that does not exist; else 0";
- six points a question at most, 72 in all.

**Q01** (item, Feature, epic). "Item 0287: which Feature specification does
it deliver against (give the Feature id and the specification path), which
epic owns that Feature, and what is the item's status?"
Facts: Feature `0001-F11`; `src/tool/Features/Rules/Set/.spec/README.md`;
epic `0001`; status done. Sources:
`src/tool/Features/Rules/Set/.issue/0287-set-rule-level-engine-edit.yml`,
`src/tool/Features/Rules/Set/.spec/README.md`.

**Q02** (ADR, item). "Which ADR records the evidence decision of spike item
0216, what did the owner rule, and which earlier ADR's clauses does it
supersede?"
Facts: ADR-0026; the owner ruled (a), do not adopt the mediator, direct calls
stay; it supersedes ADR-0010's mediator clauses. Source:
`.spec/adr/0026-the-source-generated-mediator-measured-no-benefit-and-direct-calls-stay.md`.

**Q03** (lesson, skill). "Which repository-wide lesson records that an id is
not reserved until it merges, and which skill reference file holds the rule
that lesson produced?"
Facts: lesson 0005; `.agents/skills/specht-conventions/references/delivery.md`.
Sources: `.spec/lessons/0005-an-id-is-not-reserved-until-it-merges.md`,
`.agents/skills/specht-conventions/references/delivery.md`.

**Q04** (decision, Feature). "What does decision 0001 of Feature 0132-F1
decide about the database and Markdown, and where is that Feature's
specification?"
Facts: Markdown remains authoritative; the database is a disposable local
index; answers reflect the current tree; `src/specht/Index/.spec/README.md`.
Source: `src/specht/Index/.spec/decisions/0001-markdown-authority-and-current-answers.md`.

**Q05** (Feature, epic, missing link). "Feature 0132-F4 cites Feature
0001-F18. Does a specification for 0001-F18 exist in the tree? If not, where
is its id reserved and how does 0132-F4 handle the missing dependency edge?"
Facts: no specification for `0001-F18` exists; its id is reserved in epic
`0132` (2026-10-30); `0132-F4` leaves it out of `depends_on`, and an
assumption records that the edge is added when the specification exists.
Sources: `epics/0132-model-context-protocol/epic.md`,
`src/tool/Features/Mcp/Reading/.spec/README.md`.

**Q06** (ADR, missing record). "What does ADR-0031 decide, and which Feature
does it constrain?"
Facts: ADR-0031 does not exist; the highest root ADR is ADR-0026. Source:
`.spec/adr/`.

**Q07** (epic, epic). "Epic 0324's candidate F was moved there from which
epic, and from which of that epic's candidates was it cut?"
Facts: epic `0132`; cut from candidates D and E. Source:
`epics/0324-specht-service/epic.md`.

**Q08** (Feature, Feature). "List every Feature that 0132-F4 depends on, as
qualified ids, and say which of them has status blocked."
Facts: `0132-F2`, `0132-F3`, `0001-F2`, `0001-F3`, `0001-F9`, `0001-F10`,
`0001-F13`; only `0001-F13` is blocked. Sources:
`src/tool/Features/Mcp/Reading/.spec/README.md`,
`src/tool/Features/Trace/.spec/README.md`.

**Q09** (decision, Feature). "What does decision 0005 of Feature 0001-F7
decide about how a schema version is numbered and pinned?"
Facts: `major.minor.patch`, numbered apart from the package version; the
first is `0.1.0` and an absent key means it; a pin is exact; it is moved by
the pin command. Source:
`src/tool/.spec/decisions/0005-a-schema-version-is-semver-pinned-exactly-moved-by-pin.md`.

**Q10** (decision, epic, stale link). "Decision 0008 of the work-item mirror
Feature (0055-F9) names epics that have no Summary section. Which epics does
it name, and is that statement still true for each of them at this commit?"
Facts: it names epics `0193` and `0194`; still true for `0193`; no longer
true for `0194`, which now has a Summary section. Sources:
`.github/mirror/.spec/decisions/0008-an-epic-issue-without-a-summary-or-status-label.md`,
`epics/0193-json-first-approach/epic.md`, `epics/0194-spectre-cli-ui/epic.md`.

**Q11** (item, ADR, Feature). "Spike item 0220 proposed which ADR, and which
open question in which Feature records the owner's ruling on it?"
Facts: ADR-0025 (proposed); OQ-69; Feature `0001-F7`
(`src/tool/.spec/README.md`); OQ-69 is open, owner not asked: no ruling is
recorded, so the question's premise is false. Sources:
`.spec/adr/0025-rule-pages-stay-whole-snapshots-and-single-source-is-the-candidate-if-growth-must-go.md`,
`src/tool/.spec/README.md`. Corrected after path 2's answer, as Context
records. An answer that notices the two items numbered `0220` is credited,
one that does not is not penalised.

**Q12** (lesson, decision). "Repository-wide lesson 0012 (the self-check
validates the working checker): which Feature decision does it point to as
the remedy?"
Facts: `0001-F2` decision 0014 (the self-check installs the working package).
Sources: `.spec/lessons/0012-self-check-validates-the-working-checker.md`,
`src/tool/Features/Check/.spec/decisions/0014-self-check-installs-the-working-package.md`.

## Appendix B: the agent setup and each path's answers

Every participant was a general-purpose agent of the same model, given the
same twelve question texts and the same output format - one JSON answer per
question with its facts and source paths - and told its access rule. All
three ran in parallel on 2026-11-01 against the tree at `51c00ffd`. Tokens,
tool calls and elapsed time are the harness's task notification for each.

| Path                       | Access rule                                                                                                                 |
| -------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| 1, rerun                   | Read, and read-only shell search inside the corpus: `rg`, `grep`, `find`, `ls`, `head`, `sed -n`, `cat`, `wc`               |
| 1, first run (void)        | Read, Grep and Glob inside the corpus; the shell forbidden. Grep and Glob were unavailable in that session, which left Read |
| 2 index, then source reads | The adapter in `index` mode to search, get and follow links, then Read on the paths it returns; no shell search             |
| 3 complete documents       | The adapter in `docs` mode only; no file reads                                                                              |

The answers, summarised per question from each participant's returned JSON:

| Q   | Path 1 (rerun)                                                                       | Path 2                                                                                      | Path 3                                                                                            |
| --- | ------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| Q01 | `0001-F11`, spec path, epic `0001`, done (closed 2026-10-28); cites item, spec       | `0001-F11`, spec path, epic `0001`, done; cites item, spec                                  | `0001-F11`, spec path, epic `0001`, done (closed 2026-10-28); cites item, spec, epic              |
| Q02 | ADR-0026; (a) don't adopt; supersedes in part ADR-0010's mediator clauses            | The same                                                                                    | The same                                                                                          |
| Q03 | Lesson 0005; `delivery.md`, read and cited                                           | Lesson 0005; `delivery.md` named; source: the lesson only                                   | Lesson 0005; `delivery.md` named from the lesson's link, "not stored in the service"              |
| Q04 | The decision's content; spec path; cites the decision                                | Markdown authoritative; disposable rebuildable local index; answers current; spec path      | Markdown authoritative, disposable index, current answers; spec path; cites the decision          |
| Q05 | No README with id F18 under epic `0001`; reserved in epic `0132`; `0132-F4` A-2      | No spec; reserved in epic `0132` on 2026-10-30; `0132-F4` A-2 leaves it out of `depends_on` | No spec; reserved in epic `0132` on 2026-10-30; A-2                                               |
| Q06 | Does not exist; root ADRs run 0001 to 0026, no Feature `adr/` holds 0031, none cites | Does not exist                                                                              | Does not exist                                                                                    |
| Q07 | Epic `0132`; D and E                                                                 | Epic `0132`; candidates D and E                                                             | Epic `0132`; D and E, with the reason                                                             |
| Q08 | The seven ids with statuses; only `0001-F13` blocked                                 | The seven ids; only `0001-F13` blocked                                                      | The seven ids with statuses; only `0001-F13` blocked; notes `0001-F18` cited, not in `depends_on` |
| Q09 | The full decision, its 2026-10-14 addenda included                                   | `major.minor.patch` apart from the package; `0.1.0` and absent key; exact pin; pin command  | The full decision, its 2026-10-14 addenda included                                                |
| Q10 | `0193` and `0194`; true for `0193`, no longer for `0194`                             | The same                                                                                    | The same                                                                                          |
| Q11 | ADR-0025 proposed; `0001-F7` OQ-69 open, owner not asked; notes the second `0220`    | Rejects the premise: ADR-0025 proposed, OQ-69 open; two items `0220`, the index keeps one   | ADR-0025 proposed; OQ-69 open; `item:0220` resolves to the bug; the spike "cannot be fetched"     |
| Q12 | `0001-F2` decision 0014                                                              | The same                                                                                    | The same                                                                                          |

The void first run's answers were kept unscored: Q06, Q07, Q10 and Q11
unanswered or partial, Q01's status inferred, not read.

## Appendix C: the disposable adapter

One command-line script, discarded after the trial, over a SQLite database
built from the corpus. It is described here so it can be rebuilt; the code is
not kept.

**Discovery.** Walk the corpus, skipping `.git`, `bin`, `obj` and
`node_modules`, and take:

| Kind     | File                      | Key                                                                                         |
| -------- | ------------------------- | ------------------------------------------------------------------------------------------- |
| item     | `<dir>/.issue/NNNN-*.yml` | `item:NNNN`, the file name's first four characters                                          |
| Feature  | `<dir>/.spec/README.md`   | `feature:<epic>-<id>` from frontmatter `epic` and `id`; else `feature:<dir>`                |
| epic     | `epics/<folder>/epic.md`  | `epic:NNNN`, the folder's first four characters                                             |
| ADR      | `adr/NNNN-*.md`           | `adr:NNNN` under the root `.spec/`; else `adr:<feature>/NNNN`, or `adr:<dir>/NNNN`          |
| decision | `decisions/NNNN-*.md`     | `decision:<feature>/NNNN`, or `decision:<dir>/NNNN`                                         |
| lesson   | `lessons/NNNN-*.md`       | `lesson:NNNN` under the root `.spec/`; else `lesson:<feature>/NNNN`, or `lesson:<dir>/NNNN` |

For each record it reads the frontmatter's scalar fields (`title`, `status`,
`description` or `summary`, `epic`) with a line reader, not a YAML parser.

**Links**, as `(source, relation, target)` rows, self-links dropped:

- an item's `parent` (an epic or an item), `spec` (when it names a
  discovered file), `depends_on` and `children` (when the item exists);
- a Feature's `depends_on`, `blocks` and `children` (a bare `F<n>` qualified
  by its own epic) and its `epic`;
- an epic's `children` given as `NNNN-F<n>`;
- a Feature-local ADR, decision or lesson's owning Feature;
- `mentions` from any body: `ADR-NNNN`, `NNNN-F<n>`, `lesson NNNN`, and a
  back-ticked four-digit number that is an item id;
- `links` from every relative Markdown link that stays inside the corpus, to
  the record at that path or, failing that, to `file:<path>`.

**Storage.** A `records` table keyed by the record key as primary key, with
kind, path, title, status, description and the whole body, written by insert
or replace; a `links` table; and an FTS5 table over key, title, description
and body.

**Commands**, each printing JSON:

- `rebuild`: rebuild and print the record and link counts.
- `index search <words>`: an FTS5 match over key, title and description only,
  the words joined by OR, ranked, 15 at most; returns key, kind, path, title
  and status.
- `docs search <words>`: the same match over every column, body included;
  returns key, kind, path, title and a snippet of the body.
- `<mode> get <key>`: key, kind, path, title, status and description, the
  outbound and inbound links with each target's path and status, and in
  `docs` mode the complete body. A key not found returns up to ten similar
  keys.
- `<mode> links <key>`: whether the key exists, and its outbound and inbound
  links, a target with no record marked missing.

## Appendix D: the survey's sources

Read on 2026-11-01, numbered as the survey numbered them:

1. https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/custom-versions
2. https://www.nuget.org/packages/Microsoft.Data.Sqlite
3. https://www.sqlite.org/wal.html
4. https://github.com/ericsink/SQLitePCL.raw/blob/main/v3.md
5. https://github.com/dotnet/efcore/issues/29725 (alpha-era trim warnings; current status unverified)
6. https://github.com/litedb-org/LiteDB/releases
7. https://www.litedb.org/docs/connection-string/
8. https://www.nuget.org/packages/LiteDB
9. https://docs.ravendb.net/7.1/server/embedded
10. https://www.nuget.org/packages/RavenDB.Embedded
11. https://ravendb.net/buy
12. https://github.com/ravendb/ravendb
13. https://duckdb.org/docs/2.0/connect/concurrency.html
14. https://www.nuget.org/packages/DuckDB.NET.Data.Full
15. https://duckdb.net/docs/getting-started.html
16. https://turso.tech/pricing
17. https://docs.turso.tech/features/embedded-replicas/introduction
18. https://docs.turso.tech/sync
19. https://docs.turso.tech/sync/conflict-resolution
20. https://github.com/tursodatabase/turso/tree/main/bindings/dotnet
21. https://www.nuget.org/packages/Turso.Data.Sqlite.Provider
22. https://docs.turso.tech/sync/usage
23. https://github.com/tvandinther/libsql-client-dotnet
24. https://learn.microsoft.com/en-us/azure/cosmos-db/partial-document-update
25. https://learn.microsoft.com/en-us/azure/cosmos-db/free-tier
26. https://learn.microsoft.com/en-us/azure/cosmos-db/emulator
27. https://azure.microsoft.com/en-us/pricing/details/cosmos-db/serverless/ (prices did not render)
28. https://www.nuget.org/packages/Microsoft.Azure.Cosmos
29. https://supabase.com/pricing
30. https://supabase.com/docs/guides/database/connecting-to-postgres
31. https://www.postgresql.org/docs/current/functions-sequence.html
32. https://supabase.com/docs/guides/getting-started/api-keys
33. https://www.npgsql.org/doc/release-notes/8.0.html
34. https://github.com/supabase-community/supabase-csharp
35. https://developers.cloudflare.com/d1/platform/pricing/
36. https://developers.cloudflare.com/d1/worker-api/d1-database/
37. https://developers.cloudflare.com/d1/tutorials/build-an-api-to-access-d1/
38. https://developers.cloudflare.com/cloudflare-for-platforms/workers-for-platforms/reference/limits/
39. https://developers.cloudflare.com/workers/platform/pricing/
40. https://www.mongodb.com/pricing
41. https://www.mongodb.com/docs/manual/reference/method/db.collection.findOneAndUpdate/
42. https://www.mongodb.com/docs/manual/core/write-operations-atomicity/
43. https://www.mongodb.com/docs/atlas/flex-migration/
44. https://www.nuget.org/packages/MongoDB.Driver
45. https://jira.mongodb.org/browse/CSHARP-4201 and https://jira.mongodb.org/browse/CSHARP-4840
46. https://git-scm.com/docs/git-push
47. https://neon.com/pricing
48. https://upstash.com/pricing/redis
49. https://learn.microsoft.com/en-us/dotnet/core/tools/rid-specific-tools

## Appendix E: the prompts and the raw answers

Each trial participant's prompt and final answer, verbatim, as the run logs
hold them. One substitution is made, so that no absolute path enters the
repository (AGENTS.md § Invariants): the corpus directory the prompts named
is written `<corpus>` and the adapter script's location `<adapter>`. The
question blocks of the four prompts are identical. The answers are model
output; Context records how they were scored.

### Path 1, first run - void, not scored

Void: Grep and Glob were unavailable in this session and the prompt forbade the shell, so it is not comparable with paths 2 and 3 (Context, "The void first run").

Prompt:

```text
You are a participant in a controlled retrieval trial. Answer twelve questions about a specification repository snapshot. Accuracy and source citations matter more than speed.

ACCESS RULES (path 1, filesystem) — follow exactly:
- The corpus is the directory <corpus> (a checkout of one commit). Read only inside it.
- Use only the Read, Grep and Glob tools on paths inside that directory. Do not use Bash, do not read anything outside the corpus directory, do not use the web, do not spawn agents, do not write any file.
- Answer only from what the corpus says. If a record does not exist, say so plainly; do not guess.

Record kinds in the corpus: work items (`**/.issue/NNNN-*.yml`), Feature specifications (`**/.spec/README.md`, frontmatter `id` like F11 and `epic` like 0001, cited as 0001-F11), epics (`epics/NNNN-*/epic.md`), ADRs (`.spec/adr/NNNN-*.md`), lessons (`.spec/lessons/` and a Feature's `lessons/`), and decisions (a Feature's `.spec/decisions/NNNN-*.md`, cited as `<feature> decision NNNN`).

QUESTIONS
Q01 Item 0287: which Feature specification does it deliver against (give the Feature id and the specification path), which epic owns that Feature, and what is the item's status?
Q02 Which ADR records the evidence decision of spike item 0216, what did the owner rule, and which earlier ADR's clauses does it supersede?
Q03 Which repository-wide lesson records that an id is not reserved until it merges, and which skill reference file holds the rule that lesson produced?
Q04 What does decision 0001 of Feature 0132-F1 decide about the database and Markdown, and where is that Feature's specification?
Q05 Feature 0132-F4 cites Feature 0001-F18. Does a specification for 0001-F18 exist in the tree? If not, where is its id reserved and how does 0132-F4 handle the missing dependency edge?
Q06 What does ADR-0031 decide, and which Feature does it constrain?
Q07 Epic 0324's candidate F was moved there from which epic, and from which of that epic's candidates was it cut?
Q08 List every Feature that 0132-F4 depends on, as qualified ids, and say which of them has status blocked.
Q09 What does decision 0005 of Feature 0001-F7 decide about how a schema version is numbered and pinned?
Q10 Decision 0008 of the work-item mirror Feature (0055-F9) names epics that have no Summary section. Which epics does it name, and is that statement still true for each of them at this commit?
Q11 Spike item 0220 proposed which ADR, and which open question in which Feature records the owner's ruling on it?
Q12 Repository-wide lesson 0012 (the self-check validates the working checker): which Feature decision does it point to as the remedy?

OUTPUT: reply with only a JSON object, no prose around it:
{"path": "filesystem", "answers": {"Q01": {"answer": "...", "sources": ["corpus-relative path", "..."], "confidence": "high|medium|low"}, ...}}
Sources are corpus-relative paths of the files that state each fact.
```

Raw answer:

```text
{"path": "filesystem", "note": "Grep and Glob were not available in this session; only Read worked, and the rules forbid Bash. I could not list directories, so I found files only by following links inside files I had already read, plus a few guesses at likely paths. Some records (ADR-0031, the epic files, item 0287's file, several decision files) could not be located, and the answers say so.", "answers": {
"Q01": {"answer": "Item 0287 delivers against 0001-F11, 'Setting a rule's level' (the set command, `specht rules set <id> <level>`). The specification is at src/tool/Features/Rules/Set/.spec/README.md, with frontmatter id F11 and epic 0001, so epic 0001 owns it. Its Tasks section says 0287 is 'the engine edits one rule's level in the manifest' and delivers B-001, B-003, B-004, B-005 and B-012. I could not locate the item file itself (its slug is unknown and there is no Glob), so the status is not read from the item. AGENTS.md says the set command 'is built (0001-F11, items 0287 and 0288)', and every row of the spec's § 9 is Covered, which suggests status done. That is inferred, not read from the item.", "sources": ["src/tool/Features/Rules/Set/.spec/README.md", "AGENTS.md"], "confidence": "medium"},
"Q02": {"answer": "ADR-0026, 'The source-generated mediator measured no benefit; direct calls stay', records the evidence decision of spike 0216. The owner ruled on 2026-10-31, in the words '(a) Don't adopt': martinothamar/Mediator is not adopted, ADR-0014's gate is not met for the mediator, and hosts keep direct calls through the existing composition, with no mediator, dispatcher or handwritten substitute. It supersedes in part ADR-0010 (layered engine boundaries with IO at the edge and a mediator), namely its mediator clauses. ADR-0010's other stages stay gated by ADR-0014.", "sources": [".spec/adr/0026-the-source-generated-mediator-measured-no-benefit-and-direct-calls-stay.md", ".issue/0216-evaluate-source-generated-mediator.yml"], "confidence": "high"},
"Q03": {"answer": "Lesson 0005, 'An id is not reserved until it merges' (.spec/lessons/0005-an-id-is-not-reserved-until-it-merges.md). The rule it produced, the section 'Reserve an id before a branch claims it', is in .agents/skills/specht-conventions/references/delivery.md.", "sources": [".spec/lessons/0005-an-id-is-not-reserved-until-it-merges.md", ".agents/skills/specht-conventions/references/delivery.md"], "confidence": "high"},
"Q04": {"answer": "0132-F1, 'Persistent specification index', is specified at src/specht/Index/.spec/README.md. As that spec cites it, decision 0001 says Markdown in the repository stays authoritative and the database is only a derived index that can be discarded and rebuilt (A-1, C-1). It also says every MCP answer must reflect the current tree, including direct file edits and branch switches, rather than an indexed snapshot (OQ-1, resolved by the owner on 2026-10-19 by decision 0001; C-8). I could not open the decision file itself because its file name is unknown. A 2026-10-30 requirements-revision note asks that decision 0001 be reconciled with an optional database-authority mode.", "sources": ["src/specht/Index/.spec/README.md"], "confidence": "medium"},
"Q05": {"answer": "0132-F4 says 0001-F18 is 'reserved and not yet specified (owner, 2026-10-30)', and I found no specification for it. I could not list the tree to rule one out completely. F4 says the reservation is epic 0001's ('epic `0001`'s Feature `0001-F18`'); I could not open epic 0001's epic.md to confirm where the id is recorded. F4 handles the missing edge this way: B-008 (schema list --json) and B-009 (the frontmatter-schema and template printer) cite 0001-F18 as reserved, § 5 row 7 names it, and A-2 says `depends_on` gains it once its specification exists. Its current depends_on leaves it out.", "sources": ["src/tool/Features/Mcp/Reading/.spec/README.md"], "confidence": "medium"},
"Q06": {"answer": "Not found. I could not locate ADR-0031: no file I read links to it, and without Glob I cannot list .spec/adr/. I cannot say what it decides or which Feature it constrains.", "sources": [], "confidence": "low"},
"Q07": {"answer": "Epic 0324's candidate F (document-number reservation state, allocation and its separate authority) was moved there from epic 0132 on 2026-10-30, according to 0132-F1 § 5 row 3. I could not open the epic 0324 or epic 0132 files, so I cannot say which of epic 0132's candidates it was cut from.", "sources": ["src/specht/Index/.spec/README.md"], "confidence": "low"},
"Q08": {"answer": "0132-F4's depends_on lists: 0132-F2 (the facts command; status ready), 0132-F3 (the MCP host; ready), 0001-F2 (the check command; ready-for-architecture), 0001-F3 (the report contract; ready-for-architecture), 0001-F9 (the manifest command; ready-for-architecture), 0001-F10 (the rules command; ready-for-architecture) and 0001-F13 (the trace command; blocked). Only 0001-F13 has status blocked.", "sources": ["src/tool/Features/Mcp/Reading/.spec/README.md", "src/tool/Features/Facts/.spec/README.md", "src/tool/Features/Mcp/.spec/README.md", "src/tool/Features/Check/.spec/README.md", "src/specht/Report/.spec/README.md", "src/tool/Features/Manifest/.spec/README.md", "src/tool/Features/Rules/.spec/README.md", "src/tool/Features/Trace/.spec/README.md"], "confidence": "high"},
"Q09": {"answer": "I could not open the decision file itself (its name is unknown), so this is how the 0001-F7 specification cites it. Decision 0005 (2026-10-09) makes schemaVersion a major.minor.patch semantic version, replacing the integer. A manifest with no key means 0.1.0. A version is numbered by its verdict: a patch changes no tree's verdict; from 1.0.0 on a minor only accepts more and anything that can fail a passing tree (a new SPEC### included) is a major; before 1.0.0 a 0.minor step may fail a passing tree (C-11, as amended by decision 0007). The schema version is numbered apart from the specht.tool package version (C-12). A pin is exact unless a pin policy says otherwise; only two policies are named, latest-patch and latest-minor (§ 5 rows 8 and 9). `specht schema pin` moves the pin to any shipped version, up or down, and writes nothing but schemaVersion (C-13). `upgrade` no longer moves the pin: C-3 (one version at a time) is retired, B-008 is withdrawn, and the separate downgrade command is dropped. `upgrade` brings the files to the pin.", "sources": ["src/tool/.spec/README.md", "AGENTS.md"], "confidence": "medium"},
"Q10": {"answer": "Per the 0055-F9 specification (.github/mirror/.spec/README.md), decision 0008 names epics 0193 and 0194 as having no Summary section; B-047 was amended and B-051 added for them. I could not open the decision file or the two epic files, so I could not check whether either still lacks a Summary at this commit. One hint, not verification: the snapshot's recent history includes a change that 'writes epic 0194 out', so 0194 may now have one.", "sources": [".github/mirror/.spec/README.md"], "confidence": "low"},
"Q11": {"answer": "Partly answered. Item 0220 (.issue/0220-rule-page-versioning-spike.yml, a spike, status done, closed 2026-10-31) asks for a proposed ADR in the repository-wide .spec/adr/ with the owner as decider. The item text I read does not name the ADR number, and I could not find the ADR or the open question that records the owner's ruling. A second file also claims id 0220: .performance/benchmarks/Startup/.issue/0220-cold-start-baseline-bundled-framework.yml.", "sources": [".issue/0220-rule-page-versioning-spike.yml", ".github/mirror/.spec/README.md"], "confidence": "low"},
"Q12": {"answer": "Lesson 0012 (.spec/lessons/0012-self-check-validates-the-working-checker.md) points to 0001-F2 decision 0014, which the lesson calls 'Check decision 0014'. That decision keeps the installed-package check but prepares the package from the working checkout in an isolated local manifest; B-014 is amended and B-046 and B-047 are added.", "sources": [".spec/lessons/0012-self-check-validates-the-working-checker.md", "src/tool/Features/Check/.spec/README.md"], "confidence": "high"}
}}
```

### Path 1, rerun - filesystem search and reads

Prompt:

```text
You are a participant in a controlled retrieval trial. Answer twelve questions about a specification repository snapshot. Accuracy and source citations matter more than speed.

ACCESS RULES (path 1, filesystem search and reads) — follow exactly:
- The corpus is the directory <corpus> (a checkout of one commit). Read only inside it.
- Use the Read tool, and the Bash tool for read-only search and listing only: `rg`, `grep`, `find`, `ls`, `head`, `sed -n`, `cat`, `wc` — always with paths inside the corpus directory. If Grep or Glob tools are available you may use them on the corpus too.
- Do not write, move or delete anything; do not read anything outside the corpus directory (in particular nothing else under .artifacts/); do not use git, the web, or other agents.
- Answer only from what the corpus says. If a record does not exist, say so plainly; do not guess.

Record kinds in the corpus: work items (`**/.issue/NNNN-*.yml`), Feature specifications (`**/.spec/README.md`, frontmatter `id` like F11 and `epic` like 0001, cited as 0001-F11), epics (`epics/NNNN-*/epic.md`), ADRs (`.spec/adr/NNNN-*.md`), lessons (`.spec/lessons/` and a Feature's `lessons/`), and decisions (a Feature's `.spec/decisions/NNNN-*.md`, cited as `<feature> decision NNNN`).

QUESTIONS
Q01 Item 0287: which Feature specification does it deliver against (give the Feature id and the specification path), which epic owns that Feature, and what is the item's status?
Q02 Which ADR records the evidence decision of spike item 0216, what did the owner rule, and which earlier ADR's clauses does it supersede?
Q03 Which repository-wide lesson records that an id is not reserved until it merges, and which skill reference file holds the rule that lesson produced?
Q04 What does decision 0001 of Feature 0132-F1 decide about the database and Markdown, and where is that Feature's specification?
Q05 Feature 0132-F4 cites Feature 0001-F18. Does a specification for 0001-F18 exist in the tree? If not, where is its id reserved and how does 0132-F4 handle the missing dependency edge?
Q06 What does ADR-0031 decide, and which Feature does it constrain?
Q07 Epic 0324's candidate F was moved there from which epic, and from which of that epic's candidates was it cut?
Q08 List every Feature that 0132-F4 depends on, as qualified ids, and say which of them has status blocked.
Q09 What does decision 0005 of Feature 0001-F7 decide about how a schema version is numbered and pinned?
Q10 Decision 0008 of the work-item mirror Feature (0055-F9) names epics that have no Summary section. Which epics does it name, and is that statement still true for each of them at this commit?
Q11 Spike item 0220 proposed which ADR, and which open question in which Feature records the owner's ruling on it?
Q12 Repository-wide lesson 0012 (the self-check validates the working checker): which Feature decision does it point to as the remedy?

OUTPUT: reply with only a JSON object, no prose around it:
{"path": "filesystem", "answers": {"Q01": {"answer": "...", "sources": ["corpus-relative path", "..."], "confidence": "high|medium|low"}, ...}}
Sources are corpus-relative paths of the files that state each fact.
```

Raw answer:

```text
{"path": "filesystem", "answers": {
"Q01": {"answer": "Item 0287 ('Edit one rule level in the manifest bytes', a task) delivers against Feature 0001-F11, 'Setting a rule's level'. Its spec is src/tool/Features/Rules/Set/.spec/README.md, whose frontmatter has id F11 and epic 0001. Epic 0001 owns the Feature, and the item's parent is also 0001. The item's status is done, closed 2026-10-28. It covers claims B-001, B-003, B-004, B-005 and B-012.", "sources": ["src/tool/Features/Rules/Set/.issue/0287-set-rule-level-engine-edit.yml", "src/tool/Features/Rules/Set/.spec/README.md"], "confidence": "high"},
"Q02": {"answer": "ADR-0026 records it: 'The source-generated mediator measured no benefit; direct calls stay'. Its status is accepted, 2026-10-31. The owner ruled '(a) Don't adopt': martinothamar/Mediator is not adopted, ADR-0014's benchmark gate is not met for the mediator, and hosts keep direct calls through the existing composition with no mediator, dispatcher or handwritten substitute. It supersedes, in part, ADR-0010's mediator clauses. ADR-0010's other stages stay gated by ADR-0014.", "sources": [".spec/adr/0026-the-source-generated-mediator-measured-no-benefit-and-direct-calls-stay.md", ".issue/0216-evaluate-source-generated-mediator.yml"], "confidence": "high"},
"Q03": {"answer": "Lesson 0005, 'An id is not reserved until it merges' (a process lesson dated 2026-10-09). The rule it produced, 'Reserve an id before a branch claims it', is in .agents/skills/specht-conventions/references/delivery.md (specht-conventions § Delivery). The lesson also notes one-sentence additions to the spec-and-traceability and deliver-change skills.", "sources": [".spec/lessons/0005-an-id-is-not-reserved-until-it-merges.md", ".agents/skills/specht-conventions/references/delivery.md"], "confidence": "high"},
"Q04": {"answer": "0132-F1 decision 0001, 'Markdown remains authoritative and indexed answers stay current' (owner, 2026-10-19). Markdown in the repository is the source of truth. The database is only a rebuildable, disposable index of its files and facts. Every indexed MCP answer must reflect the current tree, including after a direct Markdown edit or a branch switch. The index may read source files to establish that answer; the aim is to avoid reparsing unchanged Markdown for repeated keyed lookups. Rejected: database records as the authoritative documents, and a named snapshot that refreshes only on request. The Feature's specification is src/specht/Index/.spec/README.md (id F1, epic 0132, status blocked).", "sources": ["src/specht/Index/.spec/decisions/0001-markdown-authority-and-current-answers.md", "src/specht/Index/.spec/README.md"], "confidence": "high"},
"Q05": {"answer": "No specification for 0001-F18 exists in the tree: no README.md has id F18 under epic 0001. Its id was reserved by the owner on 2026-10-30 in epic 0132's epic.md. That record calls it a new epic-0001 Feature that prints the frontmatter schemas and templates, with `specht schema list --json`, 'not yet specified'. 0132-F4 handles the gap in assumption A-2: it cites 0001-F18 as reserved and not yet specified, and says `depends_on` gains it once its specification exists. Claims B-008 and B-009 cite it ('0001-F18, reserved; A-2'), and 0001-F18 is left out of 0132-F4's frontmatter depends_on.", "sources": ["src/tool/Features/Mcp/Reading/.spec/README.md", "epics/0132-model-context-protocol/epic.md"], "confidence": "high"},
"Q06": {"answer": "ADR-0031 does not exist in the corpus. The repository-wide .spec/adr/ runs only from 0001 to 0026, no Feature adr/ folder holds a 0031, and no file cites 'ADR-0031'. So nothing records what it decides or which Feature it constrains.", "sources": [".spec/adr/"], "confidence": "high"},
"Q07": {"answer": "Epic 0324's candidate F (Document sequence allocation) moved there from epic 0132 on 2026-10-30, together with the service mode, by the owner's decision. It keeps the letter it had in 0132. It was cut from epic 0132's candidates D and E.", "sources": ["epics/0324-specht-service/epic.md", "epics/0132-model-context-protocol/epic.md"], "confidence": "high"},
"Q08": {"answer": "0132-F4's frontmatter depends_on is [F2, F3, 0001/F2, 0001/F3, 0001/F9, 0001/F10, 0001/F13]. As qualified ids:\n- 0132-F2 (src/tool/Features/Facts, status ready)\n- 0132-F3 (src/tool/Features/Mcp, ready)\n- 0001-F2 (Check, ready-for-architecture)\n- 0001-F3 (src/specht/Report, ready-for-architecture)\n- 0001-F9 (Manifest, ready-for-architecture)\n- 0001-F10 (Rules, ready-for-architecture)\n- 0001-F13 (Trace, blocked)\nOnly 0001-F13 has status blocked. 0001-F18 is cited but not in depends_on.", "sources": ["src/tool/Features/Mcp/Reading/.spec/README.md", "src/tool/Features/Facts/.spec/README.md", "src/tool/Features/Mcp/.spec/README.md", "src/tool/Features/Check/.spec/README.md", "src/specht/Report/.spec/README.md", "src/tool/Features/Manifest/.spec/README.md", "src/tool/Features/Rules/.spec/README.md", "src/tool/Features/Trace/.spec/README.md"], "confidence": "high"},
"Q09": {"answer": "0001-F7 decision 0005, 'a schema version is semver, pinned exactly, moved by pin' (owner, 2026-10-09):\n- A schema version is major.minor.patch, numbered on its own and not from the specht.tool package version; one tool ships many schema versions.\n- The first is 0.1.0, and a manifest with no schemaVersion key means 0.1.0.\n- The bump follows the verdict: patch means no tree's verdict changes; minor means it accepts more and no passing tree fails; major means a passing tree can fail, including any new SPEC###.\n- A pin is exact by default. A pin policy (latest-patch or latest-minor) may let it float; that is a Could.\n- One command lists the shipped versions. A pin command moves to any listed version, up or down, and changes only schemaVersion. `upgrade` then rewrites the schemas and templates to match the pinned version.\nDropped: upgrade moving one version at a time, and the ban on moving down. Addenda dated 2026-10-14 name the command `specht schema pin <version>` and require an interactive Spectre selection prompt when no version is given.", "sources": ["src/tool/.spec/decisions/0005-a-schema-version-is-semver-pinned-exactly-moved-by-pin.md"], "confidence": "high"},
"Q10": {"answer": "Decision 0008 names epics 0193 and 0194 as having no Summary section. At this commit that is still true only for 0193: epics/0193-json-first-approach/epic.md (15 lines) has just its title heading. It is no longer true for 0194: epics/0194-spectre-cli-ui/epic.md now has a '## Summary' section at line 17.", "sources": [".github/mirror/.spec/decisions/0008-an-epic-issue-without-a-summary-or-status-label.md", "epics/0193-json-first-approach/epic.md", "epics/0194-spectre-cli-ui/epic.md"], "confidence": "high"},
"Q11": {"answer": "Spike item 0220 (rule-page versioning) proposed ADR-0025, 'Rule pages stay whole snapshots; single source is the candidate if growth must go'. ADR-0025 is still 'proposed - 2026-10-31'. The question it leaves the owner is 0001-F7 (Schema versioning) OQ-69. But OQ-69 records no ruling: it reads 'Open. Owner not asked. Proposed default: (a)'. No owner ruling on ADR-0025 exists in the corpus. A second, unrelated item 0220 (a cold-start benchmark bug) exists under .performance/benchmarks/Startup/.issue/.", "sources": [".issue/0220-rule-page-versioning-spike.yml", ".spec/adr/0025-rule-pages-stay-whole-snapshots-and-single-source-is-the-candidate-if-growth-must-go.md", "src/tool/.spec/README.md"], "confidence": "high"},
"Q12": {"answer": "Lesson 0012's Spec delta points to 'Check decision 0014': 0001-F2 (the check command) decision 0014, 'Self-check installs the working package'. The self-check builds and packs the working checkout's tool and installs it through an isolated local manifest under .artifacts/self-check/. Consumer installation stays separately pinned. The lesson also cites B-014 (amended), B-046 and B-047.", "sources": [".spec/lessons/0012-self-check-validates-the-working-checker.md", "src/tool/Features/Check/.spec/decisions/0014-self-check-installs-the-working-package.md"], "confidence": "high"}
}}
```

### Path 2 - index, then source reads

Prompt:

```text
You are a participant in a controlled retrieval trial. Answer twelve questions about a specification repository snapshot. Accuracy and source citations matter more than speed.

ACCESS RULES (path 2, key and relationship lookup, then source reads) — follow exactly:
- Locate records and their relationships ONLY through this lookup service, run with the Bash tool:
  `python3 <adapter> index search <words...>` — ranked matches on key, title and description (no bodies)
  `python3 <adapter> index get <key>` — one record's metadata, path, and outbound and inbound links (no body)
  `python3 <adapter> index links <key>` — links only
  Keys look like: item:0287, feature:0001-F11, epic:0132, adr:0026, decision:0132-F1/0001, lesson:0005. A link to a non-record file appears as file:<path>. A missing target is marked "missing": true.
- Then read the source files the service returns with the Read tool, at <corpus>/<path>. You may Read only paths the service returned.
- Do not use Grep, Glob, find, rg, ls, cat or any other Bash command besides the records.py lines above. Do not read anything outside the corpus directory, do not use the web, do not spawn agents, do not write any file.
- Answer only from what the corpus says. If a record does not exist, say so plainly; do not guess.

Record kinds: work items, Feature specifications (cited as 0001-F11), epics, ADRs, lessons (repository-wide or a Feature's), and decisions (cited as `<feature> decision NNNN`).

QUESTIONS
Q01 Item 0287: which Feature specification does it deliver against (give the Feature id and the specification path), which epic owns that Feature, and what is the item's status?
Q02 Which ADR records the evidence decision of spike item 0216, what did the owner rule, and which earlier ADR's clauses does it supersede?
Q03 Which repository-wide lesson records that an id is not reserved until it merges, and which skill reference file holds the rule that lesson produced?
Q04 What does decision 0001 of Feature 0132-F1 decide about the database and Markdown, and where is that Feature's specification?
Q05 Feature 0132-F4 cites Feature 0001-F18. Does a specification for 0001-F18 exist in the tree? If not, where is its id reserved and how does 0132-F4 handle the missing dependency edge?
Q06 What does ADR-0031 decide, and which Feature does it constrain?
Q07 Epic 0324's candidate F was moved there from which epic, and from which of that epic's candidates was it cut?
Q08 List every Feature that 0132-F4 depends on, as qualified ids, and say which of them has status blocked.
Q09 What does decision 0005 of Feature 0001-F7 decide about how a schema version is numbered and pinned?
Q10 Decision 0008 of the work-item mirror Feature (0055-F9) names epics that have no Summary section. Which epics does it name, and is that statement still true for each of them at this commit?
Q11 Spike item 0220 proposed which ADR, and which open question in which Feature records the owner's ruling on it?
Q12 Repository-wide lesson 0012 (the self-check validates the working checker): which Feature decision does it point to as the remedy?

OUTPUT: reply with only a JSON object, no prose around it:
{"path": "index", "answers": {"Q01": {"answer": "...", "sources": ["corpus-relative path", "..."], "confidence": "high|medium|low"}, ...}}
Sources are corpus-relative paths of the files that state each fact.
```

Raw answer:

```text
{"path": "index", "answers": {
"Q01": {"answer": "Item 0287 ('Edit one rule level in the manifest bytes') delivers against Feature 0001-F11 ('Setting a rule's level'). Its specification is at src/tool/Features/Rules/Set/.spec/README.md. Epic 0001 owns the Feature: the spec frontmatter says epic: \"0001\" and the item says parent: \"0001\". The epic file is epics/0001-specht/epic.md. The item's status is done, closed 2026-10-28. Claims: B-001, B-003, B-004, B-005, B-012.", "sources": ["src/tool/Features/Rules/Set/.issue/0287-set-rule-level-engine-edit.yml", "src/tool/Features/Rules/Set/.spec/README.md"], "confidence": "high"},
"Q02": {"answer": "ADR-0026 ('The source-generated mediator measured no benefit; direct calls stay') records item 0216's evidence decision. It is accepted, dated 2026-10-31. The owner ruled option (a), in the words \"(a) Don't adopt\": do not adopt martinothamar/Mediator. ADR-0014's gate is not met for the mediator, and hosts keep direct calls through the existing composition. It supersedes in part ADR-0010, specifically ADR-0010's mediator clauses (owner decision 2 and the related layout, request, pattern, interface, no-static and stage-7/8 clauses). ADR-0010's other stages stay gated by ADR-0014.", "sources": [".spec/adr/0026-the-source-generated-mediator-measured-no-benefit-and-direct-calls-stay.md"], "confidence": "high"},
"Q03": {"answer": "Repository-wide lesson 0005, 'An id is not reserved until it merges', records it. Its rule, 'Reserve an id before a branch claims it', went into the specht-conventions skill's Delivery reference, .agents/skills/specht-conventions/references/delivery.md. The lesson also adds one portable sentence each to spec-and-traceability and deliver-change.", "sources": [".spec/lessons/0005-an-id-is-not-reserved-until-it-merges.md"], "confidence": "high"},
"Q04": {"answer": "0132-F1 decision 0001, 'Markdown remains authoritative and indexed answers stay current' (2026-10-19, the owner). Markdown in the repository is the source of truth. The database is a disposable, rebuildable local index of its files and facts. Every indexed MCP answer must reflect the current tree, including after a direct Markdown edit or a branch switch. Rejected options: database records as the authoritative documents, and a named snapshot kept until an explicit refresh. The Feature's specification is src/specht/Index/.spec/README.md.", "sources": ["src/specht/Index/.spec/decisions/0001-markdown-authority-and-current-answers.md", "epics/0132-model-context-protocol/epic.md"], "confidence": "high"},
"Q05": {"answer": "No specification for 0001-F18 exists in the tree. The lookup reports it as missing, and epic 0132 says it is 'not yet specified'. The owner reserved the id on 2026-10-30 in epic 0132's epic file (epics/0132-model-context-protocol/epic.md, § Features and § Status) for a new epic 0001 Feature that prints the frontmatter schemas and the templates, together with `specht schema list --json`. 0132-F4 handles the missing edge in assumption A-2: it cites 0001-F18 as 'reserved' from B-008 and B-009 but leaves it out of depends_on, which 'gains it when its specification exists'.", "sources": ["epics/0132-model-context-protocol/epic.md", "src/tool/Features/Mcp/Reading/.spec/README.md"], "confidence": "high"},
"Q06": {"answer": "No ADR-0031 exists in the corpus: the lookup returns not found and a search finds nothing similar. I cannot say what it decides or which Feature it constrains.", "sources": [], "confidence": "high"},
"Q07": {"answer": "Candidate F (Document sequence allocation) moved to epic 0324 from epic 0132 on 2026-10-30, by the owner's decision, together with the service mode. Within epic 0132 it was cut from candidates D (Writing through MCP) and E (The work queue).", "sources": ["epics/0324-specht-service/epic.md", "epics/0132-model-context-protocol/epic.md"], "confidence": "high"},
"Q08": {"answer": "0132-F4 depends on 0132-F2, 0132-F3, 0001-F2, 0001-F3, 0001-F9, 0001-F10 and 0001-F13. Of these, only 0001-F13 (the trace command, src/tool/Features/Trace/.spec/README.md) has status blocked; its spec_status is approved. 0132-F2 and 0132-F3 are ready, and 0001-F2, 0001-F3, 0001-F9 and 0001-F10 are ready-for-architecture.", "sources": ["src/tool/Features/Mcp/Reading/.spec/README.md", "src/tool/Features/Trace/.spec/README.md"], "confidence": "high"},
"Q09": {"answer": "0001-F7 decision 0005 (2026-10-09, the owner): a schema version is major.minor.patch, numbered on its own and apart from the specht.tool package version, and one tool ships many schema versions. The first is 0.1.0, and a manifest with no schemaVersion key means 0.1.0. The bump follows the verdict: a patch changes no tree's verdict, a minor accepts more and fails no tree that passed, and a major can fail a tree that passed, including any new SPEC###. A pin is exact by default; a pin policy (latest-patch or latest-minor) could let it float, as a Could. One command lists the shipped versions. Another pins any listed version, up or down, and changes only schemaVersion. upgrade rewrites the schemas and templates to match the pin, not the next version. The 2026-10-14 addenda name the command `specht schema pin <version>` and make the interactive Spectre selection prompt required when no version is given.", "sources": ["src/tool/.spec/decisions/0005-a-schema-version-is-semver-pinned-exactly-moved-by-pin.md"], "confidence": "high"},
"Q10": {"answer": "0055-F9 decision 0008 (2026-10-24) names epics 0193 and 0194 as having no Summary section. At this commit it still holds for epic 0193: its file has only frontmatter and a heading. It no longer holds for epic 0194: that file, updated 2026-10-31, now has a '## Summary' section.", "sources": [".github/mirror/.spec/decisions/0008-an-epic-issue-without-a-summary-or-status-label.md", "epics/0193-json-first-approach/epic.md", "epics/0194-spectre-cli-ui/epic.md"], "confidence": "high"},
"Q11": {"answer": "The premise does not hold as stated. Item 0220 in the index is a bug, not a spike: 'The cold-start baseline restore fails under the .NET 11 SDK', done, at .performance/benchmarks/Startup/.issue/0220-cold-start-baseline-bundled-framework.yml, and it proposes no ADR. ADR-0025 ('Rule pages stay whole snapshots; single source is the candidate if growth must go') calls itself 'Item 0220's proposal' and cites a rule-page versioning spike at .issue/0220-rule-page-versioning-spike.yml. The index has no record of that file, so the id appears to be claimed twice. ADR-0025 is only proposed (2026-10-31) and leaves one question to the owner, 0001-F7 OQ-69 in src/tool/.spec/README.md. OQ-69 records no owner ruling: it is 'Open. Owner not asked', with proposed default (a), keep whole snapshots.", "sources": [".performance/benchmarks/Startup/.issue/0220-cold-start-baseline-bundled-framework.yml", ".spec/adr/0025-rule-pages-stay-whole-snapshots-and-single-source-is-the-candidate-if-growth-must-go.md", "src/tool/.spec/README.md"], "confidence": "medium"},
"Q12": {"answer": "Lesson 0012 points to the check Feature's decision 0014, 0001-F2 decision 0014, 'Self-check installs the working package'. The self-check builds and packs the working checkout's tool and installs it through an isolated local manifest under .artifacts/self-check/. The published consumer pin stays separate. The lesson also says B-014 is amended and B-046 and B-047 are added.", "sources": [".spec/lessons/0012-self-check-validates-the-working-checker.md", "src/tool/Features/Check/.spec/decisions/0014-self-check-installs-the-working-package.md"], "confidence": "high"}
}}

Protocol note: once, while checking where 0001-F18 is mentioned, I piped records.py output through grep. That broke the rule allowing only the records.py lines. It did not change any answer, because I confirmed the result through records.py get and an allowed Read.
```

### Path 3 - complete stored documents

Prompt:

```text
You are a participant in a controlled retrieval trial. Answer twelve questions about a specification repository snapshot. Accuracy and source citations matter more than speed.

ACCESS RULES (path 3, complete documents from a database) — follow exactly:
- Retrieve everything ONLY through this document service, run with the Bash tool:
  `python3 <adapter> docs search <words...>` — full-text ranked matches with a snippet
  `python3 <adapter> docs get <key>` — the complete stored document (body included), its path, and outbound and inbound links
  `python3 <adapter> docs links <key>` — links only
  Keys look like: item:0287, feature:0001-F11, epic:0132, adr:0026, decision:0132-F1/0001, lesson:0005. A link to a non-record file appears as file:<path>; such files are not stored and cannot be read. A missing target is marked "missing": true.
- Do not use the Read, Grep or Glob tools, and no Bash command other than the records.py lines above. Do not read any file directly, do not use the web, do not spawn agents, do not write any file.
- Answer only from what the documents say. If a record does not exist, say so plainly; do not guess.

Record kinds: work items, Feature specifications (cited as 0001-F11), epics, ADRs, lessons (repository-wide or a Feature's), and decisions (cited as `<feature> decision NNNN`).

QUESTIONS
Q01 Item 0287: which Feature specification does it deliver against (give the Feature id and the specification path), which epic owns that Feature, and what is the item's status?
Q02 Which ADR records the evidence decision of spike item 0216, what did the owner rule, and which earlier ADR's clauses does it supersede?
Q03 Which repository-wide lesson records that an id is not reserved until it merges, and which skill reference file holds the rule that lesson produced?
Q04 What does decision 0001 of Feature 0132-F1 decide about the database and Markdown, and where is that Feature's specification?
Q05 Feature 0132-F4 cites Feature 0001-F18. Does a specification for 0001-F18 exist in the tree? If not, where is its id reserved and how does 0132-F4 handle the missing dependency edge?
Q06 What does ADR-0031 decide, and which Feature does it constrain?
Q07 Epic 0324's candidate F was moved there from which epic, and from which of that epic's candidates was it cut?
Q08 List every Feature that 0132-F4 depends on, as qualified ids, and say which of them has status blocked.
Q09 What does decision 0005 of Feature 0001-F7 decide about how a schema version is numbered and pinned?
Q10 Decision 0008 of the work-item mirror Feature (0055-F9) names epics that have no Summary section. Which epics does it name, and is that statement still true for each of them at this commit?
Q11 Spike item 0220 proposed which ADR, and which open question in which Feature records the owner's ruling on it?
Q12 Repository-wide lesson 0012 (the self-check validates the working checker): which Feature decision does it point to as the remedy?

OUTPUT: reply with only a JSON object, no prose around it:
{"path": "documents", "answers": {"Q01": {"answer": "...", "sources": ["corpus-relative path", "..."], "confidence": "high|medium|low"}, ...}}
Sources are the corpus-relative paths the service reports for the documents that state each fact.
```

Raw answer:

```text
{"path": "documents", "answers": {
"Q01": {"answer": "Item 0287 ('Edit one rule level in the manifest bytes', a task) delivers against Feature 0001-F11, 'Setting a rule's level'. Its spec field is src/tool/Features/Rules/Set/.spec/README.md, and it covers claims B-001, B-003, B-004, B-005 and B-012. Epic 0001 owns that Feature: the item's parent and the Feature's epic are both 0001, at epics/0001-specht/epic.md. The item's status is done, closed 2026-10-28.", "sources": ["src/tool/Features/Rules/Set/.issue/0287-set-rule-level-engine-edit.yml", "src/tool/Features/Rules/Set/.spec/README.md", "epics/0001-specht/epic.md"], "confidence": "high"},
"Q02": {"answer": "ADR-0026, 'The source-generated mediator measured no benefit; direct calls stay', is spike 0216's evidence decision. On 2026-10-31 the owner ruled '(a) Don't adopt', and the ADR is accepted. martinothamar/Mediator (3.0.2) is not adopted, ADR-0014's benchmark gate is not met for the mediator, and hosts keep direct calls through the existing composition, with no mediator, dispatcher or handwritten substitute. ADR-0026 supersedes, in part, ADR-0010's mediator clauses. ADR-0010's other stages stay gated by ADR-0014.", "sources": [".spec/adr/0026-the-source-generated-mediator-measured-no-benefit-and-direct-calls-stay.md", ".issue/0216-evaluate-source-generated-mediator.yml"], "confidence": "high"},
"Q03": {"answer": "Lesson 0005, 'An id is not reserved until it merges' (a process lesson dated 2026-10-09). The rule it produced, 'Reserve an id before a branch claims it', went into the specht-conventions skill § Delivery, file .agents/skills/specht-conventions/references/delivery.md. That file is linked from the lesson but is not stored in the service, so I could not read it.", "sources": [".spec/lessons/0005-an-id-is-not-reserved-until-it-merges.md"], "confidence": "high"},
"Q04": {"answer": "Decision 0001 of 0132-F1 ('Markdown remains authoritative and indexed answers stay current', 2026-10-19, the repository owner) decides three things. Markdown in the repository is the source of truth, and the database is a disposable, rebuildable index of its files and facts. Every indexed MCP answer must reflect the current tree, including after a direct Markdown edit or a branch switch. The index may read source files to establish that, and the aim is to avoid reparsing unchanged Markdown for repeated keyed lookups. It rejects making database records the authoritative documents, and rejects a named snapshot kept until an explicit refresh. The Feature's specification is src/specht/Index/.spec/README.md (status blocked).", "sources": ["src/specht/Index/.spec/decisions/0001-markdown-authority-and-current-answers.md", "src/specht/Index/.spec/README.md"], "confidence": "high"},
"Q05": {"answer": "No. There is no specification for 0001-F18 in the tree; the service reports it missing. The owner reserved the id on 2026-10-30 in epic 0132 § Features, for a new epic 0001 Feature that prints the frontmatter schemas and templates and adds `specht schema list --json`. That epic says it is not yet specified. 0132-F4 handles the gap through assumption A-2: claims B-008 and B-009 cite 0001-F18 as 'reserved', and its depends_on leaves 0001-F18 out, gaining it when that specification exists.", "sources": ["src/tool/Features/Mcp/Reading/.spec/README.md", "epics/0132-model-context-protocol/epic.md"], "confidence": "high"},
"Q06": {"answer": "ADR-0031 does not exist in this snapshot. Getting adr:0031 returns not found, and a search for 'ADR-0031' returns no matches, so I cannot say what it decides or which Feature it constrains.", "sources": [], "confidence": "high"},
"Q07": {"answer": "Candidate F (document sequence allocation) was moved to epic 0324 from epic 0132 on 2026-10-30, by the owner's decision, together with the service mode. It keeps its letter from 0132. It was cut from epic 0132's candidates D (writing through MCP) and E (the work queue), because it reserves identifiers instead of exposing an existing tree write or reading delivery state.", "sources": ["epics/0324-specht-service/epic.md", "epics/0132-model-context-protocol/epic.md"], "confidence": "high"},
"Q08": {"answer": "0132-F4 depends on seven Features: 0132-F2 (ready), 0132-F3 (ready), 0001-F2 (ready-for-architecture), 0001-F3 (ready-for-architecture), 0001-F9 (ready-for-architecture), 0001-F10 (ready-for-architecture) and 0001-F13 (blocked). Only 0001-F13 (src/tool/Features/Trace/.spec/README.md) has status blocked. 0001-F18 is cited but is not in depends_on.", "sources": ["src/tool/Features/Mcp/Reading/.spec/README.md", "src/tool/Features/Trace/.spec/README.md", "src/tool/Features/Facts/.spec/README.md", "src/tool/Features/Mcp/.spec/README.md", "src/tool/Features/Check/.spec/README.md", "src/specht/Report/.spec/README.md", "src/tool/Features/Manifest/.spec/README.md", "src/tool/Features/Rules/.spec/README.md"], "confidence": "high"},
"Q09": {"answer": "Decision 0005 of 0001-F7 ('a schema version is semver, pinned exactly, moved by pin', 2026-10-09):\n- A schema version is major.minor.patch, numbered on its own and not from the specht.tool package version; one tool ships many schema versions.\n- The first is 0.1.0, and a manifest with no schemaVersion key means 0.1.0.\n- The bump follows the verdict: a patch changes no tree's verdict; a minor accepts more and fails no tree that passed; a major can fail a tree that passed, which includes any new SPEC###.\n- A pin is exact by default. A pin policy (latest-patch or latest-minor) may let it float, as a Could.\n- A command lists the shipped versions. A pin command moves the pin to any listed version, up or down, and changes only schemaVersion.\n- upgrade then rewrites the frontmatter schemas and templates to match the pinned version, and writes nothing if they already match.\n- The owner dropped one-version-at-a-time upgrade and the no-moving-down boundary.\n- Addenda dated 2026-10-14 name the command `specht schema pin <version>` and require the Spectre selection prompt when no version is given.", "sources": ["src/tool/.spec/decisions/0005-a-schema-version-is-semver-pinned-exactly-moved-by-pin.md"], "confidence": "high"},
"Q10": {"answer": "Decision 0008 of 0055-F9 names epics 0193 and 0194 as having no Summary section. At this commit it is still true for epic 0193 (epics/0193-json-first-approach/epic.md): its body is only the frontmatter and an H1. It is no longer true for epic 0194 (epics/0194-spectre-cli-ui/epic.md, updated 2026-10-31), which now has a '## Summary' section.", "sources": [".github/mirror/.spec/decisions/0008-an-epic-issue-without-a-summary-or-status-label.md", "epics/0193-json-first-approach/epic.md", "epics/0194-spectre-cli-ui/epic.md"], "confidence": "high"},
"Q11": {"answer": "Spike 0220 ('Weigh how rule pages are versioned without a full copy per schema version') proposed ADR-0025, 'Rule pages stay whole snapshots; single source is the candidate if growth must go'. ADR-0025's status is still proposed (2026-10-31). The open question it leaves the owner is 0001-F7 OQ-69, in src/tool/.spec/README.md: which of ADR-0025's options the tool takes for per-version rule pages. OQ-69 records no ruling yet. It reads 'Open. Owner not asked', with proposed default (a), keep whole snapshots. Retrieval caveat: the key item:0220 resolves to a different item, the bug 'The cold-start baseline restore fails under the .NET 11 SDK' at .performance/benchmarks/Startup/.issue/0220-cold-start-baseline-bundled-framework.yml. The spike's own file, .issue/0220-rule-page-versioning-spike.yml (cited by ADR-0025), cannot be fetched on its own. Its text appears only in search snippets under that key.", "sources": [".spec/adr/0025-rule-pages-stay-whole-snapshots-and-single-source-is-the-candidate-if-growth-must-go.md", "src/tool/.spec/README.md", ".performance/benchmarks/Startup/.issue/0220-cold-start-baseline-bundled-framework.yml"], "confidence": "medium"},
"Q12": {"answer": "Lesson 0012 points to the Check Feature's decision 0014 (0001-F2 decision 0014, 'Self-check installs the working package') as the remedy. The self-check builds and packs the working checkout's tool, installs that exact package through an isolated local manifest under .artifacts/self-check/, and runs it, so it still validates an installed package. The consumer installation stays separately pinned. B-014 is amended, B-046 makes preparation failures fail the gate, and B-047 prevents stale bytes at an unchanged package version.", "sources": [".spec/lessons/0012-self-check-validates-the-working-checker.md", "src/tool/Features/Check/.spec/decisions/0014-self-check-installs-the-working-package.md"], "confidence": "high"}
}}
```

### The storage survey

Its prompt only; its answer is condensed under Context, "The storage
candidates", and its sources are Appendix D.

Prompt:

```text
Research task for an engineering spike; read-only, write no files. Today is 2026-11-01.

Context: `specht` is a .NET 10/11 command-line tool (a `dotnet tool`, NuGet package) that checks a repository's Markdown/YAML specification tree. An MCP (Model Context Protocol) server mode is planned, started from the same tool over stdio. The question is what storage could hold records about the tree (work items, Features, epics, ADRs, lessons, decisions — about 600 small Markdown/YAML documents per repository, tens of thousands of link rows) for agent lookup, with:
- an offline, machine-local mode (no network, deterministic, works on macOS/Linux/Windows, installed only through the NuGet tool package — no separate server install), and
- an optional online mode where one project-wide sequence (four-digit record ids like 0326) is shared across machines so two developers never take the same next id, with defined behaviour when the service is unreachable.
Current agreement: Markdown files in git stay authoritative; any database is a disposable local index rebuilt from them.

Produce a candidate matrix comparing at least these, and add any other plausible one you find:
Embedded/local: SQLite via Microsoft.Data.Sqlite (with FTS5), LiteDB, RavenDB Embedded, DuckDB.
Hosted: Turso / libSQL (including embedded replicas and offline sync), Azure Cosmos DB, Supabase (Postgres), Cloudflare D1, MongoDB Atlas.

Criteria (one column each): offline operation; shared online project sequence (does it offer an atomic counter/sequence or a transaction that makes one, and how); durability; concurrent access (multiple processes on one machine, multiple machines); synchronization between local and online copies; schema migration; security/auth model for a CLI; cost (free tier and the first paid step, with the date you read it); operational burden; .NET client maturity and whether it works inside a packaged dotnet tool across the three OSes (native binaries, AOT/trimming notes); licence.

Rules: verify current facts on the vendors' own docs or release notes with the web tools; give a URL for every non-obvious claim; mark anything you could not verify as "unverified"; do not invent prices or features; note where a product is deprecated or changed recently. Keep the answer compact: the matrix as a Markdown table (one row per candidate), then at most 10 bullets of the most decision-relevant findings, then the source list.
```
