using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ByteAether.Ulid.Cli;

/// <summary>
/// Provides methods for inspecting existing ULIDs and analyzing their time and randomness components.
/// </summary>
public static class Inspector
{
	/// <summary>
	/// Inspects a given <see cref="ByteAether.Ulid.Ulid"/> instance.
	/// </summary>
	/// <param name="ulid">The ULID to inspect.</param>
	/// <returns>An <see cref="InspectionResult"/> detailing the ULID components.</returns>
	public static InspectionResult Inspect(Ulid ulid)
	{
		var ulidString = ulid.ToString();
		var bytes = ulid.ToByteArray();
		var ulidHex = Convert.ToHexString(bytes);
		var timestamp = (long)(BinaryPrimitives.ReadUInt64BigEndian(bytes) >> 16);
		var timeIso = timestamp <= DateTimeOffset.MaxValue.ToUnixTimeMilliseconds()
			? DateTimeOffset.FromUnixTimeMilliseconds(timestamp).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture)
			: "N/A";

		return new(
			Ulid: ulidString,
			Hex: ulidHex,
			Time: ulidString[..10],
			TimeHex: ulidHex[..12],
			TimeIso: timeIso,
			Timestamp: timestamp,
			Random: ulidString[10..],
			RandomHex: ulidHex[12..]
		);
	}

	/// <summary>
	/// Attempts to inspect a string representing a ULID in canonical Crockford Base32, hex, or GUID format.
	/// </summary>
	/// <param name="input">The string to inspect.</param>
	/// <param name="result">When this method returns, contains the inspection result if successful; otherwise, null.</param>
	/// <param name="errorMessage">When this method returns, contains an error message if parsing failed; otherwise, null.</param>
	/// <returns>True if inspection succeeded; otherwise, false.</returns>
	public static bool TryInspect(
		string input,
		[NotNullWhen(true)] out InspectionResult? result,
		[NotNullWhen(false)] out string? errorMessage)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			result = null;
			errorMessage = "ULID input cannot be empty.";
			return false;
		}

		var trimmed = input.Trim();

		// 1. Try canonical Crockford Base32 representation (26 chars)
		if (Ulid.TryParse(trimmed, null, out var ulid))
		{
			result = Inspect(ulid);
			errorMessage = null;
			return true;
		}

		// 2. Try raw 32-character Hex representation (16 bytes)
		if (Hex.TryParseBytes(trimmed, 16, out var bytes))
		{
			result = Inspect(Ulid.New(bytes));
			errorMessage = null;
			return true;
		}

		// 3. Try standard GUID representation
		if (Guid.TryParse(trimmed, out var guid))
		{
			result = Inspect(Ulid.New(guid));
			errorMessage = null;
			return true;
		}

		result = null;
		errorMessage = $"The input '{trimmed}' is not a valid ULID. Expected 26 Crockford Base32 characters, 32 hex characters, or a GUID format.";
		return false;
	}
}