Held on main, read by the collision guard (feature scope `src/specht/Manifest`):

| Type         | Held |
| ------------ | ---- |
| epic         | 364  |
| feature      | 18   |
| task         | 352  |
| adr          | 32   |
| lesson       | 16   |
| claim        | 83   |
| constraint   | 13   |
| openQuestion | 30   |
| decision     | 22   |
| scenario     | 0    |

## 5 branches from one main, mints spread over 1 s, 1000 repetitions, seed 256

| Candidate                                 | Runs with a collision | Collisions per run | Guard refusals per run | epic | feature | task  | adr   | lesson | claim | constraint | openQuestion | decision | scenario |
| ----------------------------------------- | --------------------- | ------------------ | ---------------------- | ---- | ------- | ----- | ----- | ------ | ----- | ---------- | ------------ | -------- | -------- |
| (a) sequence                              | 1000/1000             | 64                 | 0                      | 4    | 4       | 8     | 4     | 4      | 12    | 4          | 8            | 4        | 12       |
| (b) Snowflake as supplied                 | 100/1000              | 0.102              | 0.03                   | 0.01 | 0.006   | 0.015 | 0.008 | 0.006  | 0.025 | 0.003      | 0.011        | 0.003    | 0.015    |
| (b) Snowflake, worker by worktree path    | 0/1000                | 0                  | 0.03                   | 0    | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (b) Snowflake, random fields and sequence | 0/1000                | 0                  | 0                      | 0    | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (g) Nano ID base32, size 4                | 0/1000                | 0                  | 0.002                  | 0    | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (g) Nano ID base32, size 5                | 0/1000                | 0                  | 0                      | 0    | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (g) Nano ID base32, size 6                | 0/1000                | 0                  | 0                      | 0    | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (g) Nano ID base32, size 8                | 0/1000                | 0                  | 0                      | 0    | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (g) Nano ID url-safe, size 21             | 0/1000                | 0                  | 0                      | 0    | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |

## 16 branches from one main, mints spread over 1 s, 1000 repetitions, seed 256

| Candidate                                 | Runs with a collision | Collisions per run | Guard refusals per run | epic  | feature | task  | adr   | lesson | claim | constraint | openQuestion | decision | scenario |
| ----------------------------------------- | --------------------- | ------------------ | ---------------------- | ----- | ------- | ----- | ----- | ------ | ----- | ---------- | ------------ | -------- | -------- |
| (a) sequence                              | 1000/1000             | 240                | 0                      | 15    | 15      | 30    | 15    | 15     | 45    | 15         | 30           | 15       | 45       |
| (b) Snowflake as supplied                 | 690/1000              | 1.196              | 0.089                  | 0.081 | 0.072   | 0.211 | 0.047 | 0.046  | 0.302 | 0.031      | 0.11         | 0.032    | 0.264    |
| (b) Snowflake, worker by worktree path    | 0/1000                | 0                  | 0.089                  | 0     | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (b) Snowflake, random fields and sequence | 0/1000                | 0                  | 0                      | 0     | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (g) Nano ID base32, size 4                | 5/1000                | 0.005              | 0.009                  | 0     | 0       | 0.001 | 0     | 0      | 0.002 | 0.001      | 0            | 0        | 0.001    |
| (g) Nano ID base32, size 5                | 0/1000                | 0                  | 0                      | 0     | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (g) Nano ID base32, size 6                | 0/1000                | 0                  | 0                      | 0     | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (g) Nano ID base32, size 8                | 0/1000                | 0                  | 0                      | 0     | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (g) Nano ID url-safe, size 21             | 0/1000                | 0                  | 0                      | 0     | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |

## 64 branches from one main, mints spread over 1 s, 1000 repetitions, seed 256

