using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
#if NETCOREAPP
using System.Text.Json.Serialization;
#endif

#if NET5_0_OR_GREATER
[module: SkipLocalsInit]
#endif

namespace ByteAether.Ulid;

/// <summary>
/// Represents a Universally Unique Lexicographically Sortable Identifier (ULID).
/// </summary>
/// <remarks>
/// A ULID is a 128-bit identifier that is sortable by time and consists of a timestamp and random components.<br/>
/// For more information, visit <see href="https://github.com/ByteAether/Ulid">the GitHub repository</see>.
/// </remarks>
#if NETCOREAPP
[JsonConverter(typeof(UlidJsonConverter))]
#endif
[TypeConverter(typeof(UlidTypeConverter))]
[StructLayout(LayoutKind.Explicit)]
public readonly partial struct Ulid
{
	private const byte _ulidSizeTime = 6;
	private const byte _ulidSizeRandom = 10;
	private const byte _ulidSize = _ulidSizeTime + _ulidSizeRandom;

	[FieldOffset(00)] private readonly byte _t0;
	[FieldOffset(01)] private readonly byte _t1;
	[FieldOffset(02)] private readonly byte _t2;
	[FieldOffset(03)] private readonly byte _t3;
	[FieldOffset(04)] private readonly byte _t4;
	[FieldOffset(05)] private readonly byte _t5;

	[FieldOffset(06)] private readonly byte _r0;
	[FieldOffset(07)] private readonly byte _r1;
	[FieldOffset(08)] private readonly byte _r2;
	[FieldOffset(09)] private readonly byte _r3;
	[FieldOffset(10)] private readonly byte _r4;
	[FieldOffset(11)] private readonly byte _r5;
	[FieldOffset(12)] private readonly byte _r6;
	[FieldOffset(13)] private readonly byte _r7;
	[FieldOffset(14)] private readonly byte _r8;
	[FieldOffset(15)] private readonly byte _r9;

	/// <summary>
	/// Gets the random component of the ULID as a byte array.
	/// </summary>
	/// <remarks>
	/// The random component consists of the last 10 bytes of the ULID and is generated randomly to ensure uniqueness.<br/>
	/// This component does not encode any timestamp or other structured information.<br/>
	/// On .NET Standard 2.0 the returned span is backed by a new copy of the bytes, which allocates (see <see cref="AsByteSpan"/>).
	/// </remarks>
	/// <returns>
	/// A byte array containing 10 random bytes that represent the random portion of the ULID.
	/// </returns>
	[IgnoreDataMember]
	public ReadOnlySpan<byte> Random
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => AsByteSpan()[_ulidSizeTime..];
	}

	/// <summary>
	/// Gets the time component of the ULID as a byte array.
	/// </summary>
	/// <remarks>
	/// The time component consists of the first 6 bytes of the ULID and is generated during ULID creation.<br/>
	/// On .NET Standard 2.0 the returned span is backed by a new copy of the bytes, which allocates (see <see cref="AsByteSpan"/>).
	/// </remarks>
	/// <returns>
	/// A byte array containing 6 time bytes that represent the time portion of the ULID.
	/// </returns>
	[IgnoreDataMember]
	public ReadOnlySpan<byte> TimeBytes
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => AsByteSpan()[.._ulidSizeTime];
	}

	/// <summary>
	/// Gets the timestamp component of the ULID as a <see cref="DateTimeOffset"/>.
	/// </summary>
	/// <remarks>
	/// The timestamp component represents the number of milliseconds since the Unix epoch
	/// (1970-01-01T00:00:00Z). It is stored in the first 6 bytes of the ULID and ensures
	/// lexicographical sorting by time.<br/>
	/// The timestamp is extracted in a way that is compatible with both little-endian and big-endian systems.
	/// </remarks>
	/// <returns>
	/// A <see cref="DateTimeOffset"/> representing the timestamp portion of the ULID.
	/// </returns>
	[IgnoreDataMember]
	public DateTimeOffset Time
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			// The first 8 bytes in big-endian order hold the 48-bit timestamp in their upper bits
			var time = ReverseOnLittleEndian(Unsafe.As<Ulid, ulong>(ref Unsafe.AsRef(in this))) >> 16;

			return DateTimeOffset.FromUnixTimeMilliseconds((long)time);
		}
	}

#if NETSTANDARD2_0
	/// <summary>
	/// Creates a read-only span of bytes representing the current instance of the <see cref="Ulid"/> struct.
	/// </summary>
	/// <remarks>
	/// On .NET Standard 2.0 there is no GC-safe way to create a span over the instance itself.
	/// The returned span is backed by a new copy of the bytes (see <see cref="ToByteArray"/>), which allocates.
	/// </remarks>
	/// <returns>
	/// A <see cref="ReadOnlySpan{T}"/> over a copy of the raw byte representation of the current <see cref="Ulid"/> struct.
	/// </returns>
#else
	/// <summary>
	/// Creates a read-only span of bytes representing the current instance of the <see cref="Ulid"/> struct.
	/// </summary>
	/// <returns>
	/// A <see cref="ReadOnlySpan{T}"/> that points to the raw byte representation of the current <see cref="Ulid"/> struct.
	/// </returns>
#endif
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ReadOnlySpan<byte> AsByteSpan()
#if NETSTANDARD2_0
		=> ToByteArray();
#else
		=> MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<Ulid, byte>(ref Unsafe.AsRef(in this)), _ulidSize);
#endif

	/// <summary>
	/// Converts the ULID to a byte array.
	/// </summary>
	/// <returns>A byte array representing the ULID.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte[] ToByteArray()
	{
		var bytes = new byte[_ulidSize];
		Unsafe.WriteUnaligned(ref bytes[0], this);
		return bytes;
	}
}