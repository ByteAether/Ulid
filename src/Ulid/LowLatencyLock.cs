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
