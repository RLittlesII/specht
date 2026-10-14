Feature: Release baseline comparison
  As the maintainer releasing specht
  I want an automatic comparison with the previous release
  So that meaningful performance regressions receive review without blocking publication

  @B-001
  Scenario: A published release starts an advisory comparison
    Given a release has been successfully published
    When its release run reaches performance reporting
    Then a baseline comparison is started automatically for that release

  @B-002
  Scenario: The previous eligible release is selected
    Given earlier published stable releases, published prereleases, draft releases and unpublished tags exist
    When a release comparison selects its baseline
    Then it selects the immediately preceding published release, including prereleases and excluding drafts
    And it records both release tags and their resolved commits

  @B-002
  Scenario: A rerun preserves the original release pair
    Given a release comparison has selected its predecessor
    And a newer release has since been published
    When the original release comparison is rerun
    Then it compares the original release pair

  @B-003 @boundary
  Scenario: The first release has no baseline
    Given no earlier eligible published release exists
    When a release comparison runs
    Then it publishes a report marked "No baseline"
    And it makes no regression judgment

  @B-004
  Scenario: Both releases are measured together
    Given a previous release has been selected
    When their measurements are collected
    Then both revisions are measured in the same comparison session on the same runner
    And the report identifies the environment of each measurement

  @B-005
  Scenario: Matching workloads produce comparable rows
    Given both releases provide measurements of the same workload
    And their inputs, measurement boundaries, operating system, runtime version, architecture, toolchain, garbage-collection mode, benchmark job and timing confidence level match
    When the comparison is generated
    Then those measurements are paired as comparable

  @B-006 @boundary
  Scenario Outline: Changed measurement conditions are visible
    Given both releases have a measurement with the same name
    But their <condition> differ
    When the comparison is generated
    Then that pair is marked "Not comparable" with the differing condition
    And the pair receives no regression judgment

    Examples:
      | condition              |
      | workload definitions   |
      | input parameters       |
      | measurement boundaries |
      | runtimes               |
      | operating systems      |
      | toolchains             |
      | garbage-collection modes |
      | timing confidence levels |
      | architectures          |
      | benchmark jobs         |

  @B-007
  Scenario: A new benchmark remains visible
    Given a benchmark exists only in the current release's measurement set
    When the comparison is generated
    Then it is reported as "Added" without a regression judgment

  @B-008
  Scenario: A removed benchmark remains visible
    Given a benchmark exists only in the previous release's measurement set
    When the comparison is generated
    Then it is reported as "Removed" without a regression judgment

  @B-009
  Scenario: Comparable measurements show their change
    Given comparable measurements with a positive previous value
    When the comparison is generated
    Then each metric shows both values, its unit, its absolute change and its percentage change

  @B-010
  Scenario: Zero allocation does not produce an invented percentage
    Given the previous release allocated zero bytes for a comparable workload
    And the current release allocates a positive number of bytes
    When the comparison is generated
    Then the allocation percentage change is marked "Not defined"
    And both allocation values and the absolute increase remain visible
    And its allocation judgment is "Insufficient evidence"

  @B-010
  Scenario: Zero allocation in both releases is unchanged
    Given both releases allocated zero bytes for a comparable workload
    When the comparison is generated
    Then the allocation is reported as unchanged without a review flag
    And the allocation percentage change is marked "Not defined"

  @B-011
  Scenario Outline: Timing review requires both a material increase and separated confidence intervals
    Given a previous comparable mean time of 100 milliseconds with confidence interval half-width 2 milliseconds
    And a current comparable mean time of <mean> milliseconds with confidence interval half-width <error> milliseconds
    When the comparison is generated
    Then the timing judgment is "<judgment>"

    Examples:
      | mean | error | judgment              |
      | 112  | 2     | Review required       |
      | 110  | 1     | No regression flagged |
      | 111  | 10    | No regression flagged |
      | 111  | 9     | No regression flagged |
      | 80   | 2     | No regression flagged |

  @B-012
  Scenario Outline: Allocation review uses a strict percentage increase
    Given a previous comparable allocation of 1000 bytes per operation
    And a current comparable allocation of <allocation> bytes per operation
    When the comparison is generated
    Then the allocation judgment is "<judgment>"

    Examples:
      | allocation | judgment              |
      | 1101       | Review required       |
      | 1100       | No regression flagged |
      | 1000       | No regression flagged |
      | 900        | No regression flagged |

  @B-013
  Scenario: Uncertain timings cannot establish a clean comparison
    Given a comparable timing pair lacks valid uncertainty measurements
    When the comparison is generated
    Then its timing judgment is marked "Insufficient evidence"

  @B-014
  Scenario: Review flags remain visible in the release run
    Given a comparison contains results flagged "Review required"
    When its report is published
    Then the release run summary identifies those results and links to the report

  @B-015
  Scenario: People and automation receive the same findings
    Given a release comparison has completed
    When its report is published
    Then human-readable and machine-readable reports describe the same findings

  @B-016
  Scenario: A report identifies the evidence behind its judgment
    Given a release comparison has completed
    When its report is published
    Then it records the release pair, measurement environments, workload identities, comparison policy and raw result references

  @B-017 @boundary
  Scenario: A meaningful regression leaves the build successful
    Given the correctness gates passed and the release was published
    And its comparison flags a meaningful regression
    When performance reporting completes
    Then the build remains successful

  @B-018 @boundary
  Scenario Outline: A reporting failure leaves an incomplete advisory outcome
    Given the correctness gates passed and the release was published
    And the <stage> stage fails
    When performance reporting completes
    Then the run summary marks the comparison "Incomplete" with an actionable reason
    And the build remains successful
    And no successful performance verdict is reported

    Examples:
      | stage                  |
      | benchmark collection   |
      | result parsing         |
      | comparison evaluation  |
      | report rendering       |
      | release attachment     |

  @B-019
  Scenario: One review flag determines the overall review status
    Given some comparable metrics need no review
    And another comparable metric is flagged "Review required"
    When the comparison is summarised
    Then the overall judgment is "Review required"

  @B-020
  Scenario: Comparison gaps prevent an overall clean judgment
    Given no metric is flagged "Review required"
    And a required metric is missing, not comparable or has insufficient evidence
    When the comparison is summarised
    Then the overall judgment is "Incomplete"

  @B-020 @boundary
  Scenario: An empty inventory cannot establish a clean comparison
    Given the release pair has no measurable workload cases
    When the comparison is summarised
    Then the overall judgment is "Incomplete"

  @B-021
  Scenario: A complete comparison can report no review flags
    Given the release comparison requires at least one workload case
    And every metric required for the release comparison has a valid comparable result
    And no metric meets its regression policy
    When the comparison is summarised
    Then the overall judgment is "No regression flagged"

  @B-022 @boundary
  Scenario: An expired historical run does not remove a release report
    Given a release comparison report was published
    When its transient workflow artifacts expire
    Then its report and the raw evidence it references remain available with that release

  @B-023 @boundary
  Scenario: Ordinary builds do not start release comparisons
    Given a pull request, a push to main or a release dry run
    When its build runs without publishing a release
    Then no automatic release baseline comparison starts

  @B-024 @boundary
  Scenario: A missing immediate baseline is not silently replaced
    Given the immediately preceding eligible release has been selected
    And its revision cannot be resolved or measured
    When the comparison runs
    Then the comparison is marked "Incomplete" with the reason
    And an older release is not substituted

  @B-009
  Scenario: Compatible units are normalised before comparison
    Given a previous comparable timing of 1 millisecond
    And a current comparable timing of 1000000 nanoseconds
    When the comparison is generated
    Then the timing change is zero with a shared time unit

  @B-013
  Scenario Outline: Invalid timing evidence cannot establish a clean comparison
    Given a timing pair has <invalid evidence>
    When the comparison is generated
    Then its timing judgment is marked "Insufficient evidence"

    Examples:
      | invalid evidence                    |
      | a zero previous mean                |
      | a negative mean                     |
      | a non-finite mean                   |
      | a negative interval half-width      |
      | an unknown timing confidence level  |

  @B-002
  Scenario: A same-time publication tie has a stable order
    Given two earlier eligible releases have the same publication timestamp
    When a release comparison selects its baseline
    Then the release with the higher immutable release identifier is the predecessor

  @B-006 @boundary
  Scenario: Unknown workload identity is not comparable
    Given both releases have a measurement with the same name
    But their workload and measurement-condition equivalence cannot be established
    When the comparison is generated
    Then the pair is marked "Not comparable" with the reason

  @B-020 @boundary
  Scenario: Duplicate evidence does not silently choose a result
    Given a workload case has more than one result for the same release and comparison session
    And no other metric is flagged "Review required"
    When the comparison is summarised
    Then the duplicate case is reported as invalid evidence
    And the overall judgment is "Incomplete"

  @B-018 @boundary
  Scenario: A later reporting failure preserves known review flags
    Given the release was published and its comparison computed a review flag
    And attaching the report to the release fails
    When the run summary is produced
    Then the known review flag remains visible alongside the incomplete reporting state
    And no report link is presented as a successful attachment
