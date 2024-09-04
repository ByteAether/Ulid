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
| Generate        | ByteAetherUlid |  55.5954 ns | 0.0579 ns | 0.0542 ns |  55.5954 ns |      - |         - |
| Generate        | NetUlid *(1)   | 155.3359 ns | 0.4623 ns | 0.3609 ns | 155.3652 ns | 0.0095 |      80 B |
| Generate        | NUlid *(2)     |  72.1152 ns | 0.2366 ns | 0.1975 ns |  72.1685 ns | 0.0124 |     104 B |

| GenerateNonMono | ByteAetherUlid | 100.3051 ns | 0.2393 ns | 0.2238 ns | 100.3786 ns |      - |         - |
| GenerateNonMono | Ulid *(3,4)    |  43.5266 ns | 0.2679 ns | 0.2506 ns |  43.5241 ns |      - |         - |
| GenerateNonMono | NUlid          | 113.5706 ns | 0.2182 ns | 0.2041 ns | 113.5137 ns | 0.0124 |     104 B |
| GenerateNonMono | Guid           |  47.8621 ns | 0.1587 ns | 0.1407 ns |  47.8818 ns |      - |         - |

| FromByteArray   | ByteAetherUlid |   5.3383 ns | 0.0032 ns | 0.0028 ns |   5.3377 ns |      - |         - |
| FromByteArray   | NetUlid        |   5.5648 ns | 0.0265 ns | 0.0235 ns |   5.5676 ns |      - |         - |
| FromByteArray   | Ulid           |   6.9626 ns | 0.0095 ns | 0.0079 ns |   6.9600 ns |      - |         - |
| FromByteArray   | NUlid          |  10.6427 ns | 0.0296 ns | 0.0247 ns |  10.6396 ns |      - |         - |
| FromByteArray   | Guid           |   0.4327 ns | 0.0008 ns | 0.0007 ns |   0.4328 ns |      - |         - |

| FromGuid        | ByteAetherUlid |   1.6050 ns | 0.0019 ns | 0.0017 ns |   1.6045 ns |      - |         - |
| FromGuid        | NetUlid        |   8.4470 ns | 0.0485 ns | 0.0453 ns |   8.4680 ns | 0.0048 |      40 B |
| FromGuid        | Ulid           |   1.7015 ns | 0.0063 ns | 0.0059 ns |   1.6992 ns |      - |         - |
| FromGuid        | NUlid          |  14.0866 ns | 0.0483 ns | 0.0377 ns |  14.0781 ns | 0.0048 |      40 B |

| FromString      | ByteAetherUlid |  17.9185 ns | 0.2253 ns | 0.2108 ns |  17.9297 ns |      - |         - |
| FromString      | NetUlid        |  27.5687 ns | 0.0526 ns | 0.0467 ns |  27.5699 ns |      - |         - |
| FromString      | Ulid           |  14.8194 ns | 0.0256 ns | 0.0239 ns |  14.8174 ns |      - |         - |
| FromString      | NUlid          |  81.8632 ns | 0.3079 ns | 0.2571 ns |  81.9366 ns | 0.0324 |     272 B |
| FromString      | Guid           |  22.7936 ns | 0.0991 ns | 0.0927 ns |  22.7594 ns |      - |         - |

| ToByteArray     | ByteAetherUlid |   4.3005 ns | 0.0347 ns | 0.0325 ns |   4.3050 ns | 0.0048 |      40 B |
| ToByteArray     | NetUlid        |  11.5636 ns | 0.0332 ns | 0.0294 ns |  11.5699 ns | 0.0048 |      40 B |
| ToByteArray     | Ulid           |   3.8692 ns | 0.0413 ns | 0.0386 ns |   3.8776 ns | 0.0048 |      40 B |
| ToByteArray     | NUlid          |   7.1839 ns | 0.0485 ns | 0.0378 ns |   7.1964 ns | 0.0048 |      40 B |

| ToGuid          | ByteAetherUlid |   0.5056 ns | 0.0078 ns | 0.0073 ns |   0.5068 ns |      - |         - |
| ToGuid          | NetUlid        |  13.9744 ns | 0.0143 ns | 0.0126 ns |  13.9739 ns | 0.0048 |      40 B |
| ToGuid          | Ulid           |   0.5013 ns | 0.0028 ns | 0.0026 ns |   0.5023 ns |      - |         - |
| ToGuid          | NUlid          |  13.8649 ns | 0.0212 ns | 0.0198 ns |  13.8635 ns | 0.0048 |      40 B |

| ToString        | ByteAetherUlid |  21.0412 ns | 0.1279 ns | 0.1134 ns |  21.0615 ns | 0.0095 |      80 B |
| ToString        | NetUlid        |  22.5406 ns | 0.0898 ns | 0.0796 ns |  22.5525 ns | 0.0095 |      80 B |
| ToString        | Ulid           |  20.0724 ns | 0.1221 ns | 0.1019 ns |  20.0661 ns | 0.0095 |      80 B |
| ToString        | NUlid          |  53.8218 ns | 0.0633 ns | 0.0561 ns |  53.8068 ns | 0.0430 |     360 B |
| ToString        | Guid           |  24.5592 ns | 0.0231 ns | 0.0205 ns |  24.5618 ns | 0.0115 |      96 B |

| CompareTo       | ByteAetherUlid |   2.8333 ns | 0.0157 ns | 0.0146 ns |   2.8331 ns |      - |         - |
| CompareTo       | NetUlid        |   3.0532 ns | 0.0125 ns | 0.0117 ns |   3.0505 ns |      - |         - |
| CompareTo       | Ulid           |   2.0916 ns | 0.0010 ns | 0.0008 ns |   2.0917 ns |      - |         - |
| CompareTo       | NUlid          |   9.8774 ns | 0.0647 ns | 0.0540 ns |   9.8960 ns | 0.0048 |      40 B |

| Equals          | ByteAetherUlid |   0.4869 ns | 0.0076 ns | 0.0071 ns |   0.4908 ns |      - |         - |
| Equals          | NetUlid        |   0.9155 ns | 0.0091 ns | 0.0085 ns |   0.9096 ns |      - |         - |
| Equals          | Ulid           |   0.0153 ns | 0.0042 ns | 0.0038 ns |   0.0164 ns |      - |         - |
| Equals          | NUlid          |  19.0430 ns | 0.1701 ns | 0.1591 ns |  19.0015 ns | 0.0095 |      80 B |
| Equals          | Guid           |   0.0174 ns | 0.0037 ns | 0.0034 ns |   0.0191 ns |      - |         - |

| GetHashCode     | ByteAetherUlid |   0.0002 ns | 0.0002 ns | 0.0002 ns |   0.0001 ns |      - |         - |
| GetHashCode     | NetUlid        |   9.7653 ns | 0.0216 ns | 0.0202 ns |   9.7710 ns |      - |         - |
| GetHashCode     | Ulid           |   0.0000 ns | 0.0000 ns | 0.0000 ns |   0.0000 ns |      - |         - |
| GetHashCode     | NUlid          |  13.2926 ns | 0.0627 ns | 0.0524 ns |  13.3011 ns | 0.0048 |      40 B |
| GetHashCode     | Guid           |   0.0029 ns | 0.0018 ns | 0.0016 ns |   0.0030 ns |      - |         - |
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