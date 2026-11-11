using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable RedundantExtendsListEntry
#if NET7_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif

namespace ByteAether.Ulid;

[DebuggerDisplay("{ToString(),nq}")]
public readonly partial struct Ulid
	: IFormattable
#if NET6_0_OR_GREATER
	, ISpanFormattable
#if NET7_0_OR_GREATER
	, IParsable<Ulid> // Keeping this here for clarity
	, ISpanParsable<Ulid>
#if NET8_0_OR_GREATER
	, IUtf8SpanFormattable
	, IUtf8SpanParsable<Ulid>
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
	// Stored as RVA static data: no static field load, no initialization check and a constant length
	private static ReadOnlySpan<byte> _inverseBase32
	=> [
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
		// Pad with value 255 so the array size is 256
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
		255, 255, 255, 255, 255
	];

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

	/// <summary>
	/// Returns a string representation of the current instance of <see cref="Ulid"/> in its canonical Crockford's Base32 format.'
	/// </summary>
	/// <returns>Crockford's Base32 representation of the ULID</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
		return string.Create(UlidStringLength, this, static (span, ulid) => ulid.Fill(span, _base32Chars));
#else
		Span<char> span = stackalloc char[UlidStringLength];
		Fill(span, _base32Chars);
		return span.ToString();
#endif
	}

	/// <summary>
	/// Parses a ULID from the provided read-only span of characters.
	/// </summary>
	/// <param name="chars">The span of characters containing Crockford's Base32 representation of the ULID.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <returns>A parsed instance of <see cref="Ulid"/>.</returns>
	/// <inheritdoc cref="IsValid(string)" path="/remarks"/>
	/// <exception cref="FormatException">Thrown if the input span is not a valid ULID string representation.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid Parse(ReadOnlySpan<char> chars, IFormatProvider? provider = null)
	{
		var ulid = ParseCore(chars, out var isValid);
		if (!isValid)
		{
			ThrowInvalidFormat();
		}

		return ulid;
	}

	/// <summary>
	/// Parses a ULID from a read-only span of bytes and returns the corresponding ULID value.
	/// </summary>
	/// <param name="bytes">The read-only span of bytes containing the ULID string representation in Crockford's Base32 format.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <returns>The ULID parsed from the specified byte span.</returns>
	/// <inheritdoc cref="IsValid(string)" path="/remarks"/>
	/// <exception cref="FormatException">Thrown if the input byte span does not contain a valid ULID string representation.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid Parse(ReadOnlySpan<byte> bytes, IFormatProvider? provider = null)
	{
		var ulid = ParseCore(bytes, out var isValid);
		if (!isValid)
		{
			ThrowInvalidFormat();
		}

		return ulid;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Ulid ParseCore<T>(ReadOnlySpan<T> input, out bool isValid) where T : unmanaged
	{
		if (input.Length != UlidStringLength)
		{
			isValid = false;
			return default;
		}

		ref var src = ref MemoryMarshal.GetReference(input);

		if (typeof(T) == typeof(char))
		{
			// Every char must be ASCII, as only its lowest byte is decoded below
#if NET7_0_OR_GREATER
			ref var units = ref Unsafe.As<T, ushort>(ref src);
			var nonAscii = (
				Vector128.LoadUnsafe(ref units)
				| Vector128.LoadUnsafe(ref units, 8)
				| Vector128.LoadUnsafe(ref units, 10)
				| Vector128.LoadUnsafe(ref units, 18)
			) & Vector128.Create((ushort)0xFF80);

			if (nonAscii != Vector128<ushort>.Zero)
#else
			// The 26 chars (52 bytes) are checked as 6 ulongs and 1 uint, independent of the endianness
			ref var bytes = ref Unsafe.As<T, byte>(ref src);
			var units =
				Unsafe.ReadUnaligned<ulong>(ref bytes)
				| Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref bytes, 8))
				| Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref bytes, 16))
				| Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref bytes, 24))
				| Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref bytes, 32))
				| Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref bytes, 40))
				| Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref bytes, 48));

			if ((units & 0xFF80_FF80_FF80_FF80) != 0)
