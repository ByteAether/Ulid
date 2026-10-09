using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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
	/// <remarks>
	/// Applies the same rules as <see cref="IsValid(string)"/>: the input must be exactly 26 characters of
	/// Crockford's Base32 alphabet, read case-insensitively, with <c>I</c> and <c>L</c> accepted as <c>1</c>, and
	/// <c>O</c> as <c>0</c>. The first character must be between <c>0</c> and <c>7</c>.
	/// </remarks>
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
	/// <remarks>
	/// Applies the same rules as <see cref="IsValid(string)"/>: the input must be exactly 26 characters of
	/// Crockford's Base32 alphabet, read case-insensitively, with <c>I</c> and <c>L</c> accepted as <c>1</c>, and
	/// <c>O</c> as <c>0</c>. The first character must be between <c>0</c> and <c>7</c>.
	/// </remarks>
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

	private static Ulid ParseCore(ReadOnlySpan<char> input, out bool isValid)
	{
		if (input.Length != UlidStringLength)
		{
			isValid = false;
			return default;
		}

		// Every char must be ASCII, as only its lowest byte is decoded below
#if NET7_0_OR_GREATER
		ref var units = ref Unsafe.As<char, ushort>(ref MemoryMarshal.GetReference(input));
		var nonAscii = (
			Vector128.LoadUnsafe(ref units)
			| Vector128.LoadUnsafe(ref units, 8)
			| Vector128.LoadUnsafe(ref units, 10)
			| Vector128.LoadUnsafe(ref units, 18)
		) & Vector128.Create((ushort)0xFF80);

		if (nonAscii != Vector128<ushort>.Zero)
#else
		// The 26 chars (52 bytes) are checked as 6 ulongs and 1 uint, independent of the endianness
		ref var bytes = ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(input));
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

		ref var src = ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(input));
		ref var tableRef = ref _inverseBase32[0];

		// The 128-bit value is built as 2 big-endian 64-bit blocks (a | b and c | d), then the endianness is reversed.
		// Every character is decoded and accumulated right away, which keeps the register pressure low,
		// and 2 accumulators per block keep the dependency chains short.
		ulong a, b, c, d, invalidA, invalidB, value;

		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 0));
		invalidA = value << 2; // The first character must be 0..7, otherwise the value overflows 128 bits
		a = value << 61;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 1));
		invalidA |= value;
		a |= value << 56;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 2));
		invalidA |= value;
		a |= value << 51;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 3));
		invalidA |= value;
		a |= value << 46;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 4));
		invalidA |= value;
		a |= value << 41;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 5));
		invalidA |= value;
		a |= value << 36;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 6));
		invalidA |= value;
		a |= value << 31;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 7));
		invalidB = value;
		b = value << 26;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 8));
		invalidB |= value;
		b |= value << 21;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 9));
		invalidB |= value;
		b |= value << 16;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 10));
		invalidB |= value;
		b |= value << 11;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 11));
		invalidB |= value;
		b |= value << 6;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 12));
		invalidB |= value;
		b |= value << 1;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 13));
		invalidB |= value;
		b |= value >> 4;
		c = value << 60;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 14));
		invalidA |= value;
		c |= value << 55;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 15));
		invalidA |= value;
		c |= value << 50;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 16));
		invalidA |= value;
		c |= value << 45;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 17));
		invalidA |= value;
		c |= value << 40;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 18));
		invalidA |= value;
		c |= value << 35;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 19));
		invalidA |= value;
		c |= value << 30;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 20));
		invalidB |= value;
		d = value << 25;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 21));
		invalidB |= value;
		d |= value << 20;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 22));
		invalidB |= value;
		d |= value << 15;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 23));
		invalidB |= value;
		d |= value << 10;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 24));
		invalidB |= value;
		d |= value << 5;
		value = Unsafe.Add(ref tableRef, ReadLowByte(ref src, 25));
		invalidB |= value;
		d |= value << 0;

		// Every table value is either 0..31 or 255 (invalid character)
		if (((invalidA | invalidB) & 0xE0) != 0)
		{
			isValid = false;
			return default;
		}

		Unsafe.SkipInit(out Ulid ulid);
		ref var ulidRef = ref Unsafe.As<Ulid, byte>(ref ulid);
		Unsafe.WriteUnaligned(ref ulidRef, ReverseOnLittleEndian(a | b));
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref ulidRef, sizeof(ulong)), ReverseOnLittleEndian(c | d));

		isValid = true;
		return ulid;
	}

	// Reads the lowest byte of the char at index, independent of the endianness
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte ReadLowByte(ref byte src, int index)
		=> (byte)Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref src, index * sizeof(char)));

	private static Ulid ParseCore(ReadOnlySpan<byte> input, out bool isValid)
	{
		if (input.Length != UlidStringLength)
		{
			isValid = false;
			return default;
		}

		// Non-ASCII bytes (0x80 and above) are mapped to an invalid value in the table
		ref var src = ref MemoryMarshal.GetReference(input);
		ref var tableRef = ref _inverseBase32[0];

		// The 128-bit value is built as 2 big-endian 64-bit blocks (a | b and c | d), then the endianness is reversed.
		// Every character is decoded and accumulated right away, which keeps the register pressure low,
		// and 2 accumulators per block keep the dependency chains short.
		ulong a, b, c, d, invalidA, invalidB, value;

		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 0));
		invalidA = value << 2; // The first character must be 0..7, otherwise the value overflows 128 bits
		a = value << 61;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 1));
		invalidA |= value;
		a |= value << 56;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 2));
		invalidA |= value;
		a |= value << 51;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 3));
		invalidA |= value;
		a |= value << 46;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 4));
		invalidA |= value;
		a |= value << 41;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 5));
		invalidA |= value;
		a |= value << 36;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 6));
		invalidA |= value;
		a |= value << 31;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 7));
		invalidB = value;
		b = value << 26;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 8));
		invalidB |= value;
		b |= value << 21;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 9));
		invalidB |= value;
		b |= value << 16;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 10));
		invalidB |= value;
		b |= value << 11;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 11));
		invalidB |= value;
		b |= value << 6;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 12));
		invalidB |= value;
		b |= value << 1;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 13));
		invalidB |= value;
		b |= value >> 4;
		c = value << 60;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 14));
		invalidA |= value;
		c |= value << 55;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 15));
		invalidA |= value;
		c |= value << 50;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 16));
		invalidA |= value;
		c |= value << 45;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 17));
		invalidA |= value;
		c |= value << 40;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 18));
		invalidA |= value;
		c |= value << 35;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 19));
		invalidA |= value;
		c |= value << 30;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 20));
		invalidB |= value;
		d = value << 25;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 21));
		invalidB |= value;
		d |= value << 20;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 22));
		invalidB |= value;
		d |= value << 15;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 23));
		invalidB |= value;
		d |= value << 10;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 24));
		invalidB |= value;
		d |= value << 5;
		value = Unsafe.Add(ref tableRef, Unsafe.Add(ref src, 25));
		invalidB |= value;
		d |= value << 0;

		// Every table value is either 0..31 or 255 (invalid character)
		if (((invalidA | invalidB) & 0xE0) != 0)
		{
			isValid = false;
			return default;
		}

		Unsafe.SkipInit(out Ulid ulid);
		ref var ulidRef = ref Unsafe.As<Ulid, byte>(ref ulid);
		Unsafe.WriteUnaligned(ref ulidRef, ReverseOnLittleEndian(a | b));
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref ulidRef, sizeof(ulong)), ReverseOnLittleEndian(c | d));

		isValid = true;
		return ulid;
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
	/// <remarks>
	/// Applies the same rules as <see cref="IsValid(string)"/>: the input must be exactly 26 characters of
	/// Crockford's Base32 alphabet, read case-insensitively, with <c>I</c> and <c>L</c> accepted as <c>1</c>, and
	/// <c>O</c> as <c>0</c>. The first character must be between <c>0</c> and <c>7</c>.
	/// </remarks>
	/// <exception cref="FormatException">Thrown if the input string is not a valid ULID string representation.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid Parse(string s, IFormatProvider? provider = null)
		=> Parse(s.AsSpan(), provider);

	/// <summary>
	/// Attempts to parse a string representation of a ULID into a <see cref="Ulid"/> instance.
	/// </summary>
	/// <param name="s">The string representation of the ULID to parse.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <param name="result">When this method returns, contains the parsed <see cref="Ulid"/> value if the parse was successful; otherwise, the default value of <see cref="Ulid"/>.</param>
	/// <returns><c>true</c> if the parsing was successful; otherwise, <c>false</c>.</returns>
	/// <remarks>
	/// Applies the same rules as <see cref="IsValid(string)"/>: the input must be exactly 26 characters of
	/// Crockford's Base32 alphabet, read case-insensitively, with <c>I</c> and <c>L</c> accepted as <c>1</c>, and
	/// <c>O</c> as <c>0</c>. The first character must be between <c>0</c> and <c>7</c>.
	/// </remarks>
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
	/// <remarks>
	/// Applies the same rules as <see cref="IsValid(string)"/>: the input must be exactly 26 characters of
	/// Crockford's Base32 alphabet, read case-insensitively, with <c>I</c> and <c>L</c> accepted as <c>1</c>, and
	/// <c>O</c> as <c>0</c>. The first character must be between <c>0</c> and <c>7</c>.
	/// </remarks>
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
	/// <remarks>
	/// Applies the same rules as <see cref="IsValid(string)"/>: the input must be exactly 26 characters of
	/// Crockford's Base32 alphabet, read case-insensitively, with <c>I</c> and <c>L</c> accepted as <c>1</c>, and
	/// <c>O</c> as <c>0</c>. The first character must be between <c>0</c> and <c>7</c>.
	/// </remarks>
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

	private void Fill<T>(Span<T> span, T[] map) where T: unmanaged
	{
		// Encode randomness
		span[25] = map[_r9 & 0x1F];                      // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111|11111|]
		span[24] = map[((_r8 & 0x3) << 3) | (_r9 >> 5)]; // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111111|11][111|11111]
		span[23] = map[(_r8 >> 2) & 0x1F];               // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][1|11111|11][11111111]
		span[22] = map[((_r7 & 0xF) << 1) | (_r8 >> 7)]; // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][1111|1111][1|1111111][11111111]
		span[21] = map[((_r6 & 0x1) << 4) | (_r7 >> 4)]; // [11111111][11111111][11111111][11111111][11111111][11111111][1111111|1][1111|1111][11111111][11111111]
		span[20] = map[(_r6 >> 1) & 0x1F];               // [11111111][11111111][11111111][11111111][11111111][11111111][11|11111|1][11111111][11111111][11111111]
		span[19] = map[((_r5 & 0x7) << 2) | (_r6 >> 6)]; // [11111111][11111111][11111111][11111111][11111111][11111|111][11|111111][11111111][11111111][11111111]
		span[18] = map[(_r5 >> 3) & 0x1F];               // [11111111][11111111][11111111][11111111][11111111][|11111|111][11111111][11111111][11111111][11111111]
		span[17] = map[_r4 & 0x1F];                      // [11111111][11111111][11111111][11111111][111|11111|][11111111][11111111][11111111][11111111][11111111]
		span[16] = map[((_r3 & 0x3) << 3) | (_r4 >> 5)]; // [11111111][11111111][11111111][11111111][111111|11][111|11111][11111111][11111111][11111111][11111111]
		span[15] = map[(_r3 >> 2) & 0x1F];               // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[14] = map[((_r2 & 0xF) << 1) | (_r3 >> 7)]; // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[13] = map[((_r1 & 0x1) << 4) | (_r2 >> 4)]; // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[12] = map[(_r1 >> 1) & 0x1F];               // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[11] = map[((_r0 & 0x7) << 2) | (_r1 >> 6)]; // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[10] = map[(_r0 >> 3) & 0x1F];               // [|11111|111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]

		// Encode timestamp
		span[9] = map[_t5 & 0x1F];                       // 00[11111111][11111111][11111111][11111111][11111111][111|11111|]
		span[8] = map[((_t4 & 0x3) << 3) | (_t5 >> 5)];  // 00[11111111][11111111][11111111][11111111][111111|11][111|11111]
		span[7] = map[(_t4 >> 2) & 0x1F];                // 00[11111111][11111111][11111111][11111111][1|11111|11][11111111]
		span[6] = map[((_t3 & 0xF) << 1) | (_t4 >> 7)];  // 00[11111111][11111111][11111111][1111|1111][1|1111111][11111111]
		span[5] = map[((_t2 & 0x1) << 4) | (_t3 >> 4)];  // 00[11111111][11111111][1111111|1][1111|1111][11111111][11111111]
		span[4] = map[(_t2 >> 1) & 0x1F];                // 00[11111111][11111111][11|11111|1][11111111][11111111][11111111]
		span[3] = map[((_t1 & 0x7) << 2) | (_t2 >> 6)];  // 00[11111111][11111|111][11|111111][11111111][11111111][11111111]
		span[2] = map[_t1 >> 3];                         // 00[11111111][|11111|111][11111111][11111111][11111111][11111111]
		span[1] = map[_t0 & 0x1F];                       // 00[111|11111|][11111111][11111111][11111111][11111111][11111111]
		span[0] = map[_t0 >> 5];                         // |00[111|11111][11111111][11111111][11111111][11111111][11111111]
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
	/// <exception cref="FormatException">Thrown if <paramref name="str"/> is not a valid ULID string representation.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Ulid(string str) => Parse(str);
}