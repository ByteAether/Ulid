using System.Runtime.CompilerServices;

namespace ByteAether.Ulid;

/// <summary>
/// Provides a pseudo-random number generator implementation for the
/// <see cref="IRandomProvider"/> interface using the shared instance
/// of <see cref="System.Random"/>.
/// </summary>
public readonly struct PseudoRandomProvider : IRandomProvider
{
#if NET6_0_OR_GREATER
	private static Random _rng
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Random.Shared;
	}
#else
	private static readonly Random _rng = new();
#endif

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void GetBytes(Span<byte> buffer) => _rng.NextBytes(buffer);
}