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
#if !NETCOREAPP3_0_OR_GREATER
	private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
#endif

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void GetBytes(Span<byte> buffer)
	{
#if NETCOREAPP3_0_OR_GREATER
		RandomNumberGenerator.Fill(buffer);
#else
		_rng.GetBytes(buffer);
#endif
	}
}