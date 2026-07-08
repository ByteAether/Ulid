#if NETSTANDARD
using System.Runtime.CompilerServices;

// ReSharper disable All
#pragma warning disable

// https://github.com/dotnet/runtime/blob/8d796d8e60a5236cbd5f113ead1d3831064cdba1/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/MemoryMarshal.cs#L226

namespace Compatibility;

internal static class MemoryMarshal
{
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
}
#endif