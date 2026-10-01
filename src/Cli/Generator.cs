namespace ByteAether.Ulid.Cli;

/// <summary>
/// Provides methods for generating ULIDs based on optional timestamp, time bytes, and random bytes.
/// </summary>
public static class Generator
{
	/// <summary>
	/// Generates a new <see cref="ByteAether.Ulid.Ulid"/> using the provided optional components and default generation options.
	/// </summary>
	/// <param name="timestamp">Optional timestamp in milliseconds since the Unix epoch.</param>
	/// <param name="randomBytes">Optional 10-byte span representing the random component.</param>
	/// <returns>A new <see cref="ByteAether.Ulid.Ulid"/> instance.</returns>
	/// <exception cref="ArgumentException">Thrown when invalid parameters or conflicting options are specified.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the timestamp is outside the 48-bit ULID range.</exception>
	public static Ulid Generate(
		long? timestamp = null,
		ReadOnlySpan<byte> randomBytes = default)
	{
		if (randomBytes.Length > 0 && randomBytes.Length != 10)
		{
			throw new ArgumentException("Random bytes must be exactly 10 bytes in length.", nameof(randomBytes));
		}

		long effectiveTimestamp;
		if (timestamp.HasValue)
		{
			effectiveTimestamp = timestamp.Value;
		}
		else
		{
			effectiveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}

		if (effectiveTimestamp < 0 || effectiveTimestamp > InputParser.MaxTimestampMs)
		{
			throw new ArgumentOutOfRangeException(
				nameof(timestamp),
				effectiveTimestamp,
				$"Timestamp must be between 0 and {InputParser.MaxTimestampMs} milliseconds."
			);
		}

		if (randomBytes.Length != 10)
		{
			return Ulid.New(effectiveTimestamp, Ulid.DefaultGenerationOptions);
		}

		Span<byte> randomSpan = stackalloc byte[10];
		randomBytes.CopyTo(randomSpan);
		return Ulid.New(effectiveTimestamp, randomSpan);
	}
}