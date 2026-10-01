using System.Buffers.Binary;
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

	// Constant for Unix Epoch (1970-01-01 UTC) in Ticks
	private const long _unixEpochTicks = 621355968000000000;

	/// <summary>
	/// Initializes a new instance of the <see cref="Ulid"/> struct using the specified byte array.
	/// </summary>
	/// <param name="bytes">The byte array to initialize the <see cref="Ulid"/> with.</param>
	/// <returns>Given bytes as an <see cref="Ulid"/> instance.</returns>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static Ulid New(GenerationOptions? options = null)
		// We can avoid Offset-related allocations by using DateTime over DateTimeOffset
		// For public API, DateTimeOffset is an official recommendation
		=> New((DateTime.UtcNow.Ticks - _unixEpochTicks) / TimeSpan.TicksPerMillisecond, options);

	/// <summary>
	/// Creates a new <see cref="Ulid"/> with the specified timestamp.
	/// </summary>
	/// <param name="dateTimeOffset">The timestamp to use for the <see cref="Ulid"/>.</param>
	/// <param name="options">
	/// If <c>null</c> (default), the value of <see cref="DefaultGenerationOptions"/> is used.<br/>
	/// Otherwise, uses the specified <see cref="GenerationOptions"/> to control the ULID generation behavior.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static Ulid New(long timestamp, GenerationOptions? options = null)
	{
		Unsafe.SkipInit(out Ulid ulid);

		ref var ulidRef = ref Unsafe.As<Ulid, byte>(ref ulid);

		Fill(ref ulidRef, timestamp, options ?? DefaultGenerationOptions);

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
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static Ulid New(long timestamp, Span<byte> random)
	{
		Unsafe.SkipInit(out Ulid ulid);

		ref var ulidRef = ref Unsafe.As<Ulid, byte>(ref ulid);

		// Fill timestamp
		FillTimestamp(ref ulidRef, timestamp);

		// Fill random
		Unsafe.CopyBlockUnaligned(
			ref Unsafe.Add(ref ulidRef, _ulidSizeTime),
			ref random.GetPinnableReference(),
			_ulidSizeRandom
		);

		return ulid;
	}

#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	private static void FillTimestamp(ref byte ulidBytesRef, long timestamp)
	{
		var ts = (ulong)timestamp << 16;
		ts = ReverseOnLittleEndian(ts);
		Unsafe.WriteUnaligned(ref ulidBytesRef, ts);
	}

#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	private static void Fill(ref byte ulidBytesRef, long timestamp, GenerationOptions options)
	{
		// Calculate offset to a random part
		var monotonicity = options.Monotonicity;

		if (monotonicity == GenerationOptions.MonotonicityOptions.NonMonotonic)
		{
			FillTimestamp(ref ulidBytesRef, timestamp);
			options.InitialRandomSource.GetBytes(CreateSpan(ref Unsafe.Add(ref ulidBytesRef, _ulidSizeTime), _ulidSizeRandom));
			return;
		}

		FillMonotonic(ref ulidBytesRef, timestamp, options, monotonicity);
	}

#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
	private static void FillMonotonic(
		ref byte ulidBytesRef,
		long timestamp,
		GenerationOptions options,
		GenerationOptions.MonotonicityOptions monotonicity
	)
	{
		ref var ulidBytesRandomRef = ref Unsafe.Add(ref ulidBytesRef, _ulidSizeTime);

		var state = options.CurrentState;

		using (state.Lock.Enter())
		{
			// Read the last timestamp (from bytes 0-7 of "last ULID")
			// Shift it to get 48 bits.
			var lastTime = ReverseOnLittleEndian(state.LastUlidPart0);
			lastTime >>= 16;

			// If the timestamp is bigger than the last one, generate a new ULID
			if (timestamp > (long)lastTime)
			{
				// We work on "generated ULID", then copy it into "last ULID"
				FillTimestamp(ref ulidBytesRef, timestamp);

				// Generate a new random to the generated ULID
				options.InitialRandomSource.GetBytes(CreateSpan(ref ulidBytesRandomRef, _ulidSizeRandom));

				// Store the generated ULID as two native-width words in the state.
				state.LastUlidPart0 = Unsafe.ReadUnaligned<ulong>(ref ulidBytesRef);
				state.LastUlidPart1 = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref ulidBytesRef, sizeof(ulong)));
			}
			else // Otherwise, increment the last ULID
			{
				// We work on "last ULID", then copy it into "generated ULID"
				if (monotonicity == GenerationOptions.MonotonicityOptions.MonotonicIncrement)
				{
					state.Increment(0);
				}
				else
				{
					// We can use the random bytes of incomplete ULID for the random increment span
					var tempSpan = CreateSpan(ref ulidBytesRandomRef, sizeof(uint));
					options.IncrementRandomSource.GetBytes(tempSpan[..(int)monotonicity]);
					var increment = BinaryPrimitives.ReadUInt32LittleEndian(tempSpan);

					// The tempSpan may contain garbage, so mask that out
					var totalBitsToKeep = (int)monotonicity * 8;
					var mask = (uint)((1UL << totalBitsToKeep) - 1);
					increment &= mask;

					state.Increment(increment);
				}

				// Copy the state back as two native-width words.
				Unsafe.WriteUnaligned(ref ulidBytesRef, state.LastUlidPart0);
				Unsafe.WriteUnaligned(ref Unsafe.Add(ref ulidBytesRef, sizeof(ulong)), state.LastUlidPart1);
			}
		}
	}
}