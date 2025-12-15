#if NETSTANDARD
using System.Runtime.CompilerServices;

// ReSharper disable All
#pragma warning disable

// https://github.com/dotnet/runtime/blob/8d796d8e60a5236cbd5f113ead1d3831064cdba1/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/MemoryMarshal.cs#L226
// ref T GetArrayDataReference<T>(T[] array) is original.

namespace ByteAether.Ulid.Compatibility;

public static class MemoryMarshal
{
    /// <summary>
    /// Returns a reference to the 0th element of <paramref name="array"/>. If the array is empty, returns a reference to where the 0th element
    /// would have been stored. Such a reference may be used for pinning but must never be dereferenced.
    /// </summary>
    /// <exception cref="NullReferenceException"><paramref name="array"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method does not perform array variance checks. The caller must manually perform any array variance checks
    /// if the caller wishes to write to the returned reference.
    /// </remarks>
    public static unsafe ref T GetArrayDataReference<T>(T[] array)
		where T : unmanaged
	{
		fixed (T* ptr = array)
		{
			return ref Unsafe.AsRef<T>(ptr);
		}
	}

    /// <summary>
    /// Creates a new span over a portion of a regular managed object. This can be useful
    /// if part of a managed object represents a "fixed array." This is dangerous because the
    /// <paramref name="length"/> is not checked.
    /// </summary>
    /// <param name="reference">A reference to data.</param>
    /// <param name="length">The number of <typeparamref name="T"/> elements the memory contains.</param>
    /// <returns>A span representing the specified reference and length.</returns>
    /// <remarks>
    /// This method should be used with caution. It is dangerous because the length argument is not checked.
    /// Even though the ref is annotated as scoped, it will be stored into the returned span, and the lifetime
    /// of the returned span will not be validated for safety, even by span-aware languages.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static Span<T> CreateSpan<T>(scoped ref T reference, int length) =>
        new Span<T>(Unsafe.AsPointer(ref reference), length);

    /// <summary>
    /// Creates a new read-only span over a portion of a regular managed object. This can be useful
    /// if part of a managed object represents a "fixed array." This is dangerous because the
    /// <paramref name="length"/> is not checked.
    /// </summary>
    /// <param name="reference">A reference to data.</param>
    /// <param name="length">The number of <typeparamref name="T"/> elements the memory contains.</param>
    /// <returns>A read-only span representing the specified reference and length.</returns>
    /// <remarks>
    /// This method should be used with caution. It is dangerous because the length argument is not checked.
    /// Even though the ref is annotated as scoped, it will be stored into the returned span, and the lifetime
    /// of the returned span will not be validated for safety, even by span-aware languages.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static ReadOnlySpan<T> CreateReadOnlySpan<T>(scoped ref T reference, int length) =>
        new ReadOnlySpan<T>(Unsafe.AsPointer(ref reference), length);
}
#endif