using System.Buffers.Binary;
using System.Runtime.CompilerServices;
#if NETCOREAPP
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Span<byte> CreateSpan(ref byte reference, int length)
#if NETCOREAPP
		=> MemoryMarshal.CreateSpan(ref reference, length);
#else
		=> Compatibility.MemoryMarshal.CreateSpan(ref reference, length);
#endif
}