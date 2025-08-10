using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#if NETCOREAPP
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace ByteAether.Ulid;

public readonly partial struct Ulid : IEquatable<Ulid>
{
	/// <inheritdoc/>
#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public override readonly int GetHashCode()
	{
		ref var rA = ref Unsafe.As<Ulid, int>(ref Unsafe.AsRef(in this));
		return rA ^ Unsafe.Add(ref rA, 1) ^ Unsafe.Add(ref rA, 2) ^ Unsafe.Add(ref rA, 3);
	}

	/// <inheritdoc/>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
	public readonly bool Equals(Ulid other)
		=> EqualsCore(this, other);

	/// <inheritdoc/>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
	public override bool Equals([NotNullWhen(true)] object? obj)
		=> obj is Ulid ulid && EqualsCore(this, ulid);

	/// <summary>
	/// Determines whether two specified ULIDs have the same value.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is equal to the value of the right ULID; otherwise, false.</returns>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
	public static bool operator ==(Ulid left, Ulid right)
		=> EqualsCore(left, right);

	/// <summary>
	/// Determines whether two specified ULIDs have different values.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is not equal to the value of the right ULID; otherwise, false.</returns>
	public static bool operator !=(Ulid left, Ulid right)
		=> !EqualsCore(left, right);

#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	private static bool EqualsCore(in Ulid left, in Ulid right)
	{
#if NET7_0_OR_GREATER
		if (Vector128.IsHardwareAccelerated)
		{
			var vA = Unsafe.As<Ulid, Vector128<byte>>(ref Unsafe.AsRef(in left));
			var vB = Unsafe.As<Ulid, Vector128<byte>>(ref Unsafe.AsRef(in right));
			return vA == vB;
		}
#endif
#if NETCOREAPP
		if (Sse2.IsSupported)
		{
			var vA = Unsafe.As<Ulid, Vector128<byte>>(ref Unsafe.AsRef(in left));
			var vB = Unsafe.As<Ulid, Vector128<byte>>(ref Unsafe.AsRef(in right));
			return Sse2.MoveMask(Sse2.CompareEqual(vA, vB)) == 0xFFFF;
		}
#endif

		ref var rA = ref Unsafe.As<Ulid, long>(ref Unsafe.AsRef(in left));
		ref var rB = ref Unsafe.As<Ulid, long>(ref Unsafe.AsRef(in right));

		// Compare 2x 64bit long
		return rA == rB && Unsafe.Add(ref rA, 1) == Unsafe.Add(ref rB, 1);
	}
}