using System.Runtime.CompilerServices;

namespace ByteAether.Ulid;

public readonly partial struct Ulid
{
	/// <summary>
	/// Validates if the given string is a valid ULID.
	/// </summary>
	/// <param name="ulidString">The ULID string to validate.</param>
	/// <returns>
	/// <c>true</c> if the string is a valid ULID, <c>false</c> otherwise.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValid(string ulidString) => IsValid(ulidString.AsSpan());

	/// <summary>
	/// Validates if the given span of characters is a valid ULID.
	/// </summary>
	/// <param name="ulidString">The ULID character span to validate.</param>
	/// <returns>
	/// <c>true</c> if the character span is a valid ULID, <c>false</c> otherwise.
	/// </returns>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	public static bool IsValid(ReadOnlySpan<char> ulidString)
	{
		// Length check
		if (ulidString.Length != UlidStringLength)
		{
			return false;
		}

		var firstChar = ulidString[0];
		if (_inverseBase32[firstChar] > 7)
		{
			return false;
		}

		for (var i = 1; i < UlidStringLength; i++)
		{
			if (_inverseBase32[ulidString[i]] == 255)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Validates if the given byte array represents a valid ULID.
	/// </summary>
	/// <param name="ulidBytes">The byte array to validate.</param>
	/// <returns>
	/// <c>true</c> if the byte array is a valid ULID, <c>false</c> otherwise.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValid(ReadOnlySpan<byte> ulidBytes) => ulidBytes.Length == _ulidSize;
}
