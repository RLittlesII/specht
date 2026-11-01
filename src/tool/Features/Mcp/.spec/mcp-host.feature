Feature: The MCP host
  As a coding agent or an editor's assistant in a repository that pins specht
  I want the tool's own MCP server started from one client configuration entry
  So that I reach the tool version the repository pinned with no terminal session

  Background:
    Given a repository whose local tool manifest lists the tool
    And a manifest the check accepts

  @B-001
  Scenario: One configuration entry starts the server
    Given a client configured with one entry that runs the server command through the local tool manifest
    When the client starts the server
    Then the server completes the protocol's initialize handshake

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
  Scenario: The server states the tool's own version
    When the server completes the initialize handshake
    Then the version it states is the version the tool prints for itself from the same installation

  @B-007
  Scenario: The server needs no package beyond the tool
    Given the tool's package is the only one installed
    When a client starts the server
    Then the server completes the initialize handshake

  @B-008
  Scenario: A session writes nothing
    When a session runs from start to end
    Then no file under the root or elsewhere has been created, modified or deleted

  @B-009
  Scenario Outline: The server answers for the root the check would take
    Given the client starts the server <where>
    When the server is asked for an answer
    Then it answers for the repository

    Examples:
      | where                                           |
      | in the repository, naming no root               |
      | in another directory, naming the repository root |

  @B-010
  Scenario: A root that is not a directory stops the server
    Given the client names a root that is not a directory
    When the client starts the server
    Then a message naming that root as it was given is written on standard error
    And the server exits with the missing-input code before any session starts

  @B-011
  Scenario Outline: A repository without a usable manifest still gets a session
    Given the repository's manifest <state>
    When a client starts the server
    Then the server completes the initialize handshake

    Examples:
      | state                         |
      | is missing                    |
      | is one the check rejects      |

  @B-012
  Scenario: Closing the server's input ends it cleanly
    Given a session in progress
    When the client closes the server's standard input
    Then the server exits with code 0

  @B-013
  Scenario: Help names the server command's options
    When help is asked for the server command
    Then the usage names the server command's options
    And the tool exits with code 0

  @B-014
  Scenario Outline: The server agrees a protocol revision
    Given a client asking for <asked>
    When the initialize handshake completes
    Then the server has answered with <answered>

    Examples:
      | asked                                  | answered                        |
      | a revision the server supports         | that revision                   |
      | a revision the server does not support | the newest revision it supports |
