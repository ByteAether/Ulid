using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

	/// <summary>
	/// Returns a string representation of the current instance of <see cref="Ulid"/> in its canonical Crockford's Base32 format.'
	/// </summary>
	/// <returns>Crockford's Base32 representation of the ULID</returns>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
	/// <exception cref="FormatException">Thrown if the input span does not meet the ULID format requirements.</exception>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static Ulid Parse(ReadOnlySpan<char> chars, IFormatProvider? provider = null)
		=> ParseCore(chars);

	/// <summary>
	/// Parses a ULID from a read-only span of bytes and returns the corresponding ULID value.
	/// </summary>
	/// <param name="bytes">The read-only span of bytes containing the ULID string representation in Crockford's Base32 format.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <returns>The ULID parsed from the specified byte span.</returns>
	/// <exception cref="FormatException">Thrown if the input byte span does not contain a valid ULID string representation.</exception>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static Ulid Parse(ReadOnlySpan<byte> bytes, IFormatProvider? provider = null)
		=> ParseCore(bytes);

#if NET5_0_OR_GREATER
    [SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
    private static unsafe Ulid ParseCore<T>(ReadOnlySpan<T> input)
	    where T : unmanaged
	{
		// Every T element is read as a byte. Other bits are ignored.
		// We create 2 blocks of big-endian ulong values then reverse the endianness
		// Creating a big-endian ulong and then reversing it is faster than creating directly a little-endian ulong

	    if (input.Length != UlidStringLength)
	    {
	        throw new FormatException("The input sequence is not a valid ULID string representation.");
	    }

	    var stepSize = sizeof(T); // We read the span as bytes and iterate by the size of an element
	    Ulid result = default;

	    fixed (T* pSrc = &MemoryMarshal.GetReference(input))
	    {
	        var pBytes = (byte*)pSrc;
	        ref var tableRef = ref _inverseBase32[0];
	        ref var ulidRef = ref Unsafe.As<Ulid, byte>(ref result);

	        ulong t00 = Unsafe.Add(ref tableRef, pBytes[00 * stepSize]);
	        ulong t01 = Unsafe.Add(ref tableRef, pBytes[01 * stepSize]);
	        ulong t02 = Unsafe.Add(ref tableRef, pBytes[02 * stepSize]);
	        ulong t03 = Unsafe.Add(ref tableRef, pBytes[03 * stepSize]);
	        ulong t04 = Unsafe.Add(ref tableRef, pBytes[04 * stepSize]);
	        ulong t05 = Unsafe.Add(ref tableRef, pBytes[05 * stepSize]);
	        ulong t06 = Unsafe.Add(ref tableRef, pBytes[06 * stepSize]);
	        ulong t07 = Unsafe.Add(ref tableRef, pBytes[07 * stepSize]);
	        ulong t08 = Unsafe.Add(ref tableRef, pBytes[08 * stepSize]);
	        ulong t09 = Unsafe.Add(ref tableRef, pBytes[09 * stepSize]);
	        ulong r00 = Unsafe.Add(ref tableRef, pBytes[10 * stepSize]);
	        ulong r01 = Unsafe.Add(ref tableRef, pBytes[11 * stepSize]);
	        ulong r02 = Unsafe.Add(ref tableRef, pBytes[12 * stepSize]);
	        ulong r03 = Unsafe.Add(ref tableRef, pBytes[13 * stepSize]);

	        var block1 =
		        (t00 << 61)
		        | (t01 << 56)
		        | (t02 << 51)
		        | (t03 << 46)
		        | (t04 << 41)
		        | (t05 << 36)
		        | (t06 << 31)
		        | (t07 << 26)
		        | (t08 << 21)
		        | (t09 << 16)
		        | (r00 << 11)
		        | (r01 << 6)
		        | (r02 << 1)
		        | (r03 >> 4);

	        Unsafe.WriteUnaligned(ref Unsafe.Add(ref ulidRef, 0), ReverseOnLittleEndian(block1));

	        // Second block - ulong 64 bits
	        ulong r04 = Unsafe.Add(ref tableRef, pBytes[14 * stepSize]);
	        ulong r05 = Unsafe.Add(ref tableRef, pBytes[15 * stepSize]);
	        ulong r06 = Unsafe.Add(ref tableRef, pBytes[16 * stepSize]);
	        ulong r07 = Unsafe.Add(ref tableRef, pBytes[17 * stepSize]);
	        ulong r08 = Unsafe.Add(ref tableRef, pBytes[18 * stepSize]);
	        ulong r09 = Unsafe.Add(ref tableRef, pBytes[19 * stepSize]);
	        ulong r10 = Unsafe.Add(ref tableRef, pBytes[20 * stepSize]);
	        ulong r11 = Unsafe.Add(ref tableRef, pBytes[21 * stepSize]);
	        ulong r12 = Unsafe.Add(ref tableRef, pBytes[22 * stepSize]);
	        ulong r13 = Unsafe.Add(ref tableRef, pBytes[23 * stepSize]);
	        ulong r14 = Unsafe.Add(ref tableRef, pBytes[24 * stepSize]);
	        ulong r15 = Unsafe.Add(ref tableRef, pBytes[25 * stepSize]);

	        var block2 =
		        (r03 << 60)
		        | (r04 << 55)
		        | (r05 << 50)
		        | (r06 << 45)
		        | (r07 << 40)
		        | (r08 << 35)
		        | (r09 << 30)
		        | (r10 << 25)
		        | (r11 << 20)
		        | (r12 << 15)
		        | (r13 << 10)
		        | (r14 << 5)
		        | r15;

	        Unsafe.WriteUnaligned(ref Unsafe.Add(ref ulidRef, 8), ReverseOnLittleEndian(block2));
	    }

	    return result;
	}

	/// <summary>
	/// Parses a string representation of a ULID and returns the corresponding ULID instance.
	/// </summary>
	/// <param name="s">The string representation of the ULID to parse.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <returns>A new <see cref="Ulid"/> instance parsed from the specified string.</returns>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static Ulid Parse(string s, IFormatProvider? provider = null)
		=> ParseCore(s.AsSpan());

	/// <summary>
	/// Attempts to parse a string representation of a ULID into a <see cref="Ulid"/> instance.
	/// </summary>
	/// <param name="s">The string representation of the ULID to parse.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <param name="result">When this method returns, contains the parsed <see cref="Ulid"/> value if the parse was successful; otherwise, the default value of <see cref="Ulid"/>.</param>
	/// <returns><c>true</c> if the parsing was successful; otherwise, <c>false</c>.</returns>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Ulid result)
		=> TryParse(s.AsSpan(), provider, out result);

	/// <summary>
	/// Attempts to parse a ULID from a read-only span of characters.
	/// </summary>
	/// <param name="s">The read-only span of characters to parse.</param>
	/// <param name="provider">Ignored. The ULID is always formatted in its canonical Crockford's Base32 format.</param>
	/// <param name="result">When the method returns, contains the parsed ULID if the operation succeeds, or the default value if it fails.</param>
	/// <returns><c>true</c> if the parsing operation succeeded; otherwise, <c>false</c>.</returns>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Ulid result)
	{
		try
		{
			result = ParseCore(s);
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
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static bool TryParse(ReadOnlySpan<byte> s, IFormatProvider? provider, out Ulid result)
	{
		try
		{
			result = ParseCore(s);
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
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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

#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
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
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static implicit operator string(Ulid ulid) => ulid.ToString();

	/// <summary>
	/// Allows implicit conversion of <see cref="string"/> to <see cref="Ulid"/>.
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static implicit operator Ulid(string str) => Parse(str);
}