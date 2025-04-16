using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
#if NETSTANDARD2_0
using System.Buffers;
#endif

namespace ByteAether.Ulid;

public readonly partial struct Ulid
{
	/// <summary>
	/// Whether <see cref="Ulid"/>s should be generated in a monotonic manner by default.<br />
	/// Initial value is set to <c>true</c>.<br/>
	/// <b>This setting applies globally without any scoping.</b>
	/// </summary>
	/// <remarks>
	/// When set to <c>true</c> (default), <see cref="Ulid"/>s generated without explicitly specifying monotonicity
	/// will ensure that they are monotonically increasing.<br />
	/// When set to <c>false</c>, <see cref="Ulid"/>s generated without explicitly specifying monotonicity will be
	/// generated with random <see cref="Random" /> value.
	/// </remarks>
	public static bool DefaultIsMonotonic { get; set; } = true;

	private static readonly byte[] _lastUlid = new byte[_ulidSize];
	private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

#if NET9_0_OR_GREATER
	private static readonly Lock _lock = new();
#else
	private static readonly object _lock = new();
#endif

	/// <summary>
	/// Initializes a new instance of the <see cref="Ulid"/> struct using the specified byte array.
	/// </summary>
	/// <param name="bytes">The byte array to initialize the <see cref="Ulid"/> with.</param>
	/// <returns>Given bytes as an <see cref="Ulid"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(ReadOnlySpan<byte> bytes)
		=> MemoryMarshal.Read<Ulid>(bytes);

	/// <summary>
	/// Creates a new <see cref="Ulid"/> with the current timestamp.
	/// </summary>
	/// <param name="isMonotonic">
	/// If <c>null</c> (default), the value of <see cref="DefaultIsMonotonic"/> is used to determine monotonicity.<br />
	/// If <c>true</c>, ensures the <see cref="Ulid"/> is monotonically increasing.<br />
	/// If <c>false</c>, generates a random <see cref="Random" /> part in <see cref="Ulid"/>.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(bool? isMonotonic = null)
		=> New(DateTimeOffset.UtcNow, isMonotonic);

	/// <summary>
	/// Creates a new <see cref="Ulid"/> with the specified timestamp.
	/// </summary>
	/// <param name="dateTimeOffset">The timestamp to use for the <see cref="Ulid"/>.</param>
	/// <param name="isMonotonic">
	/// If <c>null</c> (default), the value of <see cref="DefaultIsMonotonic"/> is used to determine monotonicity.<br />
	/// If <c>true</c>, ensures the <see cref="Ulid"/> is monotonically increasing.<br />
	/// If <c>false</c>, generates a random <see cref="Random" /> part in <see cref="Ulid"/>.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(DateTimeOffset dateTimeOffset, bool? isMonotonic = null)
		=> New(dateTimeOffset.ToUnixTimeMilliseconds(), isMonotonic);

	/// <summary>
	/// Creates a new <see cref="Ulid"/> with the specified timestamp.
	/// </summary>
	/// <param name="dateTimeOffset">The timestamp to use for the <see cref="Ulid"/>.</param>
	/// <param name="random" >
	/// A span containing the random component of the <see cref="Ulid"/>. 
	/// It must be at least 10 bytes long, as the last 10 bytes of the <see cref="Ulid"/> are derived from this span.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(DateTimeOffset dateTimeOffset, Span<byte> random)
		=> New(dateTimeOffset.ToUnixTimeMilliseconds(), random);

	/// <summary>
	/// Creates a new <see cref="Ulid"/> with the specified timestamp in milliseconds.
	/// </summary>
	/// <param name="timestamp">The timestamp in milliseconds to use for the <see cref="Ulid"/>.</param>
	/// <param name="isMonotonic">
	/// If <c>null</c> (default), the value of <see cref="DefaultIsMonotonic"/> is used to determine monotonicity.<br />
	/// If <c>true</c>, ensures the <see cref="Ulid"/> is monotonically increasing.<br />
	/// If <c>false</c>, generates a random <see cref="Random" /> part in <see cref="Ulid"/>.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(long timestamp, bool? isMonotonic = null)
	{
		Ulid ulid = default;

		unsafe
		{
			var ulidBytes = new Span<byte>(Unsafe.AsPointer(ref Unsafe.AsRef(in ulid)), _ulidSize);

			FillTime(ulidBytes, timestamp);
			FillRandom(ulidBytes, isMonotonic ?? DefaultIsMonotonic);
		}

		return ulid;
	}

	/// <summary>
	/// Creates a new instance of the <see cref="Ulid"/> struct using the specified timestamp and random byte sequence.
	/// </summary>
	/// <param name="timestamp">
	/// A 64-bit integer representing the timestamp in milliseconds since the Unix epoch (1970-01-01T00:00:00Z).
	/// This value will be encoded into the first 6 bytes of the <see cref="Ulid"/>.
	/// </param>
	/// <param name="random">
	/// A span containing the random component of the <see cref="Ulid"/>. 
	/// It must be at least 10 bytes long, as the last 10 bytes of the <see cref="Ulid"/> are derived from this span.
	/// </param>
	/// <returns>
	/// A new <see cref="Ulid"/> instance composed of the given timestamp and random byte sequence.
	/// </returns>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(long timestamp, Span<byte> random)
	{
		Ulid ulid = default;

		unsafe
		{
			var ulidBytes = new Span<byte>(Unsafe.AsPointer(ref Unsafe.AsRef(in ulid)), _ulidSize);

			FillTime(ulidBytes, timestamp);
			random.CopyTo(ulidBytes[_ulidSizeTime..]);
		}

		return ulid;
	}

#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	private static void FillTime(Span<byte> bytes, long timestamp)
	{
		unsafe
		{
			// Get a pointer to the timestamp and convert it to a reference to the first byte.
			ref var firstByte = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref timestamp));

			if (BitConverter.IsLittleEndian)
			{
				// If the system is little-endian, reverse the bytes to store in big-endian format.
				bytes[5] = Unsafe.Add(ref firstByte, 0);

				bytes[0] = Unsafe.Add(ref firstByte, 5);
				bytes[1] = Unsafe.Add(ref firstByte, 4);
				bytes[2] = Unsafe.Add(ref firstByte, 3);
				bytes[3] = Unsafe.Add(ref firstByte, 2);
				bytes[4] = Unsafe.Add(ref firstByte, 1);
			}
			else
			{
				// If the system is big-endian, just copy the bytes directly.
				bytes[5] = Unsafe.Add(ref firstByte, 7);

				bytes[0] = Unsafe.Add(ref firstByte, 2);
				bytes[1] = Unsafe.Add(ref firstByte, 3);
				bytes[2] = Unsafe.Add(ref firstByte, 4);
				bytes[3] = Unsafe.Add(ref firstByte, 5);
				bytes[4] = Unsafe.Add(ref firstByte, 6);
			}
		}
	}

