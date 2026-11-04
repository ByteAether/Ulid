# ULID .NET Tool
*from ByteAether*

[![License](https://img.shields.io/github/license/ByteAether/Ulid?logo=github&label=License)](https://github.com/ByteAether/Ulid/blob/main/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid.Cli?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.Ulid.Cli/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.Ulid.Cli?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.Ulid.Cli/)
![.NET 6.0+](https://img.shields.io/badge/.NET-6.0+-brightgreen)

`ByteAether.Ulid.Cli` is a .NET command-line tool for generating and inspecting ULIDs. It supports canonical Crockford's Base32, hexadecimal, and GUID representations, custom time and randomness components, batch operations, JSON output, and pipelines.

## ✨ Features

- **Global or local installation** as the `ulid` command.
- **Generates** ULIDs:
    - as **canonical** [Crockford's Base32](https://www.crockford.com/base32.html), hexadecimal, or GUID output;
    - with a **custom time component** given as an ISO 8601 string, Unix epoch milliseconds, hexadecimal, or Crockford's Base32;
    - with a **custom random component** given as hexadecimal or Crockford's Base32;
    - in **batches** with `--count` / `-c`.
- **Inspects** ULIDs:
    - **decodes** canonical strings, hexadecimal, or GUIDs from arguments or from `stdin`;
    - extracts **single components** with `--part` for shell scripts (`grep`, `awk`, `cut`);
    - prints **full breakdowns** as text or as JSON (`--json`).

## 💾 Installation

Install globally:

```sh
dotnet tool install -g ByteAether.Ulid.Cli
```

Or install locally in a repository:

```sh
dotnet new tool-manifest # if the repository does not have a tool manifest
dotnet tool install ByteAether.Ulid.Cli
```

Run a global installation with `ulid` and a local installation with `dotnet ulid`. The tool requires an installed .NET runtime.

Update or remove it with `dotnet tool update` and `dotnet tool uninstall`, adding `-g` for a global installation.

## 🚀 Usage

```sh
# Generate one ULID
ulid

# Generate five ULIDs (short form: ulid -c 5)
ulid --count 5

# Inspect one or more ULIDs
ulid 01M3PGJEG0BNKP4HVTSFWH61C7

# Inspect ULIDs from standard input, one per line
cat ulids.txt | ulid --json
```

The tool runs in generation mode when there are no positional arguments and no piped standard input lines. In an interactive terminal with no arguments and no redirected input, it generates immediately instead of waiting for input. Otherwise it runs in inspection mode. Blank arguments and lines are ignored. Positional arguments are inspected before standard-input lines.

### Quick reference

| Task                                      | Command                                                |
|-------------------------------------------|--------------------------------------------------------|
| Generate one ULID                         | `ulid`                                                 |
| Generate five ULIDs as GUIDs              | `ulid -c 5 -f guid`                                    |
| Generate with a fixed time and randomness | `ulid -t 2026-09-29T12:00:00Z -r 5D6762477ACBF9130587` |
| Inspect a ULID                            | `ulid 01M3PGJEG0BNKP4HVTSFWH61C7`                      |
| Print one component                       | `ulid 01M3PGJEG0BNKP4HVTSFWH61C7 --part timeIso`       |
| Inspect many ULIDs from a file as JSON    | `cat ulids.txt \| ulid --json`                         |

The options for each mode are described in the Inspect ULIDs and Generate ULIDs sections below.

## 🔍 Inspect ULIDs

```sh
ulid <ulid...> [options]
ulid [options] < ulids.txt
```

Input can be supplied as positional arguments, one ULID per standard-input line, or both. The tool accepts a 26-character Crockford's Base32 ULID, a 32-character hexadecimal ULID, or a GUID. Hex input may also include a leading `0x` and separators (spaces, hyphens, colons, or commas).

| Option                | Description                                                          |
|-----------------------|----------------------------------------------------------------------|
| `-j`, `--json`        | Output one JSON object or, for multiple inputs, an array of objects. |
| `-p`, `--part <name>` | Output only the named component. Names are case-insensitive.         |

Without `--part`, the tool prints one breakdown per input with these fields:

| Text field            | Description                                        | `--part` name |
|-----------------------|----------------------------------------------------|---------------|
| `Ulid`                | Canonical 26-character Crockford's Base32 ULID     | `ulid`        |
| `Hex`                 | 32-character hexadecimal ULID                      | `hex`         |
| `Time`                | 10-character Crockford's Base32 time component     | `time`        |
| `Time (Hex)`          | 12-character hexadecimal time component            | `timeHex`     |
| `Time (ISO 8601)`     | UTC timestamp in `yyyy-MM-ddTHH:mm:ss.fffZ` format | `timeIso`     |
| `Timestamp (Unix ms)` | Timestamp in milliseconds since the Unix epoch     | `timestamp`   |
| `Random`              | 16-character Crockford's Base32 random component   | `random`      |
| `Random (Hex)`        | 20-character hexadecimal random component          | `randomHex`   |

JSON properties use the same names as `--part`. The full output for `01M3PGJEG0BNKP4HVTSFWH61C7` is:

```text
Ulid:                01M3PGJEG0BNKP4HVTSFWH61C7
Hex:                 01A0ED093A005D6762477ACBF9130587
Time:                01M3PGJEG0
Time (Hex):          01A0ED093A00
Time (ISO 8601):     2026-09-29T12:00:00.000Z
Timestamp (Unix ms): 1790683200000
Random:              BNKP4HVTSFWH61C7
Random (Hex):        5D6762477ACBF9130587
```

For example, extract a field as plain text:

```sh
ulid 01M3PGJEG0BNKP4HVTSFWH61C7 --part timeHex
```

```text
01A0ED093A00
```

Request the same field as JSON:

```sh
ulid 01M3PGJEG0BNKP4HVTSFWH61C7 --part timeHex --json
```

```json
"01A0ED093A00"
```

For multiple inputs, `--part` with `--json` returns an array:

```sh
printf '%s\n' 01M3PGJEG0BNKP4HVTSFWH61C7 01M3PGJEG0BNKP4HVTSFWH61C7 | ulid --part timeHex --json
```

```json
[
  "01A0ED093A00",
  "01A0ED093A00"
]
```

A full JSON breakdown is:

```json
{
  "ulid": "01M3PGJEG0BNKP4HVTSFWH61C7",
  "hex": "01A0ED093A005D6762477ACBF9130587",
  "time": "01M3PGJEG0",
  "timeHex": "01A0ED093A00",
  "timeIso": "2026-09-29T12:00:00.000Z",
  "timestamp": 1790683200000,
  "random": "BNKP4HVTSFWH61C7",
  "randomHex": "5D6762477ACBF9130587"
}
```

Without `--json`, `--part` outputs values one per line, so they can be piped into standard utilities:
```sh
cat ulids.txt | ulid --part timestamp | xargs -I {} echo "Processing timestamp: {}"
```

With `--part` and `--json`, one value is a JSON string and multiple values are an array of strings. Without `--json`, selected values are printed one per line. ULIDs with timestamps later than .NET's supported `DateTimeOffset` range are still inspectable; `timeIso` is `N/A` for those values.

## ⚡ Generate ULIDs

```sh
ulid [options]
```

| Option                    | Description                                                                  |
|---------------------------|------------------------------------------------------------------------------|
| `-c`, `--count <count>`   | Number of ULIDs to generate (default: `1`; must be at least `1`).            |
| `-t`, `--time <value>`    | Timestamp or custom 6-byte time component. Defaults to the current UTC time. |
| `-r`, `--random <value>`  | Custom 10-byte random component. Defaults to generated randomness.           |
| `-f`, `--format <format>` | Output format: `base32` (default), `hex`, or `guid`.                         |
| `-j`, `--json`            | Output a JSON string for one ULID or an array of strings for multiple ULIDs. |

The accepted `--time` and `--random` formats are listed in the input formats section below.

### Output formats

| Format   | Output                                         |
|----------|------------------------------------------------|
| `base32` | 26-character canonical Crockford's Base32 ULID |
| `hex`    | 32 uppercase hexadecimal characters            |
| `guid`   | Standard hyphenated GUID                       |

Format names are case-insensitive. A custom `--random` value is reused for every ULID in a batch; if both `--time` and `--random` are fixed, each generated ULID in the batch is identical.

### Generation examples

Generate with explicit time and random components:

```sh
ulid --time 01A0ED093A00 --random 5D6762477ACBF9130587
```

```text
01M3PGJEG0BNKP4HVTSFWH61C7
```

The same ULID in hexadecimal or GUID format:

```sh
ulid --time 01A0ED093A00 --random 5D6762477ACBF9130587 --format hex
ulid --time 01A0ED093A00 --random 5D6762477ACBF9130587 --format guid
```

```text
01A0ED093A005D6762477ACBF9130587
01a0ed09-3a00-5d67-6247-7acbf9130587
```

Generate two ULIDs as JSON:

```sh
ulid --count 2 --time 01A0ED093A00 --random 5D6762477ACBF9130587 --json
```

```json
[
  "01M3PGJEG0BNKP4HVTSFWH61C7",
  "01M3PGJEG0BNKP4HVTSFWH61C7"
]
```

## 🧮 Input formats

`--time` accepts a timestamp or six time bytes:

| Format                                | Example                           |
|---------------------------------------|-----------------------------------|
| ISO 8601 date/time                    | `2026-09-29T12:00:00Z`            |
| Unix epoch milliseconds               | `1790683200000`                   |
| 10 Crockford's Base32 characters      | `01M3PGJEG0`                      |
| 12 hexadecimal characters             | `01A0ED093A00`                    |
| Six separated decimal bytes           | `"1,160,237,9,58,0"`              |
| Six separated `0x`-prefixed hex bytes | `"0x01 0xA0 0xED 0x09 0x3A 0x00"` |

Timestamps must be in the 48-bit ULID range: `0`–`281474976710655` milliseconds. Timestamps without a time-zone offset are assumed to be UTC; explicit offsets are converted to UTC.

> [!NOTE]
> A 10-digit numeric value is read as Crockford's Base32 and a 12-digit numeric value as hexadecimal, not as epoch milliseconds. Epoch milliseconds have 13 digits for dates from 2001-09-09 onward, so this only affects earlier dates. Pad such a value with leading zeros to 11 or 13 digits to read it as epoch milliseconds.

`--random` accepts a custom randomness component in one of these forms:

| Format                                | Example                                               |
|---------------------------------------|-------------------------------------------------------|
| 16 Crockford's Base32 characters      | `BNKP4HVTSFWH61C7`                                    |
| 20 hexadecimal characters             | `5D6762477ACBF9130587`                                |
| Ten separated decimal bytes           | `"93,103,98,71,122,203,249,19,5,135"`                 |
| Ten separated `0x`-prefixed hex bytes | `"0x5D 0x67 0x62 0x47 0x7A 0xCB 0xF9 0x13 0x05 0x87"` |

Separated byte values can use commas, spaces, or hyphens. Decimal values must be between `0` and `255`.

## 🍳 Recipes

Use a ULID as a unique, time-sortable file name:

```sh
touch "$(ulid).log"
```

Find the first and last possible ULID of a date, for example to filter a table by time range:

```sh
ulid --time 2026-01-01T00:00:00Z --random 00000000000000000000
ulid --time 2026-01-01T00:00:00Z --random FFFFFFFFFFFFFFFFFFFF
```

```text
01KDVDNA000000000000000000
01KDVDNA00ZZZZZZZZZZZZZZZZ
```

Read when a ULID from a log line was created:

```sh
ulid 01KDVDNA000000000000000000 --part timeIso
```

```text
2026-01-01T00:00:00.000Z
```

Convert a list of ULIDs to Unix timestamps:

```sh
cat ulids.txt | ulid --part timestamp
```

## ⚙️ Exit codes

`--help` displays help and `--version` displays the tool version. These and every successful command exit with `0`. Any invalid argument or input (ULID, format, part, count, time, or random value) exits with `1` and writes an error to standard error.

Inspection output is written only after all inputs have been validated. Invalid input does not produce partial inspection output. The `--count`, `--time`, and `--random` options affect generation only. `--format` is validated regardless of mode but affects generated output only.

## 📜 License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/ByteAether/Ulid/blob/main/LICENSE) file for details.
