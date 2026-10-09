using System.Runtime.CompilerServices;

namespace ByteAether.Ulid;

internal struct LowLatencyLock
{
	internal int LockState;

#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	internal void Enter()
	{
		if (Interlocked.CompareExchange(ref LockState, 1, 0) != 0)
		{
			ContendedEnter(ref LockState);
		}
	}

#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
	internal void Exit()
		=> Volatile.Write(ref LockState, 0);

#if NET5_0_OR_GREATER
	[SkipLocalsInit]
#endif
#if NETCOREAPP3_0_OR_GREATER
	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
#else
	[MethodImpl(MethodImplOptions.NoInlining)]
#endif
	private static void ContendedEnter(ref int lockState)
	{
		var spinner = new SpinWait();
		while (true)
		{
			if (Interlocked.CompareExchange(ref lockState, 1, 0) == 0)
			{
				return;
			}

#if NET5_0_OR_GREATER
			spinner.SpinOnce(-1);
#else
			spinner.SpinOnce();
#endif
		}
	}
}
