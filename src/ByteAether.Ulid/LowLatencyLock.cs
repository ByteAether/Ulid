using System.Runtime.CompilerServices;
#if !NET7_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace ByteAether.Ulid;

internal struct LowLatencyLock
{
    internal int LockState;

    internal readonly ref struct Scope
    {
#if NET7_0_OR_GREATER
	    private readonly ref int _lockState;

	    [SkipLocalsInit]
	    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	    internal Scope(ref LowLatencyLock @lock)
	    {
		    _lockState = ref @lock.LockState;
	    }

	    [SkipLocalsInit]
	    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	    public void Dispose()
	    {
		    Volatile.Write(ref _lockState, 0);
	    }
#else
        // This is fully supported in .NET Standard 2.0 / .NET 5.0.
        private readonly Span<int> _lockState;

#if NET5_0_OR_GREATER
        [SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal Scope(ref LowLatencyLock @lock)
        {
            _lockState =
#if NETCOREAPP
				MemoryMarshal.CreateSpan(ref @lock.LockState, 1);
#else
				Compatibility.MemoryMarshal.CreateSpan(ref @lock.LockState, 1);
#endif
	        ;
        }

#if NET5_0_OR_GREATER
        [SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Dispose()
        {
            Volatile.Write(ref MemoryMarshal.GetReference(_lockState), 0);
        }
#endif

    }
}

internal static class LowLatencyLockExtensions
{
	private const int _spinWaitAttemptCount = 8;
	private const int _spinWaitIterationCount = 8;

#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static LowLatencyLock.Scope Enter(ref this LowLatencyLock @lock)
    {
        if (Interlocked.CompareExchange(ref @lock.LockState, 1, 0) != 0)
        {
            ContendedEnter(ref @lock);
        }

        return new(ref @lock);
    }

#if NET5_0_OR_GREATER
    [SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.NoInlining)]
#endif
    private static void ContendedEnter(ref LowLatencyLock @lock)
    {
	    // First we do Thread.SpinWait() for limited amount of times
	    var remainingAttempts = _spinWaitAttemptCount;
	    do
	    {
		    Thread.SpinWait(_spinWaitIterationCount);

		    // Test-and-Test-and-Set (TTAS)
		    // Initial read-only check keeps the cache line in the "Shared" state
		    if (@lock.LockState == 0 && Interlocked.CompareExchange(ref @lock.LockState, 1, 0) == 0)
		    {
			    return;
		    }
	    } while (--remainingAttempts > 0);

        // Later we do Thread.Yield()
        do
        {
	        Thread.Yield();
        } while (@lock.LockState != 0 || Interlocked.CompareExchange(ref @lock.LockState, 1, 0) != 0);
    }
}