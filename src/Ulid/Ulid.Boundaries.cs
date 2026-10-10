using System.Runtime.CompilerServices;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace ByteAether.Ulid;

public readonly partial struct Ulid
#if NET7_0_OR_GREATER
	: IMinMaxValue<Ulid>
#endif
{
	/// <summary>Gets the minimum value of the ULID type.</summary>
	/// <remarks>
	/// The <see cref="MinValue"/> field is a ULID with all components set to zero.
	/// It can be used as a default or placeholder value.
	/// </remarks>
	public static Ulid MinValue { get; } = default;

	/// <summary>
	/// Represents the maximum possible value for a ULID.
	/// </summary>
	/// <remarks>
	/// The <see cref="MaxValue"/> field is a ULID where all byte components are set to their highest possible value (0xFF).
	/// It can be used as a sentinel or boundary value in comparison operations or range validations.
	/// </remarks>
	public static Ulid MaxValue { get; } = FromParts(ulong.MaxValue, ulong.MaxValue);

	/// <summary>
	/// Represents an empty ULID value.
	/// </summary>
	/// <remarks>
	/// The <see cref="Empty"/> field is a ULID with all components set to zero.
	/// It can be used as a default or placeholder value.
	/// It is equivalent to <see cref="MinValue"/>, but is provided for clarity.
	/// </remarks>
	public static Ulid Empty => MinValue;

	/// <summary>
	/// Creates the minimum possible <see cref="Ulid"/> value for the specified timestamp.
	/// </summary>
	/// <param name="timestamp">The timestamp used to create the minimum <see cref="Ulid"/> value.</param>
	/// <returns>The minimum <see cref="Ulid"/> value for the given timestamp.</returns>
	public static Ulid MinAt(long timestamp) => FromParts((ulong)timestamp << 16, 0);

	/// <summary>
	/// Creates the minimum possible <see cref="Ulid"/> value for the specified timestamp.
	/// </summary>
	/// <param name="datetime">The timestamp used to create the minimum <see cref="Ulid"/> value.</param>
	/// <returns>The minimum <see cref="Ulid"/> value for the given timestamp.</returns>
	public static Ulid MinAt(DateTimeOffset datetime) => MinAt(datetime.ToUnixTimeMilliseconds());

	/// <summary>
	/// Creates the maximum possible <see cref="Ulid"/> value for the specified timestamp.
	/// </summary>
	/// <param name="timestamp">The timestamp used to create the maximum <see cref="Ulid"/> value.</param>
	/// <returns>The maximum <see cref="Ulid"/> value for the given timestamp.</returns>
	public static Ulid MaxAt(long timestamp) => FromParts(((ulong)timestamp << 16) | 0xFFFF, ulong.MaxValue);

	/// <summary>
	/// Creates the maximum possible <see cref="Ulid"/> value for the specified timestamp.
	/// </summary>
	/// <param name="datetime">The timestamp used to create the maximum <see cref="Ulid"/> value.</param>
	/// <returns>The maximum <see cref="Ulid"/> value for the given timestamp.</returns>
	public static Ulid MaxAt(DateTimeOffset datetime) => MaxAt(datetime.ToUnixTimeMilliseconds());

	// Creates a ULID from its two numeric 64-bit halves
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Ulid FromParts(ulong part0, ulong part1)
	{
		Unsafe.SkipInit(out Ulid ulid);

		ref var ulidRef = ref Unsafe.As<Ulid, byte>(ref ulid);
		Unsafe.WriteUnaligned(ref ulidRef, ReverseOnLittleEndian(part0));
		Unsafe.WriteUnaligned(ref Unsafe.Add(ref ulidRef, sizeof(ulong)), ReverseOnLittleEndian(part1));

		return ulid;
	}
}