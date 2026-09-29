namespace ByteAether.Ulid.Cli.Tests;

public class GeneratorTests
{
	[Fact]
	public void Generate_Default_CreatesValidUlid()
	{
		var ulid = Generator.Generate();
		Assert.NotEqual(default, ulid);
		Assert.Equal(26, ulid.ToString().Length);
	}

	[Fact]
	public void Generate_WithTimestamp_CreatesUlidWithSpecifiedTime()
	{
		const long expectedMs = 1790683200000; // 2026-09-29T12:00:00Z
		var ulid = Generator.Generate(timestamp: expectedMs, randomBytes: new byte[10]);

		Assert.Equal(expectedMs, ulid.Time.ToUnixTimeMilliseconds());
	}

	[Fact]
	public void Generate_WithMaximumTimestamp_CreatesUlid()
	{
		var ulid = Generator.Generate(timestamp: InputParser.MaxTimestampMs, randomBytes: new byte[10]);
		var result = Inspector.Inspect(ulid);

		Assert.StartsWith("FFFFFFFFFFFF", Convert.ToHexString(ulid.ToByteArray()));
		Assert.Equal(InputParser.MaxTimestampMs, result.Timestamp);
		Assert.Equal("N/A", result.TimeIso);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(281474976710656)]
	public void Generate_WithOutOfRangeTimestamp_ThrowsArgumentOutOfRangeException(long timestamp)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => Generator.Generate(timestamp));
	}

	[Fact]
	public void Generate_WithRandomBytes_CreatesUlidWithExactRandomBytes()
	{
		byte[] randomBytes = [0x5D, 0x67, 0x62, 0x47, 0x7A, 0xCB, 0xF9, 0x13, 0x05, 0x87];
		var ulid = Generator.Generate(randomBytes: randomBytes);

		Assert.True(randomBytes.AsSpan().SequenceEqual(ulid.Random));
	}

	[Fact]
	public void Generate_WithDateTimeOffset_CreatesUlidWithSpecifiedTime()
	{
		var ulid = Generator.Generate(1790683200000, new byte[10]);

		Assert.Equal(1790683200000, ulid.Time.ToUnixTimeMilliseconds());
	}

	[Fact]
	public void Generate_WithInvalidRandomBytesLength_ThrowsArgumentException()
	{
		byte[] invalidLength = [0x01, 0x02, 0x03];
		Assert.Throws<ArgumentException>(() => Generator.Generate(randomBytes: invalidLength));
	}
}