A dotnet tool that checks a repository's specification documents against a versioned schema.

Every violation comes back with a file, a line and a rule id.

```text
src/specht/.spec/README.md(214): error SPEC031: § 9 has no row for claim B-007 [B-007]
```

## Install

See [Quick start](https://github.com/RLittlesII/specht#quick-start) in the repository.

## Commands

```sh
specht [--root <dir>] [--report <path>] [--strict] [--json]
specht init [--root <dir>]
specht upgrade [--root <dir>]
specht --explain SPEC031
```

Each rule has a page under [`docs/rules/v1/`](https://github.com/RLittlesII/specht/tree/main/docs/rules/v1).

## License

[MIT](https://github.com/RLittlesII/specht/blob/main/LICENSE.md)
