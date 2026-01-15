namespace ByteAether.Ulid.Tests;

public class UlidBoundariesTests
{
	[Fact]
	public void EmptyUlid_ShouldBeDefault()
	{
		// Arrange
		var ulid = Ulid.Empty;
		var emptyBytes = new byte[16];

		// Assert
		Assert.Equal(default, ulid);
		Assert.Equal(emptyBytes, ulid.AsByteSpan());
	}

	[Fact]
	public void MaxUlid_ShouldHaveAllBytesSetToMax()
	{
		// Arrange
		var ulid = Ulid.Max;
		var expected = Enumerable.Repeat((byte)0xFF, 16).ToArray();

		// Assert
		Assert.Equal(expected, ulid.AsByteSpan());
	}

	[Fact]
	public void MinAt_WithLongTimestamp_ShouldHaveZeroRandomComponent()
	{
		// Arrange
		const long timestamp = 1234567890L;

		// Act
		var ulid = Ulid.MinAt(timestamp);

		// Assert
		// Last 10 bytes (random part) should be 0
		Assert.All(ulid.AsByteSpan()[^10..].ToArray(), x => Assert.Equal(0, x));
		Assert.Equal(timestamp, ulid.Time.ToUnixTimeMilliseconds());
	}

	[Fact]
	public void MinAt_WithDateTimeOffset_ShouldHaveZeroRandomComponent()
	{
		// Arrange
		var dto = DateTimeOffset.UtcNow;

		// Act
		var ulid = Ulid.MinAt(dto);

		// Assert
		// Last 10 bytes (random part) should be 0
		Assert.All(ulid.AsByteSpan()[^10..].ToArray(), x => Assert.Equal(0, x));
		Assert.Equal(dto.ToUnixTimeMilliseconds(), ulid.Time.ToUnixTimeMilliseconds());
	}

	[Fact]
	public void MaxAt_WithLongTimestamp_ShouldHaveMaxRandomComponent()
	{
		// Arrange
		const long timestamp = 1234567890L;

		// Act
		var ulid = Ulid.MaxAt(timestamp);

		// Assert
		// Last 10 bytes (random part) should be 0xFF
		Assert.All(ulid.AsByteSpan()[^10..].ToArray(), x => Assert.Equal(0xFF, x));
		Assert.Equal(timestamp, ulid.Time.ToUnixTimeMilliseconds());
	}

	[Fact]
	public void MaxAt_WithDateTimeOffset_ShouldHaveMaxRandomComponent()
	{
		// Arrange
		var dto = DateTimeOffset.UtcNow;

		// Act
		var ulid = Ulid.MaxAt(dto);

		// Assert
		// Last 10 bytes (random part) should be 0xFF
		Assert.All(ulid.AsByteSpan()[^10..].ToArray(), x => Assert.Equal(0xFF, x));
		Assert.Equal(dto.ToUnixTimeMilliseconds(), ulid.Time.ToUnixTimeMilliseconds());
	}
}