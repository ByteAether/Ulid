namespace ByteAether.Ulid;

public partial struct Ulid
{
	private static readonly byte[] _randomMin = Enumerable.Repeat((byte)0x00, _ulidSizeRandom).ToArray();
	private static readonly byte[] _randomMax = Enumerable.Repeat((byte)0xFF, _ulidSizeRandom).ToArray();

	/// <summary>
	/// Represents an empty ULID value.
	/// </summary>
	/// <remarks>
	/// The <see cref="Empty"/> field is a ULID with all components set to zero.
	/// It can be used as a default or placeholder value.
	/// </remarks>
	public static readonly Ulid Empty = default;

	/// <summary>
	/// Represents the maximum possible value for a ULID.
	/// </summary>
	/// <remarks>
	/// The <see cref="Max"/> field is a ULID where all byte components are set to their highest possible value (0xFF).
	/// It can be used as a sentinel or boundary value in comparison operations or range validations.
	/// </remarks>
	public static readonly Ulid Max = New(Enumerable.Repeat((byte)0xFF, _ulidSize).ToArray());

	/// <summary>
	/// Creates the minimum possible <see cref="Ulid"/> value for the specified timestamp.
	/// </summary>
	/// <param name="timestamp">The timestamp used to create the minimum <see cref="Ulid"/> value.</param>
	/// <returns>The minimum <see cref="Ulid"/> value for the given timestamp.</returns>
	public static Ulid MinAt(long timestamp) => New(timestamp, _randomMin);

	/// <summary>
	/// Creates the minimum possible <see cref="Ulid"/> value for the specified timestamp.
	/// </summary>
	/// <param name="datetime">The timestamp used to create the minimum <see cref="Ulid"/> value.</param>
	/// <returns>The minimum <see cref="Ulid"/> value for the given timestamp.</returns>
	public static Ulid MinAt(DateTimeOffset datetime) => New(datetime, _randomMin);

	/// <summary>
	/// Creates the maximum possible <see cref="Ulid"/> value for the specified timestamp.
	/// </summary>
	/// <param name="timestamp">The timestamp used to create the maximum <see cref="Ulid"/> value.</param>
	/// <returns>The maximum <see cref="Ulid"/> value for the given timestamp.</returns>
	public static Ulid MaxAt(long timestamp) => New(timestamp, _randomMax);

	/// <summary>
	/// Creates the maximum possible <see cref="Ulid"/> value for the specified timestamp.
	/// </summary>
	/// <param name="datetime">The timestamp used to create the maximum <see cref="Ulid"/> value.</param>
	/// <returns>The maximum <see cref="Ulid"/> value for the given timestamp.</returns>
	public static Ulid MaxAt(DateTimeOffset datetime) => New(datetime, _randomMax);
}