Feature: Id generation
  As an author on a branch cut in parallel with others
  I want the pinned specht to mint a new id for a record type my repository opts in
  So that no other branch takes the same id, and the types I did not opt in keep their own numbering

  Background:
    Given a repository root with a valid Specht manifest

  @B-001 @boundary
  Scenario Outline: A type not opted in gets no id
    Given a manifest that <opts in>
    When an ADR number is requested
    Then nothing is written on standard output

    Examples:
      | opts in                     |
      | opts no type in             |
      | opts only lesson numbers in |

  @B-002 @boundary
  Scenario Outline: A type not opted in is refused
    Given a manifest that <leaves ADR numbers out>
    When an ADR number is requested
    Then the request ends as it does for something named on the command line that is not found

    Examples:
      | leaves ADR numbers out                                 |
      | opts only lesson numbers in                            |
      | lists ADR numbers without a strategy                   |
      | lists ADR numbers with a number in place of a strategy |

  @B-003
  Scenario: A minted id is the whole output
    Given a manifest that opts ADR numbers in to Nano ID
    When an ADR number is requested
    Then standard output is one line holding only the minted number

  @B-004
  Scenario: A printed id ends in success
    Given a manifest that opts ADR numbers in to Nano ID
    When an ADR number is requested
    Then the request ends in success

  @B-005
  Scenario: A Nano ID has the configured size
    Given a manifest that opts ADR numbers in to Nano ID with a size of 10
    When an ADR number is requested
    Then the minted number is 10 characters long after its prefix

  @B-006
  Scenario: A Nano ID draws only from the configured alphabet
    Given a manifest that opts ADR numbers in to Nano ID with an alphabet of lowercase letters and digits
    When an ADR number is requested
    Then every character of the minted number after its prefix is a lowercase letter or a digit

  @B-007
  Scenario: A Snowflake id carries the time since its epoch
    Given a manifest that opts ADR numbers in to Snowflake with an epoch of 2026-01-01
    And the time is noon on 2026-10-23, universal time
    When an ADR number is requested
    Then the minted number, decoded, holds the milliseconds from 2026-01-01 to noon on 2026-10-23

  @B-008
  Scenario: Each type follows its own settings
    Given a manifest that opts ADR numbers in to Nano ID with a size of 8
    And it opts lesson numbers in to Nano ID with a size of 12
    When an ADR number and a lesson number are requested
    Then the ADR number is 8 characters long after its prefix
    And the lesson number is 12 characters long after its prefix

  @B-009
  Scenario: A minted id starts with its type's prefix
    Given a manifest that opts ADR numbers in with the prefix "ADR-"
    When an ADR number is requested
    Then the minted number starts with "ADR-"

  @B-010
  Scenario: An id the tree already holds is never handed out
    Given a manifest that opts claim ids in to Nano ID
    And a Feature whose specification already holds the claim id that would be minted first
    When a claim id is requested for that Feature
    Then the minted id is one that Feature's specification does not hold

  @B-011 @boundary
  Scenario: An id its type's grammar rejects is not printed
    Given a manifest that opts ADR numbers in to Snowflake
    And its ADR grammar admits only four-digit ADR numbers
    When an ADR number is requested
    Then nothing is written on standard output

  @B-012 @boundary
  Scenario: An id its type's grammar rejects is refused
    Given a manifest that opts ADR numbers in to Snowflake
    And its ADR grammar admits only four-digit ADR numbers
    When an ADR number is requested
    Then the request ends with a failure code the check already gives

  @B-013 @boundary
  Scenario Outline: A request writes nothing
    Given a manifest that <opts in>
    And ADRs that are already numbered
    When an ADR number is requested
    Then no file under the root or elsewhere has been created, modified or deleted

    Examples:
      | opts in                          |
      | opts ADR numbers in to Nano ID   |
      | opts ADR numbers in to Snowflake |
      | opts no type in                  |

  @B-014 @boundary
  Scenario: Opting types in changes no verdict
    Given a tree whose check reports a violation
    And a manifest that opts claim ids in to Nano ID
    When the tree is checked
    Then the violations reported are the ones the same tree gets with the id settings removed

  @B-015
  Scenario: Id settings the pinned version rejects are invalid
    Given a manifest pinned to a schema version that has no id settings
    And the manifest sets them
    When an ADR number is requested
    Then the request ends as it does for an invalid manifest

  @B-016 @boundary
  Scenario Outline: A repository-wide type is refused a Feature
    Given a manifest that opts <type> in to Nano ID
    When <a type id> is requested for a Feature
    Then nothing is written on standard output

    Examples:
      | type           | a type id       |
      | ADR numbers    | an ADR number   |
      | lesson numbers | a lesson number |

  @B-017 @boundary
  Scenario Outline: A request that prints no id ends in failure
    Given <a request that prints no id>
    When the request ends
    Then it ends with a failure code the check already gives

    Examples:
      | a request that prints no id                                        |
      | an ADR number requested of a manifest that opts no type in         |
      | an ADR number requested for a Feature                              |
      | an ADR number requested of a tree holding every candidate it tries |

  @B-018 @boundary
  Scenario Outline: Epic, Feature and task ids are not minted
    Given a manifest that opts <type> in to Nano ID
    When <an id> is requested
    Then nothing is written on standard output

    Examples:
      | type        | an id        |
      | epic ids    | an epic id   |
      | Feature ids | a Feature id |
      | task ids    | a task id    |

  @B-057 @boundary
  Scenario Outline: A per-Feature id requested without its Feature is not printed
    Given a manifest that opts <type> in to Nano ID
    When <a type id> is requested without naming a Feature
    Then nothing is written on standard output

    Examples:
      | type              | a type id           |
      | decision numbers  | a decision number   |
      | claim ids         | a claim id          |
      | constraint ids    | a constraint id     |
      | open question ids | an open question id |
      | scenario ids      | a scenario id       |

  @B-058 @boundary
  Scenario Outline: A per-Feature id requested without its Feature is a usage failure
    Given a manifest that opts <type> in to Nano ID
    When <a type id> is requested without naming a Feature
    Then the request ends as a usage failure

    Examples:
      | type              | a type id           |
      | decision numbers  | a decision number   |
      | claim ids         | a claim id          |
      | constraint ids    | a constraint id     |
      | open question ids | an open question id |
      | scenario ids      | a scenario id       |

  @B-019
  Scenario: A Nano ID with no alphabet draws from lower-case base32
    Given a manifest that opts ADR numbers in to Nano ID with no alphabet
    When an ADR number is requested
    Then every character of the minted number after its prefix is a digit or a lower-case letter other than i, l, o and u

  @B-020
  Scenario Outline: A repository-wide Nano ID with no size is 8 characters
    Given a manifest that opts <type> in to Nano ID with no size
    When <a type id> is requested
    Then the minted id is 8 characters long after its prefix

    Examples:
      | type           | a type id       |
      | ADR numbers    | an ADR number   |
      | lesson numbers | a lesson number |

  @B-021
  Scenario Outline: A per-Feature Nano ID with no size is 6 characters
    Given a manifest that opts <type> in to Nano ID with no size
    When <a type id> is requested for a Feature
    Then the minted id is 6 characters long after its prefix

    Examples:
      | type              | a type id           |
      | decision numbers  | a decision number   |
      | claim ids         | a claim id          |
      | constraint ids    | a constraint id     |
      | open question ids | an open question id |
      | scenario ids      | a scenario id       |

  @B-022
  Scenario: A Snowflake id with no epoch counts from 2026
    Given a manifest that opts ADR numbers in to Snowflake with no epoch
    And the time is noon on 2026-11-07, universal time
    When an ADR number is requested
    Then the minted number, decoded, holds the milliseconds from midnight on 2026-01-01, universal time, to noon on 2026-11-07

  @B-023
  Scenario: A Snowflake id with no widths has the default layout
    Given a manifest that opts ADR numbers in to Snowflake with a datacenter of 3, a worker of 9 and no field widths
    When an ADR number is requested
    Then the minted number's lowest 12 bits are its sequence
    And the 5 bits above them hold the worker 9
    And the 5 bits above those hold the datacenter 3

  @B-024
  Scenario: A Snowflake datacenter left unnamed is random
    Given a manifest that opts ADR numbers in to Snowflake with no datacenter
    And the random draw for the datacenter is 17
    When an ADR number is requested
    Then the minted number, decoded, holds the datacenter 17

  @B-025
  Scenario: A Snowflake worker left unnamed is random
    Given a manifest that opts ADR numbers in to Snowflake with no worker
    And the random draw for the worker is 22
    When an ADR number is requested
    Then the minted number, decoded, holds the worker 22

  @B-026
  Scenario: A Snowflake sequence start left unnamed is random
    Given a manifest that opts ADR numbers in to Snowflake with no sequence start
    And the random draw for the sequence start is 3001
    When an ADR number is requested
    Then the minted number, decoded, holds the sequence 3001

  @B-027
  Scenario: A named Snowflake datacenter is the one minted
    Given a manifest that opts ADR numbers in to Snowflake with a datacenter of 3
    When an ADR number is requested
    Then the minted number, decoded, holds the datacenter 3

  @B-028
  Scenario: A named Snowflake worker is the one minted
    Given a manifest that opts ADR numbers in to Snowflake with a worker of 9
    When an ADR number is requested
    Then the minted number, decoded, holds the worker 9

  @B-029 @boundary
  Scenario: A clock moving back during a request mints nothing
    Given a manifest that opts ADR numbers in to Snowflake
    And an ADR already numbered with the ADR number that would be minted first
    And the clock reads earlier for the second ADR number than for the first
    When an ADR number is requested
    Then nothing is written on standard output

  @B-030
  Scenario: A base32 Snowflake id is the number in base32
    Given a manifest that opts ADR numbers in to Snowflake with the encoding "base32"
    When an ADR number is requested
    Then the minted number after its prefix, read as lower-case base32, is the Snowflake number

  @B-031
  Scenario: A base32 Snowflake id is 13 characters
    Given a manifest that opts ADR numbers in to Snowflake with the encoding "base32"
    When an ADR number is requested
    Then the minted number is 13 characters long after its prefix

  @B-032
  Scenario: An unencoded Snowflake id is decimal
    Given a manifest that opts ADR numbers in to Snowflake with the encoding "none"
    When an ADR number is requested
    Then the minted number after its prefix is the Snowflake number in decimal digits

  @B-056
  Scenario: A Snowflake id that names no encoding is the number in base32
    Given a manifest that opts ADR numbers in to Snowflake with no encoding
    When an ADR number is requested
    Then the minted number after its prefix, read as lower-case base32, is the Snowflake number

  @B-033 @boundary
  Scenario: A Nano ID is not encoded in base32
    Given a manifest that opts ADR numbers in to Nano ID with the encoding "base32"
    When an ADR number is requested
    Then nothing is written on standard output

  @B-053 @boundary
  Scenario Outline: Nothing is written for a prefix that is not text
    Given a manifest that opts ADR numbers in to Nano ID with a prefix given as <a prefix>
    When an ADR number is requested
    Then nothing is written on standard output

    Examples:
      | a prefix                  |
      | the number 7              |
      | a list                    |
      | a group of named settings |
      | a yes-or-no value         |

  @B-054 @boundary
  Scenario Outline: Nothing is written for an encoding formatting does not offer
    Given a manifest that opts ADR numbers in to Nano ID with the encoding "<encoding>"
    When an ADR number is requested
    Then nothing is written on standard output

    Examples:
      | encoding |
      | base64   |
      | decimal  |

  @B-055
  Scenario Outline: A repository-wide type that names no prefix mints an id with none
    Given a manifest that opts <type> in to Nano ID with no prefix
    When <a type id> is requested
    Then the minted id is the drawn characters with nothing ahead of them

    Examples:
      | type           | a type id       |
      | ADR numbers    | an ADR number   |
      | lesson numbers | a lesson number |

  @B-055
  Scenario Outline: A per-Feature type that names no prefix takes the prefix its records already carry
    Given a manifest that opts <type> in to Nano ID with no prefix
    When <a type id> is requested for a Feature
    Then the minted id is the drawn characters with <a prefix> ahead of them

    Examples:
      | type              | a type id           | a prefix |
      | decision numbers  | a decision number   | nothing  |
      | claim ids         | a claim id          | "B-"     |
      | constraint ids    | a constraint id     | "C-"     |
      | open question ids | an open question id | "OQ-"    |
      | scenario ids      | a scenario id       | "S-"     |

  @B-034
  Scenario: An id held in another letter case is never handed out
    Given a manifest that opts ADR numbers in to Nano ID
    And an ADR already numbered with the ADR number that would be minted first, in capital letters
    When an ADR number is requested
    Then the minted number is one no ADR in the tree is numbered with, in any letter case

  @B-035
  Scenario: The guard tries eight candidates by default
    Given a manifest that opts ADR numbers in to Nano ID with no number of attempts
    And ADRs already numbered with each of the first seven ADR numbers that would be minted
    When an ADR number is requested
    Then the minted number is the eighth that would be minted

  @B-036 @boundary
  Scenario: Eight held candidates refuse the request by default
    Given a manifest that opts ADR numbers in to Nano ID with no number of attempts
    And ADRs already numbered with each of the first eight ADR numbers that would be minted
    When an ADR number is requested
    Then nothing is written on standard output

  @B-037 @boundary
  Scenario: A type's own number of attempts bounds the guard
    Given a manifest that opts ADR numbers in to Nano ID with 3 attempts
    And ADRs already numbered with each of the first three ADR numbers that would be minted
    When an ADR number is requested
    Then nothing is written on standard output

  @B-052 @boundary
  Scenario Outline: Nothing is written for a number of attempts that is not a positive whole number
    Given a manifest that opts ADR numbers in to Nano ID with a number of attempts of <attempts>
    When an ADR number is requested
    Then nothing is written on standard output

    Examples:
      | attempts           |
      | 0                  |
      | -3                 |
      | 2.5                |
      | "8", given as text |

  @B-038
  Scenario: An ADR number is checked against every ADR folder
    Given a manifest that opts ADR numbers in to Nano ID
    And a Feature's own ADR already numbered with the ADR number that would be minted first
    When an ADR number is requested
    Then the minted number is one no ADR in the tree is numbered with

  @B-039
  Scenario: A lesson number is checked against every lessons folder
    Given a manifest that opts lesson numbers in to Nano ID
    And a Feature's own lesson already numbered with the lesson number that would be minted first
    When a lesson number is requested
    Then the minted number is one no lesson in the tree is numbered with

  @B-040
  Scenario: A decision number is checked against the Feature's decisions
    Given a manifest that opts decision numbers in to Nano ID
    And a Feature with a decision already numbered with the decision number that would be minted first
    When a decision number is requested for that Feature
    Then the minted number is one no decision of that Feature is numbered with

  @B-041
  Scenario Outline: A claim, constraint or question id is checked against its Feature
    Given a manifest that opts <type> in to Nano ID
    And a Feature whose specification already holds the <one id> that would be minted first
    When <a type id> is requested for that Feature
    Then the minted id is one that Feature's specification does not hold

    Examples:
      | type              | one id           | a type id           |
      | claim ids         | claim id         | a claim id          |
      | constraint ids    | constraint id    | a constraint id     |
      | open question ids | open question id | an open question id |

  @B-042
  Scenario: A scenario id is checked against the Feature's scenario tags
    Given a manifest that opts scenario ids in to Nano ID with the prefix "S-"
    And a Feature with a scenario already tagged with the scenario id that would be minted first
    When a scenario id is requested for that Feature
    Then the minted id is one no scenario of that Feature is tagged with

  @B-059 @boundary
  Scenario Outline: A per-Feature id for a folder that holds no Feature is not printed
    Given a manifest that opts claim ids in to Nano ID
    And <a folder>
    When a claim id is requested for that folder
    Then nothing is written on standard output

    Examples:
      | a folder                                            |
      | a folder under the root that holds no specification |
      | a Feature folder in the legacy epics layout         |

  @B-060 @boundary
  Scenario Outline: A per-Feature id for a folder that holds no Feature is not found
    Given a manifest that opts claim ids in to Nano ID
    And <a folder>
    When a claim id is requested for that folder
    Then the request ends as it does for something named on the command line that is not found

    Examples:
      | a folder                                            |
      | a folder under the root that holds no specification |
      | a Feature folder in the legacy epics layout         |

  @B-043 @boundary
  Scenario Outline: A type opted in with no grammar is an invalid manifest
    Given a manifest pinned to schema 0.3.1
    And it opts ADR numbers in to Nano ID
    And it declares no grammar for ADR numbers
    When <an id> is requested
    Then the request ends as it does for an invalid manifest

    Examples:
      | an id           |
      | an ADR number   |
      | a lesson number |

  @B-044
  Scenario: Schema 0.3.1 accepts the id settings
    Given a manifest pinned to schema 0.3.1
    And it opts claim ids in to Nano ID
    When the tree is checked
    Then the check does not end as it does for an invalid manifest

  @B-045
  Scenario Outline: Schema 0.3.1 accepts the four new grammars
    Given a manifest pinned to schema 0.3.1
    And it declares a grammar for <type>
    When the tree is checked
    Then the check does not end as it does for an invalid manifest

    Examples:
      | type             |
      | ADR numbers      |
      | lesson numbers   |
      | decision numbers |
      | scenario ids     |

  @B-061 @boundary
  Scenario Outline: Opting in an epic, Feature or task type is an invalid manifest for minting
    Given a manifest that opts <type> in to Nano ID
    When <an id> is requested
    Then the request ends as it does for an invalid manifest

    Examples:
      | type        | an id         |
      | epic ids    | an epic id    |
      | Feature ids | a Feature id  |
      | task ids    | a task id     |
      | epic ids    | an ADR number |

  @B-062
  Scenario Outline: The check accepts an epic, Feature or task entry under schema 0.3.1
    Given a manifest pinned to schema 0.3.1
    And it opts <type> in to Nano ID
    When the tree is checked
    Then the check does not end as it does for an invalid manifest

    Examples:
      | type        |
      | epic ids    |
      | Feature ids |
      | task ids    |

  @B-047 @boundary
  Scenario Outline: No id is minted for an entry that names no strategy
    Given a manifest that lists ADR numbers <with no strategy>
    When an ADR number is requested
    Then no ADR number is minted

    Examples:
      | with no strategy                     |
      | without a strategy                   |
      | with a number in place of a strategy |

  @B-048 @boundary
  Scenario Outline: No id is minted through a strategy that does not exist
    Given a manifest that lists ADR numbers with the strategy "<strategy>"
    When an ADR number is requested
    Then no ADR number is minted

    Examples:
      | strategy |
      | sequence |
      | uuid     |

  @B-049 @boundary
  Scenario Outline: No id is minted from Nano ID settings that are not named settings
    Given a manifest that opts ADR numbers in to Nano ID with its Nano ID settings given as <a value>
    When an ADR number is requested
    Then no ADR number is minted

    Examples:
      | a value  |
      | a number |
      | a list   |
      | text     |

  @B-050 @boundary
  Scenario Outline: No id is minted from an alphabet that is empty or not text
    Given a manifest that opts ADR numbers in to Nano ID with <an alphabet>
    When an ADR number is requested
    Then no ADR number is minted

    Examples:
      | an alphabet                      |
      | an empty alphabet                |
      | a number in place of an alphabet |

  @B-051 @boundary
  Scenario Outline: No id is minted for a size that is not a positive whole number
    Given a manifest that opts ADR numbers in to Nano ID with a size of <size>
    When an ADR number is requested
    Then no ADR number is minted

    Examples:
      | size               |
      | 0                  |
      | -3                 |
      | 2.5                |
      | "8", given as text |

  @B-063
  Scenario Outline: A Feature that declares a prefix mints its record's form
    Given a manifest pinned to schema 0.4.0
    And a Feature that declares the claim prefix "ACTIVITY"
    And the manifest opts <type> in to Nano ID
    And its <type> record has the form "<form>"
    When <a type id> is requested for that Feature
    Then the minted id is the drawn characters with "<head>" ahead of them

    Examples:
      | type              | a type id           | form                     | head                 |
      | claim ids         | a claim id          | {prefix}-{id}            | ACTIVITY-            |
      | scenario ids      | a scenario id       | {prefix}-SCENARIO-{id}   | ACTIVITY-SCENARIO-   |
      | constraint ids    | a constraint id     | {prefix}-CONSTRAINT-{id} | ACTIVITY-CONSTRAINT- |
      | open question ids | an open question id | {prefix}-Q-{id}          | ACTIVITY-Q-          |
      | decision numbers  | a decision number   | {prefix}-D-{id}          | ACTIVITY-D-          |
      | claim ids         | a claim id          | {prefix}{id}             | ACTIVITY             |

  @B-065
  Scenario Outline: A request outside a prefixed Feature mints the record's plain text
    Given a manifest pinned to schema 0.4.0
    And the manifest opts <type> in to Nano ID
    And its <type> record has the plain text "<plain>"
    When <a type id> is requested
    Then the minted id is the drawn characters with <a head> ahead of them

    Examples:
      | type           | plain    | a type id                                              | a head  |
      | claim ids      | REQ-{id} | a claim id for a Feature that declares no claim prefix | "REQ-"  |
      | ADR numbers    | ADR-{id} | an ADR number                                          | "ADR-"  |
      | lesson numbers | {id}     | a lesson number                                        | nothing |

  @B-066 @boundary
  Scenario Outline: A record with no form keeps its plain text in a prefixed Feature
    Given a manifest pinned to schema 0.4.0
    And a Feature that declares the claim prefix "ACTIVITY"
    And the manifest opts <type> in to Nano ID
    And its <type> record has no form
    When <a type id> is requested for that Feature
    Then the minted id is the drawn characters with <a prefix> ahead of them

    Examples:
      | type              | a type id           | a prefix |
      | decision numbers  | a decision number   | nothing  |
      | constraint ids    | a constraint id     | "C-"     |
      | open question ids | an open question id | "OQ-"    |
      | scenario ids      | a scenario id       | "S-"     |

  @B-067 @boundary
  Scenario Outline: A claim id for a Feature with an invalid prefix takes its record's plain text
    Given a manifest pinned to schema 0.4.0 that opts claim ids in to Nano ID
    And its records leave their forms and plain texts to the shipped records
    And a Feature whose claim prefix is <prefix>
    When a claim id is requested for that Feature
    Then the minted id begins with "B-"
    And the check accepts it as a claim id of that Feature

    Examples:
      | prefix                                              |
      | white space alone                                   |
      | "S-", which gives its claims and scenarios one head |

  @B-067 @boundary
  Scenario Outline: A constraint id for a Feature with an invalid prefix takes its record's plain text
    Given a manifest pinned to schema 0.4.0 that opts constraint ids in to Nano ID
    And its constraint record has the form "{prefix}-C-{id}"
    And its other records leave their forms and plain texts to the shipped records
    And a Feature whose claim prefix is <prefix>
    When a constraint id is requested for that Feature
    Then the minted id begins with "C-"

    Examples:
      | prefix                                              |
      | white space alone                                   |
      | "S-", which gives its claims and scenarios one head |

  @B-068 @boundary
  Scenario: A draw its record's id rejects is not printed
    Given a manifest pinned to schema 0.4.0
    And a Feature that declares the claim prefix "ACTIVITY"
    And the manifest opts claim ids in to Nano ID with a size of 6
    And its claim record has the form "{prefix}-{id}" and an id that admits only three digits
    When a claim id is requested for that Feature
    Then nothing is written on standard output

  @B-070
  Scenario Outline: A prefixed claim, constraint or question id is checked against its Feature
    Given a manifest pinned to schema 0.4.0
    And a Feature that declares the claim prefix "ACTIVITY"
    And the manifest opts <type> in to Nano ID
    And its <type> record has the form "{prefix}-{id}"
    And that Feature's specification already holds the <one id> that would be minted first
    When <a type id> is requested for that Feature
    Then the minted id is one that Feature's specification does not hold

    Examples:
      | type              | one id           | a type id           |
      | claim ids         | claim id         | a claim id          |
      | constraint ids    | constraint id    | a constraint id     |
      | open question ids | open question id | an open question id |

  @B-071
  Scenario: A prefixed scenario id is checked against the Feature's scenario tags
    Given a manifest pinned to schema 0.4.0
    And a Feature that declares the claim prefix "ACTIVITY"
    And the manifest opts scenario ids in to Nano ID
    And its scenario record has the form "{prefix}-SCENARIO-{id}"
    And a scenario of that Feature is already tagged with the scenario id that would be minted first
    When a scenario id is requested for that Feature
    Then the minted id is one no scenario of that Feature is tagged with

  @B-072
  Scenario: A Nano ID after a Feature's head has the configured size
    Given a manifest pinned to schema 0.4.0
    And a Feature that declares the claim prefix "ACTIVITY"
    And the manifest opts claim ids in to Nano ID with a size of 9
    And its claim record has the form "{prefix}-{id}" and an id that admits nine characters
    When a claim id is requested for that Feature
    Then the minted id is 9 characters long after "ACTIVITY-"

  @B-073
  Scenario: A Snowflake id after a Feature's head carries the time since its epoch
    Given a manifest pinned to schema 0.4.0
    And a Feature that declares the claim prefix "ACTIVITY"
    And the manifest opts claim ids in to Snowflake with an epoch of 2026-01-01
    And its claim record has the form "{prefix}-{id}"
    And the time is noon on 2026-10-23, universal time
    When a claim id is requested for that Feature
    Then the minted id, decoded after "ACTIVITY-", holds the milliseconds from 2026-01-01 to noon on 2026-10-23

  @B-074
  Scenario Outline: Schema 0.4.0's records admit every type's default draws
    Given a manifest pinned to schema 0.4.0
    And its only record for <type> opts it in to <strategy> with no size or encoding
    When <a type id> is requested
    Then exactly one line is written on standard output

    Examples:
      | type              | strategy  | a type id                                                 |
      | ADR numbers       | Nano ID   | an ADR number                                             |
      | ADR numbers       | Snowflake | an ADR number                                             |
      | lesson numbers    | Nano ID   | a lesson number                                           |
      | lesson numbers    | Snowflake | a lesson number                                           |
      | decision numbers  | Nano ID   | a decision number for a Feature that declares no prefix   |
      | decision numbers  | Snowflake | a decision number for a Feature that declares no prefix   |
      | claim ids         | Nano ID   | a claim id for a Feature that declares no prefix          |
      | claim ids         | Snowflake | a claim id for a Feature that declares no prefix          |
      | claim ids         | Nano ID   | a claim id for a Feature that declares a prefix           |
      | constraint ids    | Nano ID   | a constraint id for a Feature that declares no prefix     |
      | constraint ids    | Snowflake | a constraint id for a Feature that declares no prefix     |
      | open question ids | Nano ID   | an open question id for a Feature that declares no prefix |
      | open question ids | Snowflake | an open question id for a Feature that declares no prefix |
      | scenario ids      | Nano ID   | a scenario id for a Feature that declares no prefix       |
      | scenario ids      | Snowflake | a scenario id for a Feature that declares no prefix       |

  @B-075 @boundary
  Scenario Outline: Schema 0.4.0 mints nothing under the old id settings
    Given a manifest pinned to schema 0.4.0
    And it holds <old setting>
    When a claim id is requested for a Feature that declares no prefix
    Then the request ends as it does for an invalid manifest

    Examples:
      | old setting                                |
      | a mint key that opts claim ids in          |
      | a claim grammar under identifiers          |
      | a claim record whose opt-in names a prefix |

  @B-076 @boundary
  Scenario: A scenario id the check would read as a claim tag is not printed
    Given a manifest pinned to schema 0.4.0
    And a Feature that declares the claim prefix "ACTIVITY"
    And its claim record has the form "{prefix}-S{id}"
    And its scenario record has the form "{prefix}-{id}" and an id that admits any text
    And the manifest opts scenario ids in to Nano ID with an alphabet of the one letter "S"
    When a scenario id is requested for that Feature
    Then nothing is written on standard output

  @B-077 @boundary
  Scenario: A claim id the check would read as a scenario tag is not printed
    Given a manifest pinned to schema 0.4.0
    And a Feature that declares the claim prefix "ACTIVITY"
    And its claim record has the form "{prefix}-{id}" and an id that admits any text
    And its scenario record has the form "{prefix}-S{id}"
    And the manifest opts claim ids in to Nano ID with an alphabet of the one letter "S"
    When a claim id is requested for that Feature
    Then nothing is written on standard output

  @B-077 @boundary
  Scenario: A claim id under a longer scenario head is printed
    Given a manifest pinned to schema 0.4.0
    And a Feature that declares the claim prefix "ACTIVITY"
    And its claim record has the form "{prefix}-{id}"
    And its scenario record has the form "{prefix}-SCENARIO-{id}"
    And the manifest opts claim ids in to Nano ID
    When a claim id is requested for that Feature
    Then the minted id begins with "ACTIVITY-"
    And the check accepts it as a claim id of that Feature
