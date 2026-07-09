# ULID LinqToDB Integration
*from ByteAether*

[![License](https://img.shields.io/github/license/ByteAether/Ulid?logo=github&label=License)](https://github.com/ByteAether/Ulid/blob/main/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid.Linq2Db?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.Ulid.Linq2Db/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.Ulid.Linq2Db?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.Ulid.Linq2Db/)

An official extension package for `ByteAether.Ulid`, providing seamless integration with LinqToDB. It enables effortless mapping of `Ulid` and `Ulid?` properties to database columns using customizable persistence strategies.

For the core library and full details, visit our [GitHub repository](https://github.com/ByteAether/Ulid).

## Features
![.NET AOT Ready](https://img.shields.io/badge/.NET-AOT_Ready-blue)
![.NET 10.0](https://img.shields.io/badge/.NET-10.0-brightgreen)
![.NET 9.0](https://img.shields.io/badge/.NET-9.0-brightgreen)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-brightgreen)

- **Automated Configuration**: Register mappings globally for both nullable and non-nullable `Ulid` types using a single extension method on your `MappingSchema`.
- **Flexible Storage Strategies**: Choose how your identifiers are persisted based on your database engine constraints:
	- `String`: 26-character [Crockford's Base32](https://www.crockford.com/base32.html) string (mapped to `DataType.Char`). **(Default)**
	- `Binary`: 16-byte binary payload (mapped to `DataType.Binary`).
	- `Guid`: Native UUID format (mapped to `DataType.Guid`). _(Limited compatibility)_
	- `SqlServerGuid`: Shuffled sequential `uniqueidentifier` optimized to maintain index sorting properties inside Microsoft SQL Server.

## Installation

Install the stable package via NuGet:

```sh
dotnet add package ByteAether.Ulid.Linq2Db
```

## Usage

Call the `RegisterUlid` extension method on your `MappingSchema` instance to register the type mappings across your LinqToDB queries:

```csharp
using LinqToDB.Mapping;
using ByteAether.Ulid.Linq2Db;

var mappingSchema = new MappingSchema();
// Configures mappings using your chosen database storage format
// Supports: UlidStorageFormat.String (Default), Binary, Guid, and SqlServerGuid
mappingSchema.RegisterUlid(UlidStorageFormat.Binary);

var options = new DataOptions()
    .UseSQLite()
    .UseConnectionString(connectionString)
    .UseMappingSchema(mappingSchema); // Use the configured schema
```

## ⚠️ Important Limitations and Configuration Warnings

### Range Queries & Sorting Compatibility (`>=`, `<=`, `OrderBy`)

Because ULIDs contain an embedded big-endian timestamp component, native database sorting and range filters depend entirely on the underlying byte alignment of the storage format:

* **Supported Globally (`String` and `Binary`)**: These formats preserve the sequential left-to-right chronological order of ULIDs. Database indexes on these types can perform efficient range scans across all major providers (SQLite, PostgreSQL, SQL Server, etc.).
* **Supported Only on SQL Server (`SqlServerGuid`)**: This format reshuffles the chronological timestamp bytes into the trailing positions prioritized by SQL Server's unique sorting rules. It will execute correctly **only** on a real Microsoft SQL Server instance.
* **NOT SUPPORTED FOR RANGES (`Guid`)**: Standard .NET GUID structures use a mixed-endian layout that scrambles the left-to-right chronological sorting of ULID bytes.

> **CRITICAL**: Do not attempt to run index-backed database range queries (`>=`, `<=`) or chronological `OrderBy` clauses against `UlidStorageFormat.Guid` or `UlidStorageFormat.SqlServerGuid` on engines like SQLite or PostgreSQL. These database engines treat GUID configurations as raw byte streams compared left-to-right, resulting in mathematically broken data retrieval and missing records due to the scrambled layout.

## Native AOT & Trimming Compatibility

`ByteAether.Ulid.Linq2Db` is fully trimmed and annotated for **Native AOT** compilation. It introduces zero reflection or dynamic code generation.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/ByteAether/Ulid/blob/main/LICENSE) file for details.
