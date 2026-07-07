using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace ByteAether.Ulid;

public readonly partial struct Ulid
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong ReverseOnLittleEndian(ulong value)
		=> BitConverter.IsLittleEndian
			? BinaryPrimitives.ReverseEndianness(value)
			: value;
}