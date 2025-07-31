using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ByteAether.Ulid;

public readonly partial struct Ulid
{
	/// <summary>
	/// The default <see cref="GenerationOptions"/> used for generating new ULIDs.
	/// </summary>
	/// <remarks>
	/// Allows customization of the generation behavior for all new ULIDs.<br/>
	/// It includes settings for monotonicity and the source of randomness for initial and
	/// incremented scenarios during the generation of the ULID. Modifying this property
	/// affects the global default behavior for ULID generation across the application.
	/// </remarks>
	public static GenerationOptions DefaultGenerationOptions { get; set; } = new();

	private static readonly byte[] _lastUlid = new byte[_ulidSize];

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
	/// <param name="options">
	/// If <c>null</c> (default), the value of <see cref="DefaultGenerationOptions"/> is used.<br/>
	/// Otherwise, uses the specified <see cref="GenerationOptions"/> to control the ULID generation behavior.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(GenerationOptions? options = null)
		=> New(DateTimeOffset.UtcNow, options);

	/// <summary>
	/// Creates a new <see cref="Ulid"/> with the specified timestamp.
	/// </summary>
	/// <param name="dateTimeOffset">The timestamp to use for the <see cref="Ulid"/>.</param>
	/// <param name="options">
	/// If <c>null</c> (default), the value of <see cref="DefaultGenerationOptions"/> is used.<br/>
	/// Otherwise, uses the specified <see cref="GenerationOptions"/> to control the ULID generation behavior.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(DateTimeOffset dateTimeOffset, GenerationOptions? options = null)
		=> New(dateTimeOffset.ToUnixTimeMilliseconds(), options);

	/// <summary>
	/// Creates a new <see cref="Ulid"/> with the specified timestamp.
	/// </summary>
	/// <param name="dateTimeOffset">The timestamp to use for the <see cref="Ulid"/>.</param>
	/// <param name="random" >
	/// A span containing the random component of the <see cref="Ulid"/>.<br/>
	/// Must be at least 10 bytes long to populate the random component of the Ulid.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(DateTimeOffset dateTimeOffset, Span<byte> random)
		=> New(dateTimeOffset.ToUnixTimeMilliseconds(), random);

	/// <summary>
	/// Creates a new <see cref="Ulid"/> with the specified timestamp in milliseconds.
	/// </summary>
	/// <param name="timestamp">The timestamp in milliseconds to use for the <see cref="Ulid"/>.</param>
	/// <param name="options">
	/// If <c>null</c> (default), the value of <see cref="DefaultGenerationOptions"/> is used.<br/>
	/// Otherwise, uses the specified <see cref="GenerationOptions"/> to control the ULID generation behavior.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Ulid New(long timestamp, GenerationOptions? options = null)
	{
		Ulid ulid = default;

		unsafe
		{
			var ulidBytes = new Span<byte>(Unsafe.AsPointer(ref Unsafe.AsRef(in ulid)), _ulidSize);

			FillTime(ulidBytes, timestamp);
			FillRandom(ulidBytes, options ?? DefaultGenerationOptions);
		}

		return ulid;
	}

	/// <summary>
	/// Creates a new instance of the <see cref="Ulid"/> struct using the specified timestamp and random byte sequence.
	/// </summary>
	/// <param name="timestamp">
	/// A 64-bit integer representing the timestamp in milliseconds since the Unix epoch (1970-01-01T00:00:00Z).<br/>
	/// This value will be encoded into the first 6 bytes of the <see cref="Ulid"/>.
	/// </param>
	/// <param name="random">
	/// A span containing the random component of the <see cref="Ulid"/>.<br/>
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
	private static void FillRandom(Span<byte> bytes, GenerationOptions options)
	{
		if (options.Monotonicity == GenerationOptions.MonotonicityOptions.NonMonotonic)
		{
			options.InitialRandomSource.GetBytes(bytes[_ulidSizeTime..]);
			return;
		}

		// ReSharper disable once InconsistentlySynchronizedField creating a span is safe
		var lastUlidSpan = _lastUlid.AsSpan();

		lock (_lock)
		{
			// If the timestamp is the same or lesser than the last one, increment the last ULID by one
			if (bytes[.._ulidSizeTime].SequenceCompareTo(lastUlidSpan[.._ulidSizeTime]) <= 0)
			{
				// We can use the last bytes of incomplete ULID for the increment parameter
				var randomByteCount = (int)options.Monotonicity;
				var tempSpan = bytes.Slice(_ulidSize - randomByteCount, randomByteCount);

				if (randomByteCount > 0)
				{
					options.IncrementRandomSource.GetBytes(tempSpan);
				}

				IncrementByteSpan(lastUlidSpan, tempSpan);
			}
			// Otherwise, generate a new ULID
			else
			{
				bytes[.._ulidSizeTime].CopyTo(lastUlidSpan);
				options.InitialRandomSource.GetBytes(lastUlidSpan[_ulidSizeTime..]);
			}

			_lastUlid.CopyTo(bytes);
		}
	}

#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void IncrementByteSpan(Span<byte> targetSpan, ReadOnlySpan<byte> sourceSpan)
	{
		ushort carry = 1; // max sum 255 + 255 + 1 = 511; guarantee at least +1

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

			if (carry == 0)
			{
				return;
			}
		}

		// Phase 2: Process the remaining part of targetSpan (only carry propagation)
		// Runs from the point where sourceSpan ended, towards the MSB end of targetSpan
		for (var i = lengthDifference - 1; i >= 0; --i)
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