#endif
			{
				isValid = false;
				return default;
			}
		}

		// The 128-bit value is the first character (3 bits) followed by 5 groups of 5 characters (25 bits each).
		// Every table value is either 0..31 or 255 (invalid character).
		ulong invalid = 0;
		var first = Decode(ref src, 0);
		var group1 = DecodeGroup(ref src, 1, ref invalid);
		var group2 = DecodeGroup(ref src, 6, ref invalid);
		var group3 = DecodeGroup(ref src, 11, ref invalid);
		var group4 = DecodeGroup(ref src, 16, ref invalid);
		var group5 = DecodeGroup(ref src, 21, ref invalid);

		// The first character must be 0..7, otherwise the value overflows 128 bits
		invalid |= first << 2;

		if ((invalid & 0xE0) != 0)
		{
			isValid = false;
			return default;
		}

		var hi = (first << 61) | (group1 << 36) | (group2 << 11) | (group3 >> 14);
		var lo = (group3 << 50) | (group4 << 25) | group5;

		Unsafe.SkipInit(out Ulid ulid);
		ref var ulidRef = ref Unsafe.As<Ulid, byte>(ref ulid);
		Unsafe.WriteUnaligned(ref ulidRef, ReverseOnLittleEndian(hi));
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref ulidRef, sizeof(ulong)), ReverseOnLittleEndian(lo));

		isValid = true;
		return ulid;
	}

	// Decodes the character at the index. Non-ASCII bytes (0x80 and above) are mapped to an invalid value in the table.
	// For chars only the lowest byte is read, independent of the endianness.
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong Decode<T>(ref T src, int index) where T : unmanaged
		=> Unsafe.Add(
			ref MemoryMarshal.GetReference(_inverseBase32),
			typeof(T) == typeof(char)
				? (byte)Unsafe.As<T, char>(ref Unsafe.Add(ref src, index))
				: Unsafe.As<T, byte>(ref Unsafe.Add(ref src, index))
		);

	// Decodes 5 characters into 25 bits and accumulates their invalid markers
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong DecodeGroup<T>(ref T src, int index, ref ulong invalid) where T : unmanaged
	{
		var v0 = Decode(ref src, index);
		var v1 = Decode(ref src, index + 1);
		var v2 = Decode(ref src, index + 2);
		var v3 = Decode(ref src, index + 3);
		var v4 = Decode(ref src, index + 4);

		invalid |= (v0 | v1) | (v2 | v3) | v4;

		return ((v0 << 20) | (v1 << 15)) | ((v2 << 10) | (v3 << 5)) | v4;
	}

	[DoesNotReturn]
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ThrowInvalidFormat()
		=> throw new FormatException("The input sequence is not a valid ULID string representation.");

	/// <summary>
	/// Parses a string representation of a ULID and returns the corresponding ULID instance.
	/// </summary>
	/// <param name="s">The string representation of the ULID to parse.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <returns>A new <see cref="Ulid"/> instance parsed from the specified string.</returns>
	/// <inheritdoc cref="IsValid(string)" path="/remarks"/>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="s"/> is <c>null</c>.</exception>
	/// <exception cref="FormatException">Thrown if the input string is not a valid ULID string representation.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid Parse(string s, IFormatProvider? provider = null)
	{
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (s is null)
		{
			ThrowNullString();
		}

		return Parse(s.AsSpan(), provider);
	}

	[DoesNotReturn]
	[MethodImpl(MethodImplOptions.NoInlining)]
	[SuppressMessage("ReSharper", "NotResolvedInText")]
	private static void ThrowNullString()
		=> throw new ArgumentNullException("s");

	/// <summary>
	/// Attempts to parse a string representation of a ULID into a <see cref="Ulid"/> instance.
	/// </summary>
	/// <param name="s">The string representation of the ULID to parse.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <param name="result">When this method returns, contains the parsed <see cref="Ulid"/> value if the parse was successful; otherwise, the default value of <see cref="Ulid"/>.</param>
	/// <returns><c>true</c> if the parsing was successful; otherwise, <c>false</c>.</returns>
	/// <inheritdoc cref="IsValid(string)" path="/remarks"/>
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
	/// <inheritdoc cref="IsValid(string)" path="/remarks"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Ulid result)
	{
		result = ParseCore(s, out var isValid);
		return isValid;
	}

	/// <summary>
	/// Attempts to parse a ULID from the specified span of bytes.
	/// </summary>
	/// <param name="s">The span of bytes containing the ULID representation to parse.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <param name="result">When the method returns, contains the parsed ULID if parsing was successful; otherwise, the default value for ULID.</param>
	/// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
	/// <inheritdoc cref="IsValid(string)" path="/remarks"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse(ReadOnlySpan<byte> s, IFormatProvider? provider, out Ulid result)
	{
		result = ParseCore(s, out var isValid);
		return isValid;
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
	public bool TryFormat(
		Span<char> destination,
		out int charsWritten,
		ReadOnlySpan<char> format,
		IFormatProvider? provider = null
	)
	{
		if (destination.Length < UlidStringLength)
		{
			charsWritten = 0;
			return false;
		}

		Fill(destination, _base32Chars);
		charsWritten = UlidStringLength;
		return true;
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
	public bool TryFormat(
		Span<byte> destination,
		out int bytesWritten,
		ReadOnlySpan<char> format,
		IFormatProvider? provider = null
	)
	{
		if (destination.Length < UlidStringLength)
		{
			bytesWritten = 0;
			return false;
		}

		Fill(destination, _base32Bytes);
		bytesWritten = UlidStringLength;
		return true;
	}

	private void Fill<T>(Span<T> span, T[] map) where T : unmanaged
	{
		// Load the ULID as two big-endian 64-bit halves and extract the 5-bit groups with shifts
		ref var ulidRef = ref Unsafe.As<Ulid, byte>(ref Unsafe.AsRef(in this));
		var hi = ReverseOnLittleEndian(Unsafe.ReadUnaligned<ulong>(ref ulidRef));
		var lo = ReverseOnLittleEndian(Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref ulidRef, sizeof(ulong))));

		// Writing the last element first removes the bounds checks of the other elements
		span[25] = map[(int)lo & 0x1F];
		span[24] = map[(int)(lo >> 5) & 0x1F];
		span[23] = map[(int)(lo >> 10) & 0x1F];
		span[22] = map[(int)(lo >> 15) & 0x1F];
		span[21] = map[(int)(lo >> 20) & 0x1F];
		span[20] = map[(int)(lo >> 25) & 0x1F];
		span[19] = map[(int)(lo >> 30) & 0x1F];
		span[18] = map[(int)(lo >> 35) & 0x1F];
		span[17] = map[(int)(lo >> 40) & 0x1F];
		span[16] = map[(int)(lo >> 45) & 0x1F];
		span[15] = map[(int)(lo >> 50) & 0x1F];
		span[14] = map[(int)(lo >> 55) & 0x1F];
		span[13] = map[(int)((hi << 4) | (lo >> 60)) & 0x1F]; // Lowest bit of hi, highest 4 bits of lo
		span[12] = map[(int)(hi >> 1) & 0x1F];
		span[11] = map[(int)(hi >> 6) & 0x1F];
		span[10] = map[(int)(hi >> 11) & 0x1F];
		span[9] = map[(int)(hi >> 16) & 0x1F];
		span[8] = map[(int)(hi >> 21) & 0x1F];
		span[7] = map[(int)(hi >> 26) & 0x1F];
		span[6] = map[(int)(hi >> 31) & 0x1F];
		span[5] = map[(int)(hi >> 36) & 0x1F];
		span[4] = map[(int)(hi >> 41) & 0x1F];
		span[3] = map[(int)(hi >> 46) & 0x1F];
		span[2] = map[(int)(hi >> 51) & 0x1F];
		span[1] = map[(int)(hi >> 56) & 0x1F];
		span[0] = map[(int)(hi >> 61)];
	}

	/// <summary>
	/// Allows implicit conversion of <see cref="Ulid"/> to <see cref="string"/>.
	/// </summary>
	/// <param name="ulid"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator string(Ulid ulid) => ulid.ToString();

	/// <summary>
	/// Allows implicit conversion of <see cref="string"/> to <see cref="Ulid"/>.
	/// </summary>
	/// <param name="str">The ULID string representation. See <see cref="Parse(string, IFormatProvider)"/>.</param>
	/// <returns>The parsed <see cref="Ulid"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is <c>null</c>.</exception>
	/// <exception cref="FormatException">Thrown if <paramref name="str"/> is not a valid ULID string representation.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Ulid(string str) => Parse(str);
}
