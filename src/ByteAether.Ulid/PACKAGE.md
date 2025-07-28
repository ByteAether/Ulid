# ULID
*from ByteAether*

[![License](https://img.shields.io/github/license/ByteAether/Ulid?logo=github&label=License)](https://github.com/ByteAether/Ulid/blob/main/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.Ulid/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.Ulid?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.Ulid/)
[![GitHub Build Status](https://img.shields.io/github/actions/workflow/status/ByteAether/Ulid/build-and-test.yml?logo=github&label=Build%20%26%20Test)](https://github.com/ByteAether/Ulid/actions/workflows/build-and-test.yml)
[![GitHub Security](https://img.shields.io/github/actions/workflow/status/ByteAether/Ulid/codeql.yml?logo=github&label=Security%20Validation)](https://github.com/ByteAether/Ulid/actions/workflows/codeql.yml)

A high-performance .NET implementation of ULIDs (Universally Unique Lexicographically Sortable Identifiers) that fully complies with the [official ULID specification](https://github.com/ulid/spec).

For more detailed documentation, visit our [GitHub repository](https://github.com/ByteAether/Ulid).

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
using ByteAether.Ulid;

class Program
{
	static void Main()
	{
		// Create a new ULID
		var ulid = Ulid.New();

		// Convert to byte array and back
		byte[] byteArray = ulid.ToByteArray();
		var ulidFromByteArray = new Ulid(byteArray);

		// Convert to GUID and back
		Guid guid = ulid.ToGuid();
		var ulidFromGuid = new Ulid(guid);

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

- `Ulid.New()`\
  Generates a new ULID using default generation options.
- `Ulid.New(GenerationOptions options)`\
  Generates a new ULID using the specified generation options.
- `Ulid.New(DateTimeOffset dateTimeOffset)`\
  Generates a new ULID using the specified `DateTimeOffset` and default generation options.
- `Ulid.New(DateTimeOffset dateTimeOffset, GenerationOptions options)`\
  Generates a new ULID using the specified `DateTimeOffset` and generation options.
- `Ulid.New(long timestamp)`\
  Generates a new ULID using the specified Unix timestamp in milliseconds (`long`) and default generation options.
- `Ulid.New(long timestamp, GenerationOptions options)`\
  Generates a new ULID using the specified Unix timestamp in milliseconds (`long`) and generation options.
- `Ulid.New(DateTimeOffset dateTimeOffset, ReadOnlySpan<byte> random)`\
  Generates a new ULID using the specified `DateTimeOffset` and a pre-existing random byte array.
- `Ulid.New(long timestamp, ReadOnlySpan<byte> random)`\
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

- `Ulid.Empty`\
  Represents an empty ULID, equivalent to `default(Ulid)` and `Ulid.New(new byte[16])`.
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
- Provides implicit operators to and from `Guid`.

### GenerationOptions

The `GenerationOptions` allows you to customize the behavior of ULID generation.

#### `Monotonicity`

Gets or sets the monotonicity behavior for ULID generation. Default: `MonotonicityOptions.MonotonicIncrement`.

- **`NonMonotonic`**: ULIDs are generated without any monotonic guarantees. The random component is entirely random.
- **`MonotonicIncrement`**: Guarantees strict monotonic progression by incrementing the random portion of the ULID.
- **`MonotonicRandom1Byte` to `MonotonicRandom4Byte`**: Ensures monotonicity by adding a random value of 1 to 4 bytes to the random component. This mitigates the risk of predictable ULIDs when generated in the same millisecond.

#### `InitialRandomSource`

Gets or sets the random source used for generating the initial random component of the ULID. Default: `RandomSourceOptions.CryptographicallySecure`.

- **`CryptographicallySecure`**: Uses a cryptographically strong source of randomness, ensuring high entropy.
- **`PseudoRandom`**: Uses a pseudo-random algorithm, which is faster but not cryptographically secure.

#### `IncrementRandomSource`

Gets or sets the random source used for generating the random increment when ensuring monotonicity. Default: `RandomSourceOptions.PseudoRandom`.

- **`CryptographicallySecure`**: Uses a cryptographically secure random number generator for the increment.
- **`PseudoRandom`**: Uses a pseudo-random number generator for the increment, prioritizing performance.

## Integration with Other Libraries

### ASP.NET Core

Supports seamless integration as a route or query parameter with built-in `TypeConverter`.

### System.Text.Json

Includes a `JsonConverter` for easy serialization and deserialization.

### Other Libraries

Check out [README in GitHub repository](https://github.com/ByteAether/Ulid/blob/main/README.md) for examples to integrate with Entity Framework Core, Dapper, MessagePack, and Newtonsoft.Json.

## Prior Art

Much of this implementation is either based on or inspired by existing works. This library is standing on the shoulders of giants.

  * [NetUlid](https://github.com/ultimicro/netulid)
  * [Ulid](https://github.com/Cysharp/Ulid)
  * [NUlid](https://github.com/RobThree/NUlid)
  * [Official ULID specification](https://github.com/ulid/spec)
  * [Crockford's Base32](https://www.crockford.com/base32.html)

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
