using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ByteAether.Ulid;

public readonly partial struct Ulid
{
	/// <summary>
	/// Defines the monotonicity behavior for ULID generation.
	/// </summary>
	/// <remarks>
	/// The <see cref="Monotonicity"/> enum provides various options to configure
	/// the generation of ULIDs with respect to their monotonic properties.
	/// These options determine how the ULID sequence behaves in scenarios
	/// where time does not progress or progresses non-linearly.
	/// </remarks>
	public enum Monotonicity
	{
		/// <summary>
		/// Indicates that ULIDs are generated in a completely non-monotonic manner.
		/// </summary>
		/// <remarks>
		/// When <see cref="Monotonicity.NonMonotonic"/> is used, ULIDs are created
		/// without any monotonic guarantees. The random component of the ULID is
		/// entirely random, and the sequence does not ensure order or incrementality.
		/// This is the default behavior when monotonicity settings are not explicitly defined.
		/// </remarks>
		NonMonotonic = -1,

		/// <summary>
		/// Indicates that ULIDs are generated with a strictly monotonic increment in the random component.
		/// </summary>
		/// <remarks>
		/// When <see cref="Monotonicity.MonotonicIncrement"/> is used, the random portion of the ULID is
		/// adjusted to ensure strict monotonic progression. This guarantees that the sequence of generated
		/// ULIDs is always ordered and incremental, making it suitable for scenarios where strict ordering
		/// is required without introducing additional randomness.
		/// </remarks>
		MonotonicIncrement = 0,

		/// <summary>
		/// Generates ULIDs with monotonicity by adding a random 8-bit value to the existing random component.
		/// </summary>
		/// <remarks>
		/// When <see cref="Monotonicity.MonotonicRandom1Byte"/> is used, a random value between 1 and 256
		/// is added to the existing random component. This addition may cause carries across all bytes of
		/// the random component, ensuring the resulting ULID is greater than the previous one while
		/// maintaining a degree of randomness in the increment.
		/// </remarks>
		MonotonicRandom1Byte = 1,

		/// <summary>
		/// Generates ULIDs with monotonicity by adding a random 16-bit value to the existing random component.
		/// </summary>
		/// <remarks>
		/// When <see cref="Monotonicity.MonotonicRandom2Byte"/> is used, a random value between 1 and 65 536
		/// is added to the existing random component. This addition may cause carries across all bytes of
		/// the random component, ensuring the resulting ULID is greater than the previous one while
		/// providing a larger range of possible increments.
		/// </remarks>
		MonotonicRandom2Byte = 2,

		/// <summary>
		/// Generates ULIDs with monotonicity by adding a random 24-bit value to the existing random component.
		/// </summary>
		/// <remarks>
		/// When <see cref="Monotonicity.MonotonicRandom3Byte"/> is used, a random value between 1 and 16 777 216
		/// is added to the existing random component. This addition may cause carries across all bytes of
		/// the random component, ensuring the resulting ULID is greater than the previous one while
		/// providing a significantly larger range of possible increments.
		/// </remarks>
		MonotonicRandom3Byte = 3,

		/// <summary>
		/// Generates ULIDs with monotonicity by adding a random 32-bit value to the existing random component.
		/// </summary>
		/// <remarks>
		/// When <see cref="Monotonicity.MonotonicRandom4Byte"/> is used, a random value between 1 and 4 294 967 296
		/// is added to the existing random component. This addition may cause carries across all bytes of
		/// the random component, ensuring the resulting ULID is greater than the previous one while
		/// providing the maximum range of possible increments.
		/// </remarks>
		MonotonicRandom4Byte = 4,
	}

	/// <summary>
	/// Default monotonicity mode for generating ULIDs.
	/// </summary>
	/// <remarks>
	/// Determines the monotonicity behavior used during ULID creation.
	/// It can be configured to influence how sequential ULIDs are generated.
	/// </remarks>
	public static Monotonicity DefaultMonotonicity { get; set; } = Monotonicity.MonotonicIncrement;

	private static readonly byte[] _lastUlid = new byte[_ulidSize];
	private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
#if NET6_0_OR_GREATER
	private static Random _rngSimple => System.Random.Shared;
#else
	private static readonly Random _rngSimple = new();
#endif

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
	/// <param name="monotonicity">
	/// If <c>null</c> (default), the value of <see cref="DefaultMonotonicity"/> is used.<br />
	/// Otherwise, uses the specified <see cref="Monotonicity"/> value to control the ULID generation behavior.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(Monotonicity? monotonicity = null)
		=> New(DateTimeOffset.UtcNow, monotonicity);

	/// <summary>
	/// Creates a new <see cref="Ulid"/> with the specified timestamp.
	/// </summary>
	/// <param name="dateTimeOffset">The timestamp to use for the <see cref="Ulid"/>.</param>
	/// <param name="monotonicity">
	/// If <c>null</c> (default), the value of <see cref="DefaultMonotonicity"/> is used.<br />
	/// Otherwise, uses the specified <see cref="Monotonicity"/> value to control the ULID generation behavior.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(DateTimeOffset dateTimeOffset, Monotonicity? monotonicity = null)
		=> New(dateTimeOffset.ToUnixTimeMilliseconds(), monotonicity);

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
	/// <param name="monotonicity">
	/// If <c>null</c> (default), the value of <see cref="DefaultMonotonicity"/> is used.<br />
	/// Otherwise, uses the specified <see cref="Monotonicity"/> value to control the ULID generation behavior.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(long timestamp, Monotonicity? monotonicity = null)
	{
		Ulid ulid = default;

		unsafe
		{
			var ulidBytes = new Span<byte>(Unsafe.AsPointer(ref Unsafe.AsRef(in ulid)), _ulidSize);

			FillTime(ulidBytes, timestamp);
			FillRandom(ulidBytes, monotonicity ?? DefaultMonotonicity);
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
				// If the system is big-endian, copy the bytes directly.
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void FillRandom(Span<byte> bytes, Monotonicity monotonicity)
	{
		if (monotonicity == Monotonicity.NonMonotonic)
		{
			_rng.GetBytes(bytes[_ulidSizeTime..]);
			return;
		}

		var lastUlidSpan = _lastUlid.AsSpan();

		lock (_lock)
		{
			// If the timestamp is the same or lesser than the last one, increment the last ULID by one
			if (bytes[.._ulidSizeTime].SequenceCompareTo(lastUlidSpan[.._ulidSizeTime]) <= 0)
			{
				// We can use the last bytes of incomplete ULID for the increment parameter
				var randomByteCount = (int)monotonicity;
				var tempSpan = bytes.Slice(_ulidSize - randomByteCount, randomByteCount);

				if (randomByteCount > 0)
				{
					_rngSimple.NextBytes(tempSpan);
				}

				IncrementByteSpan(lastUlidSpan, tempSpan);
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

#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	private static void IncrementByteSpan(Span<byte> targetSpan, ReadOnlySpan<byte> sourceSpan)
	{
		ushort carry = 1; // max sum 255+255+1 = 511; guarantee at least +1

		// This value represents the offset from the start of targetSpan to the start of sourceSpan
		var lengthDifference = targetSpan.Length - sourceSpan.Length;
		ushort sum; // Per-byte additions placeholder

		// Phase 1: Process the common length part (where both targetSpan and sourceSpan contribute)
		if (sourceSpan.Length != 0)
		{
			for (var i = targetSpan.Length - 1; i >= lengthDifference; --i)
			{
				var sourceIdx = i - lengthDifference;

				var byteFromTarget = targetSpan[i];
				var byteFromSource = sourceSpan[sourceIdx];

				sum = (ushort)(byteFromTarget + byteFromSource + carry);
				targetSpan[i] = (byte)(sum & 0xFF);
				carry = (ushort)(sum >> 8);
			}
		}

		// Phase 2: Process the remaining part of targetSpan (only carry propagation)
		if (carry == 0)
		{
			return;
		}

		// Runs from the point where sourceSpan ended, towards the MSB end of targetSpan
		for (var i = lengthDifference - 1; i >= 0; i--)
		{
			var byteFromTarget = targetSpan[i];
			sum = (ushort)(byteFromTarget + carry);
			targetSpan[i] = (byte)(sum & 0xFF);
			carry = (ushort)(sum >> 8);

			if (carry == 0)
			{
				return; // No more carry to propagate
			}
		}

		// If there's still a carry (we have not returned from the method),
		// it indicates an overflow beyond the original targetSpan's capacity.
		throw new OverflowException("Addition resulted in a value larger than the target span's capacity.");
	}
}