using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace ByteAether.Ulid;

/// <summary>
/// Provides cryptographically secure random number generation functionality.<br/>
/// Implements the <see cref="IRandomProvider"/> interface to generate random bytes
/// securely using a system-provided implementation of the <see cref="RandomNumberGenerator"/>.
/// </summary>
public readonly struct CryptographicallySecureRandomProvider : IRandomProvider
{
	private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

	/// <inheritdoc/>
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	public void GetBytes(Span<byte> buffer) => _rng.GetBytes(buffer);
}