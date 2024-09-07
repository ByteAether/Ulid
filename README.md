# ULID
*from **ByteAether***

[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid)](https://www.nuget.org/packages/ByteAether.Ulid/)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/ByteAether/Ulid/build-and-test.yml)](https://github.com/ByteAether/Ulid/actions/workflows/build-and-test.yml)

A .NET implementation of ULIDs (Universally Unique Lexicographically Sortable Identifiers) that is fully compatible with the [official ULID specification](https://github.com/ulid/spec).

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [API](#api)
- [Contributing](#contributing)
- [License](#license)

## Introduction

ULIDs are a type of identifier that are designed to be universally unique and lexicographically sortable. They are useful for generating unique identifiers that can be easily sorted and compared. This repository contains a .NET implementation of ULIDs that is fully compatible with the official ULID specification. Unlike many other implementations that deviate from the specification, sometimes on crucial points, this implementation adheres strictly to the official guidelines.

Additionally, this implementation addresses a potential issue in the official specification where generating multiple ULIDs within the same millisecond can cause the "random" part of the ULID to overflow, leading to an overflow exception being thrown. To ensure dependability and guarantee the generation of unique ULIDs, this implementation allows overflow to increment the "timestamp" part of the ULID, thereby eliminating the possibility of randomly occuring exception.

Relevant issue with same suggestion is opened on official ULID specification: [Guarantee a minimum number of IDs before overflow of the random component #39](https://github.com/ulid/spec/issues/39#issuecomment-2252145597)

For almost all systems in the world, both GUID and integer IDs should be abandoned in favor of ULIDs. GUIDs, while unique, lack sortability and readability, making them less efficient for indexing and querying. Integer IDs, on the other hand, are sortable but not universally unique, leading to potential conflicts in distributed systems. ULIDs combine the best of both worlds, offering both uniqueness and sortability, making them an ideal choice for modern applications that require scalable and efficient identifier generation. This library provides a robust and reliable implementation of ULIDs, ensuring that your application can benefit from these advantages without compromising on performance or compliance with the official specification.

## Features

- **Universally Unique**: ULIDs are designed to be globally unique.
- **Lexicographically Sortable**: ULIDs can be sorted lexicographically, making them useful for time-based sorting.
- **Efficient**: The implementation is designed to be efficient and performant.
- **Compatible**: The implementation provides methods to convert ULIDs to and from GUIDs, Crockford's Base32 strings, and byte arrays.
- **Fully Compliant**: This implementation adheres strictly to the official ULID specification.
- **Enhanced Reliability**: Eliminates the possibility of throwing an exception when the "random" part gets overflown.

## Benchmarking
To ensure the performance and efficiency of this ULID implementation, benchmarking was conducted using BenchmarkDotNet.

For comparison, [NetUlid](https://github.com/ultimicro/netulid) 2.1.0, [Ulid](https://github.com/Cysharp/Ulid) 1.3.4 and [NUlid](https://github.com/RobThree/NUlid) 1.7.2 implementations were benchmarked alongside.

The following benchmarks were performed:
```
BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.4651/22H2/2022Update)
AMD Ryzen 7 3700X, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.302
  [Host]     : .NET 8.0.6 (8.0.624.26715), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.6 (8.0.624.26715), X64 RyuJIT AVX2

| Type            | Method         | Mean        | Error     | StdDev    | Median      | Gen0   | Allocated |
|---------------- |--------------- |------------:|----------:|----------:|------------:|-------:|----------:|
| Generate        | ByteAetherUlid |  56.3572 ns | 0.0701 ns | 0.0622 ns |  56.3653 ns |      - |         - |
| Generate        | NetUlid *(1)   | 157.0403 ns | 1.0922 ns | 1.0216 ns | 156.5876 ns | 0.0095 |      80 B |
| Generate        | NUlid *(2)     |  72.6663 ns | 0.5514 ns | 0.4604 ns |  72.7732 ns | 0.0124 |     104 B |

| GenerateNonMono | ByteAetherUlid |  96.5382 ns | 0.5341 ns | 0.4996 ns |  96.7502 ns |      - |         - |
| GenerateNonMono | Ulid *(3,4)    |  43.9369 ns | 0.1319 ns | 0.1234 ns |  43.8794 ns |      - |         - |
| GenerateNonMono | NUlid          | 116.2871 ns | 0.6558 ns | 0.5476 ns | 116.1259 ns | 0.0124 |     104 B |
| GenerateNonMono | Guid           |  47.9964 ns | 0.2624 ns | 0.2454 ns |  48.0948 ns |      - |         - |

| FromByteArray   | ByteAetherUlid |   0.2859 ns | 0.0166 ns | 0.0147 ns |   0.2853 ns |      - |         - |
| FromByteArray   | NetUlid        |   5.6834 ns | 0.0562 ns | 0.0499 ns |   5.6680 ns |      - |         - |
| FromByteArray   | Ulid           |   7.0427 ns | 0.0347 ns | 0.0308 ns |   7.0335 ns |      - |         - |
| FromByteArray   | NUlid          |  10.8101 ns | 0.0530 ns | 0.0496 ns |  10.8234 ns |      - |         - |
| FromByteArray   | Guid           |   0.4294 ns | 0.0033 ns | 0.0027 ns |   0.4303 ns |      - |         - |

| FromGuid        | ByteAetherUlid |   1.6238 ns | 0.0053 ns | 0.0047 ns |   1.6241 ns |      - |         - |
| FromGuid        | NetUlid        |   8.8354 ns | 0.2060 ns | 0.4161 ns |   8.6969 ns | 0.0048 |      40 B |
| FromGuid        | Ulid           |   1.7052 ns | 0.0083 ns | 0.0074 ns |   1.7030 ns |      - |         - |
| FromGuid        | NUlid          |  14.5802 ns | 0.1935 ns | 0.1616 ns |  14.5280 ns | 0.0048 |      40 B |

| FromString      | ByteAetherUlid |  15.0717 ns | 0.0388 ns | 0.0344 ns |  15.0555 ns |      - |         - |
| FromString      | NetUlid        |  27.8942 ns | 0.1240 ns | 0.1035 ns |  27.8519 ns |      - |         - |
| FromString      | Ulid           |  14.9791 ns | 0.0595 ns | 0.0556 ns |  14.9838 ns |      - |         - |
| FromString      | NUlid          |  87.4631 ns | 1.7932 ns | 3.3680 ns |  86.6425 ns | 0.0324 |     272 B |
| FromString      | Guid           |  23.3885 ns | 0.3360 ns | 0.2979 ns |  23.2575 ns |      - |         - |

| ToByteArray     | ByteAetherUlid |   4.1512 ns | 0.1371 ns | 0.1877 ns |   4.0842 ns | 0.0048 |      40 B |
| ToByteArray     | NetUlid        |  10.8610 ns | 0.2149 ns | 0.2010 ns |  10.8313 ns | 0.0048 |      40 B |
| ToByteArray     | Ulid           |   3.9299 ns | 0.0871 ns | 0.0773 ns |   3.9133 ns | 0.0048 |      40 B |
| ToByteArray     | NUlid          |   7.5605 ns | 0.1892 ns | 0.1677 ns |   7.5743 ns | 0.0048 |      40 B |

| ToGuid          | ByteAetherUlid |   0.5201 ns | 0.0214 ns | 0.0179 ns |   0.5146 ns |      - |         - |
| ToGuid          | NetUlid        |  14.1831 ns | 0.0412 ns | 0.0344 ns |  14.1709 ns | 0.0048 |      40 B |
| ToGuid          | Ulid           |   0.4444 ns | 0.0028 ns | 0.0025 ns |   0.4447 ns |      - |         - |
| ToGuid          | NUlid          |  14.3233 ns | 0.2111 ns | 0.1975 ns |  14.2974 ns | 0.0048 |      40 B |

| ToString        | ByteAetherUlid |  23.2149 ns | 0.3957 ns | 0.3702 ns |  23.3001 ns | 0.0095 |      80 B |
| ToString        | NetUlid        |  23.5301 ns | 0.5171 ns | 0.5748 ns |  23.4131 ns | 0.0095 |      80 B |
| ToString        | Ulid           |  21.1645 ns | 0.4603 ns | 0.5300 ns |  21.1597 ns | 0.0095 |      80 B |
| ToString        | NUlid          |  56.9675 ns | 1.1865 ns | 1.3664 ns |  56.5184 ns | 0.0430 |     360 B |
| ToString        | Guid           |  12.2343 ns | 0.2885 ns | 0.2558 ns |  12.1706 ns | 0.0115 |      96 B |

| CompareTo       | ByteAetherUlid |   2.2098 ns | 0.0128 ns | 0.0113 ns |   2.2027 ns |      - |         - |
| CompareTo       | NetUlid        |   3.1096 ns | 0.0366 ns | 0.0342 ns |   3.1074 ns |      - |         - |
| CompareTo       | Ulid           |   2.1023 ns | 0.0107 ns | 0.0095 ns |   2.1005 ns |      - |         - |
| CompareTo       | NUlid          |  10.3192 ns | 0.1550 ns | 0.1449 ns |  10.3099 ns | 0.0048 |      40 B |

| Equals          | ByteAetherUlid |   0.4843 ns | 0.0026 ns | 0.0020 ns |   0.4839 ns |      - |         - |
| Equals          | NetUlid        |   0.9311 ns | 0.0180 ns | 0.0159 ns |   0.9311 ns |      - |         - |
| Equals          | Ulid           |   0.0329 ns | 0.0293 ns | 0.0260 ns |   0.0178 ns |      - |         - |
| Equals          | NUlid          |  19.8798 ns | 0.4241 ns | 0.8271 ns |  19.7657 ns | 0.0095 |      80 B |
| Equals          | Guid           |   0.0243 ns | 0.0152 ns | 0.0127 ns |   0.0207 ns |      - |         - |

| GetHashCode     | ByteAetherUlid |   0.0003 ns | 0.0004 ns | 0.0004 ns |   0.0001 ns |      - |         - |
| GetHashCode     | NetUlid        |   9.8575 ns | 0.0128 ns | 0.0114 ns |   9.8609 ns |      - |         - |
| GetHashCode     | Ulid           |   0.0024 ns | 0.0057 ns | 0.0053 ns |   0.0000 ns |      - |         - |
| GetHashCode     | NUlid          |  13.5207 ns | 0.2050 ns | 0.1817 ns |  13.5608 ns | 0.0048 |      40 B |
| GetHashCode     | Guid           |   0.0000 ns | 0.0000 ns | 0.0000 ns |   0.0000 ns |      - |         - |
```
All competitive libraries deviate from the official ULID specification in various ways or have other drawbacks:
  1. `NetUlid`: Can only maintain monotonicity in the scope of a single thread.
  2. `NUlid`: Requires special configuration to enable monotonic generation. You have to write your own wrapper with state.
  3. `Ulid`: Does not implement monotonicity.
  4. `Ulid`: This library uses a cryptographically non-secure `XOR-Shift` random value generation. Only the initial seed is generated by a cryptographically secure generator.

Both `NetUlid` and `NUlid`, which do provide monotonicity, may randomly throw `OverflowException`, when stars align against you. (Random-part overflow)

As such, it can be concluded that this implementation is either the fastest or very close to the fastest ones, while also adhering most completely to the official ULID specification and can be relied on.

## Installation

You can install the package via NuGet:

```sh
dotnet add package ByteAether.Ulid
```

## Usage

Here is a basic example of how to use the ULID implementation:

```csharp
using System;
using YourNamespace; // Replace with the actual namespace

class Program
{
    static void Main()
    {
        // Create a new ULID
        var ulid = Ulid.New();

        // Convert to byte array & back
        byte[] byteArray = ulid.ToByteArray();
        Console.WriteLine($"Byte Array: {BitConverter.ToString(byteArray)}");
        var ulidFromByteArray = Ulid.New(byteArray);

        // Convert to GUID & back
        Guid guid = ulid.ToGuid();
        Console.WriteLine($"GUID: {guid}");
        var ulidFromGuid = Ulid.New(guid);

        // Convert to string & back
        string str = ulid.ToString();
        Console.WriteLine($"String: {str}");
        var ulidFromStr = Ulid.Parse(str);
    }
}
```

## API

The `Ulid` implementation provides the following methods:

- `Ulid.New(bool isMonotonic = true)`: Generates new ULID
- `Ulid.New(ReadOnlySpan<byte> bytes)`: Create from existing array of bytes.
- `Ulid.New(Guid guid)`: Create from existing `Guid`.
- `Ulid.New(DateTimeOffset dateTimeOffset, bool isMonotonic = true)`: Generates new ULID using given `DateTimeOffset`
- `Ulid.New(long timestamp, bool isMonotonic = true)`: Generates new ULID using given `long` unix timestamp.
- `Ulid.Parse(ReadOnlySpan<char> chars, IFormatProvider? provider = null)`: Creates from existing array of `char`s. `IFormatProvider` is irrelevant.
- `Ulid.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Ulid result)`: Same as previous using `Try*()` pattern.
- `Ulid.Parse(string s, IFormatProvider? provider = null)`: Creates from existing `string`. `IFormatProvider` is irrelevant.
- `Ulid.TryParse(string? s, IFormatProvider? provider, out Ulid result)`: Same as previous using `Try*()` pattern.
- `.ToByteArray()`: Converts the ULID to a byte array.
- `.ToGuid()`: Converts the ULID to a GUID.
- `.ToString(string? format = null, IFormatProvider? formatProvider = null)`: Converts the ULID to a string representation. (Formatting arguments are irrelevant)
- `.Copy()`: Creates another ULID with identical value
- All comparison operators: `GetHashCode`, `Equals`, `CompareTo`, `==`, `!=`, `<`, `<=`, `>`, `>=`.
- Explicit operators to and from `Guid`.

## Integration with other libraries

### ASP.NET Core
The `Ulid` structure has `TypeConverter` applied so ULIDs can be used as an argument on the action (e.g. query string, route parameter, etc.) without any additional works. The input will be accepted as canonical form.

### System.Text.Json
The `Ulid` structure has `JsonConverterAttribute` applied so that mean it can be using with `System.Text.Json` without any additional works.

## Prior Art

Much of this implementation is either based on or inspired by existing implementations. This library is standing on the shoulders of giants.

  * [NetUlid](https://github.com/ultimicro/netulid)
  * [Ulid](https://github.com/Cysharp/Ulid)
  * [NUlid](https://github.com/RobThree/NUlid)
  * [Official ULID specification](https://github.com/ulid/spec)

## Contributing

Contributions are welcome! Please follow these steps to contribute:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Make your changes and commit them with descriptive commit messages.
4. Push your changes to your fork.
5. Open a pull request against the `main` branch of this repository.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.