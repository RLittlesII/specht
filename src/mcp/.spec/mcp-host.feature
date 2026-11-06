Feature: The MCP host
  As a coding agent or an editor's assistant in a repository that pins specht
  I want an MCP server started from one client configuration entry that answers from the tool the repository pinned
  So that I reach that tool version with no terminal session

  Background:
    Given a repository whose local tool manifest lists the tool
    And the pinned tool is restored
    And a manifest the check accepts

  @B-001
  Scenario Outline: A discovery request gets the server's identity
    Given a client configured with one entry that runs the MCP <installed>
    When the client sends the protocol's discovery request
    Then the server answers with the protocol revisions it supports, its capabilities and its identity

    Examples:
      | installed                       |
      | through the local tool manifest |
      | from a global install           |

  @B-002
  Scenario: The server speaks over its standard input and output
    When a client starts the server and calls it
    Then every protocol message passes over the server's standard input and output

  @B-003 @boundary
  Scenario: The server opens no network port
    When a session runs from start to end
    Then the server has opened no listening network port

  @B-004 @boundary
  Scenario: The server calls nothing outside the machine
    When a session runs from start to end
    Then the server has made no outbound network call

  @B-005
  Scenario: The standard output carries only protocol messages
    When a session runs from start to end
    Then everything the server wrote on its standard output is a protocol message

  @B-006
  Scenario: The server states its own package's version
    When the client sends the protocol's discovery request
    Then the version the server states for itself is the version of the MCP's own package

  @B-008
  Scenario: A session writes nothing
    When a session runs from start to end
    Then the server has created, modified or deleted no file under the root or elsewhere

  @B-009
  Scenario Outline: The server answers for the root the check would take
    Given the client starts the server <where>
    When the client calls the check tool
    Then the answer is for the repository

    Examples:
      | where                                            |
      | in the repository, naming no root                |
      | in another directory, naming the repository root |

  @B-010
  Scenario: A root that is not a directory stops the server
    Given the client names a root that is not a directory
    When the client starts the server
    Then a message naming that root as it was given is written on standard error
    And the server exits with the missing-input code before any session starts

  @B-011
  Scenario Outline: A repository without a usable manifest still answers discovery
    Given the repository's manifest <state>
    When the client sends the protocol's discovery request
    Then the server answers it

    Examples:
      | state                                |
      | is removed                           |
      | is replaced by one the check rejects |

  @B-012
  Scenario: Closing the server's input ends it cleanly
    Given a session in progress
    When the client closes the server's standard input
    Then the server exits with code 0

  @B-013
  Scenario: Help names the server's options
    When help is asked for the MCP
    Then the usage names the MCP's options
    And the MCP exits with code 0

  @B-014
  Scenario: A request naming the supported revision is answered
    Given a client asking for the protocol revision the server supports
    When the client sends a request
    Then the server answers it without a version error

  @B-015
  Scenario: The MCP page gives the configuration entries
    Given the tool's MCP documentation page
    Then it gives a client configuration entry that runs the MCP through the local tool manifest, naming the repository root
    And it gives one that runs the MCP from a global install, naming the repository root

  @B-016
  Scenario: The MCP page documents exactly the tools the server offers
    Given the tools the server offers
    When the tool's MCP documentation page is compared with them
    Then each tool has exactly one entry on the page
    And every tool with an entry is one the server offers

  @B-017
  Scenario: Every call runs the version the repository pins
    Given the repository pins the tool at a version other than the MCP's own
    When a client calls the check tool
    Then the tool that ran is the pinned version

  @B-017 @boundary
  Scenario: A tool the path resolves is never run while a pin exists
    Given another version of the tool is reachable from the path
    When a client calls the check tool
    Then the tool that ran is the pinned version and not the other

  @B-018 @boundary
  Scenario: A tool that is not restored is not restored by the server
    Given the pinned tool's restored copy is removed
    When a client calls the check tool
    Then the server has not restored the pinned tool

  @B-019
  Scenario: An unsupported revision gets the unsupported-version error
    Given a client asking for a protocol revision the server does not support
    When the client sends a request
    Then the server answers with the protocol's unsupported-version error

  @B-033
  Scenario: The unsupported-version error lists the supported revisions
    Given a client asking for a protocol revision the server does not support
    When the client sends a request
    Then the error lists the revisions the server supports

  @B-020
  Scenario: A pinned tool that is not restored is named in the error
    Given the pinned tool's restored copy is removed
    When a client calls the check tool
    Then the call fails with an error
    And the error names the tool and the version that is missing

  @B-021
  Scenario: The not-restored error says where to read how to restore
    Given the pinned tool's restored copy is removed
    When a client calls the check tool
    Then the error names where to read how to restore the tool

  @B-022 @boundary
  Scenario: A pinned tool that is not restored is never replaced
    Given the pinned tool's restored copy is removed
    And a global tool is installed
    When a client calls the check tool
    Then no other tool ran

  @B-023
  Scenario Outline: With no pin the global tool answers
    Given the repository's local tool manifest <state>
    And a global tool is installed
    When a client calls the check tool
    Then the global tool ran

    Examples:
      | state                    |
      | is removed               |
      | no longer lists the tool |

  @B-024
  Scenario: With no pin the answer is reported as unpinned
    Given the repository's local tool manifest no longer lists the tool
    And a global tool is installed
    When a client calls the check tool
    Then the result carries a part, apart from the part holding the command's output, that tells the user the answer is unpinned

  @B-025
  Scenario: The unpinned report says where to read how to pin
    Given the repository's local tool manifest no longer lists the tool
    And a global tool is installed
    When a client calls the check tool
    Then the report names where to read how to pin the tool

  @B-026
  Scenario: The tool's version is checked once per session
    Given a session in which a client calls the check tool twice
    When the session ends
    Then the server has checked the tool's version once

  @B-026
  Scenario: The global tool's version is checked once per session
    Given the repository's local tool manifest no longer lists the tool
    And a global tool is installed
    And a session in which a client calls the check tool twice
    When the session ends
    Then the server has checked the global tool's version once

  @B-027
  Scenario: A tool older than the minimum answers nothing
    Given the tool is older than the minimum version the server requires
    When a client calls the check tool
    Then the call fails with an error

  @B-034
  Scenario: A tool older than the minimum is given no work
    Given the tool is older than the minimum version the server requires
    When a client calls the check tool
    Then the tool has answered nothing

  @B-027
  Scenario: A global tool older than the minimum answers nothing
    Given the repository's local tool manifest no longer lists the tool
    And a global tool is installed that is older than the minimum version the server requires
    When a client calls the check tool
    Then the call fails with an error

  @B-034
  Scenario: A global tool older than the minimum is given no work
    Given the repository's local tool manifest no longer lists the tool
    And a global tool is installed that is older than the minimum version the server requires
    When a client calls the check tool
    Then the global tool has answered nothing

  @B-028
  Scenario: The too-old error names the minimum version
    Given the tool is older than the minimum version the server requires
    When a client calls the check tool
    Then the error names the minimum version required

  @B-029
  Scenario: The too-old error says where to read how to update
    Given the tool is older than the minimum version the server requires
    When a client calls the check tool
    Then the error names where to read how to update the tool

  @B-030
  Scenario: With no pin and no global tool the error says neither was found
    Given the repository's local tool manifest no longer lists the tool
    And no global tool is installed
    When a client calls the check tool
    Then the call fails with an error
    And the error names the pin and the global tool as not found

  @B-031
  Scenario: The MCP page has an entry for restoring, pinning and updating
    Given the tool's MCP documentation page
    Then it has one entry on restoring the tool
    And it has one entry on pinning the tool
    And it has one entry on updating the tool

  @B-032
  Scenario: A legacy initialize request gets an error naming the revision
    Given a client that opens with a legacy initialize request
    When the client sends it
    Then the server answers with an error
    And the error names the protocol revision the server supports

  @B-035
  Scenario: A repository release publishes the MCP package with the tool
    Given a repository release
    When its packages are published
    Then the MCP package is published to nuget.org with the tool and analysis packages
    And the three carry the repository release version
