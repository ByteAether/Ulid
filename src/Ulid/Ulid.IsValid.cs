using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
	public static unsafe bool IsValid(ReadOnlySpan<char> ulidString)
	{
		if (ulidString.Length != UlidStringLength) // 26
		{
			return false;
		}

		fixed (char* src = &MemoryMarshal.GetReference(ulidString))
		{
			// 1. Fast check for the first character (prevent 128-bit overflow)
			uint c0 = src[0];
			if (c0 > 255 || _inverseBase32[c0] > 7)
			{
				return false;
			}

			fixed (byte* table = _inverseBase32)
			{
				// Block A: Indices 1 to 6
				if (((src[1] | src[2] | src[3] | src[4] | src[5] | src[6]) & 0xFF00) != 0)
				{
					return false;
				}

				if (
					table[src[1]] == 255 || table[src[2]] == 255 || table[src[3]] == 255
					|| table[src[4]] == 255 || table[src[5]] == 255 || table[src[6]] == 255
				)
				{
					return false;
				}

				// Block B: Indices 7 to 12
				if (((src[7] | src[8] | src[9] | src[10] | src[11] | src[12]) & 0xFF00) != 0)
				{
					return false;
				}

				if (
					table[src[7]] == 255 || table[src[8]] == 255 || table[src[9]] == 255
					|| table[src[10]] == 255 || table[src[11]] == 255 || table[src[12]] == 255
				)
				{
					return false;
				}

				// Block C: Indices 13 to 18
				if (((src[13] | src[14] | src[15] | src[16] | src[17] | src[18]) & 0xFF00) != 0)
				{
					return false;
				}

				if (
					table[src[13]] == 255 || table[src[14]] == 255 || table[src[15]] == 255
					|| table[src[16]] == 255 || table[src[17]] == 255 || table[src[18]] == 255
				)
				{
					return false;
				}

				// Block D: Indices 19 to 25
				if (((src[19] | src[20] | src[21] | src[22] | src[23] | src[24] | src[25]) & 0xFF00) != 0)
				{
					return false;
				}

				if (
					table[src[19]] == 255 || table[src[20]] == 255 || table[src[21]] == 255
					|| table[src[22]] == 255 || table[src[23]] == 255 || table[src[24]] == 255
					|| table[src[25]] == 255
				)
				{
					return false;
				}
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
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static bool IsValid(ReadOnlySpan<byte> ulidBytes) => ulidBytes.Length == _ulidSize;
}