| Candidate                                 | Runs with a collision | Collisions per run | Guard refusals per run | epic  | feature | task  | adr   | lesson | claim | constraint | openQuestion | decision | scenario |
| ----------------------------------------- | --------------------- | ------------------ | ---------------------- | ----- | ------- | ----- | ----- | ------ | ----- | ---------- | ------------ | -------- | -------- |
| (a) sequence                              | 1000/1000             | 1008               | 0                      | 63    | 63      | 126   | 63    | 63     | 189   | 63         | 126          | 63       | 189      |
| (b) Snowflake as supplied                 | 1000/1000             | 19.872             | 0.393                  | 1.349 | 1.083   | 3.291 | 0.738 | 0.726  | 5.312 | 0.584      | 2.145        | 0.541    | 4.103    |
| (b) Snowflake, worker by worktree path    | 349/1000              | 0.427              | 0.393                  | 0.021 | 0.023   | 0.065 | 0.018 | 0.016  | 0.124 | 0.014      | 0.05         | 0.008    | 0.088    |
| (b) Snowflake, random fields and sequence | 0/1000                | 0                  | 0                      | 0     | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (g) Nano ID base32, size 4                | 53/1000               | 0.054              | 0.053                  | 0.001 | 0       | 0.008 | 0.002 | 0.005  | 0.015 | 0.004      | 0.01         | 0.002    | 0.007    |
| (g) Nano ID base32, size 5                | 1/1000                | 0.001              | 0                      | 0     | 0       | 0     | 0     | 0      | 0.001 | 0          | 0            | 0        | 0        |
| (g) Nano ID base32, size 6                | 0/1000                | 0                  | 0                      | 0     | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (g) Nano ID base32, size 8                | 0/1000                | 0                  | 0                      | 0     | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |
| (g) Nano ID url-safe, size 21             | 0/1000                | 0                  | 0                      | 0     | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |

## 5 branches from one main, mints spread over 60 s, 1000 repetitions, seed 256

| Candidate                                 | Runs with a collision | Collisions per run | Guard refusals per run | epic | feature | task  | adr | lesson | claim | constraint | openQuestion | decision | scenario |
| ----------------------------------------- | --------------------- | ------------------ | ---------------------- | ---- | ------- | ----- | --- | ------ | ----- | ---------- | ------------ | -------- | -------- |
| (b) Snowflake as supplied                 | 8/1000                | 0.008              | 0.03                   | 0    | 0       | 0.001 | 0   | 0.001  | 0.003 | 0          | 0.001        | 0.001    | 0.001    |
| (b) Snowflake, worker by worktree path    | 0/1000                | 0                  | 0.03                   | 0    | 0       | 0     | 0   | 0      | 0     | 0          | 0            | 0        | 0        |
| (b) Snowflake, random fields and sequence | 0/1000                | 0                  | 0                      | 0    | 0       | 0     | 0   | 0      | 0     | 0          | 0            | 0        | 0        |

## 16 branches from one main, mints spread over 60 s, 1000 repetitions, seed 256

| Candidate                                 | Runs with a collision | Collisions per run | Guard refusals per run | epic  | feature | task  | adr | lesson | claim | constraint | openQuestion | decision | scenario |
| ----------------------------------------- | --------------------- | ------------------ | ---------------------- | ----- | ------- | ----- | --- | ------ | ----- | ---------- | ------------ | -------- | -------- |
| (b) Snowflake as supplied                 | 54/1000               | 0.056              | 0.089                  | 0.003 | 0.002   | 0.006 | 0   | 0.001  | 0.022 | 0.001      | 0.007        | 0.002    | 0.012    |
| (b) Snowflake, worker by worktree path    | 0/1000                | 0                  | 0.089                  | 0     | 0       | 0     | 0   | 0      | 0     | 0          | 0            | 0        | 0        |
| (b) Snowflake, random fields and sequence | 0/1000                | 0                  | 0                      | 0     | 0       | 0     | 0   | 0      | 0     | 0          | 0            | 0        | 0        |

## 64 branches from one main, mints spread over 60 s, 1000 repetitions, seed 256

