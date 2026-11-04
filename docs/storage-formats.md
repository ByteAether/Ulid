# Storage formats

The database packages ([Entity Framework Core](https://www.nuget.org/packages/ByteAether.Ulid.EntityFrameworkCore/), [LinqToDB](https://www.nuget.org/packages/ByteAether.Ulid.linq2db/) and [Dapper](https://www.nuget.org/packages/ByteAether.Ulid.Dapper/)) store a `Ulid` in one of four formats. The format decides whether `>=`, `<=` and `ORDER BY` on the column follow the chronological order of the ULIDs.

A ULID sorts correctly when its bytes are compared left to right, with the big-endian timestamp first. `System.Guid` is mixed-endian, and database engines compare `uuid` and `uniqueidentifier` values with their own rules, so both the format and the engine matter.

## Format overview

| Format | Column type | Range queries and `ORDER BY` | Portable between engines |
| --- | --- | --- | --- |
| `String` (default) | `CHAR(26)` | Follow ULID order on every engine, with a binary collation. | Yes |
| `Binary` | `BINARY(16)` | Follow ULID order on every engine. | Yes |
| `Guid` | native `uuid` | PostgreSQL: follow ULID order. SQL Server, SQLite and others: do not. Equality lookups work everywhere. | Yes |
| `SqlServerGuid` | `uniqueidentifier` | SQL Server: ordered by timestamp to the millisecond. See [`SqlServerGuid`](#sqlserverguid) for the same-millisecond limit. | No |

The column types are the SQL Server and MySQL names. Use the engine's equivalent elsewhere, for example `bytea` for `Binary` and `uuid` for `Guid` on PostgreSQL.

## Choosing a format

- PostgreSQL: `Guid` (native `uuid`), `String` or `Binary`.
- SQL Server: `SqlServerGuid` with a `uniqueidentifier` column, `String` or `Binary`.
- SQLite, MySQL and other engines: `String` or `Binary`.
- Several engines, or data that moves between them: `String` or `Binary`.

## Format details

### `String` and `Binary`

Both preserve the raw left-to-right order of the ULID bytes, so they sort and compare correctly in every database engine. `String` is easier to read and debug. `Binary` is smaller.

`String` is ordered by the column's collation. The Crockford Base32 alphabet is `0-9` followed by `A-Z`, which matches byte order under a binary or ordinal collation. If the engine's default collation is linguistic, give the column a binary collation (for example `utf8mb4_bin` on MySQL or `Latin1_General_BIN2` on SQL Server) before relying on range queries or `ORDER BY`.

### `Guid`

A .NET `Guid` stores its first three fields in little-endian byte order, and each engine treats that differently:

- PostgreSQL: the driver corrects the byte order when it maps a `Guid` to a native `uuid` column, so chronological sorting and range queries work.
- SQLite and others: engines that store GUIDs as raw bytes or text compare the mixed-endian layout, which breaks chronological comparisons. Range queries and `ORDER BY` return wrong results, although equality lookups stay correct.
- SQL Server: `uniqueidentifier` has its own comparison order (see below), which does not match the `Guid` byte layout. Use `SqlServerGuid`.

### `SqlServerGuid`

SQL Server compares `uniqueidentifier` values by byte groups, from the last group to the first: bytes 10 to 15, then 8 and 9, 6 and 7, 4 and 5, and 0 to 3. `SqlServerGuid` moves the 6-byte timestamp into bytes 10 to 15, so the timestamp is compared first. The 10-byte random part goes into bytes 0 to 9.

- The column must be `uniqueidentifier`. A `BINARY(16)` or `VARCHAR` column breaks the ordering.
- Time-window queries are exact. `Ulid.MinAt(start)` and `Ulid.MaxAt(end)` return the first and last possible ULID of a millisecond (random part all zeros or all ones), so `Id >= min AND Id <= max` selects exactly the ULIDs created in that window.
- Within one millisecond, SQL Server compares the byte groups of the random part in a different order than the ULID does. IDs generated in the same millisecond can therefore sort differently from their ULID order, including IDs from one monotonic sequence. Ordering by timestamp is always correct. If your queries depend on the exact order of IDs generated in the same millisecond, for example keyset pagination with `Id > @last` on a busy table, use `String` or `Binary`.
- The byte shuffling costs portability. Rows read or migrated directly into PostgreSQL or SQLite do not contain valid ULID bytes. In return, SQL Server fills index pages in timestamp order, which reduces page splits and fragmentation on insert.

## Time-range filtering

When the storage format keeps the ULID order, a range query returns the same rows in the database as in memory. Build the bounds with `Ulid.MinAt` and `Ulid.MaxAt`:

```csharp
Ulid min = Ulid.MinAt(DateTimeOffset.UtcNow.AddDays(-7));
Ulid max = Ulid.MaxAt(DateTimeOffset.UtcNow);
```

Check the table above for your database engine before running range queries or `ORDER BY` against a column. A format that does not preserve order returns wrong rows or skips records without raising an error.
