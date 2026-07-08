#if NETSTANDARD2_0
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

// ReSharper disable once CheckNamespace
namespace ByteAether.Ulid;

internal static class RandomNumberGeneratorExtensions
{
	// In NetStandard 2.0, RandomNumberGenerator.GetBytes() does not support Span<byte> overloads.
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetBytes(this RandomNumberGenerator rng, Span<byte> buffer)
	{
		var rndInc = ArrayPool<byte>.Shared.Rent(buffer.Length);
		rng.GetBytes(rndInc, 0, buffer.Length);
		new ReadOnlySpan<byte>(rndInc, 0, buffer.Length).CopyTo(buffer);
	}

	// In NetStandard 2.0, Random.NextBytes() does not support Span<byte> overloads.
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void NextBytes(this Random rng, Span<byte> buffer)
	{
		var rndInc = ArrayPool<byte>.Shared.Rent(buffer.Length);
		rng.NextBytes(rndInc);
		new ReadOnlySpan<byte>(rndInc, 0, buffer.Length).CopyTo(buffer);
	}
}
#endif