using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
#if !(NETSTANDARD2_1_OR_GREATER || NETCOREAPP)
using System.Runtime.InteropServices;
#endif

namespace ByteAether.Ulid;

[DebuggerDisplay("{ToString(),nq}")]
public readonly partial struct Ulid
	: IFormattable
#if NET6_0_OR_GREATER
	, ISpanFormattable
#if NET7_0_OR_GREATER
	, ISpanParsable<Ulid>
#if NET8_0_OR_GREATER
	, IUtf8SpanFormattable
#endif
#endif
#endif
{
	/// <summary>
	/// The length of a ULID when encoded as a string in its canonical format.
	/// </summary>
	/// <remarks>
	/// A ULID string consists of 26 characters, encoded using Crockford's Base32 encoding.
	/// </remarks>
	public const byte UlidStringLength = 26;

	private static readonly char[] _base32Chars = "0123456789ABCDEFGHJKMNPQRSTVWXYZ".ToCharArray();
	private static readonly byte[] _base32Bytes = Encoding.UTF8.GetBytes(_base32Chars);
	private static readonly byte[] _inverseBase32 =
	[
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // controls
		255, // space
		255, // !
		255, // "
		255, // #
		255, // $
		255, // %
		255, // &
		255, // '
		255, // (
		255, // )
		255, // *
		255, // +
		255, // ,
		255, // -
		255, // .
		255, // /
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9, // 0-9
		255, 255, 255, 255, 255, 255, 255, // :-@
		10, 11, 12, 13, 14, 15, 16, 17, // A-H
		1, // I
		18, 19, // J-K
		1, // L
		20, 21, // M-N
		0, // O
		22, 23, 24, 25, 26, // P-T
		255, // U
		27, 28, 29, 30, 31, // V-Z
		255, 255, 255, 255, 255, 255, // [-`
		10, 11, 12, 13, 14, 15, 16, 17, // a-h
		1, // i
		18, 19, // j-k
		1, // l
		20, 21, // m-n
		0, // o
		22, 23, 24, 25, 26, // p-t
		255, // u
		27, 28, 29, 30, 31, // v-z
	];

	/// <inheritdoc /> 
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly string ToString(string? format = null, IFormatProvider? formatProvider = null)
	{
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
		return string.Create(UlidStringLength, this, (span, ulid) => ulid.TryFill(span, _base32Chars));
#else
		Span<char> span = stackalloc char[UlidStringLength];
		TryFill(span, _base32Chars);
		unsafe
		{
			return new string((char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), 0, UlidStringLength);
		}
#endif
	}

	/// <summary>
	/// Parses a ULID from the provided read-only span of characters.
	/// </summary>
	/// <param name="chars">The span of characters containing Crockford's Base32 representation of the ULID.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <returns>A parsed instance of <see cref="Ulid"/>.</returns>
	/// <exception cref="FormatException">Thrown if the input span does not meet the ULID format requirements.</exception>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	public static Ulid Parse(ReadOnlySpan<char> chars, IFormatProvider? provider = null)
	{
		// Sanity check.
		if (chars.Length != UlidStringLength)
		{
			throw new FormatException();
		}

		// Decode.
		Ulid ulid = default;

		unsafe
		{
			var ulidBytes = new Span<byte>(Unsafe.AsPointer(ref Unsafe.AsRef(in ulid)), _ulidSize);

			ulidBytes[15] = (byte)((_inverseBase32[chars[24]] << 5) | _inverseBase32[chars[25]]);

			ulidBytes[00] = (byte)((_inverseBase32[chars[0]] << 5) | _inverseBase32[chars[1]]);
			ulidBytes[01] = (byte)((_inverseBase32[chars[2]] << 3) | (_inverseBase32[chars[3]] >> 2));
			ulidBytes[02] = (byte)((_inverseBase32[chars[3]] << 6) | (_inverseBase32[chars[4]] << 1) | (_inverseBase32[chars[5]] >> 4));
			ulidBytes[03] = (byte)((_inverseBase32[chars[5]] << 4) | (_inverseBase32[chars[6]] >> 1));
			ulidBytes[04] = (byte)((_inverseBase32[chars[6]] << 7) | (_inverseBase32[chars[7]] << 2) | (_inverseBase32[chars[8]] >> 3));
			ulidBytes[05] = (byte)((_inverseBase32[chars[8]] << 5) | _inverseBase32[chars[9]]);
			ulidBytes[06] = (byte)((_inverseBase32[chars[10]] << 3) | (_inverseBase32[chars[11]] >> 2));
			ulidBytes[07] = (byte)((_inverseBase32[chars[11]] << 6) | (_inverseBase32[chars[12]] << 1) | (_inverseBase32[chars[13]] >> 4));
			ulidBytes[08] = (byte)((_inverseBase32[chars[13]] << 4) | (_inverseBase32[chars[14]] >> 1));
			ulidBytes[09] = (byte)((_inverseBase32[chars[14]] << 7) | (_inverseBase32[chars[15]] << 2) | (_inverseBase32[chars[16]] >> 3));
			ulidBytes[10] = (byte)((_inverseBase32[chars[16]] << 5) | _inverseBase32[chars[17]]);
			ulidBytes[11] = (byte)((_inverseBase32[chars[18]] << 3) | (_inverseBase32[chars[19]] >> 2));
			ulidBytes[12] = (byte)((_inverseBase32[chars[19]] << 6) | (_inverseBase32[chars[20]] << 1) | (_inverseBase32[chars[21]] >> 4));
			ulidBytes[13] = (byte)((_inverseBase32[chars[21]] << 4) | (_inverseBase32[chars[22]] >> 1));
			ulidBytes[14] = (byte)((_inverseBase32[chars[22]] << 7) | (_inverseBase32[chars[23]] << 2) | (_inverseBase32[chars[24]] >> 3));
		}

		return ulid;
	}

	/// <summary>
	/// Parses a ULID from a read-only span of bytes and returns the corresponding ULID value.
	/// </summary>
	/// <param name="chars">The read-only span of bytes containing the ULID string representation in Crockford's Base32 format.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <returns>The ULID parsed from the specified byte span.</returns>
	/// <exception cref="FormatException">Thrown if the input byte span does not contain a valid ULID string representation.</exception>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	public static Ulid Parse(ReadOnlySpan<byte> chars, IFormatProvider? provider = null)
	{
		// Sanity check.
		if (chars.Length != UlidStringLength)
		{
			throw new FormatException();
		}

		// Decode.
		Ulid ulid = default;

		unsafe
		{
			var ulidBytes = new Span<byte>(Unsafe.AsPointer(ref Unsafe.AsRef(in ulid)), _ulidSize);

			ulidBytes[15] = (byte)((_inverseBase32[chars[24]] << 5) | _inverseBase32[chars[25]]);

			ulidBytes[00] = (byte)((_inverseBase32[chars[0]] << 5) | _inverseBase32[chars[1]]);
			ulidBytes[01] = (byte)((_inverseBase32[chars[2]] << 3) | (_inverseBase32[chars[3]] >> 2));
			ulidBytes[02] = (byte)((_inverseBase32[chars[3]] << 6) | (_inverseBase32[chars[4]] << 1) | (_inverseBase32[chars[5]] >> 4));
			ulidBytes[03] = (byte)((_inverseBase32[chars[5]] << 4) | (_inverseBase32[chars[6]] >> 1));
			ulidBytes[04] = (byte)((_inverseBase32[chars[6]] << 7) | (_inverseBase32[chars[7]] << 2) | (_inverseBase32[chars[8]] >> 3));
			ulidBytes[05] = (byte)((_inverseBase32[chars[8]] << 5) | _inverseBase32[chars[9]]);
			ulidBytes[06] = (byte)((_inverseBase32[chars[10]] << 3) | (_inverseBase32[chars[11]] >> 2));
			ulidBytes[07] = (byte)((_inverseBase32[chars[11]] << 6) | (_inverseBase32[chars[12]] << 1) | (_inverseBase32[chars[13]] >> 4));
			ulidBytes[08] = (byte)((_inverseBase32[chars[13]] << 4) | (_inverseBase32[chars[14]] >> 1));
			ulidBytes[09] = (byte)((_inverseBase32[chars[14]] << 7) | (_inverseBase32[chars[15]] << 2) | (_inverseBase32[chars[16]] >> 3));
			ulidBytes[10] = (byte)((_inverseBase32[chars[16]] << 5) | _inverseBase32[chars[17]]);
			ulidBytes[11] = (byte)((_inverseBase32[chars[18]] << 3) | (_inverseBase32[chars[19]] >> 2));
			ulidBytes[12] = (byte)((_inverseBase32[chars[19]] << 6) | (_inverseBase32[chars[20]] << 1) | (_inverseBase32[chars[21]] >> 4));
			ulidBytes[13] = (byte)((_inverseBase32[chars[21]] << 4) | (_inverseBase32[chars[22]] >> 1));
			ulidBytes[14] = (byte)((_inverseBase32[chars[22]] << 7) | (_inverseBase32[chars[23]] << 2) | (_inverseBase32[chars[24]] >> 3));
		}

		return ulid;
	}

	/// <summary>
	/// Parses a string representation of a ULID and returns the corresponding ULID instance.
	/// </summary>
	/// <param name="s">The string representation of the ULID to parse.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <returns>A new <see cref="Ulid"/> instance parsed from the specified string.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid Parse(string s, IFormatProvider? provider = null)
		=> Parse(s.AsSpan());

	/// <summary>
	/// Attempts to parse a string representation of a ULID into a <see cref="Ulid"/> instance.
	/// </summary>
	/// <param name="s">The string representation of the ULID to parse.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <param name="result">When this method returns, contains the parsed <see cref="Ulid"/> value if the parse was successful; otherwise, the default value of <see cref="Ulid"/>.</param>
	/// <returns><c>true</c> if the parsing was successful; otherwise, <c>false</c>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Ulid result)
		=> TryParse(s.AsSpan(), provider, out result);

	/// <summary>
	/// Attempts to parse a ULID from a read-only span of characters.
	/// </summary>
	/// <param name="s">The read-only span of characters to parse.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <param name="result">When the method returns, contains the parsed ULID if the operation succeeds, or the default value if it fails.</param>
	/// <returns><c>true</c> if the parsing operation succeeded; otherwise, <c>false</c>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Ulid result)
	{
		try
		{
			result = Parse(s);
			return true;
		}
		catch
		{
			result = default;
			return false;
		}
	}

	/// <summary>
	/// Attempts to parse a ULID from the specified span of bytes.
	/// </summary>
	/// <param name="s">The span of bytes containing the ULID representation to parse.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <param name="result">When the method returns, contains the parsed ULID if parsing was successful; otherwise, the default value for ULID.</param>
	/// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse(ReadOnlySpan<byte> s, IFormatProvider? provider, out Ulid result)
	{
		try
		{
			result = Parse(s);
			return true;
		}
		catch
		{
			result = default;
			return false;
		}
	}

	/// <summary>
	/// Attempts to format the current instance of <see cref="Ulid"/> into the provided character span.
	/// </summary>
	/// <param name="destination">A span of characters where the formatted <see cref="Ulid"/> will be written, if successful.</param>
	/// <param name="charsWritten">The number of characters written to the destination span.</param>
	/// <param name="format">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <returns>
	/// <c>true</c> if the formatting is successful and the destination span is large enough to contain the formatted data; otherwise, <c>false</c>.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryFormat(
		Span<char> destination,
		out int charsWritten,
		ReadOnlySpan<char> format,
		IFormatProvider? provider = null
	)
	{
		if (TryFill(destination, _base32Chars))
		{
			charsWritten = UlidStringLength;
			return true;
		}

		charsWritten = 0;
		return false;
	}

	/// <summary>
	/// Attempts to format the current Ulid instance as a sequence of bytes.
	/// </summary>
	/// <param name="destination">The span of bytes to write the formatted Ulid into.</param>
	/// <param name="bytesWritten">When this method returns, contains the number of bytes that were written to the <paramref name="destination"/> span.</param>
	/// <param name="format">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <returns>
	/// <c>true</c> if the formatting was successful; <c>false</c> if the destination span was too short to contain the formatted value.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool TryFormat(
		Span<byte> destination,
		out int bytesWritten,
		ReadOnlySpan<char> format,
		IFormatProvider? provider = null
	)
	{
		if (TryFill(destination, _base32Bytes))
		{
			bytesWritten = UlidStringLength;
			return true;
		}

		bytesWritten = 0;
		return false;
	}

	private bool TryFill<T>(Span<T> span, T[] map)
	{
		if (span.Length < UlidStringLength)
		{
			return false;
		}

		// Eliminate bounds-check of span
		span[25] = map[_r9 & 0x1F];                      // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111|11111|]

		// Encode timestamp
		span[0] = map[_t0 >> 5];                         // |00[111|11111][11111111][11111111][11111111][11111111][11111111]
		span[1] = map[_t0 & 0x1F];                       // 00[111|11111|][11111111][11111111][11111111][11111111][11111111]
		span[2] = map[_t1 >> 3];                         // 00[11111111][|11111|111][11111111][11111111][11111111][11111111]
		span[3] = map[((_t1 & 0x7) << 2) | (_t2 >> 6)];  // 00[11111111][11111|111][11|111111][11111111][11111111][11111111]
		span[4] = map[(_t2 >> 1) & 0x1F];                // 00[11111111][11111111][11|11111|1][11111111][11111111][11111111]
		span[5] = map[((_t2 & 0x1) << 4) | (_t3 >> 4)];  // 00[11111111][11111111][1111111|1][1111|1111][11111111][11111111]
		span[6] = map[((_t3 & 0xF) << 1) | (_t4 >> 7)];  // 00[11111111][11111111][11111111][1111|1111][1|1111111][11111111]
		span[7] = map[(_t4 >> 2) & 0x1F];                // 00[11111111][11111111][11111111][11111111][1|11111|11][11111111]
		span[8] = map[((_t4 & 0x3) << 3) | (_t5 >> 5)];  // 00[11111111][11111111][11111111][11111111][111111|11][111|11111]
		span[9] = map[_t5 & 0x1F];                       // 00[11111111][11111111][11111111][11111111][11111111][111|11111|]

		// Encode randomness
		span[10] = map[(_r0 >> 3) & 0x1F];               // [|11111|111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[11] = map[((_r0 & 0x7) << 2) | (_r1 >> 6)]; // [11111|111][11|111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[12] = map[(_r1 >> 1) & 0x1F];               // [11111111][11|11111|1][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[13] = map[((_r1 & 0x1) << 4) | (_r2 >> 4)]; // [11111111][1111111|1][1111|1111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[14] = map[((_r2 & 0xF) << 1) | (_r3 >> 7)]; // [11111111][11111111][1111|1111][1|1111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[15] = map[(_r3 >> 2) & 0x1F];               // [11111111][11111111][11111111][1|11111|11][11111111][11111111][11111111][11111111][11111111][11111111]
		span[16] = map[((_r3 & 0x3) << 3) | (_r4 >> 5)]; // [11111111][11111111][11111111][111111|11][111|11111][11111111][11111111][11111111][11111111][11111111]
		span[17] = map[_r4 & 0x1F];                      // [11111111][11111111][11111111][11111111][111|11111|][11111111][11111111][11111111][11111111][11111111]
		span[18] = map[(_r5 >> 3) & 0x1F];               // [11111111][11111111][11111111][11111111][11111111][|11111|111][11111111][11111111][11111111][11111111]
		span[19] = map[((_r5 & 0x7) << 2) | (_r6 >> 6)]; // [11111111][11111111][11111111][11111111][11111111][11111|111][11|111111][11111111][11111111][11111111]
		span[20] = map[(_r6 >> 1) & 0x1F];               // [11111111][11111111][11111111][11111111][11111111][11111111][11|11111|1][11111111][11111111][11111111]
		span[21] = map[((_r6 & 0x1) << 4) | (_r7 >> 4)]; // [11111111][11111111][11111111][11111111][11111111][11111111][1111111|1][1111|1111][11111111][11111111]
		span[22] = map[((_r7 & 0xF) << 1) | (_r8 >> 7)]; // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][1111|1111][1|1111111][11111111]
		span[23] = map[(_r8 >> 2) & 0x1F];               // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][1|11111|11][11111111]
		span[24] = map[((_r8 & 0x3) << 3) | (_r9 >> 5)]; // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111111|11][111|11111]

		return true;
	}
}
