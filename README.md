# ![ULID from ByteAether](assets/header.png)

[![License](https://img.shields.io/github/license/ByteAether/Ulid?logo=github&label=License)](https://github.com/ByteAether/Ulid/blob/main/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.Ulid/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.Ulid?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.Ulid/)
[![GitHub Build Status](https://img.shields.io/github/actions/workflow/status/ByteAether/Ulid/build-and-test.yml?logo=github&label=Build%20%26%20Test)](https://github.com/ByteAether/Ulid/actions/workflows/build-and-test.yml)
[![GitHub Security](https://img.shields.io/github/actions/workflow/status/ByteAether/Ulid/codeql.yml?logo=github&label=Security%20Validation)](https://github.com/ByteAether/Ulid/actions/workflows/codeql.yml)

A high-performance, fully compliant .NET implementation of ULIDs (Universally Unique Lexicographically Sortable Identifiers), adhering to the [official ULID specification](https://github.com/ulid/spec).

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

ULIDs are universally unique, lexicographically sortable identifiers, ideal for distributed systems and time-ordered data due to their sortability and human-readability—advantages GUIDs lack. This library offers a robust, fully compliant .NET implementation, addressing limitations found in other ULID solutions.

This implementation addresses a potential `OverflowException` that can occur when generating multiple ULIDs within the same millisecond due to the "random" part overflowing. To ensure dependable, unique ULID generation, our solution increments the timestamp component upon random part overflow, eliminating such exceptions. This behavior aligns with discussions in [ULID specification issue #39](https://github.com/ulid/spec/issues/39#issuecomment-2252145597).

This library uniquely addresses the predictability of monotonic ULIDs generated within the same millisecond by allowing random increments to the random component. This mitigates enumeration attack vulnerabilities, as discussed in [ULID specification issue #105](https://github.com/ulid/spec/issues/105). You can configure the random increment with a random value ranging from 1-byte (1–256) to 4-bytes (1–4,294,967,296), enhancing randomness while preserving lexicographical sortability.

For most modern systems, ULIDs offer a superior alternative to both GUIDs and integer IDs. While GUIDs provide uniqueness, they lack sortability and readability, impacting indexing and querying efficiency. Integer IDs are sortable but not universally unique, leading to potential conflicts in distributed environments. ULIDs combine universal uniqueness with lexicographical sortability, making them the optimal choice for scalable and efficient identifier generation in modern applications. This library provides a robust, reliable, and compliant ULID implementation, enabling your application to leverage these benefits without compromising performance or adherence to the official specification.

## Features

![.NET 10.0](https://img.shields.io/badge/.NET-10.0-brightgreen)
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
- **Error-Free Generation**: Prevents `OverflowException` by incrementing the timestamp component when the random part overflows, ensuring continuous unique ULID generation.

These features collectively make ByteAether.Ulid a robust and efficient choice for managing unique identifiers in your .NET applications.

## Installation

Install the latest stable package via NuGet:
```sh
dotnet add package ByteAether.Ulid
```
To install a specific [preview version](https://www.nuget.org/packages/ByteAether.Ulid/absoluteLatest), use the `--version` option:
```sh
dotnet add package ByteAether.Ulid --version <VERSION_NUMBER>
```

## Usage

Here is a basic example of how to use the ULID implementation:
```csharp
using System;
using ByteAether.Ulid;

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
```

### Filtering by Time Range (LINQ)

Since ULIDs are lexicographically sortable and contain a timestamp, you can use `Ulid.MinAt()` and `Ulid.MaxAt()` to generate boundary ULIDs for a specific time range. This allows EF Core to translate these into efficient range comparisons (e.g., `WHERE Id >= @min AND Id <= @max`) in your database.

```csharp
public async Task<List<Entity>> GetEntitiesFromYesterday(MyDbContext context)
{
    var startOfYesterday = DateTimeOffset.UtcNow.AddDays(-1).Date;
    var endOfYesterday = startOfYesterday.AddDays(1).AddTicks(-1);

    // Create boundary ULIDs for the time range
    var minUlid = Ulid.MinAt(startOfYesterday);
    var maxUlid = Ulid.MaxAt(endOfYesterday);

    // This query uses the primary key index for high performance
    return await context.Entities
        .Where(e => e.Id >= minUlid && e.Id <= maxUlid)
        .ToListAsync();
}
```

### Advanced Generation

You can customize ULID generation by providing `GenerationOptions`. This allows you to control monotonicity and the source of randomness.

#### Example: Monotonic ULID with Random Increments

To generate ULIDs that are monotonically increasing with a random increment, you can specify the `Monotonicity` option.
```csharp
using System;
using ByteAether.Ulid;
using static ByteAether.Ulid.Ulid.GenerationOptions;

// Configure options for a 2-byte random increment
var options = new Ulid.GenerationOptions
{
	Monotonicity = MonotonicityOptions.MonotonicRandom2Byte
};

// Generate a ULID with the specified options
var ulid = Ulid.New(options);

Console.WriteLine($"ULID with random increment: {ulid}");
```
#### Example: Setting Default Generation Options

You can set default generation options for the entire application. This is useful for consistently applying specific behaviors, such as prioritizing performance over cryptographic security.
```csharp
using System;
using ByteAether.Ulid;
using static ByteAether.Ulid.Ulid.GenerationOptions;

// Set default generation options for the entire application
Ulid.DefaultGenerationOptions = new()
{
	Monotonicity = MonotonicityOptions.MonotonicIncrement,
	InitialRandomSource = new PseudoRandomProvider(),
	IncrementRandomSource = new PseudoRandomProvider()
};

// Now, any subsequent call to Ulid.New() will use these options
var ulid = Ulid.New();

Console.WriteLine($"ULID from pseudo-random source: {ulid}");
```

## API

The `Ulid` implementation provides the following properties and methods:

### Creation

- `Ulid.New(GenerationOptions? options = null)`\
Generates a new ULID using default generation options. Accepts an optional `GenerationOptions` parameter to customize the generation behavior.
- `Ulid.New(DateTimeOffset dateTimeOffset, GenerationOptions? options = null)`\
Generates a new ULID using the specified `DateTimeOffset` and default generation options. Accepts an optional `GenerationOptions` parameter to customize the generation behavior.
- `Ulid.New(long timestamp, GenerationOptions? options = null)`\
Generates a new ULID using the specified Unix timestamp in milliseconds (`long`) and default generation options. Accepts an optional `GenerationOptions` parameter to customize the generation behavior.
- `Ulid.New(DateTimeOffset dateTimeOffset, ReadOnlySpan<byte> random)`\
Generates a new ULID using the specified `DateTimeOffset` and a pre-existing random byte array.
- `Ulid.New(long timestamp, ReadOnlySpan<byte> random)`\
Generates a new ULID using the specified Unix timestamp in milliseconds (`long`) and a pre-existing random byte array.
- `Ulid.New(ReadOnlySpan<byte> bytes)`\
Creates a ULID from an existing byte array.
- `Ulid.New(Guid guid)`\
Create from existing `Guid`.
- `Ulid.MinAt(DateTimeOffset datetime)`\
Creates the minimum possible ULID value for the specified `DateTimeOffset`.
- `Ulid.MinAt(long timestamp)`\
Creates the minimum possible ULID value for the specified Unix timestamp in milliseconds (`long`).
- `Ulid.MaxAt(DateTimeOffset datetime)`\
Creates the maximum possible ULID value for the specified `DateTimeOffset`.
- `Ulid.MaxAt(long timestamp)`\
Creates the maximum possible ULID value for the specified Unix timestamp in milliseconds (`long`).

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

- `Ulid.Empty`\
Represents an empty ULID, equivalent to `default(Ulid)` and `Ulid.New(new byte[16])`.
- `Ulid.Max`\
Represents the maximum possible value for a ULID (all bytes set to `0xFF`).
- `Ulid.DefaultGenerationOptions`\
Default configuration for ULID generation when no options are provided by the `Ulid.New(...)` call.
- `.Time`\
Gets the timestamp component of the ULID as a `DateTimeOffset`.
- `.TimeBytes`\
Gets the time component of the ULID as a `ReadOnlySpan<byte>`.
- `.Random`\
Gets the random component of the ULID as a `ReadOnlySpan<byte>`.

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
- Provides implicit operators to and from `Guid` and `string`.

### GenerationOptions

The `GenerationOptions` class provides detailed configuration for ULID generation, with the following key properties:

- `Monotonicity`\
  Controls the behavior of ULID generation when multiple identifiers are created within the same millisecond. It determines whether ULIDs are strictly increasing or allow for random ordering within that millisecond. Available options include: `NonMonotonic`, `MonotonicIncrement` (default), `MonotonicRandom1Byte`, `MonotonicRandom2Byte`, `MonotonicRandom3Byte`, `MonotonicRandom4Byte`.

- `InitialRandomSource`\
An `IRandomProvider` for generating the random bytes of a ULID. The default `CryptographicallySecureRandomProvider` ensures robust, unpredictable ULIDs using a cryptographically secure generator.

- `IncrementRandomSource`\
An `IRandomProvider` that provides randomness for monotonic random increments. The default `PseudoRandomProvider` is a faster, non-cryptographically secure source optimized for this specific purpose.

This library comes with two default `IRandomProvider` implementations:

- **`CryptographicallySecureRandomProvider`**\
Utilizes `System.Security.Cryptography.RandomNumberGenerator` for high-quality, cryptographically secure random data.
- **`PseudoRandomProvider`**\
A faster, non-cryptographically secure option based on `System.Random`, ideal for performance-critical scenarios where cryptographic security is not required for random increments.

Custom `IRandomProvider` implementations can also be created.

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

	public UlidToBytesConverter(ConverterMappingHints? mappingHints = null) : base(
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
using System;

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
using Newtonsoft.Json;
using System.Collections.Generic;

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

Benchmarking was performed using [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) to demonstrate the performance and efficiency of this ULID implementation. Comparisons include [NetUlid](https://github.com/ultimicro/netulid) 2.1.0, [Ulid](https://github.com/Cysharp/Ulid) 1.4.1, [NUlid](https://github.com/RobThree/NUlid) 1.7.3, and `Guid` for overlapping functionalities like creation, parsing, and byte conversions.

Benchmark scenarios also include comparisons against `Guid`, where functionality overlaps, such as creation, parsing, and byte conversions.

*Note:*
* `ByteAetherUlidR1Bc` & `ByteAetherUlidR4Bc` are configured to use a cryptographically secure random increment of 1 byte and 4 bytes, respectively, during monotonic ULID generation.
* `ByteAetherUlidR1Bp` & `ByteAetherUlidR4Bp` are configured to use a pseudo-random increment of 1 byte and 4 bytes, respectively, during monotonic ULID generation.
* `ByteAetherUlidP` is configured to use a pseudo-random source for the random component during non-monotonic ULID generation.

The following benchmarks were performed:
```
BenchmarkDotNet v0.15.8, Windows 10 (10.0.19044.6691/21H2/November2021Update)
AMD Ryzen 7 3700X 3.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3

Job=DefaultJob

| Type            | Method             | Mean        | Error     | Gen0   | Allocated |
|---------------- |------------------- |------------:|----------:|-------:|----------:|
| Generate        | ByteAetherUlid     |  42.7482 ns | 0.1075 ns |      - |         - |
| Generate        | ByteAetherUlidR1Bp |  48.1939 ns | 0.3909 ns |      - |         - |
| Generate        | ByteAetherUlidR4Bp |  52.3962 ns | 0.1214 ns |      - |         - |
| Generate        | ByteAetherUlidR1Bc |  91.2941 ns | 0.2795 ns |      - |         - |
| Generate        | ByteAetherUlidR4Bc |  99.4539 ns | 0.4657 ns |      - |         - |
| Generate        | NetUlid *(1)       | 158.9262 ns | 1.0281 ns | 0.0095 |      80 B |
| Generate        | NUlid *(2)         |  50.0544 ns | 0.2260 ns |      - |         - |

| GenerateNonMono | ByteAetherUlid     |  91.3593 ns | 0.3431 ns |      - |         - |
| GenerateNonMono | ByteAetherUlidP    |  41.9809 ns | 0.1385 ns |      - |         - |
| GenerateNonMono | Ulid *(3,4)        |  39.9820 ns | 0.2004 ns |      - |         - |
| GenerateNonMono | NUlid              |  92.1577 ns | 0.4041 ns |      - |         - |
| GenerateNonMono | Guid *(5)          |  48.5804 ns | 0.1707 ns |      - |         - |
| GenerateNonMono | GuidV7 *(3,5)      |  79.2241 ns | 0.3652 ns |      - |         - |

| FromByteArray   | ByteAetherUlid     |   0.0211 ns | 0.0030 ns |      - |         - |
| FromByteArray   | NetUlid            |   0.6503 ns | 0.0081 ns |      - |         - |
| FromByteArray   | Ulid               |   0.2572 ns | 0.0030 ns |      - |         - |
| FromByteArray   | NUlid              |   0.0104 ns | 0.0068 ns |      - |         - |
| FromByteArray   | Guid               |   0.0193 ns | 0.0041 ns |      - |         - |

| FromGuid        | ByteAetherUlid     |   0.0138 ns | 0.0107 ns |      - |         - |
| FromGuid        | NetUlid            |   1.2947 ns | 0.0436 ns |      - |         - |
| FromGuid        | Ulid               |   1.7548 ns | 0.0164 ns |      - |         - |
| FromGuid        | NUlid              |   0.5480 ns | 0.0344 ns |      - |         - |

| FromString      | ByteAetherUlid     |  14.2074 ns | 0.2594 ns |      - |         - |
| FromString      | NetUlid            |  28.0119 ns | 0.5557 ns |      - |         - |
| FromString      | Ulid               |  15.3590 ns | 0.2057 ns |      - |         - |
| FromString      | NUlid              |  53.5128 ns | 0.3265 ns | 0.0086 |      72 B |
| FromString      | Guid               |  20.6887 ns | 0.2020 ns |      - |         - |

| ToByteArray     | ByteAetherUlid     |   4.5071 ns | 0.1399 ns | 0.0048 |      40 B |
| ToByteArray     | NetUlid            |  10.4349 ns | 0.2341 ns | 0.0048 |      40 B |
| ToByteArray     | Ulid               |   4.1026 ns | 0.1328 ns | 0.0048 |      40 B |
| ToByteArray     | NUlid              |   4.4312 ns | 0.1437 ns | 0.0048 |      40 B |

| ToGuid          | ByteAetherUlid     |   0.2604 ns | 0.0043 ns |      - |         - |
| ToGuid          | NetUlid            |  10.4844 ns | 0.0151 ns |      - |         - |
| ToGuid          | Ulid               |   0.7432 ns | 0.0079 ns |      - |         - |
| ToGuid          | NUlid              |   0.2733 ns | 0.0056 ns |      - |         - |

| ToString        | ByteAetherUlid     |  12.4495 ns | 0.2869 ns | 0.0096 |      80 B |
| ToString        | NetUlid            |  24.2338 ns | 0.3168 ns | 0.0095 |      80 B |
| ToString        | Ulid               |  12.4809 ns | 0.2004 ns | 0.0096 |      80 B |
| ToString        | NUlid              |  29.8794 ns | 0.0559 ns | 0.0095 |      80 B |
| ToString        | Guid               |   7.9268 ns | 0.0546 ns | 0.0115 |      96 B |

| CompareTo       | ByteAetherUlid     |   0.0002 ns | 0.0005 ns |      - |         - |
| CompareTo       | NetUlid            |   3.7498 ns | 0.0348 ns |      - |         - |
| CompareTo       | Ulid               |   0.0034 ns | 0.0033 ns |      - |         - |
| CompareTo       | NUlid              |   0.3966 ns | 0.0080 ns |      - |         - |

| Equals          | ByteAetherUlid     |   0.0019 ns | 0.0022 ns |      - |         - |
| Equals          | NetUlid            |   1.0192 ns | 0.0189 ns |      - |         - |
| Equals          | Ulid               |   0.0001 ns | 0.0003 ns |      - |         - |
| Equals          | NUlid              |   0.0111 ns | 0.0123 ns |      - |         - |
| Equals          | Guid               |   0.0013 ns | 0.0017 ns |      - |         - |

| GetHashCode     | ByteAetherUlid     |   0.0000 ns | 0.0000 ns |      - |         - |
| GetHashCode     | NetUlid            |   9.9751 ns | 0.0332 ns |      - |         - |
| GetHashCode     | Ulid               |   0.0001 ns | 0.0006 ns |      - |         - |
| GetHashCode     | NUlid              |   6.0511 ns | 0.0297 ns |      - |         - |
| GetHashCode     | Guid               |   0.0002 ns | 0.0008 ns |      - |         - |
```

Existing competitive libraries exhibit various deviations from the official ULID specification or present drawbacks:
  1. `NetUlid`: Only supports monotonicity within a single thread.
  2. `NUlid`: Requires custom wrappers and state management for monotonic generation.
  3. `Ulid` & `GuidV7`: Do not implement monotonicity.
  4. `Ulid`: Utilizes a cryptographically non-secure `XOR-Shift` for random value generation, with only the initial seed being cryptographically secure.
  5. `Guid` & `GuidV7`: [The Guid documentation explicitly states](https://learn.microsoft.com/en-us/dotnet/api/system.guid.newguid?view=net-9.0#remarks) that its random component may not be generated using a cryptographically secure random number generator (RNG), and that `Guid` values should not be used for cryptographic purposes.

Furthermore, both `NetUlid` and `NUlid`, despite offering monotonicity, are susceptible to `OverflowException` due to random-part overflow.

This implementation demonstrates performance comparable to or exceeding its closest competitors. Crucially, it provides the most complete adherence to the official ULID specification, ensuring superior reliability and robustness for your applications compared to other libraries.

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