#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	private static void FillRandom(Span<byte> bytes, bool isMonotonic)
	{
		if (!isMonotonic)
		{
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
			_rng.GetBytes(bytes[_ulidSizeTime..]);
#else
			// In NetStandard 2.0, RandomNumberGenerator.GetBytes() does not support Span<byte> overloads.
			var random = ArrayPool<byte>.Shared.Rent(_ulidSizeRandom);

			_rng.GetBytes(random, 0, _ulidSizeRandom);
			new ReadOnlySpan<byte>(random, 0, _ulidSizeRandom).CopyTo(bytes[_ulidSizeTime..]);

			ArrayPool<byte>.Shared.Return(random, true);
#endif
			return;
		}

		var lastUlidSpan = _lastUlid.AsSpan();

		lock (_lock)
		{
			// If the timestamp is the same or lesser than the last one, increment the last ULID by one
			if (bytes[.._ulidSizeTime].SequenceCompareTo(lastUlidSpan[.._ulidSizeTime]) <= 0)
			{
				var i = _ulidSize;
				while (i > 0)
				{
					if (++_lastUlid[--i] != 0)
					{
						break;
					}
				}
			}
			// Otherwise, generate a new ULID
			else
			{
				bytes[.._ulidSizeTime].CopyTo(lastUlidSpan);
				_rng.GetBytes(_lastUlid, _ulidSizeTime, _ulidSizeRandom);
			}

			_lastUlid.CopyTo(bytes);
		}
	}
}
