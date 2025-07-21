using System.Runtime.CompilerServices;

namespace ByteAether.Ulid;
public readonly partial struct Ulid : IComparable, IComparable<Ulid>
{
	/// <summary>
	/// Determines whether the value of the left ULID is less than the value of the right ULID.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is less than the value of the right ULID; otherwise, false.</returns>
	public static bool operator <(Ulid left, Ulid right)
		=> left.CompareTo(right) < 0;

	/// <summary>
	/// Determines whether the value of the left ULID is less than or equal to the value of the right ULID.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is less than or equal to the value of the right ULID; otherwise, false.</returns>
	public static bool operator <=(Ulid left, Ulid right)
		=> left.CompareTo(right) <= 0;

	/// <summary>
	/// Determines whether the value of the left ULID is greater than the value of the right ULID.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is greater than the value of the right ULID; otherwise, false.</returns>
	public static bool operator >(Ulid left, Ulid right)
		=> left.CompareTo(right) > 0;

	/// <summary>
	/// Determines whether the value of the left ULID is greater than or equal to the value of the right ULID.
	/// </summary>
	/// <param name="left">The first ULID to compare.</param>
	/// <param name="right">The second ULID to compare.</param>
	/// <returns>True if the value of the left ULID is greater than or equal to the value of the right ULID; otherwise, false.</returns>
	public static bool operator >=(Ulid left, Ulid right)
		=> left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public readonly int CompareTo(object? obj)
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
	public readonly int CompareTo(Ulid other)
		=> CompareToCore(this, other);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int CompareToCore(in Ulid left, in Ulid right)
		=> left._t0 != right._t0 ? GetResult(left._t0, right._t0)
			: left._t1 != right._t1 ? GetResult(left._t1, right._t1)
			: left._t2 != right._t2 ? GetResult(left._t2, right._t2)
			: left._t3 != right._t3 ? GetResult(left._t3, right._t3)
			: left._t4 != right._t4 ? GetResult(left._t4, right._t4)
			: left._t5 != right._t5 ? GetResult(left._t5, right._t5)
			: left._r0 != right._r0 ? GetResult(left._r0, right._r0)
			: left._r1 != right._r1 ? GetResult(left._r1, right._r1)
			: left._r2 != right._r2 ? GetResult(left._r2, right._r2)
			: left._r3 != right._r3 ? GetResult(left._r3, right._r3)
			: left._r4 != right._r4 ? GetResult(left._r4, right._r4)
			: left._r5 != right._r5 ? GetResult(left._r5, right._r5)
			: left._r6 != right._r6 ? GetResult(left._r6, right._r6)
			: left._r7 != right._r7 ? GetResult(left._r7, right._r7)
			: left._r8 != right._r8 ? GetResult(left._r8, right._r8)
			: left._r9 != right._r9 ? GetResult(left._r9, right._r9)
			: 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetResult(byte left, byte right) => left < right ? -1 : 1;
}
