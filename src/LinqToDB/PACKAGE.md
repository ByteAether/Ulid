# ULID Integration for LinqToDB
*from ByteAether*

[![License](https://img.shields.io/github/license/ByteAether/Ulid?logo=github&label=License)](https://github.com/ByteAether/Ulid/blob/main/LICENSE)
![LinqToDB 6.0.0+](https://img.shields.io/badge/LinqToDB-6.0.0+-orange)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid.linq2db?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.Ulid.linq2db/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.Ulid.linq2db?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.Ulid.linq2db/)

An extension package for `ByteAether.Ulid` that maps `Ulid` and `Ulid?` properties to database columns in [LinqToDB](https://github.com/linq2db/linq2db) using one of four storage formats.

The core library is documented in the [GitHub repository](https://github.com/ByteAether/Ulid).

## ✨ Features

![.NET AOT Ready](https://img.shields.io/badge/.NET-AOT_Ready-blue)
![.NET 10.0](https://img.shields.io/badge/.NET-10.0-brightgreen)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-brightgreen)
![.NET Standard 2.0](https://img.shields.io/badge/.NET-Standard_2.0-brightgreen)

- Compatible with [LinqToDB](https://github.com/linq2db/linq2db) **6.0.0 and newer**.
- **One `DataOptions` extension method** registers the mappings for both `Ulid` and `Ulid?`.
- **Four storage formats** are available:
    - **`String`** (default) stores a 26-character [Crockford's Base32](https://www.crockford.com/base32.html) string, mapped to `DataType.Char`.
    - **`Binary`** stores 16 bytes, mapped to `DataType.Binary`.
    - **`Guid`** stores a native UUID, mapped to `DataType.Guid`.
    - **`SqlServerGuid`** stores a shuffled `uniqueidentifier` that keeps the index order in Microsoft SQL Server.

## 💾 Installation

Install the stable package via NuGet:

```sh
dotnet add package ByteAether.Ulid.linq2db
```

> [!NOTE]
> This package automatically includes `ByteAether.Ulid` as a transitive dependency, so installing it separately is unnecessary.
> If you install `ByteAether.Ulid` directly, its version must be greater than or equal to the version of `ByteAether.Ulid.linq2db`. An older version causes a NU1605 (Package Downgrade) build error; update it or remove the direct reference.

## 🚀 Usage

Call the `RegisterUlid` extension method on your `DataOptions` instance to register the type mappings across your LinqToDB queries. `DataOptions` is immutable, so use the returned instance:

```csharp
using ByteAether.Ulid;
using LinqToDB;
using ByteAether.Ulid.LinqToDB;

var options = new DataOptions()
	.UseSQLite()
	.UseConnectionString(connectionString)
	// Registers mapping for both Ulid and Ulid? types.
	// Supports: UlidStorageFormat.String (Default), Binary, Guid, and SqlServerGuid
	.RegisterUlid(UlidStorageFormat.Binary);
```

The mapping applies to every `DataConnection` or `DataContext` created from these options. Once registered, `Ulid` and `Ulid?` members work like any other mapped type, including inserts, comparisons, and time-range filters:

```csharp
using System;
using System.Linq;
using ByteAether.Ulid;
using LinqToDB.Data;
using LinqToDB.Mapping;

public class Order
{
	[PrimaryKey]
	public Ulid Id { get; set; }

	[Column, Nullable]
	public Ulid? ParentId { get; set; }
}

using var db = new DataConnection(options);

// Insert
db.Insert(new Order { Id = Ulid.New() });

// Time-range query
var min = Ulid.MinAt(DateTimeOffset.UtcNow.AddDays(-1));
var max = Ulid.MaxAt(DateTimeOffset.UtcNow);

var recent = db.GetTable<Order>()
	.Where(o => o.Id >= min && o.Id <= max)
	.ToList();
```

Range queries follow the ULID order only for storage formats that preserve it; see the sort order section below.

## ⚠️ Sort order by storage format

The storage format decides whether range queries (`>=`, `<=`) and `ORDER BY` follow the chronological order of the ULIDs. ULIDs sort by a big-endian timestamp, while `System.Guid` is mixed-endian and database engines compare `uuid` and `uniqueidentifier` values with their own rules.

| Format             | Column type        | Range queries and `ORDER BY` follow ULID order                                                                       |
|--------------------|--------------------|----------------------------------------------------------------------------------------------------------------------|
| `String` (default) | `CHAR(26)`         | Yes, with a binary or ordinal collation.                                                                             |
| `Binary`           | `BINARY(16)`       | Yes, on every database engine.                                                                                       |
| `Guid`             | native `uuid`      | PostgreSQL: yes, the driver corrects the byte order. SQL Server, SQLite and others: no; equality lookups still work. |
| `SqlServerGuid`    | `uniqueidentifier` | SQL Server: yes, by timestamp to the millisecond. Not portable to other engines.                                     |

- The `SqlServerGuid` column must be `uniqueidentifier`. A `BINARY(16)` or `VARCHAR` column breaks the ordering.
- `String` columns need a binary or ordinal collation for range queries and `ORDER BY` to follow ULID order.
- With `SqlServerGuid`, SQL Server compares the random part in a different byte-group order than the ULID does, so IDs generated within the same millisecond can sort differently from their ULID order. Time-window queries built with `Ulid.MinAt` and `Ulid.MaxAt` are exact. Use `String` or `Binary` if you depend on the exact order of IDs generated within one millisecond.
- `SqlServerGuid` trades cross-database portability for fewer SQL Server index page splits and faster inserts. Rows read or migrated directly into PostgreSQL or SQLite do not contain valid ULID bytes.

See [storage formats](https://github.com/ByteAether/Ulid/blob/main/docs/storage-formats.md) for details on each format.

## ⚡ Native AOT

`ByteAether.Ulid.linq2db` is annotated `IsAotCompatible` and uses no reflection or dynamic code generation of its own. Its conversions are `Expression<Func<...>>` registrations made with `MappingSchema.SetConvertExpression`, which LinqToDB compiles and caches.

## 📜 License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/ByteAether/Ulid/blob/main/LICENSE) file for details.
