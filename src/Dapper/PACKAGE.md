# ULID Integration for Dapper
*from ByteAether*

[![License](https://img.shields.io/github/license/ByteAether/Ulid?logo=github&label=License)](https://github.com/ByteAether/Ulid/blob/main/LICENSE)
![Dapper 2.0.4+](https://img.shields.io/badge/Dapper-2.0.4+-orange)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid.Dapper?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.Ulid.Dapper/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.Ulid.Dapper?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.Ulid.Dapper/)

An extension package for `ByteAether.Ulid` that maps `Ulid` and `Ulid?` properties to database columns in [Dapper](https://github.com/DapperLib/Dapper) using one of four global storage formats.

The core library is documented in the [GitHub repository](https://github.com/ByteAether/Ulid).

## ✨ Features

![.NET AOT Ready](https://img.shields.io/badge/.NET-AOT_Ready-blue)
![.NET 10.0](https://img.shields.io/badge/.NET-10.0-brightgreen)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-brightgreen)
![.NET Standard 2.0](https://img.shields.io/badge/.NET-Standard_2.0-brightgreen)

- Compatible with [Dapper](https://github.com/DapperLib/Dapper) **2.0.4 and newer**.
- **Global registration** maps both `Ulid` and `Ulid?` with one call.
- **Four storage formats** are available:
    - **`String`** (default) stores a 26-character [Crockford's Base32](https://www.crockford.com/base32.html) string, for example in a `CHAR(26)` column.
    - **`Binary`** stores 16 bytes, for example in a `BINARY(16)` column.
    - **`Guid`** stores a native UUID, for example in a PostgreSQL `uuid` column.
    - **`SqlServerGuid`** stores a shuffled SQL Server `uniqueidentifier` that keeps the native index order.

## 💾 Installation

Install the stable package via NuGet:
```sh
dotnet add package ByteAether.Ulid.Dapper
```

> [!NOTE]
> This package automatically includes `ByteAether.Ulid` as a transitive dependency, so installing it separately is unnecessary.
> If you install `ByteAether.Ulid` directly, its version must be greater than or equal to the version of `ByteAether.Ulid.Dapper`. An older version causes a NU1605 (Package Downgrade) build error; update it or remove the direct reference.

## 🚀 Usage

Call `DapperUlid.RegisterUlid()` during application startup, for example in `Program.cs`, before executing any queries:

```csharp
using ByteAether.Ulid;
using ByteAether.Ulid.Dapper;

// Configures the mapping globally using your chosen database storage format.
// Supports: UlidStorageFormat.String (Default), Binary, Guid, and SqlServerGuid
DapperUlid.RegisterUlid(UlidStorageFormat.Binary);
```

Once registered, Dapper converts `Ulid` parameters and members automatically:

```csharp
using System;
using ByteAether.Ulid;
using Dapper;

public class User
{
	public int Id { get; set; }
	public Ulid AccountId { get; set; }
	public Ulid? ManagedById { get; set; }
}

// Insert: Ulid and Ulid? values (including null) are converted automatically
connection.Execute(
	"INSERT INTO Users (AccountId, ManagedById) VALUES (@AccountId, @ManagedById)",
	new User { AccountId = Ulid.New(), ManagedById = null }
);

// Query by Ulid parameter
var user = connection.QueryFirstOrDefault<User>(
	"SELECT * FROM Users WHERE AccountId = @Id",
	new { Id = myUlid }
);

// Time-range query
var recent = connection.Query<User>(
	"SELECT * FROM Users WHERE AccountId >= @Min AND AccountId <= @Max",
	new
	{
		Min = Ulid.MinAt(DateTimeOffset.UtcNow.AddDays(-1)),
		Max = Ulid.MaxAt(DateTimeOffset.UtcNow)
	}
);
```

Range queries follow the ULID order only for storage formats that preserve it; see the sort order section below.

## ⚠️ Limitations

### One format per application

Unlike full object-relational mappers such as Entity Framework Core, Dapper maps .NET types globally in a 1:1 scheme (`Type` → `TypeHandler`). An application can use exactly one storage format. If you call `RegisterUlid(UlidStorageFormat.String)`, you cannot also have tables that store `Ulid` as `BINARY(16)` in the same process. The last registration overrides all earlier ones.

If one table needs a different format, pass and read that column as a `string` or `byte[]` and convert it yourself. The registered format then applies only to `Ulid`-typed members and parameters. For example, with `Binary` registered globally and a legacy table that stores `CHAR(26)`:

```csharp
public class LegacyOrder
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
}

var order = connection.QueryFirstOrDefault<LegacyOrder>(
	"SELECT Id, Name FROM LegacyOrders WHERE Id = @Id",
	new { Id = myUlid.ToString() }
);

var orderId = Ulid.Parse(order.Id);
```

### Sort order by storage format

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

`ByteAether.Ulid.Dapper` registers explicit `SqlMapper.TypeHandler<T>` classes and uses no reflection or runtime code generation, so it is safe for Native AOT and trimming.

> Dapper's own Native AOT support is configured separately from this package; see [Dapper.AOT](https://github.com/DapperLib/DapperAOT).

## 📜 License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/ByteAether/Ulid/blob/main/LICENSE) file for details.
