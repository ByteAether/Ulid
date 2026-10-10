using System.Buffers.Binary;
using System.Runtime.CompilerServices;
#if !NETSTANDARD2_0
using System.Runtime.InteropServices;
#endif

namespace ByteAether.Ulid;

public readonly partial struct Ulid
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong ReverseOnLittleEndian(ulong value)
		=> BitConverter.IsLittleEndian
			? BinaryPrimitives.ReverseEndianness(value)
			: value;

	// On .NET Standard 2.0 the span is not tracked by the GC, so it may only point to stack memory.
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Span<byte> CreateSpan(ref byte reference, int length)
#if !NETSTANDARD2_0
		=> MemoryMarshal.CreateSpan(ref reference, length);
#else
		=> Compatibility.MemoryMarshal.CreateSpan(ref reference, length);
#endif
}