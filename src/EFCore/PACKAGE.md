# ULID Entity Framework Core Integration
*from ByteAether*

[![License](https://img.shields.io/github/license/ByteAether/Ulid?logo=github&label=License)](https://github.com/ByteAether/Ulid/blob/main/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid.EntityFrameworkCore?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.Ulid.EntityFrameworkCore/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.Ulid.EntityFrameworkCore?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.Ulid.EntityFrameworkCore/)

An official extension package for `ByteAether.Ulid`, providing seamless integration with Entity Framework Core. It enables effortless mapping of `Ulid` and `Ulid?` properties to database columns using customizable persistence strategies.

For the core library and full details, visit our [GitHub repository](https://github.com/ByteAether/Ulid).

## Features
![.NET AOT Ready](https://img.shields.io/badge/.NET-AOT_Ready-blue)
![.NET 10.0](https://img.shields.io/badge/.NET-10.0-brightgreen)
![.NET 9.0](https://img.shields.io/badge/.NET-9.0-brightgreen)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-brightgreen)
![.NET 7.0](https://img.shields.io/badge/.NET-7.0-green)
![.NET 6.0](https://img.shields.io/badge/.NET-6.0-green)

- **Automated Configuration**: Register mappings globally for both nullable and non-nullable `Ulid` types using a single extension method.
- **Flexible Storage Strategies**: Choose how your identifiers are persisted based on your database engine constraints:
	- `String`: 26-character Crockford Base32 string (e.g., `CHAR(26)`). **(Default)**
	- `Binary`: 16-byte binary payload (e.g., `BINARY(16)`).
	- `Guid`: Native UUID format (ideal for PostgreSQL `uuid`).
	- `SqlServerGuid`: Shuffled SQL Server sequential `uniqueidentifier` to maintain native index sorting properties.

## Installation

Install the stable package via NuGet:
```sh
dotnet add package ByteAether.Ulid.EntityFrameworkCore
```

## Usage

Override the `ConfigureConventions` method in your `DbContext` to register the type mappings across all entities:

```csharp
using Microsoft.EntityFrameworkCore;
using ByteAether.Ulid.EntityFrameworkCore;

public class MyDbContext : DbContext
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Configures mappings globally using your chosen database storage format
        // Valid options are String (default, if left empty), Binary, Guid, and SqlServerGuid
        configurationBuilder.RegisterUlid(UlidStorageFormat.Binary);
    }
}
```

### Per-Property Mapping
For mixed-database strategies or fine-grained column mapping, apply the dedicated ValueConverter classes individually by overriding `OnModelCreating` in your `DbContext`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Persist as CHAR(26) string
    modelBuilder.Entity<User>().Property(u => u.Id).HasConversion<UlidToStringConverter>();

    // Persist as BINARY(16) array
    modelBuilder.Entity<Order>().Property(o => o.Id).HasConversion<UlidToBytesConverter>();

    // Persist as native UUID/Guid
    modelBuilder.Entity<Product>().Property(p => p.Id).HasConversion<UlidToGuidConverter>();

    // Persist as an optimized, ordered SQL Server sequential uniqueidentifier
    modelBuilder.Entity<LogEntry>().Property(l => l.Id).HasConversion<UlidToSqlServerGuidConverter>();
}
```

### Time Range Queries (LINQ Translation)

Because ULIDs contain an embedded timestamp component, you can perform high-performance index-backed range queries natively inside EF Core without storing a separate `CreatedAt` column.

This technique is fully supported across `String`, `Binary`, and standard native `Guid` storage strategies (such as PostgreSQL's `uuid` type, which evaluates bytes sequentially from left to right).

> Do not use this pattern if you are using the `SqlServerGuid` format tailored for Microsoft SQL Server. Because `SqlServerGuid` intentionally pushes the timestamp bytes to the end of the structure to optimize physical index insertion, SQL Server's internal right-to-left sorting logic will cause chronological range comparisons to break.

```csharp
using Microsoft.EntityFrameworkCore;

public async Task<List<User>> GetUsersFromPastDay(MyDbContext context)
{
    var cutoffTime = DateTimeOffset.UtcNow.AddDays(-1);
    
    // Generate boundary constraint
    var minUlid = Ulid.MinAt(cutoffTime);

    // Translates directly to: WHERE Id >= @minUlid
    return await context.Users
        .Where(u => u.Id >= minUlid)
        .ToListAsync();
}
```

## Native AOT & Trimming Compatibility

`ByteAether.Ulid.EntityFrameworkCore` is fully trimmed and annotated for **Native AOT** compilation. It introduces zero reflection or dynamic code generation.

> While this extension package is entirely AOT-safe, your underlying application must still conform to Entity Framework Core's native AOT constraints (such as using EF Core Precompiled Models via `dotnet ef dbcontext optimize`).

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/ByteAether/Ulid/blob/main/LICENSE) file for details.
