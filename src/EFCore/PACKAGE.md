# ULID Integration for Entity Framework Core
*from ByteAether*

[![License](https://img.shields.io/github/license/ByteAether/Ulid?logo=github&label=License)](https://github.com/ByteAether/Ulid/blob/main/LICENSE)
![Entity Framework Core 6.0.0+](https://img.shields.io/badge/Entity_Framework_Core-6.0.0+-orange)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid.EntityFrameworkCore?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.Ulid.EntityFrameworkCore/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.Ulid.EntityFrameworkCore?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.Ulid.EntityFrameworkCore/)

An extension package for `ByteAether.Ulid` that maps `Ulid` and `Ulid?` properties to database columns in [Entity Framework Core](https://github.com/dotnet/efcore) using one of four storage formats.

The core library is documented in the [GitHub repository](https://github.com/ByteAether/Ulid).

## ✨ Features

![.NET AOT Ready](https://img.shields.io/badge/.NET-AOT_Ready-blue)
![.NET 10.0](https://img.shields.io/badge/.NET-10.0-brightgreen)
![.NET 9.0](https://img.shields.io/badge/.NET-9.0-brightgreen)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-brightgreen)
![.NET 7.0](https://img.shields.io/badge/.NET-7.0-brightgreen)
![.NET 6.0](https://img.shields.io/badge/.NET-6.0-brightgreen)

- Compatible with [Entity Framework Core](https://github.com/dotnet/efcore) **6.0.0 and newer**.
- **One extension method** registers the mappings for both `Ulid` and `Ulid?`.
- **Four storage formats** are available:
    - **`String`** (default) stores a 26-character [Crockford's Base32](https://www.crockford.com/base32.html) string, for example in a `CHAR(26)` column.
    - **`Binary`** stores 16 bytes, for example in a `BINARY(16)` column.
    - **`Guid`** stores a native UUID, for example in a PostgreSQL `uuid` column.
    - **`SqlServerGuid`** stores a shuffled SQL Server `uniqueidentifier` that keeps the native index order.

## 💾 Installation

Install the stable package via NuGet:
```sh
dotnet add package ByteAether.Ulid.EntityFrameworkCore
```

> [!NOTE]
> This package automatically includes `ByteAether.Ulid` as a transitive dependency, so installing it separately is unnecessary.
> If you install `ByteAether.Ulid` directly, its version must be greater than or equal to the version of `ByteAether.Ulid.EntityFrameworkCore`. An older version causes a NU1605 (Package Downgrade) build error; update it or remove the direct reference.

## 🚀 Usage

Override the `ConfigureConventions` method in your `DbContext` to register the type mappings across all entities:

```csharp
using ByteAether.Ulid;
using Microsoft.EntityFrameworkCore;
using ByteAether.Ulid.EntityFrameworkCore;

public class MyDbContext : DbContext
{
	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
	{
		// Configures mappings globally using your chosen database storage format
		// Supports: UlidStorageFormat.String (Default), Binary, Guid, and SqlServerGuid
		configurationBuilder.RegisterUlid(UlidStorageFormat.Binary);
	}
}
```

### Per-property mapping

To use different formats for different columns, apply the converter classes to individual properties in `OnModelCreating`:

```csharp
using ByteAether.Ulid;
using Microsoft.EntityFrameworkCore;
using ByteAether.Ulid.EntityFrameworkCore;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	// Persist as CHAR(26) string
	modelBuilder.Entity<User>().Property(u => u.Id).HasConversion<UlidToStringConverter>();

	// Persist as a flat BINARY(16) column
	modelBuilder.Entity<Order>().Property(o => o.Id).HasConversion<UlidToBytesConverter>();

	// Persist as native UUID/Guid
	modelBuilder.Entity<Product>().Property(p => p.Id).HasConversion<UlidToGuidConverter>();

	// Persist as an optimized, ordered SQL Server sequential uniqueidentifier
	modelBuilder.Entity<LogEntry>().Property(l => l.Id)
		.HasConversion<UlidToSqlServerGuidConverter>()
		.HasColumnType("uniqueidentifier"); // Required for correct index sorting
}
```

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

`ByteAether.Ulid.EntityFrameworkCore` is annotated `IsAotCompatible` and uses no reflection or dynamic code generation of its own. Its converters are `Expression<Func<...>>`-based `ValueConverter<,>` registrations, which EF Core compiles once per model at model-build time, not per query.

> Your application must still follow [Entity Framework Core's native AOT constraints](https://learn.microsoft.com/en-us/ef/core/performance/nativeaot-and-precompiled-queries) (such as using EF Core Precompiled Models via `dotnet ef dbcontext optimize`).

## 📜 License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/ByteAether/Ulid/blob/main/LICENSE) file for details.
