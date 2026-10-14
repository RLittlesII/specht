A dotnet tool that checks a repository's specification documents against a versioned schema.

Every violation comes back with a file, a line and a rule id.

```text
src/specht/.spec/README.md(214): error SPEC031: § 9 has no row for claim B-007 [B-007]
```

## Install

See [Quick start](https://github.com/RLittlesII/specht#quick-start) in the repository.

## Commands

```sh
specht [--root <dir>] [--strict] [--json] [--schema-source <embedded|disk>]
specht schema list [--root <dir>]
specht schema pin [version] [--root <dir>]
specht init [--root <dir>] [--schemas] [--templates] [--all]
            [--upstream <address>] [--upstream-version <label>] [--upstream-hash <kind=hash>]...
specht --version
```

Every command and option is in the [command-line reference](https://github.com/RLittlesII/specht/blob/main/docs/cli.md).

Each rule has a page under [`docs/rules/v0.1.0/`](https://github.com/RLittlesII/specht/tree/main/docs/rules/v0.1.0).

## License

[MIT](https://github.com/RLittlesII/specht/blob/main/LICENSE.md)
