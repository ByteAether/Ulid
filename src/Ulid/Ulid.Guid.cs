using System.Buffers.Binary;
using System.Runtime.CompilerServices;
#if NETCOREAPP
using System.Runtime.Intrinsics;
#if !NET7_0_OR_GREATER
using System.Runtime.Intrinsics.X86;
#endif
#endif

namespace ByteAether.Ulid;

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
		=> BitConverter.IsLittleEndian
			? Shuffle<Guid, Ulid>(ref guid)
			: Unsafe.As<Guid, Ulid>(ref guid);

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
		=> BitConverter.IsLittleEndian
			? Shuffle<Ulid, Guid>(ref Unsafe.AsRef(in this))
			: Unsafe.As<Ulid, Guid>(ref Unsafe.AsRef(in this));

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
	// Created inline from constants, so the JIT emits it as a constant vector in every tier
	private static Vector128<byte> _shuffleMask
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Vector128.Create((byte)3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15);
	}
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
#if NET7_0_OR_GREATER
		if (Vector128.IsHardwareAccelerated)
		{
			var vector = Unsafe.As<TIn, Vector128<byte>>(ref bytes);
			vector = Vector128.Shuffle(vector, _shuffleMask);
			return Unsafe.As<Vector128<byte>, TOut>(ref vector);
		}
#elif NETCOREAPP3_0_OR_GREATER
		if (Ssse3.IsSupported)
		{
			var vector = Unsafe.As<TIn, Vector128<byte>>(ref bytes);
			vector = Ssse3.Shuffle(vector, _shuffleMask);
			return Unsafe.As<Vector128<byte>, TOut>(ref vector);
		}
#endif

		// |A|B|C|D|E|F|G|H|I|J|K|L|M|N|O|P|
		// |D|C|B|A|...
		//      ...|F|E|H|G|...
		//              ...|I|J|K|L|M|N|O|P|
		ref var src = ref Unsafe.As<TIn, byte>(ref bytes);

		var lower = BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<uint>(ref src));

		var upper = Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref src, 4));
		upper = ((upper & 0x00_FF_00_FF) << 8) | ((upper & 0xFF_00_FF_00) >> 8);

		Unsafe.SkipInit(out TOut result);
		ref var dst = ref Unsafe.As<TOut, byte>(ref result);

		Unsafe.WriteUnaligned(ref dst, lower);
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref dst, 4), upper);
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref dst, 8), Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref src, 8)));

		return result;
	}
}