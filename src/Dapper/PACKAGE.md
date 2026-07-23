# ULID Integration for Dapper
*from ByteAether*

[![License](https://img.shields.io/github/license/ByteAether/Ulid?logo=github&label=License)](https://github.com/ByteAether/Ulid/blob/main/LICENSE)
![Dapper 2.0.0+](https://img.shields.io/badge/Dapper-2.0.0+-orange)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid.Dapper?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.Ulid.Dapper/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.Ulid.Dapper?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.Ulid.Dapper/)

An official extension package for `ByteAether.Ulid`, providing seamless integration with [Dapper](https://github.com/DapperLib/Dapper). It enables effortless mapping of `Ulid` and `Ulid?` properties to database columns using customizable global persistence strategies.

For the core library and full details, visit our [GitHub repository](https://github.com/ByteAether/Ulid).

## ✨ Features
![.NET AOT Ready](https://img.shields.io/badge/.NET-AOT_Ready-blue)
![.NET 10.0](https://img.shields.io/badge/.NET-10.0-brightgreen)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-brightgreen)
![.NET Standard 2.0](https://img.shields.io/badge/.NET-Standard_2.0-green)

- **Version Support**: Fully compatible with **[Dapper](https://github.com/DapperLib/Dapper) versions 2.0.0 and newer**.
- **Automated Configuration**: Register mappings globally across your entire application runtime using a single configuration call.
- **Flexible Storage Strategies**: Choose how your identifiers are mapped based on your database engine constraints:
	- `String`: 26-character [Crockford's Base32](https://www.crockford.com/base32.html) string (e.g., `CHAR(26)`). **(Default)**
	- `Binary`: 16-byte binary payload (e.g., `BINARY(16)`).
	- `Guid`: Native UUID format (ideal for PostgreSQL `uuid`).
	- `SqlServerGuid`: Shuffled SQL Server sequential `uniqueidentifier` to maintain native index sorting properties.

## 💾 Installation

Install the stable package via NuGet:
```sh
dotnet add package ByteAether.Ulid.Dapper
```

> [!NOTE]
> This package automatically includes `ByteAether.Ulid` as a transitive dependency, so installing it separately is unnecessary.
> If you do install `ByteAether.Ulid` directly, its version must be **greater than or equal to** `ByteAether.Ulid.Dapper`. Referencing an older version will trigger a **NU1605 (Package Downgrade)** build error.

## 🚀 Usage

Call `DapperUlid.RegisterUlid()` during your application's startup lifecycle (e.g., inside `Program.cs` or a global initialization block) before executing any queries:

```csharp
using ByteAether.Ulid.Dapper;

// Configures the mapping globally using your chosen database storage format.
// Supports: UlidStorageFormat.String (Default), Binary, Guid, and SqlServerGuid
DapperUlid.RegisterUlid(UlidStorageFormat.Binary);
```

Once registered, queries executing via Dapper parameters or multi-mapping configurations handle the translation transparently:

```csharp
public class User
{
    public int Id { get; set; }
    public Ulid AccountId { get; set; }
    public Ulid? ManagedById { get; set; }
}

// Queries execute seamlessly
var user = connection.QueryFirstOrDefault<User>(
    "SELECT * FROM Users WHERE AccountId = @Id", 
    new { Id = myUlid }
);
```

## ⚠️ Important Architectural Limitations & Warnings

### 1. No Per-Property / Mixed Mappings
Unlike full object-relational mappers (like Entity Framework Core) which preserve structural metadata per table column, **Dapper maps .NET types globally via a 1:1 scheme (`Type` → `TypeHandler`)**.

* **The Rule**: You can choose **exactly one** global strategy for your application lifecycle.
* **The Constraint**: If you call `RegisterUlid(UlidStorageFormat.String)`, you **cannot** have some tables storing `Ulid` as `BINARY(16)` and others as `VARCHAR(26)` within the same execution path. The last configuration registered will override any previous configuration globally. If a mixed scheme is explicitly required, wrapper primitive types or custom parameter objects must be introduced manually.

### 2. Range Queries & Sorting Compatibility (`>=`, `<=`, `ORDER BY`)

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

`ByteAether.Ulid.Dapper` introduces no runtime reflection, dynamic IL injection, or dynamic code compilation patterns inside its mapping block. The explicit `SqlMapper.TypeHandler<T>` strategy is fully safe for **Native AOT compilation** and trimming.

> Ensure your version of the underlying Dapper framework itself is explicitly configured to support AOT workloads

## 📜 License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/ByteAether/Ulid/blob/main/LICENSE) file for details.
