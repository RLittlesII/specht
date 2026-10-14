---
title: Commands & Settings
description: CommandArgument/CommandOption attributes, required options, Validate(), flag values, dictionary/lookup options, and custom type converters.
type: reference
---

# Commands & Settings

## Core Structure

Every command's input is a `CommandSettings` subclass. Properties are decorated with:

- **`[CommandArgument]`** — positional parameters (order matters).
- **`[CommandOption]`** — named flags and options (order-independent).

## Argument & Option Syntax

Arguments use bracket notation for required/optional:

- `<name>` — required
- `[name]` — optional

Options use pipe-separated short/long forms:

```csharp
[CommandOption("-f|--force")]      // Both short and long
[CommandOption("--preserve")]      // Long form only
[CommandOption("-v")]              // Short form only
```

```csharp
public sealed class Settings : CommandSettings
{
    [CommandArgument(0, "<source>")]
    [Description("The source file to copy.")]
    public required string Source { get; init; }

    [CommandArgument(1, "[destination]")]
    [Description("The destination path (defaults to current directory).")]
    public string? Destination { get; init; }

    [CommandOption("-f|--force")]
    [Description("Overwrite existing files without prompting.")]
    public bool Force { get; init; }

    [CommandOption("-b|--buffer-size")]
    [Description("Buffer size in KB for the copy operation.")]
    [DefaultValue(64)]
    public int BufferSize { get; init; } = 64;
}
```

- Boolean properties automatically become flags — present means `true`, absent means `false`.
- `[DefaultValue]` both sets the default and shows it in help text.
- Array-typed properties capture multiple values; an array argument must have the highest position
  index.
- Enum-typed options are validated against the enum's members automatically, and the allowed values
  are listed in help.

## Required Options

Two mechanisms, for two different scopes:

**Single-option requirement** — `isRequired` on the attribute:

```csharp
[CommandOption("-e|--environment <TARGET>", isRequired: true)]
[Description("Target environment.")]
public required string Environment { get; init; }
```

Help text marks it automatically: `-e, --environment    Target environment. Required`.

**Cross-field requirements** (mutually exclusive options, "at least one of," etc.) — override
`Validate()`:

```csharp
public override ValidationResult Validate()
{
    if (string.IsNullOrEmpty(ConnectionString) && string.IsNullOrEmpty(Host))
    {
        return ValidationResult.Error("Provide either --connection-string or --host.");
    }

    return ValidationResult.Success();
}
```

`Validate()` runs before `ExecuteAsync` — never duplicate this check inside the command body.

## Flag Values (`FlagValue<T>`)

Use `FlagValue<T>` when a flag's _presence_ and its _value_ both carry meaning — omitted,
present-without-value, and present-with-value are three distinct states a nullable type cannot
express:

```csharp
[CommandOption("--port [PORT]")]
[Description("The port to listen on (default: 3000 if flag present).")]
[DefaultValue(3000)]
public required FlagValue<int> Port { get; init; }
```

```csharp
if (settings.Port.IsSet)
{
    Console.WriteLine($"Port: {settings.Port.Value}");
}
else
{
    Console.WriteLine("Port: not specified");
}
```

- `myapp` → `IsSet` = `false`
- `myapp --port` → `IsSet` = `true`, `Value` = type default (or `DefaultValue`)
- `myapp --port 8080` → `IsSet` = `true`, `Value` = `8080`

## Dictionary & Lookup Options

Accept repeated `key=value` pairs via `IDictionary<TKey, TValue>`, `IReadOnlyDictionary<TKey, TValue>`,
or `ILookup<TKey, TValue>` (multiple values per key):

```csharp
[CommandOption("--value <VALUE>")]
[Description("Configuration values in key=value format.")]
public IDictionary<string, int>? Values { get; set; }
```

`myapp --value port=8080 --value timeout=30` parses and type-converts both sides automatically.

Use `ILookup<TKey, TValue>` when the same key can repeat with different values
(`--lookup env=dev --lookup env=staging`):

```csharp
[CommandOption("--lookup <VALUE>")]
public ILookup<string, string>? Lookups { get; set; }
```

Prefer `IReadOnlyDictionary<TKey, TValue>` over `IDictionary<TKey, TValue>` when the command never
mutates the parsed values.

## Custom Type Converters

For domain-specific value types with no built-in conversion, implement `TypeConverter`:

```csharp
public sealed class PointConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string str)
        {
            var parts = str.Split(',');
            if (parts.Length == 2 &&
                int.TryParse(parts[0].Trim(), out var x) &&
                int.TryParse(parts[1].Trim(), out var y))
            {
                return new Point(x, y);
            }

            throw new FormatException($"Invalid point format: '{str}'. Expected format: X,Y (e.g., 10,20)");
        }

        return base.ConvertFrom(context, culture, value);
    }
}
```

Apply it via `[TypeConverter]` on the property:

```csharp
[CommandOption("--point <POINT>")]
[Description("A point in X,Y format (e.g., 10,20).")]
[TypeConverter(typeof(PointConverter))]
public required Point Location { get; init; }
```

If you own the target type, put `[TypeConverter(typeof(PointConverter))]` on the type itself instead
of repeating it on every property that uses it.

**Always** throw `FormatException` with a message that shows the expected format — Spectre surfaces
it to the user verbatim.
