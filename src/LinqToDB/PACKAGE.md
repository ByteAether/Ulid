# ULID Integration for LinqToDB
*from ByteAether*

[![License](https://img.shields.io/github/license/ByteAether/Ulid?logo=github&label=License)](https://github.com/ByteAether/Ulid/blob/main/LICENSE)
![LinqToDB 6.0.0+](https://img.shields.io/badge/LinqToDB-6.0.0+-orange)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid.linq2db?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.Ulid.linq2db/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.Ulid.linq2db?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.Ulid.linq2db/)

An official extension package for `ByteAether.Ulid`, providing seamless integration with [LinqToDB](https://github.com/linq2db/linq2db). It enables effortless mapping of `Ulid` and `Ulid?` properties to database columns using customizable persistence strategies.

For the core library and full details, visit our [GitHub repository](https://github.com/ByteAether/Ulid).

## ✨ Features
![.NET AOT Ready](https://img.shields.io/badge/.NET-AOT_Ready-blue)
![.NET 10.0](https://img.shields.io/badge/.NET-10.0-brightgreen)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-brightgreen)
![.NET Standard 2.0](https://img.shields.io/badge/.NET-Standard_2.0-green)

- **Version Support**: Fully compatible with **[LinqToDB](https://github.com/linq2db/linq2db) versions 6.0.0 and newer**.
- **Automated Configuration**: Register mappings globally for both nullable and non-nullable `Ulid` types using a single extension method on your `DataOptions`.
- **Flexible Storage Strategies**: Choose how your identifiers are persisted based on your database engine constraints:
	- `String`: 26-character [Crockford's Base32](https://www.crockford.com/base32.html) string (mapped to `DataType.Char`). **(Default)**
	- `Binary`: 16-byte binary payload (mapped to `DataType.Binary`).
	- `Guid`: Native UUID format (mapped to `DataType.Guid`).
	- `SqlServerGuid`: Shuffled sequential `uniqueidentifier` optimized to maintain index sorting properties inside Microsoft SQL Server.

## 💾 Installation

Install the stable package via NuGet:

```sh
dotnet add package ByteAether.Ulid.linq2db
```

> [!NOTE]
> This package automatically includes `ByteAether.Ulid` as a transitive dependency, so installing it separately is unnecessary.
> If you do install `ByteAether.Ulid` directly, its version must be **greater than or equal to** `ByteAether.Ulid.linq2db`. Referencing an older version will trigger a **NU1605 (Package Downgrade)** build error.

## 🚀 Usage

Call the `RegisterUlid` extension method on your `DataOptions` instance to register the type mappings across your LinqToDB queries:

```csharp
using LinqToDB;
using ByteAether.Ulid.LinqToDB;

var options = new DataOptions()
    .UseSQLite()
    .UseConnectionString(connectionString)
    // Registers mapping for both Ulid and Ulid? types.
    // Supports: UlidStorageFormat.String (Default), Binary, Guid, and SqlServerGuid
    .RegisterUlid(UlidStorageFormat.Binary);
```

## ⚠️ Important Limitations and Configuration Warnings

### Range Queries & Sorting Compatibility (`>=`, `<=`, `OrderBy`)

While all storage formats are fully supported, their ability to preserve chronological order and execute valid range queries depends on how the underlying database engine handles byte-order comparisons and GUID representations. Because ULIDs rely on a big-endian timestamp for sorting, your choice of database provider determines which formats remain index-friendly:

* **Globally Safe (`String` and `Binary`)**: These formats preserve the raw left-to-right chronological order of ULIDs natively across all database engines (SQLite, PostgreSQL, SQL Server, etc.).
* **Provider Dependent (`Guid`)**: Standard `.NET Guid` structures use a mixed-endian layout.
	* **PostgreSQL**: Supported. The connection driver automatically corrects the endianness when mapping to native `uuid` columns, preserving chronological sorting.
	* **SQLite / Others**: Incompatible for range queries. These engines store GUIDs as raw bytes or text strings, causing standard mixed-endian byte ordering to corrupt chronological comparisons (**though equality lookups remain fully functional**).
* **SQL Server Specific (`SqlServerGuid`)**: This format explicitly optimizes byte shuffling for Microsoft SQL Server's unique sequential indexing rules.
	* **Constraint**: This format **only** works as intended if the underlying column is typed as `uniqueidentifier`. Storing it as `BINARY(16)` or `VARCHAR` will break sorting.
	* **Trade-off**: This byte-shuffling strategy sacrifices cross-database data portability (e.g., directly reading or migrating database rows to PostgreSQL or SQLite) to optimize index page fragmentation and B-tree insertion performance in SQL Server.

> [!CAUTION]
> Before using `Guid` or `SqlServerGuid` formats for range queries (`>=`, `<=`) or `OrderBy` clauses, verify your database provider's native UUID comparison behavior. Misaligning the format with the engine's native comparison logic will lead to incorrect query ordering and omitted records during range filtering.

## ⚡ Native AOT & Trimming Compatibility

`ByteAether.Ulid.linq2db` is fully trimmed and annotated for **Native AOT** compilation. It introduces zero reflection or dynamic code generation.

## 📜 License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/ByteAether/Ulid/blob/main/LICENSE) file for details.
