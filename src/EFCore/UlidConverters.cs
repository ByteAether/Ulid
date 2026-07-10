using ByteAether.Ulid.DB.Shared;
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
	ulid => MsSqlUlidShuffler.ToSqlServerGuid(ulid),
	guid => MsSqlUlidShuffler.FromSqlServerGuid(guid),
	_defaultHints
)
{
	private static readonly ConverterMappingHints _defaultHints = new(size: 16);
}