# ULID .NET Tool
*from ByteAether*

[![License](https://img.shields.io/github/license/ByteAether/Ulid?logo=github&label=License)](https://github.com/ByteAether/Ulid/blob/main/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid.Cli?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.Ulid.Cli/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.Ulid.Cli?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.Ulid.Cli/)
![.NET 6.0+](https://img.shields.io/badge/.NET-6.0+-brightgreen)

`ByteAether.Ulid.Cli` is a .NET command-line tool for generating and inspecting ULIDs. It supports canonical Crockford's Base32, hexadecimal, and GUID representations, custom time and randomness components, batch operations, JSON output, and pipelines.

## ✨ Features

- **Flexible Installation**: Install globally or locally as the `ulid` command.
- **ULID Generation**:
	- Generate canonical [Crockford's Base32](https://www.crockford.com/base32.html), Hex, or GUID representations.
	- Customize time components using ISO 8601 strings, Unix epoch milliseconds, Hex, or Crockford's Base32.
	- Customize randomness components using Hex or Crockford's Base32.
	- Batch generation via `--count` / `-c`.
- **ULID Inspection**:
	- Decode canonical strings, Hex, or GUIDs provided as arguments or piped via `stdin`.
	- Extract specific components using `--part` for shell scripting (`grep`, `awk`, `cut`).
	- Output full breakdowns as human-readable text or structured JSON (`--json`).

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

Run a global installation with `ulid` and a local installation with `dotnet ulid`.

## 🚀 Usage

```sh
# Generate one ULID
ulid

# Generate five ULIDs
ulid --count 5

# Inspect one or more ULIDs
ulid 01AN4Z07BY79KA1307SR9X4MV3

# Inspect ULIDs from standard input, one per line
cat ulids.txt | ulid --json
```

The tool runs in **generation mode** when no positional arguments or piped standard input lines are provided; otherwise, it runs in **inspection mode**. Blank arguments and lines are ignored. Positional arguments are inspected before standard-input lines.

> *When run interactively in a terminal without positional arguments or redirected input (`stdin`), the tool immediately enters generation mode rather than waiting for `stdin` input.*

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

### Time and random input formats

`--time` accepts a timestamp or six time bytes:

| Format                                | Example                           |
|---------------------------------------|-----------------------------------|
| ISO 8601 date/time                    | `2026-09-29T12:00:00Z`            |
| Unix epoch milliseconds               | `1790683200000`                   |
| 10 Crockford's Base32 characters      | `01M3PGJEG0`                      |
| 12 hexadecimal characters             | `01A0ED093A00`                    |
| Six separated decimal bytes           | `"1,160,237,9,58,0"`              |
| Six separated `0x`-prefixed hex bytes | `"0x01 0xA0 0xED 0x09 0x3A 0x00"` |

Timestamps must be in the 48-bit ULID range: `0`–`281474976710655` milliseconds. Timestamps without a time-zone offset are assumed to be UTC, explicit offsets are converted to UTC. Ten-digit numeric values can be interpreted as Base32, and 12-digit numeric values as hexadecimal; use 11 digits or 13 or more digits to specify epoch milliseconds unambiguously (pad with leading zeros if needed).

`--random` accepts a custom randomness component in one of these forms:

| Format                                | Example                                               |
|---------------------------------------|-------------------------------------------------------|
| 16 Crockford's Base32 characters      | `BNKP4HVTSFWH61C7`                                    |
| 20 hexadecimal characters             | `5D6762477ACBF9130587`                                |
| Ten separated decimal bytes           | `"93,103,98,71,122,203,249,19,5,135"`                 |
| Ten separated `0x`-prefixed hex bytes | `"0x5D 0x67 0x62 0x47 0x7A 0xCB 0xF9 0x13 0x05 0x87"` |

Separated byte values can use commas, spaces, or hyphens. Decimal values must be between `0` and `255`.

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

JSON properties use the same names as `--part`. The full output for the sample ULID above is:

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

Without `--json`, `--part` outputs values one per line, making it easy to pipe into standard utilities:
```sh
cat ulids.txt | ulid --part timestamp | xargs -I {} echo "Processing timestamp: {}"
```

With `--part` and `--json`, one value is a JSON string and multiple values are an array of strings. Without `--json`, selected values are printed one per line. ULIDs with timestamps later than .NET's supported `DateTimeOffset` range are still inspectable, `timeIso` is `N/A` for those values.

## ⚙️ Options, errors, and exit codes

`--help` displays help and `--version` displays the tool version. Both return exit code `0`. Successful commands return `0`. Invalid arguments, ULID inputs, formats, parts, counts, or time/random values return `1` and report an error to standard error.

Inspection output is written only after all inputs have been validated. Invalid input does not produce partial inspection output. The `--count`, `--time`, and `--random` options affect generation only. `--format` is validated regardless of mode but affects generated output only.

## 📜 License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/ByteAether/Ulid/blob/main/LICENSE) file for details.
