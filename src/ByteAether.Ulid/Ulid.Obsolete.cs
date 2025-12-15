using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ByteAether.Ulid;

public readonly partial struct Ulid
{
	/// <summary>
	/// Whether <see cref="Ulid"/>s should be generated in a monotonic manner by default.<br />
	/// Initial value is set to <c>true</c>.<br/>
	/// <b>This setting applies globally without any scoping.</b>
	/// </summary>
	/// <remarks>
	/// When set to <c>true</c> (default), <see cref="Ulid"/>s generated without explicitly specifying monotonicity
	/// will ensure that they are monotonically increasing.<br />
	/// When set to <c>false</c>, <see cref="Ulid"/>s generated without explicitly specifying monotonicity will be
	/// generated with random <see cref="Random" /> value.
	/// </remarks>
	[Obsolete("Use DefaultGenerationOptions instead.")]
	public static bool DefaultIsMonotonic
	{
		get => ObsoleteHelper.GetByGenerationOptions(DefaultGenerationOptions);
		set => DefaultGenerationOptions = ObsoleteHelper.GetByBoolean(value);
	}

	/// <summary>
	/// Creates a new <see cref="Ulid"/> with the current timestamp.
	/// </summary>
	/// <param name="isMonotonic">
	/// If <c>true</c>, ensures the <see cref="Ulid"/> is monotonically increasing.<br />
	/// If <c>false</c>, generates a random <see cref="Random" /> part in <see cref="Ulid"/>.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Obsolete("Use method with DefaultGenerationOptions argument instead.")]
	public static Ulid New(bool isMonotonic)
		=> New(ObsoleteHelper.GetByBoolean(isMonotonic));

	/// <summary>
	/// Creates a new <see cref="Ulid"/> with the specified timestamp.
	/// </summary>
	/// <param name="dateTimeOffset">The timestamp to use for the <see cref="Ulid"/>.</param>
	/// <param name="isMonotonic">
	/// If <c>true</c>, ensures the <see cref="Ulid"/> is monotonically increasing.<br />
	/// If <c>false</c>, generates a random <see cref="Random" /> part in <see cref="Ulid"/>.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Obsolete("Use method with DefaultGenerationOptions argument instead.")]
	public static Ulid New(DateTimeOffset dateTimeOffset, bool isMonotonic)
		=> New(dateTimeOffset, ObsoleteHelper.GetByBoolean(isMonotonic));

	/// <summary>
	/// Creates a new <see cref="Ulid"/> with the specified timestamp in milliseconds.
	/// </summary>
	/// <param name="timestamp">The timestamp in milliseconds to use for the <see cref="Ulid"/>.</param>
	/// <param name="isMonotonic">
	/// If <c>true</c>, ensures the <see cref="Ulid"/> is monotonically increasing.<br />
	/// If <c>false</c>, generates a random <see cref="Random" /> part in <see cref="Ulid"/>.
	/// </param>
	/// <returns>A new <see cref="Ulid"/> instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Obsolete("Use method with DefaultGenerationOptions argument instead.")]
	public static Ulid New(long timestamp, bool isMonotonic)
		=> New(timestamp, ObsoleteHelper.GetByBoolean(isMonotonic));
}

internal static class ObsoleteHelper
{
	private static readonly Ulid.GenerationOptions _monotonicDefaultOptions = new();

	private static readonly Ulid.GenerationOptions _nonmonotonicDefaultOptions = new()
	{
		InitialRandomSource = new CryptographicallySecureRandomProvider(),
		IncrementRandomSource = new PseudoRandomProvider(), // This has no effect
		Monotonicity = Ulid.GenerationOptions.MonotonicityOptions.NonMonotonic
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[return: NotNullIfNotNull(nameof(isMonotonic))]
	public static Ulid.GenerationOptions? GetByBoolean(bool? isMonotonic)
		=> isMonotonic switch
		{
			true => _monotonicDefaultOptions,
			false => _nonmonotonicDefaultOptions,
			_ => null
		};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool GetByGenerationOptions(Ulid.GenerationOptions options)
		=> options.Monotonicity != Ulid.GenerationOptions.MonotonicityOptions.NonMonotonic;
}