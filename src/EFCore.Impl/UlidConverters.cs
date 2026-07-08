using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ByteAether.Ulid.EntityFrameworkCore;

// ReSharper disable ClassNeverInstantiated.Global

/// <inheritdoc />
public class UlidToBytesConverter() : ValueConverter<Ulid, byte[]>(
	ulid => ulid.ToByteArray(),
	bytes => Ulid.New(bytes),
	_defaultHints
)
{
	private static readonly ConverterMappingHints _defaultHints = new(size: 16);
}

/// <inheritdoc />
public class UlidToStringConverter() : ValueConverter<Ulid, string>(
	ulid => ulid.ToString(),
	str => Ulid.Parse(str, null),
	_defaultHints
)
{
	private static readonly ConverterMappingHints _defaultHints = new(size: 26, unicode: false);
}

/// <inheritdoc />
public class UlidToGuidConverter() : ValueConverter<Ulid, Guid>(
	ulid => ulid.ToGuid(),
	guid => Ulid.New(guid),
	_defaultHints
)
{
	private static readonly ConverterMappingHints _defaultHints = new(size: 16);
}

/// <inheritdoc />
public class UlidToSqlServerGuidConverter() : ValueConverter<Ulid, Guid>(
	ulid => ToSqlServerGuid(ulid),
	guid => FromSqlServerGuid(guid),
	_defaultHints
)
{
	private static readonly ConverterMappingHints _defaultHints = new(size: 16);

	internal static Guid ToSqlServerGuid(Ulid ulid)
    {
        var source = ulid.AsByteSpan();
        Span<byte> shuffled = stackalloc byte[16];

        // MSSQL compares uniqueidentifier values from right to left across byte groups,
        // with bytes 10-15 having the highest sorting priority.
        // We move the 6-byte ULID timestamp (bytes 0-5) to the end (bytes 10-15).
        source[0..6].CopyTo(shuffled[10..16]);

        // Move the 10-byte random/increment part to bytes 0-9.
        source[6..16].CopyTo(shuffled[0..10]);

        // Hand over the shuffled structure to the core library to produce a compliant .NET Guid.
        return Ulid.New(shuffled).ToGuid();
    }

	internal static Ulid FromSqlServerGuid(Guid guid)
    {
        // Materialize the Guid back into a Big-Endian byte layout via the core library.
        var shuffledUlid = Ulid.New(guid);
        var shuffledBytes = shuffledUlid.AsByteSpan();

        Span<byte> originalBytes = stackalloc byte[16];

        // Reverse the shuffling: move timestamp from bytes 10-15 back to 0-5.
        shuffledBytes[10..16].CopyTo(originalBytes[0..6]);

        // Move randomness from bytes 0-9 back to 6-15.
        shuffledBytes[0..10].CopyTo(originalBytes[6..16]);

        return Ulid.New(originalBytes);
    }
}