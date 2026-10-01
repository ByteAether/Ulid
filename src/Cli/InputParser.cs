using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ByteAether.Ulid.Cli;

/// <summary>
/// Helper methods for parsing command-line inputs into ULID components.
/// </summary>
public static class InputParser
{
	/// <summary>
	/// Maximum allowable 48-bit timestamp value in milliseconds (year 10889).
	/// </summary>
	public const long MaxTimestampMs = 0x0000_FFFF_FFFF_FFFFL; // 281474976710655

	/// <summary>
	/// Parses a time input which may be an ISO 8601 date string, Unix epoch milliseconds, or custom 6 time bytes.
	/// </summary>
	public static bool TryParseTime(
		string input,
		[NotNullWhen(true)] out long? timestampMs,
		[NotNullWhen(false)] out string? errorMessage)
	{
		if (TryParseTimeBytes(input, out var timeBytes, out _))
		{
			if (timeBytes.Length != 6)
			{
				timestampMs = null;
				errorMessage = "Time bytes must be exactly 6 bytes in length.";
				return false;
			}

			timestampMs =
				((long)timeBytes[0] << 40) |
				((long)timeBytes[1] << 32) |
				((long)timeBytes[2] << 24) |
				((long)timeBytes[3] << 16) |
				((long)timeBytes[4] << 8) |
				timeBytes[5];

			errorMessage = null;
			return true;
		}

		timestampMs = ParseTimestamp(input, out errorMessage);
		if (timestampMs.HasValue)
		{
			return true;
		}

		errorMessage ??= $"Invalid timestamp/time bytes '{input.Trim()}'. Expected an ISO 8601 date string, Unix epoch milliseconds, 12 hex time bytes, 10 Crockford Base32 time bytes, or separated byte values.";
		return false;
	}

	private static long? ParseTimestamp(
		string input,
		out string? rangeError)
	{
		var trimmed = input.Trim();

		// Try numeric Unix epoch milliseconds
		if (long.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedLong))
		{
			if (parsedLong < 0 || parsedLong > MaxTimestampMs)
			{
				rangeError = $"Timestamp '{trimmed}' is out of range. It must be between 0 and {MaxTimestampMs} milliseconds.";
				return null;
			}

			rangeError = null;
			return parsedLong;
		}

		// Try ISO 8601 / DateTimeOffset
		if (DateTimeOffset.TryParse(
			trimmed,
			CultureInfo.InvariantCulture,
			DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
			out var parsedDto
		))
		{
			var ms = parsedDto.ToUnixTimeMilliseconds();
			if (ms < 0 || ms > MaxTimestampMs)
			{
				rangeError = $"Date '{trimmed}' evaluates to {ms} ms, which is out of the valid 48-bit ULID range (0 to {MaxTimestampMs}).";
				return null;
			}

			rangeError = null;
			return ms;
		}

		rangeError = null;
		return null;
	}

	/// <summary>
	/// Parses custom 6 time bytes from hex or 10-character Crockford Base32.
	/// </summary>
	public static bool TryParseTimeBytes(
		string input,
		[NotNullWhen(true)] out byte[]? timeBytes,
		[NotNullWhen(false)] out string? errorMessage)
	{
		var trimmed = input.Trim();

		if (Hex.TryParseBytes(trimmed, 6, out var hexBytes))
		{
			timeBytes = hexBytes;
			errorMessage = null;
			return true;
		}

		// Check if provided as 10-char Crockford Base32
		if (trimmed.Length == 10 && Ulid.TryParse(trimmed + "0000000000000000", null, out var dummyUlid))
		{
			timeBytes = dummyUlid.TimeBytes.ToArray();
			errorMessage = null;
			return true;
		}

		// Check if provided as comma/space/dash-separated byte values
		if (TryParseSeparatedBytes(trimmed, 6, out timeBytes))
		{
			errorMessage = null;
			return true;
		}

		timeBytes = null;
		errorMessage = $"Invalid time bytes '{trimmed}'. Expected 12 hex characters (e.g. '018D3A5F89B2') or 10 Crockford Base32 characters (e.g. '01AN4Z07BY').";
		return false;
	}

	/// <summary>
	/// Parses custom 10 random bytes from hex or 16-character Crockford Base32.
	/// </summary>
	public static bool TryParseRandomBytes(
		string input,
		[NotNullWhen(true)] out byte[]? randomBytes,
		[NotNullWhen(false)] out string? errorMessage)
	{
		var trimmed = input.Trim();

		if (Hex.TryParseBytes(trimmed, 10, out var hexBytes))
		{
			randomBytes = hexBytes;
			errorMessage = null;
			return true;
		}

		// Check if provided as 16-char Crockford Base32
		if (trimmed.Length == 16 && Ulid.TryParse("0000000000" + trimmed, null, out var dummyUlid))
		{
			randomBytes = dummyUlid.Random.ToArray();
			errorMessage = null;
			return true;
		}

		// Check if provided as comma/space/dash-separated byte values
		if (TryParseSeparatedBytes(trimmed, 10, out randomBytes))
		{
			errorMessage = null;
			return true;
		}

		randomBytes = null;
		errorMessage = $"Invalid random bytes '{trimmed}'. Expected 20 hex characters or 16 Crockford Base32 characters.";
		return false;
	}

	private static bool TryParseSeparatedBytes(string input, int expectedCount, [NotNullWhen(true)] out byte[]? bytes)
	{
		var parts = input.Split([',', ' ', '-'], StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length != expectedCount)
		{
			bytes = null;
			return false;
		}

		var result = new byte[expectedCount];
		for (var i = 0; i < parts.Length; i++)
		{
			var part = parts[i].Trim();
			var isHex = part.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
			var value = isHex ? part[2..] : part;
			var style = isHex ? NumberStyles.HexNumber : NumberStyles.Integer;
			if (byte.TryParse(value, style, CultureInfo.InvariantCulture, out result[i]))
			{
				continue;
			}

			bytes = null;
			return false;
		}

		bytes = result;
		return true;
	}
}