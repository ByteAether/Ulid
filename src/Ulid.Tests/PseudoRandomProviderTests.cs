namespace ByteAether.Ulid.Tests;

public class PseudoRandomProviderTests
{
	[Fact]
	public void GetBytes_ShouldNotRepeatValuesWhenCalledConcurrently()
	{
		// Arrange
		const int threadCount = 8;
		const int perThread = 100_000;
		var options = new Ulid.GenerationOptions
		{
			Monotonicity = Ulid.GenerationOptions.MonotonicityOptions.NonMonotonic,
			InitialRandomSource = new PseudoRandomProvider(),
		};
		var generated = new Ulid[threadCount * perThread];
		using var barrier = new Barrier(threadCount);

		// Act
		// The timestamp is fixed, so only the random part can differ. Dedicated threads start together for maximum contention.
		var threads = Enumerable.Range(0, threadCount)
			.Select(t => new Thread(() =>
			{
				// ReSharper disable once AccessToDisposedClosure
				barrier.SignalAndWait();
				for (var i = t * perThread; i < (t + 1) * perThread; i++)
				{
					generated[i] = Ulid.New(0, options);
				}
			}))
			.ToArray();

		foreach (var thread in threads)
		{
			thread.Start();
		}

		foreach (var thread in threads)
		{
			thread.Join();
		}

		// Assert
		Assert.Equal(generated.Length, generated.Distinct().Count());
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(7)]
	[InlineData(8)]
	[InlineData(9)]
	[InlineData(10)]
	[InlineData(16)]
	[InlineData(33)]
	[InlineData(64)]
	public void GetBytes_ShouldFillExactlyTheRequestedLength(int length)
	{
		// Arrange
		const byte sentinel = 0xAA;
		var provider = new PseudoRandomProvider();
		var buffer = new byte[length + 2];
		Array.Fill(buffer, sentinel);

		// Act
		provider.GetBytes(buffer.AsSpan(1, length));

		// Assert
		Assert.Equal(sentinel, buffer[0]);
		Assert.Equal(sentinel, buffer[^1]);
		if (length >= 8)
		{
			Assert.Contains(buffer.AsSpan(1, length).ToArray(), b => b != sentinel);
		}
	}

	[Fact]
	public void GetBytes_ShouldProduceDifferentValuesOnConsecutiveCalls()
	{
		// Arrange
		var provider = new PseudoRandomProvider();
		var first = new byte[32];
		var second = new byte[32];

		// Act
		provider.GetBytes(first);
		provider.GetBytes(second);

		// Assert
		Assert.NotEqual(first, second);
	}
}