| Candidate                                 | Runs with a collision | Collisions per run | Guard refusals per run | epic  | feature | task  | adr   | lesson | claim | constraint | openQuestion | decision | scenario |
| ----------------------------------------- | --------------------- | ------------------ | ---------------------- | ----- | ------- | ----- | ----- | ------ | ----- | ---------- | ------------ | -------- | -------- |
| (b) Snowflake as supplied                 | 619/1000              | 0.978              | 0.393                  | 0.032 | 0.043   | 0.129 | 0.022 | 0.026  | 0.279 | 0.027      | 0.115        | 0.027    | 0.278    |
| (b) Snowflake, worker by worktree path    | 15/1000               | 0.015              | 0.393                  | 0.001 | 0       | 0.004 | 0     | 0      | 0.002 | 0          | 0.002        | 0.001    | 0.005    |
| (b) Snowflake, random fields and sequence | 0/1000                | 0                  | 0                      | 0     | 0       | 0     | 0     | 0      | 0     | 0          | 0            | 0        | 0        |

## 5 branches from one main, mints spread over 3600 s, 1000 repetitions, seed 256

| Candidate                                 | Runs with a collision | Collisions per run | Guard refusals per run | epic | feature | task | adr | lesson | claim | constraint | openQuestion | decision | scenario |
| ----------------------------------------- | --------------------- | ------------------ | ---------------------- | ---- | ------- | ---- | --- | ------ | ----- | ---------- | ------------ | -------- | -------- |
| (b) Snowflake as supplied                 | 0/1000                | 0                  | 0.03                   | 0    | 0       | 0    | 0   | 0      | 0     | 0          | 0            | 0        | 0        |
| (b) Snowflake, worker by worktree path    | 0/1000                | 0                  | 0.03                   | 0    | 0       | 0    | 0   | 0      | 0     | 0          | 0            | 0        | 0        |
| (b) Snowflake, random fields and sequence | 0/1000                | 0                  | 0                      | 0    | 0       | 0    | 0   | 0      | 0     | 0          | 0            | 0        | 0        |

## 16 branches from one main, mints spread over 3600 s, 1000 repetitions, seed 256

| Candidate                                 | Runs with a collision | Collisions per run | Guard refusals per run | epic | feature | task | adr | lesson | claim | constraint | openQuestion | decision | scenario |
| ----------------------------------------- | --------------------- | ------------------ | ---------------------- | ---- | ------- | ---- | --- | ------ | ----- | ---------- | ------------ | -------- | -------- |
| (b) Snowflake as supplied                 | 1/1000                | 0.001              | 0.089                  | 0    | 0       | 0    | 0   | 0      | 0     | 0          | 0.001        | 0        | 0        |
| (b) Snowflake, worker by worktree path    | 0/1000                | 0                  | 0.089                  | 0    | 0       | 0    | 0   | 0      | 0     | 0          | 0            | 0        | 0        |
| (b) Snowflake, random fields and sequence | 0/1000                | 0                  | 0                      | 0    | 0       | 0    | 0   | 0      | 0     | 0          | 0            | 0        | 0        |

## 64 branches from one main, mints spread over 3600 s, 1000 repetitions, seed 256

| Candidate                                 | Runs with a collision | Collisions per run | Guard refusals per run | epic | feature | task  | adr | lesson | claim | constraint | openQuestion | decision | scenario |
| ----------------------------------------- | --------------------- | ------------------ | ---------------------- | ---- | ------- | ----- | --- | ------ | ----- | ---------- | ------------ | -------- | -------- |
| (b) Snowflake as supplied                 | 23/1000               | 0.023              | 0.393                  | 0    | 0       | 0.001 | 0   | 0      | 0.008 | 0.001      | 0.003        | 0        | 0.01     |
| (b) Snowflake, worker by worktree path    | 0/1000                | 0                  | 0.393                  | 0    | 0       | 0     | 0   | 0      | 0     | 0          | 0            | 0        | 0        |
| (b) Snowflake, random fields and sequence | 0/1000                | 0                  | 0                      | 0    | 0       | 0     | 0   | 0      | 0     | 0          | 0            | 0        | 0        |
