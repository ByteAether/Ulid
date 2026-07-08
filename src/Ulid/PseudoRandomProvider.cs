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
#if NETCOREAPP3_0_OR_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		get => Random.Shared;
	}
#else
	private static readonly Random _rng = new();
#endif

	/// <inheritdoc/>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public void GetBytes(Span<byte> buffer) => _rng.NextBytes(buffer);
}