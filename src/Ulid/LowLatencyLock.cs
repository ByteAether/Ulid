using System.Runtime.CompilerServices;

namespace ByteAether.Ulid;

internal struct LowLatencyLock
{
	internal int LockState;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Enter()
	{
		if (Interlocked.CompareExchange(ref LockState, 1, 0) != 0)
		{
			ContendedEnter(ref LockState);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Exit()
		=> Volatile.Write(ref LockState, 0);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ContendedEnter(ref int lockState)
	{
		// Test-and-test-and-set: spin on a plain read, which keeps the cache line shared,
		// and only attempt the exclusive compare-exchange once the lock looks free.
		var spinner = new SpinWait();
		while (true)
		{
			if (Volatile.Read(ref lockState) == 0 && Interlocked.CompareExchange(ref lockState, 1, 0) == 0)
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
