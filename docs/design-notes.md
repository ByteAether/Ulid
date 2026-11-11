# Design notes

These notes explain four design points of ByteAether.Ulid: how random-part overflow is handled, how predictable sequences are limited, how ordering within a millisecond compares with UUIDv7, and why the library keeps big-endian byte order when it talks to databases and `Guid`.

## Overflow handling

Within one millisecond, a monotonic generator increments the 80-bit random component of each new ULID. Traditional libraries throw an `OverflowException` if that component saturates, to protect strict timestamp boundaries.

ByteAether.Ulid advances the timestamp by one millisecond and continues the sequence from there. The wall clock catches up with the advanced timestamp later. If the sequence reaches the absolute maximum ULID, the next increment throws `OverflowException`. This follows the workaround in [ULID specification issue #39](https://github.com/ulid/spec/issues/39#issuecomment-2252145597) and is the one deliberate deviation from the specification, which requires an error when the random component overflows.

## Mitigating enumeration attacks

With the default `MonotonicIncrement`, IDs generated within one millisecond differ by exactly 1. Anyone who sees one of them can compute its neighbors, which makes enumeration attacks easier.

The `MonotonicRandom1Byte` to `MonotonicRandom4Byte` options add a random increment instead, as discussed in [ULID specification issue #105](https://github.com/ulid/spec/issues/105). The IDs stay strictly ordered and become harder to guess. This is opt-in, and it only affects IDs generated within the same millisecond.

The increments come from `IncrementRandomSource`, which defaults to `PseudoRandomProvider`. That source is fast but predictable: it makes guessing harder, not impossible. Set `IncrementRandomSource` to `CryptographicallySecureRandomProvider` if the increments must not be predictable. See [generation options](generation-options.md) for the available sources.

## Ordering within a millisecond compared with UUIDv7

UUIDv7 also sorts by timestamp, but [RFC 9562](https://www.rfc-editor.org/rfc/rfc9562#name-monotonicity-and-counters) makes sub-millisecond monotonicity optional. [The .NET UUIDv7 provider (`Guid.CreateVersion7`)](https://github.com/dotnet/runtime/blob/571b044582ceb7fe426b7f143c703064aa9ea4db/src/libraries/System.Private.CoreLib/src/System/Guid.cs#L306) fills the sub-millisecond bits with random data instead of a sequential counter, so IDs created in the same millisecond are not guaranteed to be in order.

## `Guid` byte order in databases

`System.Guid` has a mixed-endian internal layout. Most database providers, including SQLite's, write that memory layout unchanged, and a big-endian timestamp stored this way no longer sorts chronologically. The PostgreSQL driver is an exception: it converts the layout when it writes a native `uuid`. SQL Server compares `uniqueidentifier` values in [its own byte-group order](https://learn.microsoft.com/en-us/dotnet/api/system.data.sqltypes.sqlguid.compareto?view=net-10.0#remarks), so a ULID stored there as-is arrives in an order unrelated to its timestamp, which causes page splits and fragmentation on a clustered index.

The ULID specification orders IDs by their big-endian bytes, and ByteAether.Ulid keeps that order. It offers four storage formats (`String`, `Binary`, `Guid` and `SqlServerGuid`) for EF Core, LinqToDB and Dapper. [Storage formats](storage-formats.md) shows which format keeps order on which database engine.
