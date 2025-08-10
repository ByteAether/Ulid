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
	// HACK: We assume the layout of a Guid is the following:
	// Int32, Int16, Int16, Int8, Int8, Int8, Int8, Int8, Int8, Int8, Int8
	// source: https://github.com/dotnet/runtime/blob/5c4686f831d34c2c127e943d0f0d144793eeb0ad/src/libraries/System.Private.CoreLib/src/System/Guid.cs
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
	public static Ulid New(Guid guid)
	{
#if NET6_0_OR_GREATER
		if (BitConverter.IsLittleEndian && _isVector128Supported)
		{
			var vector = Unsafe.As<Guid, Vector128<byte>>(ref guid);
			var shuffled = Shuffle(vector);

			return Unsafe.As<Vector128<byte>, Ulid>(ref shuffled);
		}
#endif
		Span<byte> ulidBytes = stackalloc byte[_ulidSize];

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
	/// Converts the ULID to a GUID.
	/// </summary>
	/// <returns>A GUID representing the ULID.</returns>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
	public readonly Guid ToGuid()
	{
#if NETCOREAPP
		if (BitConverter.IsLittleEndian && _isVector128Supported)
		{
			var vector = Unsafe.As<Ulid, Vector128<byte>>(ref Unsafe.AsRef(in this));
			var shuffled = Shuffle(vector);

			return Unsafe.As<Vector128<byte>, Guid>(ref shuffled);
		}
#endif
		Span<byte> guidBytes = stackalloc byte[_ulidSize];
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
	private static readonly bool _isVector128Supported =
#if NET7_0_OR_GREATER
		Vector128.IsHardwareAccelerated;
#else
		Sse3.IsSupported;
#endif

	private static readonly Vector128<byte> _shuffleMask
		= Vector128.Create((byte)3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15);

#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	private static Vector128<byte> Shuffle(Vector128<byte> value)
	{
		return
#if NET7_0_OR_GREATER
			Vector128.IsHardwareAccelerated ? Vector128.Shuffle(value, _shuffleMask) :
#endif
			Ssse3.IsSupported ? Ssse3.Shuffle(value, _shuffleMask) :
			throw new NotImplementedException();
	}
#endif
}