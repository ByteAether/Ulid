namespace ByteAether.Ulid;

public readonly partial struct Ulid
{
	/// <summary>
	/// Specifies the configuration options for ULID generation.
	/// </summary>
	public class GenerationOptions
	{
		/// <summary>
		/// Defines the monotonicity behavior for ULID generation.
		/// </summary>
		/// <remarks>
		/// The <see cref="MonotonicityOptions"/> enum provides various options to configure
		/// the generation of ULIDs with respect to their monotonic properties.
		/// These options determine how the ULID sequence behaves in scenarios
		/// where time does not progress or progresses non-linearly.
		/// </remarks>
		public enum MonotonicityOptions
		{
			/// <summary>
			/// Indicates that ULIDs are generated in a completely non-monotonic manner.
			/// </summary>
			/// <remarks>
			/// When <see cref="NonMonotonic"/> is used, ULIDs are created
			/// without any monotonic guarantees. The random component of the ULID is
			/// entirely random, and the sequence does not ensure order or incrementality.
			/// This is the default behavior when monotonicity settings are not explicitly defined.
			/// </remarks>
			NonMonotonic = -1,

			/// <summary>
			/// Indicates that ULIDs are generated with a strictly monotonic increment in the random component.
			/// </summary>
			/// <remarks>
			/// When <see cref="MonotonicIncrement"/> is used, the random portion of the ULID is
			/// adjusted to ensure strict monotonic progression. This guarantees that the sequence of generated
			/// ULIDs is always ordered and incremental, making it suitable for scenarios where strict ordering
			/// is required without introducing additional randomness.
			/// </remarks>
			MonotonicIncrement = 0,

			/// <summary>
			/// Generates ULIDs with monotonicity by adding a random 8-bit value to the existing random component.
			/// </summary>
			/// <remarks>
			/// When <see cref="MonotonicRandom1Byte"/> is used, a random value between 1 and 256
			/// is added to the existing random component. This addition may cause carries across all bytes of
			/// the random component, ensuring the resulting ULID is greater than the previous one while
			/// maintaining a degree of randomness in the increment.
			/// </remarks>
			MonotonicRandom1Byte = 1,

			/// <summary>
			/// Generates ULIDs with monotonicity by adding a random 16-bit value to the existing random component.
			/// </summary>
			/// <remarks>
			/// When <see cref="MonotonicRandom2Byte"/> is used, a random value between 1 and 65 536
			/// is added to the existing random component. This addition may cause carries across all bytes of
			/// the random component, ensuring the resulting ULID is greater than the previous one while
			/// providing a larger range of possible increments.
			/// </remarks>
			MonotonicRandom2Byte = 2,

			/// <summary>
			/// Generates ULIDs with monotonicity by adding a random 24-bit value to the existing random component.
			/// </summary>
			/// <remarks>
			/// When <see cref="MonotonicRandom3Byte"/> is used, a random value between 1 and 16 777 216
			/// is added to the existing random component. This addition may cause carries across all bytes of
			/// the random component, ensuring the resulting ULID is greater than the previous one while
			/// providing a significantly larger range of possible increments.
			/// </remarks>
			MonotonicRandom3Byte = 3,

			/// <summary>
			/// Generates ULIDs with monotonicity by adding a random 32-bit value to the existing random component.
			/// </summary>
			/// <remarks>
			/// When <see cref="MonotonicRandom4Byte"/> is used, a random value between 1 and 4 294 967 296
			/// is added to the existing random component. This addition may cause carries across all bytes of
			/// the random component, ensuring the resulting ULID is greater than the previous one while
			/// providing the maximum range of possible increments.
			/// </remarks>
			MonotonicRandom4Byte = 4,
		}

		/// <summary>
		/// Gets or sets the monotonicity behavior for ULID generation.
		/// </summary>
		/// <remarks>
		/// This property determines how the timestamp and randomness components
		/// will behave in regard to ordering and predictability:
		/// - NonMonotonic: No monotonic guarantees, randomness is fully independent.
		/// - MonotonicIncrement: Guarantees monotonic ordering by incrementing
		/// the previous timestamp if the same timestamp is generated consecutively.
		/// - MonotonicRandom1Byte to MonotonicRandom4Byte: Ensures monotonicity by introducing
		/// a pseudo-randomized variation in 1 to 4 bytes when timestamps are identical.
		/// </remarks>
		/// <value>
		/// A value of the <see cref="MonotonicityOptions"/> enum that specifies the monotonicity behavior.
		/// Defaults to <see cref="MonotonicityOptions.MonotonicIncrement"/>.
		/// </value>
		public MonotonicityOptions Monotonicity
		{
			get;
			init => field = Enum.IsDefined(typeof(MonotonicityOptions), value)
				? value
				: throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid monotonicity option.");
		} = MonotonicityOptions.MonotonicIncrement;

		/// <summary>
		/// Gets or sets the initial random source used for ULID generation.
		/// </summary>
		/// <remarks>
		/// This property specifies the random number generator to be used for the initial
		/// randomness during ULID creation. By default, a cryptographically secure random
		/// provider is used, ensuring high unpredictability and security for generated IDs.
		/// </remarks>
		/// <value>
		/// An instance of a class that implements the <see cref="IRandomProvider"/> interface
		/// to provide the random number generation logic for the initial randomness component.
		/// Defaults to <see cref="CryptographicallySecureRandomProvider"/>.
		/// </value>
		public IRandomProvider InitialRandomSource { get; init; } = new CryptographicallySecureRandomProvider();

		/// <summary>
		/// Gets or sets the random source used during monotonic ULID generation when
		/// timestamps are identical and incremental randomness is required.
		/// </summary>
		/// <remarks>
		/// Specifies the random provider used to supply entropy for the Random
		/// component when consecutive ULIDs share the same timestamp. It is utilized
		/// in maintaining monotonicity while ensuring random variation in ULID values.
		/// The default provider is <see cref="PseudoRandomProvider"/>, which uses a deterministic
		/// pseudo-random approach.
		/// </remarks>
		/// <value>
		/// An implementation of the <see cref="IRandomProvider"/> interface that provides
		/// randomness for incremental updates. Defaults to an instance of <see cref="PseudoRandomProvider"/>.
		/// </value>
		public IRandomProvider IncrementRandomSource { get; init; } = new PseudoRandomProvider();
	};
}