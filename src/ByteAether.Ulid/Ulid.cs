using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
#if NETCOREAPP3_0_OR_GREATER
using System.Text.Json.Serialization;
using System.Runtime.Intrinsics.X86;
#endif
#if NET6_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif

namespace ByteAether.Ulid;

#if NET8_0_OR_GREATER
// We need to target netstandard2.1, so keep using ref for MemoryMarshal.Write
// CS9191: The 'ref' modifier for argument 2 corresponding to 'in' parameter is equivalent to 'in'. Consider using 'in' instead.
#pragma warning disable CS9191
#endif

/// <summary>
/// Represents a Universally Unique Lexicographically Sortable Identifier (ULID).
/// </summary>
/// <remarks>
/// For detailed documentation, see the <see cref="IComparable"/>, <see cref="IComparable{T}"/>, <see cref="IEquatable{T}"/> interfaces.
/// </remarks>
[DebuggerDisplay("{ToString(),nq}")]
[StructLayout(LayoutKind.Sequential, Size = 16)]
[TypeConverter(typeof(UlidConverter))]
#if NETCOREAPP3_0_OR_GREATER
[JsonConverter(typeof(UlidJsonConverter))]
#endif
public struct Ulid : IComparable, IComparable<Ulid>, IEquatable<Ulid>
#if NET6_0_OR_GREATER
, ISpanFormattable
#endif
#if NET7_0_OR_GREATER
, ISpanParsable<Ulid>
#endif
#if NET8_0_OR_GREATER
, IUtf8SpanFormattable
#endif
{
	#region Constants
	private const byte _ulidByteLength = 16;
	private const byte _ulidStringLength = 26;
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
	#endregion

	private static readonly byte[] _lastUlid = new byte[_ulidByteLength];

	private unsafe fixed byte _bytes[_ulidByteLength];

	#region New

	/// <summary>
	/// Initializes a new instance of the <see cref="Ulid"/> struct using the specified byte array.
	/// </summary>
	/// <param name="bytes">The byte array to initialize the ULID with.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(ReadOnlySpan<byte> bytes) => MemoryMarshal.Read<Ulid>(bytes);


	/// <summary>
	/// Creates a new ULID using the specified GUID.
	/// </summary>
	/// <param name="guid">The GUID to initialize the ULID with.</param>
	// HACK: We assume the layout of a Guid is the following:
	// Int32, Int16, Int16, Int8, Int8, Int8, Int8, Int8, Int8, Int8, Int8
	// source: https://github.com/dotnet/runtime/blob/5c4686f831d34c2c127e943d0f0d144793eeb0ad/src/libraries/System.Private.CoreLib/src/System/Guid.cs
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	public static Ulid New(Guid guid)
	{
#if NET6_0_OR_GREATER
		if (_isVector128Supported && BitConverter.IsLittleEndian)
		{
			var vector = Unsafe.As<Guid, Vector128<byte>>(ref guid);
			var shuffled = Shuffle(vector, Vector128.Create((byte)3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15));

			return Unsafe.As<Vector128<byte>, Ulid>(ref shuffled);
		}
#endif
		Span<byte> ulidBytes = stackalloc byte[_ulidByteLength];
		if (BitConverter.IsLittleEndian)
		{
			// |A|B|C|D|E|F|G|H|I|J|K|L|M|N|O|P|
			// |D|C|B|A|...
			//      ...|F|E|H|G|...
			//              ...|I|J|K|L|M|N|O|P|
			ref var ptr = ref Unsafe.As<Guid, uint>(ref guid);
			var lower = BinaryPrimitives.ReverseEndianness(ptr);
			MemoryMarshal.Write(ulidBytes, ref lower);

			ptr = ref Unsafe.Add(ref ptr, 1);
			var upper = ((ptr & 0x00_FF_00_FF) << 8) | ((ptr & 0xFF_00_FF_00) >> 8);
			MemoryMarshal.Write(ulidBytes[4..], ref upper);

			ref var upperBytes = ref Unsafe.As<uint, ulong>(ref Unsafe.Add(ref ptr, 1));
			MemoryMarshal.Write(ulidBytes[8..], ref upperBytes);
		}
		else
		{
			MemoryMarshal.Write(ulidBytes, ref guid);
		}

		return MemoryMarshal.Read<Ulid>(ulidBytes);
	}

	/// <summary>
	/// Creates a new ULID with the current timestamp.
	/// </summary>
	/// <param name="isMonotonic">If true, ensures the ULID is monotonically increasing.</param>
	/// <returns>A new ULID instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(bool isMonotonic = true)
		=> New(DateTimeOffset.UtcNow, isMonotonic);

	/// <summary>
	/// Creates a new ULID with the specified timestamp.
	/// </summary>
	/// <param name="dateTimeOffset">The timestamp to use for the ULID.</param>
	/// <param name="isMonotonic">If true, ensures the ULID is monotonically increasing.</param>
	/// <returns>A new ULID instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(DateTimeOffset dateTimeOffset, bool isMonotonic = true)
		=> New(dateTimeOffset.ToUnixTimeMilliseconds(), isMonotonic);

	/// <summary>
	/// Creates a new ULID with the specified timestamp in milliseconds.
	/// </summary>
	/// <param name="timestamp">The timestamp in milliseconds to use for the ULID.</param>
	/// <param name="isMonotonic">If true, ensures the ULID is monotonically increasing.</param>
	/// <returns>A new ULID instance.</returns>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(long timestamp, bool isMonotonic = true)
	{
		Span<byte> timestampBytes = stackalloc byte[8];
		BinaryPrimitives.WriteInt64BigEndian(timestampBytes, timestamp);

		return Create(timestampBytes, isMonotonic);
	}

	/// <summary>
	/// Creates a copy of the existing ULID.
	/// </summary>
	/// <returns>A new instance of <see cref="Ulid"/> that is a copy of the current instance.</returns>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Ulid Copy()
	{
		Span<byte> bytes = stackalloc byte[_ulidByteLength];

		Unsafe.WriteUnaligned(ref bytes[0], this);

		return New(bytes);
	}

#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	private static Ulid Create(Span<byte> timestampBytes, bool isMonotonic = true)
	{
		// We need a new copy to be returned, as our private _lastUlid changes over time
		Span<byte> bytes = stackalloc byte[_ulidByteLength];

		if (!isMonotonic)
		{
			timestampBytes[2..].CopyTo(bytes);
			RandomNumberGenerator.Fill(bytes[6..]);
			return New(bytes);
		}

		lock (_lastUlid)
		{
			var lastUlidSpan = _lastUlid.AsSpan();

			// If the timestamp is the same or lesser than the last one, increment the last ULID by one
			if (timestampBytes[2..].SequenceCompareTo(lastUlidSpan[..6]) <= 0)
			{
				for (byte i = _ulidByteLength - 1; i >= 0; --i)
				{
					if (++_lastUlid[i] != 0)
					{
						break;
					}
				}
			}
			// Otherwise, generate a new ULID
			else
			{
				timestampBytes[2..].CopyTo(lastUlidSpan);
				RandomNumberGenerator.Fill(lastUlidSpan[6..]);
			}

			_lastUlid.CopyTo(bytes);

			return New(bytes);
		}
	}
	#endregion

	#region To*
	/// <summary>
	/// Converts the ULID to a byte array.
	/// </summary>
	/// <returns>A byte array representing the ULID.</returns>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	public readonly byte[] ToByteArray()
	{
		var bytes = new byte[_ulidByteLength];

		Unsafe.WriteUnaligned(ref bytes[0], this);

		return bytes;
	}

	/// <summary>
	/// Converts the ULID to a GUID.
	/// </summary>
	/// <returns>A GUID representing the ULID.</returns>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	public readonly Guid ToGuid()
	{
#if NET6_0_OR_GREATER
		if (_isVector128Supported && BitConverter.IsLittleEndian)
		{
			var vector = Unsafe.As<Ulid, Vector128<byte>>(ref Unsafe.AsRef(in this));
			var shuffled = Shuffle(vector, Vector128.Create((byte)3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15));

			return Unsafe.As<Vector128<byte>, Guid>(ref shuffled);
		}
#endif
		Span<byte> guidBytes = stackalloc byte[_ulidByteLength];
		if (BitConverter.IsLittleEndian)
		{
			// |A|B|C|D|E|F|G|H|I|J|K|L|M|N|O|P|
			// |D|C|B|A|...
			//      ...|F|E|H|G|...
			//              ...|I|J|K|L|M|N|O|P|
			ref var ptr = ref Unsafe.As<Ulid, uint>(ref Unsafe.AsRef(in this));
			var lower = BinaryPrimitives.ReverseEndianness(ptr);
			MemoryMarshal.Write(guidBytes, ref lower);

			ptr = ref Unsafe.Add(ref ptr, 1);
			var upper = ((ptr & 0x00_FF_00_FF) << 8) | ((ptr & 0xFF_00_FF_00) >> 8);
			MemoryMarshal.Write(guidBytes[4..], ref upper);

			ref var upperBytes = ref Unsafe.As<uint, ulong>(ref Unsafe.Add(ref ptr, 1));
			MemoryMarshal.Write(guidBytes[8..], ref upperBytes);
		}
		else
		{
			MemoryMarshal.Write(guidBytes, ref Unsafe.AsRef(in this));
		}

		return MemoryMarshal.Read<Guid>(guidBytes);
	}

	/// <inheritdoc/>
	public readonly string ToString(string? format = null, IFormatProvider? formatProvider = null)
	{
#if NETCOREAPP2_1_OR_GREATER
		return string.Create(_ulidStringLength, this, (span, ulid) => ulid.WriteChars(span));
#else
		Span<char> span = stackalloc char[_ulidStringLength];
		WriteChars(span);
		unsafe
		{
			return new string((char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), 0, _ulidStringLength);
		}
#endif
	}

	/// <summary>
	/// Explicitly converts a ULID to a GUID.
	/// </summary>
	/// <param name="ulid">The ULID to convert.</param>
	/// <returns>A GUID representing the ULID.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator Guid(Ulid ulid) => ulid.ToGuid();

	/// <summary>
	/// Explicitly converts a GUID to a ULID.
	/// </summary>
	/// <param name="guid">The GUID to convert.</param>
	/// <returns>A ULID representing the GUID.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator Ulid(Guid guid) => New(guid);
	#endregion

	#region Parse
	/// <inheritdoc/>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	public static Ulid Parse(ReadOnlySpan<char> chars, IFormatProvider? provider = null)
	{
		// Sanity check.
		if (chars.Length != _ulidStringLength)
		{
			throw new FormatException();
		}

		// Decode.
		Span<byte> data = stackalloc byte[_ulidByteLength];

		data[15] = (byte)((_inverseBase32[(uint)chars[24]] << 5) | _inverseBase32[(uint)chars[25]]);

		data[00] = (byte)((_inverseBase32[(uint)chars[0]] << 5) | _inverseBase32[(uint)chars[1]]);                                  // |00[111|11111|][11111111][11111111][11111111][11111111][11111111]
		data[01] = (byte)((_inverseBase32[(uint)chars[2]] << 3) | (_inverseBase32[(uint)chars[3]] >> 2));                               // 00[11111111][|11111|111][11|111111][11111111][11111111][11111111]
		data[02] = (byte)((_inverseBase32[(uint)chars[3]] << 6) | (_inverseBase32[(uint)chars[4]] << 1) | (_inverseBase32[(uint)chars[5]] >> 4));    // 00[11111111][11111|111][11|11111|1][1111|1111][11111111][11111111]
		data[03] = (byte)((_inverseBase32[(uint)chars[5]] << 4) | (_inverseBase32[(uint)chars[6]] >> 1));                               // 00[11111111][11111111][1111111|1][1111|1111][1|1111111][11111111]
		data[04] = (byte)((_inverseBase32[(uint)chars[6]] << 7) | (_inverseBase32[(uint)chars[7]] << 2) | (_inverseBase32[(uint)chars[8]] >> 3));    // 00[11111111][11111111][11111111][1111|1111][1|11111|11][111|11111]
		data[05] = (byte)((_inverseBase32[(uint)chars[8]] << 5) | _inverseBase32[(uint)chars[9]]);                                  // 00[11111111][11111111][11111111][11111111][111111|11][111|11111|]
		data[06] = (byte)((_inverseBase32[(uint)chars[10]] << 3) | (_inverseBase32[(uint)chars[11]] >> 2));                         // [|11111|111][11|111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		data[07] = (byte)((_inverseBase32[(uint)chars[11]] << 6) | (_inverseBase32[(uint)chars[12]] << 1) | (_inverseBase32[(uint)chars[13]] >> 4)); // [11111|111][11|11111|1][1111|1111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		data[08] = (byte)((_inverseBase32[(uint)chars[13]] << 4) | (_inverseBase32[(uint)chars[14]] >> 1));                         // [11111111][1111111|1][1111|1111][1|1111111][11111111][11111111][11111111][11111111][11111111][11111111]
		data[09] = (byte)((_inverseBase32[(uint)chars[14]] << 7) | (_inverseBase32[(uint)chars[15]] << 2) | (_inverseBase32[(uint)chars[16]] >> 3)); // [11111111][11111111][1111|1111][1|11111|11][111|11111][11111111][11111111][11111111][11111111][11111111]
		data[10] = (byte)((_inverseBase32[(uint)chars[16]] << 5) | _inverseBase32[(uint)chars[17]]);                                    // [11111111][11111111][11111111][111111|11][111|11111|][11111111][11111111][11111111][11111111][11111111]
		data[11] = (byte)((_inverseBase32[(uint)chars[18]] << 3) | (_inverseBase32[(uint)chars[19]] >> 2));                         // [11111111][11111111][11111111][11111111][11111111][|11111|111][11|111111][11111111][11111111][11111111]
		data[12] = (byte)((_inverseBase32[(uint)chars[19]] << 6) | (_inverseBase32[(uint)chars[20]] << 1) | (_inverseBase32[(uint)chars[21]] >> 4)); // [11111111][11111111][11111111][11111111][11111111][11111|111][11|11111|1][1111|1111][11111111][11111111]
		data[13] = (byte)((_inverseBase32[(uint)chars[21]] << 4) | (_inverseBase32[(uint)chars[22]] >> 1));                         // [11111111][11111111][11111111][11111111][11111111][11111111][1111111|1][1111|1111][1|1111111][11111111]
		data[14] = (byte)((_inverseBase32[(uint)chars[22]] << 7) | (_inverseBase32[(uint)chars[23]] << 2) | (_inverseBase32[(uint)chars[24]] >> 3)); // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][1111|1111][1|11111|11][111|11111]

		return MemoryMarshal.Read<Ulid>(data);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid Parse(string s, IFormatProvider? provider = null)
		=> Parse(s.AsSpan());

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Ulid result)
	{
		try
		{
			result = Parse(s);
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Ulid result)
		=> TryParse(s.AsSpan(), provider, out result);

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
	{
		if (WriteChars(destination))
		{
			charsWritten = _ulidStringLength;
			return true;
		}

		charsWritten = 0;
		return false;
	}

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<byte> destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
	{
		if (WriteBytes(destination))
		{
			bytesWritten = _ulidStringLength;
			return true;
		}

		bytesWritten = 0;
		return false;
	}
	#endregion

	#region Comparisons
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool Equals(Ulid other)
	{
#if NET7_0_OR_GREATER
		if (Vector128.IsHardwareAccelerated)
		{
			var vA = Unsafe.As<Ulid, Vector128<byte>>(ref Unsafe.AsRef(in this));
			var vB = Unsafe.As<Ulid, Vector128<byte>>(ref Unsafe.AsRef(in other));
			return Vector128.EqualsAll(vA, vB);
		}
#endif
#if NET6_0_OR_GREATER
		if (Sse2.IsSupported)
		{
			var vA = Unsafe.As<Ulid, Vector128<byte>>(ref Unsafe.AsRef(in this));
			var vB = Unsafe.As<Ulid, Vector128<byte>>(ref Unsafe.AsRef(in other));
			return Sse2.MoveMask(Sse2.CompareEqual(vA, vB)) == 0xFFFF;
		}
#endif

		ref var rA = ref Unsafe.As<Ulid, long>(ref Unsafe.AsRef(in this));
		ref var rB = ref Unsafe.As<Ulid, long>(ref Unsafe.AsRef(in other));

		// Compare 2x 64bit long
		return rA == rB && Unsafe.Add(ref rA, 1) == Unsafe.Add(ref rB, 1);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Ulid ulid && Equals(ulid);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly int CompareTo(object? obj)
	{
		if (obj == null)
		{
			return 1;
		}
		else if (obj.GetType() != GetType())
		{
			throw new ArgumentException($"The value is not an instance of {GetType()}.", nameof(obj));
		}

		return CompareTo((Ulid)obj);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly unsafe int CompareTo(Ulid other)
	{
		fixed (byte* ptr = _bytes)
		{
			return
				_bytes[0] != other._bytes[0] ? GetResult(_bytes[0], other._bytes[0])
				: _bytes[1] != other._bytes[1] ? GetResult(_bytes[1], other._bytes[1])
				: _bytes[2] != other._bytes[2] ? GetResult(_bytes[2], other._bytes[2])
				: _bytes[3] != other._bytes[3] ? GetResult(_bytes[3], other._bytes[3])
				: _bytes[4] != other._bytes[4] ? GetResult(_bytes[4], other._bytes[4])
				: _bytes[5] != other._bytes[5] ? GetResult(_bytes[5], other._bytes[5])
				: _bytes[6] != other._bytes[6] ? GetResult(_bytes[6], other._bytes[6])
				: _bytes[7] != other._bytes[7] ? GetResult(_bytes[7], other._bytes[7])
				: _bytes[8] != other._bytes[8] ? GetResult(_bytes[8], other._bytes[8])
				: _bytes[9] != other._bytes[9] ? GetResult(_bytes[9], other._bytes[9])
				: _bytes[10] != other._bytes[10] ? GetResult(_bytes[10], other._bytes[10])
				: _bytes[11] != other._bytes[11] ? GetResult(_bytes[11], other._bytes[11])
				: _bytes[12] != other._bytes[12] ? GetResult(_bytes[12], other._bytes[12])
				: _bytes[13] != other._bytes[13] ? GetResult(_bytes[13], other._bytes[13])
				: _bytes[14] != other._bytes[14] ? GetResult(_bytes[14], other._bytes[14])
				: _bytes[15] != other._bytes[15] ? GetResult(_bytes[15], other._bytes[15])
				: 0;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetResult(byte left, byte right) => left < right ? -1 : 1;

	/// <summary>
	/// Determines whether two specified ULIDs have the same value.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is equal to the value of the right ULID; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Ulid left, Ulid right) => left.Equals(right);

	/// <summary>
	/// Determines whether two specified ULIDs have different values.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is not equal to the value of the right ULID; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Ulid left, Ulid right) => !(left == right);

	/// <summary>
	/// Determines whether the value of the left ULID is less than the value of the right ULID.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is less than the value of the right ULID; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <(Ulid left, Ulid right) => left.CompareTo(right) < 0;

	/// <summary>
	/// Determines whether the value of the left ULID is less than or equal to the value of the right ULID.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is less than or equal to the value of the right ULID; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <=(Ulid left, Ulid right) => left.CompareTo(right) <= 0;

	/// <summary>
	/// Determines whether the value of the left ULID is greater than the value of the right ULID.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is greater than the value of the right ULID; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >(Ulid left, Ulid right) => left.CompareTo(right) > 0;

	/// <summary>
	/// Determines whether the value of the left ULID is greater than or equal to the value of the right ULID.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is greater than or equal to the value of the right ULID; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >=(Ulid left, Ulid right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override readonly int GetHashCode()
	{
		// 128bit = 4x 32bit int
		ref var rA = ref Unsafe.As<Ulid, int>(ref Unsafe.AsRef(in this));
		return rA ^ Unsafe.Add(ref rA, 1) ^ Unsafe.Add(ref rA, 2) ^ Unsafe.Add(ref rA, 3);
	}
	#endregion

	#region Internals
	private readonly unsafe bool WriteChars(Span<char> span)
	{
		if (span.Length < _ulidStringLength)
		{
			return false;
		}

		// Eliminate bounds-check of span
		span[25] = _base32Chars[_bytes[15] & 0x1F];                              // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111|11111|]

		// Encode timestamp
		span[0] = _base32Chars[_bytes[0] >> 5];                                  // |00[111|11111][11111111][11111111][11111111][11111111][11111111]
		span[1] = _base32Chars[_bytes[0] & 0x1F];                                // 00[111|11111|][11111111][11111111][11111111][11111111][11111111]
		span[2] = _base32Chars[_bytes[1] >> 3];                                  // 00[11111111][|11111|111][11111111][11111111][11111111][11111111]
		span[3] = _base32Chars[((_bytes[1] & 0x7) << 2) | (_bytes[2] >> 6)];     // 00[11111111][11111|111][11|111111][11111111][11111111][11111111]
		span[4] = _base32Chars[(_bytes[2] >> 1) & 0x1F];                         // 00[11111111][11111111][11|11111|1][11111111][11111111][11111111]
		span[5] = _base32Chars[((_bytes[2] & 0x1) << 4) | (_bytes[3] >> 4)];     // 00[11111111][11111111][1111111|1][1111|1111][11111111][11111111]
		span[6] = _base32Chars[((_bytes[3] & 0xF) << 1) | (_bytes[4] >> 7)];     // 00[11111111][11111111][11111111][1111|1111][1|1111111][11111111]
		span[7] = _base32Chars[(_bytes[4] >> 2) & 0x1F];                         // 00[11111111][11111111][11111111][11111111][1|11111|11][11111111]
		span[8] = _base32Chars[((_bytes[4] & 0x3) << 3) | (_bytes[5] >> 5)];     // 00[11111111][11111111][11111111][11111111][111111|11][111|11111]
		span[9] = _base32Chars[_bytes[5] & 0x1F];                                // 00[11111111][11111111][11111111][11111111][11111111][111|11111|]

		// Encode randomness
		span[10] = _base32Chars[(_bytes[6] >> 3) & 0x1F];                        // [|11111|111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[11] = _base32Chars[((_bytes[6] & 0x7) << 2) | (_bytes[7] >> 6)];    // [11111|111][11|111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[12] = _base32Chars[(_bytes[7] >> 1) & 0x1F];                        // [11111111][11|11111|1][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[13] = _base32Chars[((_bytes[7] & 0x1) << 4) | (_bytes[8] >> 4)];    // [11111111][1111111|1][1111|1111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[14] = _base32Chars[((_bytes[8] & 0xF) << 1) | (_bytes[9] >> 7)];    // [11111111][11111111][1111|1111][1|1111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[15] = _base32Chars[(_bytes[9] >> 2) & 0x1F];                        // [11111111][11111111][11111111][1|11111|11][11111111][11111111][11111111][11111111][11111111][11111111]
		span[16] = _base32Chars[((_bytes[9] & 0x3) << 3) | (_bytes[10] >> 5)];   // [11111111][11111111][11111111][111111|11][111|11111][11111111][11111111][11111111][11111111][11111111]
		span[17] = _base32Chars[_bytes[10] & 0x1F];                              // [11111111][11111111][11111111][11111111][111|11111|][11111111][11111111][11111111][11111111][11111111]
		span[18] = _base32Chars[(_bytes[11] >> 3) & 0x1F];                       // [11111111][11111111][11111111][11111111][11111111][|11111|111][11111111][11111111][11111111][11111111]
		span[19] = _base32Chars[((_bytes[11] & 0x7) << 2) | (_bytes[12] >> 6)];  // [11111111][11111111][11111111][11111111][11111111][11111|111][11|111111][11111111][11111111][11111111]
		span[20] = _base32Chars[(_bytes[12] >> 1) & 0x1F];                       // [11111111][11111111][11111111][11111111][11111111][11111111][11|11111|1][11111111][11111111][11111111]
		span[21] = _base32Chars[((_bytes[12] & 0x1) << 4) | (_bytes[13] >> 4)];  // [11111111][11111111][11111111][11111111][11111111][11111111][1111111|1][1111|1111][11111111][11111111]
		span[22] = _base32Chars[((_bytes[13] & 0xF) << 1) | (_bytes[14] >> 7)];  // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][1111|1111][1|1111111][11111111]
		span[23] = _base32Chars[(_bytes[14] >> 2) & 0x1F];                       // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][1|11111|11][11111111]
		span[24] = _base32Chars[((_bytes[14] & 0x3) << 3) | (_bytes[15] >> 5)];  // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111111|11][111|11111]

		return true;
	}

	private readonly unsafe bool WriteBytes(Span<byte> span)
	{
		if (span.Length < _ulidStringLength)
		{
			return false;
		}

		// Eliminate bounds-check of span
		span[25] = _base32Bytes[_bytes[15] & 0x1F];                              // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111|11111|]

		// Encode timestamp
		span[0] = _base32Bytes[_bytes[0] >> 5];                                  // |00[111|11111][11111111][11111111][11111111][11111111][11111111]
		span[1] = _base32Bytes[_bytes[0] & 0x1F];                                // 00[111|11111|][11111111][11111111][11111111][11111111][11111111]
		span[2] = _base32Bytes[_bytes[1] >> 3];                                  // 00[11111111][|11111|111][11111111][11111111][11111111][11111111]
		span[3] = _base32Bytes[((_bytes[1] & 0x7) << 2) | (_bytes[2] >> 6)];     // 00[11111111][11111|111][11|111111][11111111][11111111][11111111]
		span[4] = _base32Bytes[(_bytes[2] >> 1) & 0x1F];                         // 00[11111111][11111111][11|11111|1][11111111][11111111][11111111]
		span[5] = _base32Bytes[((_bytes[2] & 0x1) << 4) | (_bytes[3] >> 4)];     // 00[11111111][11111111][1111111|1][1111|1111][11111111][11111111]
		span[6] = _base32Bytes[((_bytes[3] & 0xF) << 1) | (_bytes[4] >> 7)];     // 00[11111111][11111111][11111111][1111|1111][1|1111111][11111111]
		span[7] = _base32Bytes[(_bytes[4] >> 2) & 0x1F];                         // 00[11111111][11111111][11111111][11111111][1|11111|11][11111111]
		span[8] = _base32Bytes[((_bytes[4] & 0x3) << 3) | (_bytes[5] >> 5)];     // 00[11111111][11111111][11111111][11111111][111111|11][111|11111]
		span[9] = _base32Bytes[_bytes[5] & 0x1F];                                // 00[11111111][11111111][11111111][11111111][11111111][111|11111|]

		// Encode randomness
		span[10] = _base32Bytes[(_bytes[6] >> 3) & 0x1F];                        // [|11111|111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[11] = _base32Bytes[((_bytes[6] & 0x7) << 2) | (_bytes[7] >> 6)];    // [11111|111][11|111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[12] = _base32Bytes[(_bytes[7] >> 1) & 0x1F];                        // [11111111][11|11111|1][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[13] = _base32Bytes[((_bytes[7] & 0x1) << 4) | (_bytes[8] >> 4)];    // [11111111][1111111|1][1111|1111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[14] = _base32Bytes[((_bytes[8] & 0xF) << 1) | (_bytes[9] >> 7)];    // [11111111][11111111][1111|1111][1|1111111][11111111][11111111][11111111][11111111][11111111][11111111]
		span[15] = _base32Bytes[(_bytes[9] >> 2) & 0x1F];                        // [11111111][11111111][11111111][1|11111|11][11111111][11111111][11111111][11111111][11111111][11111111]
		span[16] = _base32Bytes[((_bytes[9] & 0x3) << 3) | (_bytes[10] >> 5)];   // [11111111][11111111][11111111][111111|11][111|11111][11111111][11111111][11111111][11111111][11111111]
		span[17] = _base32Bytes[_bytes[10] & 0x1F];                              // [11111111][11111111][11111111][11111111][111|11111|][11111111][11111111][11111111][11111111][11111111]
		span[18] = _base32Bytes[(_bytes[11] >> 3) & 0x1F];                       // [11111111][11111111][11111111][11111111][11111111][|11111|111][11111111][11111111][11111111][11111111]
		span[19] = _base32Bytes[((_bytes[11] & 0x7) << 2) | (_bytes[12] >> 6)];  // [11111111][11111111][11111111][11111111][11111111][11111|111][11|111111][11111111][11111111][11111111]
		span[20] = _base32Bytes[(_bytes[12] >> 1) & 0x1F];                       // [11111111][11111111][11111111][11111111][11111111][11111111][11|11111|1][11111111][11111111][11111111]
		span[21] = _base32Bytes[((_bytes[12] & 0x1) << 4) | (_bytes[13] >> 4)];  // [11111111][11111111][11111111][11111111][11111111][11111111][1111111|1][1111|1111][11111111][11111111]
		span[22] = _base32Bytes[((_bytes[13] & 0xF) << 1) | (_bytes[14] >> 7)];  // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][1111|1111][1|1111111][11111111]
		span[23] = _base32Bytes[(_bytes[14] >> 2) & 0x1F];                       // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][1|11111|11][11111111]
		span[24] = _base32Bytes[((_bytes[14] & 0x3) << 3) | (_bytes[15] >> 5)];  // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111111|11][111|11111]

		return true;
	}

#if NET6_0_OR_GREATER
	private static bool _isVector128Supported =>
#if NET7_0_OR_GREATER
			Vector128.IsHardwareAccelerated;
#else
			Sse3.IsSupported;
#endif

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vector128<byte> Shuffle(Vector128<byte> value, Vector128<byte> mask)
	{
		Debug.Assert(BitConverter.IsLittleEndian);
		Debug.Assert(_isVector128Supported);

		return
#if NET7_0_OR_GREATER
			Vector128.IsHardwareAccelerated ? Vector128.Shuffle(value, mask) :
#endif
			Ssse3.IsSupported ? Ssse3.Shuffle(value, mask) :
			throw new NotImplementedException();
	}
#endif

	#endregion
}