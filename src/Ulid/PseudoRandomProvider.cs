using System.Runtime.CompilerServices;
#if !NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace ByteAether.Ulid;

/// <summary>
/// Provides a pseudo-random number generator implementation for the
/// <see cref="IRandomProvider"/> interface using <see cref="System.Random"/>.
/// </summary>
/// <remarks>
/// This provider is thread-safe. It uses the shared instance of <see cref="System.Random"/>
/// where available, and a separate instance for each thread on older targets.
/// </remarks>
public readonly struct PseudoRandomProvider : IRandomProvider
{
#if NET6_0_OR_GREATER
	private static Random _rng
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => Random.Shared;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void GetBytes(Span<byte> buffer) => _rng.NextBytes(buffer);
#else
	[ThreadStatic]
	private static Random? _threadRng;

	/// <inheritdoc/>
	public void GetBytes(Span<byte> buffer)
	{
		var rng = _threadRng ?? CreateThreadRng();
#if NETSTANDARD2_0
		// Random.NextBytes accepts only an array on this target and always fills all of it.
		// Each Random.Next() call yields 31 random bits, of which 3 whole bytes are used.
		for (var i = 0; i < buffer.Length;)
		{
			var value = rng.Next();
			for (var j = 0; j < 3 && i < buffer.Length; j++, i++, value >>= 8)
			{
				buffer[i] = (byte)value;
			}
		}
#else
		rng.NextBytes(buffer);
#endif
	}

	// The seed comes from the cryptographically secure source: threads started within the same clock tick would
	// otherwise get the same time-dependent default seed of System.Random on .NET Framework.
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static Random CreateThreadRng()
	{
		Span<byte> seed = stackalloc byte[sizeof(int)];
		new CryptographicallySecureRandomProvider().GetBytes(seed);

		return _threadRng = new(MemoryMarshal.Read<int>(seed));
	}
#endif
}
