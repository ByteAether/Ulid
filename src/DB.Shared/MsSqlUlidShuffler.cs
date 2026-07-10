namespace ByteAether.Ulid.DB.Shared;

public static class MsSqlUlidShuffler
{
	public static Guid ToSqlServerGuid(Ulid ulid)
	{
		var source = ulid.AsByteSpan();
		Span<byte> shuffled = stackalloc byte[16];

		// MSSQL sorts uniqueidentifier values from right to left across byte groups (bytes 10-15 highest).
		// Move the 6-byte ULID timestamp (bytes 0-5) to the end (bytes 10-15).
		source[0..6].CopyTo(shuffled[10..16]);

		// Move the 10-byte random/increment part to bytes 0-9.
		source[6..16].CopyTo(shuffled[0..10]);

		return Ulid.New(shuffled).ToGuid();
	}

	public static Ulid FromSqlServerGuid(Guid guid)
	{
		var shuffledUlid = Ulid.New(guid);
		var shuffledBytes = shuffledUlid.AsByteSpan();

		Span<byte> originalBytes = stackalloc byte[16];

		// Reverse shuffling: move timestamp from bytes 10-15 back to 0-5.
		shuffledBytes[10..16].CopyTo(originalBytes[0..6]);

		// Move randomness from bytes 0-9 back to 6-15.
		shuffledBytes[0..10].CopyTo(originalBytes[6..16]);

		return Ulid.New(originalBytes);
	}
}