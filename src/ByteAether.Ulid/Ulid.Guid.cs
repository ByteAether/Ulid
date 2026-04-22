using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETCOREAPP
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace ByteAether.Ulid;

#if NET8_0_OR_GREATER
// We need to target netstandard2.1, so keep using ref for MemoryMarshal.Write
// CS9191: The 'ref' modifier for argument 2 corresponding to 'in' parameter is equivalent to 'in'. Consider using 'in' instead.
#pragma warning disable CS9191
#endif

public readonly partial struct Ulid
{
	/// <summary>
	/// Creates a new ULID using the specified GUID.
	/// </summary>
	/// <param name="guid">The GUID to initialize the ULID with.</param>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static Ulid New(Guid guid)
	{
		if (BitConverter.IsLittleEndian)
		{
			return Shuffle<Guid, Ulid>(ref guid);
		}

		return Unsafe.As<Guid, Ulid>(ref guid);
	}

	/// <summary>
	/// Converts the ULID to a GUID.
	/// </summary>
	/// <returns>A GUID representing the ULID.</returns>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public Guid ToGuid()
	{
		if (BitConverter.IsLittleEndian)
		{
			return Shuffle<Ulid, Guid>(ref Unsafe.AsRef(in this));
		}

		return Unsafe.As<Ulid, Guid>(ref Unsafe.AsRef(in this));
	}

	/// <summary>
	/// Implicitly converts a ULID to a GUID.
	/// </summary>
	/// <param name="ulid">The ULID to convert.</param>
	/// <returns>A GUID representing the ULID.</returns>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static implicit operator Guid(Ulid ulid) => ulid.ToGuid();

	/// <summary>
	/// Implicitly converts a GUID to a ULID.
	/// </summary>
	/// <param name="guid">The GUID to convert.</param>
	/// <returns>A ULID representing the GUID.</returns>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static implicit operator Ulid(Guid guid) => New(guid);

#if NETCOREAPP
	private static readonly Vector128<byte> _shuffleMask
		= Vector128.Create((byte)3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15);

	private static readonly bool _isAccelerated =
#if NET7_0_OR_GREATER
		Vector128.IsHardwareAccelerated ||
#endif
		Ssse3.IsSupported;
#endif

	// HACK: We assume the layout of a Guid is the following:
	// Int32, Int16, Int16, Int8, Int8, Int8, Int8, Int8, Int8, Int8, Int8
	// Source: https://github.com/dotnet/runtime/blob/5c4686f831d34c2c127e943d0f0d144793eeb0ad/src/libraries/System.Private.CoreLib/src/System/Guid.cs
	// More info: https://stackoverflow.com/questions/10190817/guid-byte-order-in-net/10191075#10191075
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	private static TOut Shuffle<TIn, TOut>(ref TIn bytes)
	{
#if NETCOREAPP
		if (_isAccelerated)
		{
			var vector = Unsafe.As<TIn, Vector128<byte>>(ref bytes);

#if NET7_0_OR_GREATER
			if (Vector128.IsHardwareAccelerated)
			{
				vector = Vector128.Shuffle(vector, _shuffleMask);
				return Unsafe.As<Vector128<byte>, TOut>(ref vector);
			}
#endif

			vector = Ssse3.Shuffle(vector, _shuffleMask);
			return Unsafe.As<Vector128<byte>, TOut>(ref vector);
		}
#endif

		// |A|B|C|D|E|F|G|H|I|J|K|L|M|N|O|P|
		// |D|C|B|A|...
		//      ...|F|E|H|G|...
		//              ...|I|J|K|L|M|N|O|P|
		Span<byte> result = new byte[_ulidSize];

		ref var ptr = ref Unsafe.As<TIn, uint>(ref bytes);
		var lower = BinaryPrimitives.ReverseEndianness(ptr);

		ptr = ref Unsafe.Add(ref ptr, 1);
		var upper = ((ptr & 0x00_FF_00_FF) << 8) | ((ptr & 0xFF_00_FF_00) >> 8);

		ref var upperBytes = ref Unsafe.As<uint, ulong>(ref Unsafe.Add(ref ptr, 1));

		MemoryMarshal.Write(result, ref lower);
		MemoryMarshal.Write(result[4..], ref upper);
		MemoryMarshal.Write(result[8..], ref upperBytes);

		return Unsafe.As<byte, TOut>(ref result.GetPinnableReference());
	}
}