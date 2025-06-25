# ![ULID from ByteAether](assets/header.png)

[![License](https://img.shields.io/github/license/ByteAether/Ulid?logo=github&label=License)](https://github.com/ByteAether/Ulid/blob/main/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.Ulid/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.Ulid?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.Ulid/)
[![GitHub Build Status](https://img.shields.io/github/actions/workflow/status/ByteAether/Ulid/build-and-test.yml?logo=github&label=Build%20%26%20Test)](https://github.com/ByteAether/Ulid/actions/workflows/build-and-test.yml)
[![GitHub Security](https://img.shields.io/github/actions/workflow/status/ByteAether/Ulid/codeql.yml?logo=github&label=Security%20Validation)](https://github.com/ByteAether/Ulid/actions/workflows/codeql.yml)

A high-performance .NET implementation of ULIDs (Universally Unique Lexicographically Sortable Identifiers) that fully complies with the [official ULID specification](https://github.com/ulid/spec).

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [API](#api)
- [Integration with Other Libraries](#integration-with-other-libraries)
- [Benchmarking](#benchmarking)
- [Prior Art](#prior-art)
- [Contributing](#contributing)
- [License](#license)

## Introduction

<img align="right" width="100px" src="assets/logo.png" />

ULIDs are identifiers designed to be universally unique and lexicographically sortable, making them ideal for distributed systems and time-ordered data. Unlike GUIDs, ULIDs are both sortable and human-readable. This library provides a robust and fully compliant .NET implementation of ULIDs, addressing some limitations found in other implementations.

Additionally, this implementation addresses a potential issue in the official specification where generating multiple ULIDs within the same millisecond can cause the "random" part of the ULID to overflow, leading to an overflow exception being thrown. To ensure dependability and guarantee the generation of unique ULIDs, this implementation allows overflow to increment the "timestamp" part of the ULID, thereby eliminating the possibility of randomly occuring exception.

Relevant issue with same suggestion is opened on official ULID specification: [Guarantee a minimum number of IDs before overflow of the random component #39](https://github.com/ulid/spec/issues/39#issuecomment-2252145597)

For almost all systems in the world, both GUID and integer IDs should be abandoned in favor of ULIDs. GUIDs, while unique, lack sortability and readability, making them less efficient for indexing and querying. Integer IDs, on the other hand, are sortable but not universally unique, leading to potential conflicts in distributed systems. ULIDs combine the best of both worlds, offering both uniqueness and sortability, making them an ideal choice for modern applications that require scalable and efficient identifier generation. This library provides a robust and reliable implementation of ULIDs, ensuring that your application can benefit from these advantages without compromising on performance or compliance with the official specification.

## Features

![.NET 9.0](https://img.shields.io/badge/.NET-9.0-brightgreen)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-brightgreen)
![.NET 7.0](https://img.shields.io/badge/.NET-7.0-green)
![.NET 6.0](https://img.shields.io/badge/.NET-6.0-green)
![.NET 5.0](https://img.shields.io/badge/.NET-5.0-yellow)
![.NET Standard 2.1](https://img.shields.io/badge/.NET-Standard_2.1-yellow)
![.NET Standard 2.0](https://img.shields.io/badge/.NET-Standard_2.0-green)

- **Universally Unique**: Ensures global uniqueness across systems.
- **Sortable**: Lexicographically ordered for time-based sorting.
- **Fast and Efficient**: Optimized for high performance and low memory usage.
- **Specification-Compliant**: Fully adheres to the ULID specification.
- **Interoperable**: Includes conversion methods to and from GUIDs, [Crockford's Base32](https://www.crockford.com/base32.html) strings, and byte arrays.
- **Ahead-of-Time (AoT) Compilation Compatible**: Fully compatible with AoT compilation for improved startup performance and smaller binary sizes.
- **Error-Free Generation**: Prevents overflow exceptions by incrementing timestamps during random part overflow.

## Installation

Install the latest stable package via NuGet:

```sh
dotnet add package ByteAether.Ulid
```

Use the `--version` option to specify a [preview version](https://www.nuget.org/packages/ByteAether.Ulid/absoluteLatest) to install.

## Usage

Here is a basic example of how to use the ULID implementation:

```csharp
using System;

class Program
{
    static void Main()
    {
        // Create a new ULID
        var ulid = Ulid.New();

        // Convert to byte array and back
        byte[] byteArray = ulid.ToByteArray();
        var ulidFromByteArray = Ulid.New(byteArray);

        // Convert to GUID and back
        Guid guid = ulid.ToGuid();
        var ulidFromGuid = Ulid.New(guid);

        // Convert to string and back
        string ulidString = ulid.ToString();
        var ulidFromString = Ulid.Parse(ulidString);

        Console.WriteLine($"ULID: {ulid}, GUID: {guid}, String: {ulidString}");
    }
}
```

## API

The `Ulid` implementation provides the following properties and methods:

### Creation

- `Ulid.DefaultIsMonotonic = true`\
Sets the default behavior for generating ULIDs unless overridden during generation. If `true` (default), ensures monotonicity during timestamp collisions.
- `Ulid.New(bool? isMonotonic = null)`\
Generates a new ULID. If `isMonotonic` is `null` (default), uses `Ulid.DefaultIsMonotonic` for monotonicity setting.
- `Ulid.New(DateTimeOffset dateTimeOffset, bool? isMonotonic = null)`\
Generates a new ULID using the specified `DateTimeOffset`.
- `Ulid.New(long timestamp, bool? isMonotonic = null)`\
Generates a new ULID using the specified Unix timestamp in milliseconds (`long`).
- `Ulid.New(DateTimeOffset dateTimeOffset, Span<byte> random)`\
Generates a new ULID using the specified `DateTimeOffset` and a pre-existing random byte array.
- `Ulid.New(long timestamp, Span<byte> random)`\
Generates a new ULID using the specified Unix timestamp in milliseconds (`long`) and a pre-existing random byte array.
- `Ulid.New(ReadOnlySpan<byte> bytes)`\
Creates a ULID from an existing byte array.
- `Ulid.New(Guid guid)`\
Create from existing `Guid`.

### Checking Validity

- `Ulid.IsValid(string ulidString)`\
Validates if the given string is a valid ULID.
- `Ulid.IsValid(ReadOnlySpan<char> ulidString)`\
Validates if the given span of characters is a valid ULID.
- `Ulid.IsValid(ReadOnlySpan<byte> ulidBytes)`\
Validates if the given byte array represents a valid ULID.

### Parsing

- `Ulid.Parse(ReadOnlySpan<char> chars, IFormatProvider? provider = null)`\
Parses a ULID from a character span in canonical format. The `IFormatProvider` is ignored.
- `Ulid.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Ulid result)`\
Tries to parse a ULID from a character span in canonical format. Returns `true` if successful.
- `Ulid.Parse(string s, IFormatProvider? provider = null)`\
Parses a ULID from a string in canonical format. The `IFormatProvider` is ignored.
- `Ulid.TryParse(string? s, IFormatProvider? provider, out Ulid result)`\
Tries to parse a ULID from a string in canonical format. Returns `true` if successful.

### Properties

- `.Time`\
Gets the timestamp component of the ULID as a `DateTimeOffset`.
- `.TimeBytes`\
Gets the time component of the ULID as a `ReadOnlySpan<byte>`.
- `.Random`\
Gets the random component of the ULID as a `ReadOnlySpan<byte>`.
- `Ulid.Empty`\
Represents an empty ULID, equivalent to `default(Ulid)` and `Ulid.New(new byte[16])`.

### Conversion Methods

- `.AsByteSpan()`\
Provides a `ReadOnlySpan<byte>` representing the contents of the ULID.
- `.ToByteArray()`\
Converts the ULID to a byte array.
- `.ToGuid()`\
Converts the ULID to a `Guid`.
- `.ToString(string? format = null, IFormatProvider? formatProvider = null)`\
Converts the ULID to a canonical string representation. Format arguments are ignored.

### Comparison Operators

- Supports all comparison operators:\
`==`, `!=`, `<`, `<=`, `>`, `>=`.
- Implements standard comparison and equality methods:\
`CompareTo`, `Equals`, `GetHashCode`.
- Provides implicit operators to and from `Guid`.

## Integration with Other Libraries

### ASP.NET Core

Supports seamless integration as a route or query parameter with built-in `TypeConverter`.

### System.Text.Json (.NET 5.0+)

Includes a `JsonConverter` for easy serialization and deserialization.

### EF Core Integration

To use ULIDs as primary keys or properties in Entity Framework Core, you can create a custom **ValueConverter** to handle the conversion between `Ulid` and `byte[]`. Here's how to do it:

#### 1. Create a custom `ValueConverter` to convert `Ulid` to `byte[]` and vice versa:

```csharp
public class UlidToBytesConverter : ValueConverter<Ulid, byte[]>
{
	private static readonly ConverterMappingHints DefaultHints = new(size: 16);

	public UlidToBytesConverter() : this(defaultHints) { }

	public UlidToBytesConverter(ConverterMappingHints? mappingHints = null)
		: base(
			convertToProviderExpression: x => x.ToByteArray(),
			convertFromProviderExpression: x => Ulid.New(x),
			mappingHints: defaultHints.With(mappingHints)
		)
	{ }
}
```

#### 2. Register the Converter in ConfigureConventions

Once the converter is created, you need to register it in your `DbContext`'s `ConfigureConventions` virtual method to apply it to `Ulid` properties:

```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
	// ...
	configurationBuilder
		.Properties<Ulid>()
		.HaveConversion<UlidToBytesConverter>();
	// ...
}
```

### Dapper Integration
To use ULIDs with Dapper, you can create a custom **TypeHandler** to convert between `Ulid` and `byte[]`. Here's how to set it up:

#### 1. Create the ULID Type Handler

```csharp
using Dapper;
using System.Data;

public class UlidTypeHandler : SqlMapper.TypeHandler<Ulid>
{
    public override void SetValue(IDbDataParameter parameter, Ulid value)
    {
        parameter.Value = value.ToByteArray();
    }

    public override Ulid Parse(object value)
    {
        return Ulid.New((byte[])value);
    }
}
```

#### 2. Register the Type Handler
After creating the `UlidTypeHandler`, you need to register it with Dapper. You can do this during application startup (e.g., in the `Main` method or `ConfigureServices` for ASP.NET Core).

```csharp
Dapper.SqlMapper.AddTypeHandler(new UlidTypeHandler());
```

### MessagePack Integration
To use ULIDs with **MessagePack**, you can create a custom **MessagePackResolver** to handle the serialization and deserialization of `Ulid` as `byte[]`. Here's how to set it up:

#### 1. Create the Custom Formatter

First, create a custom formatter for `Ulid` to handle its conversion to and from `byte[]`:

```csharp
using MessagePack;
using MessagePack.Formatters;

public class UlidFormatter : IMessagePackFormatter<Ulid>
{
    public Ulid Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var bytes = reader.ReadByteArray();
        return Ulid.New(bytes);
    }

    public void Serialize(ref MessagePackWriter writer, Ulid value, MessagePackSerializerOptions options)
    {
        writer.Write(value.ToByteArray());
    }
}
```

#### 2. Register the Formatter

Once the `UlidFormatter` is created, you need to register it with the `MessagePackSerializer` to handle the `Ulid` type.

```csharp
MessagePack.Resolvers.CompositeResolver.Register(
    new IMessagePackFormatter[] { new UlidFormatter() },
    MessagePack.Resolvers.StandardResolver.GetFormatterWithVerify<Ulid>()
);
```

Alternatively, you can register the formatter globally when configuring MessagePack options:

```csharp
MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions
    .WithResolver(MessagePack.Resolvers.CompositeResolver.Create(
        new IMessagePackFormatter[] { new UlidFormatter() },
        MessagePack.Resolvers.StandardResolver.Instance
    ));
```

### Newtonsoft.Json Integration

To use ULIDs with **Newtonsoft.Json**, you need to create a custom **JsonConverter** to handle the serialization and deserialization of ULID values. Here's how to set it up:

#### 1. Create the Custom JsonConverter

First, create a custom `JsonConverter` for `Ulid` to serialize and deserialize it as a `string`:

```csharp
using Newtonsoft.Json;

public class UlidJsonConverter : JsonConverter<Ulid>
{
    public override Ulid ReadJson(JsonReader reader, Type objectType, Ulid existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var value = (string)reader.Value;
        return Ulid.Parse(value);
    }

    public override void WriteJson(JsonWriter writer, Ulid value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
```

#### 2. Register the JsonConverter

Once the `UlidJsonConverter` is created, you need to register it with **Newtonsoft.Json** to handle `Ulid` serialization and deserialization. You can register the converter globally when configuring your JSON settings:

```csharp
JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
    Converters = new List<JsonConverter> { new UlidJsonConverter() }
};
```

Alternatively, you can specify the converter explicitly in individual serialization or deserialization calls:

```csharp
var settings = new JsonSerializerSettings();
settings.Converters.Add(new UlidJsonConverter());

var json = JsonConvert.SerializeObject(myObject, settings);
var deserializedObject = JsonConvert.DeserializeObject<MyObject>(json, settings);
```

## Benchmarking
To ensure the performance and efficiency of this ULID implementation, benchmarking was conducted using [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet).

For comparison, [NetUlid](https://github.com/ultimicro/netulid) 2.1.0, [Ulid](https://github.com/Cysharp/Ulid) 1.3.4 and [NUlid](https://github.com/RobThree/NUlid) 1.7.3 implementations were benchmarked alongside ByteAether.Ulid v1.1.1.

Benchmark scenarios also include comparisons against `Guid`, where functionality overlaps, such as creation, parsing, and byte conversions.

The following benchmarks were performed:
```
BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5965/22H2/2022Update)
AMD Ryzen 7 3700X, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.301
  [Host]     : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=DefaultJob

| Type            | Method         | Mean        | Error     | Gen0   | Allocated |
|---------------- |--------------- |------------:|----------:|-------:|----------:|
| Generate        | ByteAetherUlid |  52.5884 ns | 0.1624 ns |      - |         - |
| Generate        | NetUlid *(1)   | 157.7909 ns | 0.2937 ns | 0.0095 |      80 B |
| Generate        | NUlid *(2)     |  59.5673 ns | 0.1030 ns |      - |         - |

| GenerateNonMono | ByteAetherUlid |  94.7806 ns | 0.1406 ns |      - |         - |
| GenerateNonMono | Ulid *(3,4)    |  43.7608 ns | 0.0845 ns |      - |         - |
| GenerateNonMono | NUlid          |  98.1727 ns | 0.2122 ns |      - |         - |
| GenerateNonMono | Guid *(5)      |  46.5160 ns | 0.0599 ns |      - |         - |
| GenerateNonMono | GuidV7 *(3,5)  |  81.2190 ns | 0.1536 ns |      - |         - |

| FromByteArray   | ByteAetherUlid |   0.2563 ns | 0.0075 ns |      - |         - |
| FromByteArray   | NetUlid        |   0.6812 ns | 0.0086 ns |      - |         - |
| FromByteArray   | Ulid           |   6.9435 ns | 0.0100 ns |      - |         - |
| FromByteArray   | NUlid          |   1.9263 ns | 0.0133 ns |      - |         - |
| FromByteArray   | Guid           |   0.0224 ns | 0.0044 ns |      - |         - |

| FromGuid        | ByteAetherUlid |   1.4399 ns | 0.0089 ns |      - |         - |
| FromGuid        | NetUlid        |   4.6075 ns | 0.0257 ns | 0.0048 |      40 B |
| FromGuid        | Ulid           |   1.4525 ns | 0.0102 ns |      - |         - |
| FromGuid        | NUlid          |   4.7050 ns | 0.0204 ns |      - |         - |

| FromString      | ByteAetherUlid |  14.5609 ns | 0.0296 ns |      - |         - |
| FromString      | NetUlid        |  26.9962 ns | 0.2493 ns |      - |         - |
| FromString      | Ulid           |  15.5439 ns | 0.3153 ns |      - |         - |
| FromString      | NUlid          |  57.0196 ns | 0.0652 ns | 0.0124 |     104 B |
| FromString      | Guid           |  23.1006 ns | 0.2679 ns |      - |         - |

| ToByteArray     | ByteAetherUlid |   3.2943 ns | 0.0420 ns | 0.0048 |      40 B |
| ToByteArray     | NetUlid        |   9.1148 ns | 0.1869 ns | 0.0048 |      40 B |
| ToByteArray     | Ulid           |   3.2581 ns | 0.0123 ns | 0.0048 |      40 B |
| ToByteArray     | NUlid          |   6.4877 ns | 0.0203 ns | 0.0048 |      40 B |

| ToGuid          | ByteAetherUlid |   0.2531 ns | 0.0065 ns |      - |         - |
| ToGuid          | NetUlid        |  11.6821 ns | 0.0379 ns | 0.0048 |      40 B |
| ToGuid          | Ulid           |   0.7169 ns | 0.0073 ns |      - |         - |
| ToGuid          | NUlid          |   0.2614 ns | 0.0060 ns |      - |         - |

| ToString        | ByteAetherUlid |  19.4590 ns | 0.1668 ns | 0.0095 |      80 B |
| ToString        | NetUlid        |  20.8832 ns | 0.1329 ns | 0.0095 |      80 B |
| ToString        | Ulid           |  20.1657 ns | 0.0962 ns | 0.0095 |      80 B |
| ToString        | NUlid          |  26.3051 ns | 0.1416 ns | 0.0095 |      80 B |
| ToString        | Guid           |  11.9987 ns | 0.1148 ns | 0.0115 |      96 B |

| CompareTo       | ByteAetherUlid |   0.0118 ns | 0.0072 ns |      - |         - |
| CompareTo       | NetUlid        |   2.7883 ns | 0.0012 ns |      - |         - |
| CompareTo       | Ulid           |   1.8580 ns | 0.0111 ns |      - |         - |
| CompareTo       | NUlid          |   8.7885 ns | 0.0321 ns | 0.0048 |      40 B |

| Equals          | ByteAetherUlid |   0.0000 ns | 0.0000 ns |      - |         - |
| Equals          | NetUlid        |   0.8809 ns | 0.0121 ns |      - |         - |
| Equals          | Ulid           |   0.0120 ns | 0.0036 ns |      - |         - |
| Equals          | NUlid          |   0.0133 ns | 0.0029 ns |      - |         - |
| Equals          | Guid           |   0.0000 ns | 0.0000 ns |      - |         - |

| GetHashCode     | ByteAetherUlid |   0.0000 ns | 0.0000 ns |      - |         - |
| GetHashCode     | NetUlid        |   9.7114 ns | 0.0392 ns |      - |         - |
| GetHashCode     | Ulid           |   0.0000 ns | 0.0000 ns |      - |         - |
| GetHashCode     | NUlid          |   7.7445 ns | 0.0400 ns |      - |         - |
| GetHashCode     | Guid           |   0.0134 ns | 0.0028 ns |      - |         - |

```
All competitive libraries deviate from the official ULID specification in various ways or have other drawbacks:
  1. `NetUlid`: Can only maintain monotonicity in the scope of a single thread.
  2. `NUlid`: Requires special configuration to enable monotonic generation. You have to write your own wrapper with state.
  3. `Ulid` & `GuidV7`: Does not implement monotonicity.
  4. `Ulid`: This library uses a cryptographically non-secure `XOR-Shift` random value generation. Only the initial seed is generated by a cryptographically secure generator.
  5. `Guid` & `GuidV7`: [The Guid documentation explicitly states](https://learn.microsoft.com/en-us/dotnet/api/system.guid.newguid?view=net-9.0#remarks) that its random component may not be generated using a cryptographically secure random number generator (RNG), and that `Guid` values should not be used for cryptographic purposes.

Both `NetUlid` and `NUlid`, which do provide monotonicity, may randomly throw `OverflowException`, when stars align against you. (Random-part overflow)

As such, it can be concluded that this implementation is either the fastest or very close to the fastest ones, while also adhering most completely to the official ULID specification and can be relied on.

## Prior Art

Much of this implementation is either based on or inspired by existing works. This library is standing on the shoulders of giants.

  * [NetUlid](https://github.com/ultimicro/netulid)
  * [Ulid](https://github.com/Cysharp/Ulid)
  * [NUlid](https://github.com/RobThree/NUlid)
  * [Official ULID specification](https://github.com/ulid/spec)
  * [Crockford's Base32](https://www.crockford.com/base32.html)

## Contributing

We welcome all contributions! You can:

 * **Open a Pull Request:** Fork the repository, create a branch, make your changes, and submit a pull request to the `main` branch.
 * **Report Issues:** Found a bug or have a suggestion? [Open an issue](https://github.com/ByteAether/Ulid/issues) with details.

Thank you for helping improve the project!

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
