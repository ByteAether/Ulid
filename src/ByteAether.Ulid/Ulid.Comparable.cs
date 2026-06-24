using System.Buffers.Binary;
using System.Runtime.CompilerServices;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace ByteAether.Ulid;
public readonly partial struct Ulid : IComparable, IComparable<Ulid>
#if NET7_0_OR_GREATER
	, IComparisonOperators<Ulid, Ulid, bool>
#endif
{
	/// <summary>
	/// Determines whether the value of the left ULID is less than the value of the right ULID.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is less than the value of the right ULID; otherwise, false.</returns>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static bool operator <(Ulid left, Ulid right)
		=> left.CompareTo(right) < 0;

	/// <summary>
	/// Determines whether the value of the left ULID is less than or equal to the value of the right ULID.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is less than or equal to the value of the right ULID; otherwise, false.</returns>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static bool operator <=(Ulid left, Ulid right)
		=> left.CompareTo(right) <= 0;

	/// <summary>
	/// Determines whether the value of the left ULID is greater than the value of the right ULID.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is greater than the value of the right ULID; otherwise, false.</returns>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static bool operator >(Ulid left, Ulid right)
		=> left.CompareTo(right) > 0;

	/// <summary>
	/// Determines whether the value of the left ULID is greater than or equal to the value of the right ULID.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is greater than or equal to the value of the right ULID; otherwise, false.</returns>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public static bool operator >=(Ulid left, Ulid right)
		=> left.CompareTo(right) >= 0;

	/// <inheritdoc/>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public int CompareTo(object? obj)
	{
		if (obj == null)
		{
			return 1;
		}

		if (obj is not Ulid ulid)
		{
			throw new ArgumentException($"The value is not an instance of {nameof(Ulid)}.", nameof(obj));
		}

		return CompareTo(ulid);
	}

	/// <inheritdoc/>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public int CompareTo(Ulid other)
		=> CompareToCore(this, other);

#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
	private static int CompareToCore(in Ulid left, in Ulid right)
	{
		ref var rA = ref Unsafe.As<Ulid, ulong>(ref Unsafe.AsRef(in left));
		ref var rB = ref Unsafe.As<Ulid, ulong>(ref Unsafe.AsRef(in right));

		var a = BinaryPrimitives.ReverseEndianness(rA);
		var b = BinaryPrimitives.ReverseEndianness(rB);

		if (a != b)
		{
			return a < b ? -1 : 1;
		}

		a = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rA, 1));
		b = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rB, 1));

		if (a != b)
		{
			return a < b ? -1 : 1;
		}

		return 0;
	}
}