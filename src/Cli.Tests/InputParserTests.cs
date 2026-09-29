namespace ByteAether.Ulid.Cli.Tests;

public class InputParserTests
{
	[Theory]
	[InlineData("1790683200000", 1790683200000)]
	[InlineData("2026-09-29T12:00:00Z", 1790683200000)]
	[InlineData("2026-09-29T15:00:00+03:00", 1790683200000)]
	public void TryParseTime_TimestampInputs_ReturnsExpectedMs(string input, long expectedMs)
	{
		var success = InputParser.TryParseTime(input, out var actualMs, out var error);

		Assert.True(success);
		Assert.Null(error);
		Assert.Equal(expectedMs, actualMs);
	}

	[Theory]
	[InlineData("definitely not a date")]
	public void TryParseTime_InvalidTimestamp_ReturnsCombinedError(string input)
	{
		var success = InputParser.TryParseTime(input, out _, out var error);

		Assert.False(success);
		Assert.Contains("Invalid timestamp/time bytes", error);
	}

	[Theory]
	[InlineData("-1", "out of range")]
	[InlineData("281474976710656", "out of range")]
	[InlineData("1969-12-31T23:59:59Z", "out of the valid 48-bit ULID range")]
	public void TryParseTime_OutOfRangeTimestamp_PreservesRangeError(string input, string expectedError)
	{
		var success = InputParser.TryParseTime(input, out _, out var error);

		Assert.False(success);
		Assert.Contains(expectedError, error);
	}

	[Theory]
	[InlineData("01A0ED093A00")]
	[InlineData("0x01A0ED093A00")]
	[InlineData("01-A0-ED-09-3A-00")]
	[InlineData("01 A0 ED 09 3A 00")]
	[InlineData("01M3PGJEG0")] // Crockford 10 chars
	public void TryParseTimeBytes_ValidInputs_Returns6Bytes(string input)
	{
		byte[] expected = [0x01, 0xA0, 0xED, 0x09, 0x3A, 0x00];

		var success = InputParser.TryParseTimeBytes(input, out var timeBytes, out var error);

		Assert.True(success);
		Assert.Null(error);
		Assert.NotNull(timeBytes);
		Assert.Equal(expected, timeBytes);
	}

	[Theory]
	[InlineData("5D6762477ACBF9130587")]
	[InlineData("0x5D6762477ACBF9130587")]
	[InlineData("5D-67-62-47-7A-CB-F9-13-05-87")]
	[InlineData("BNKP4HVTSFWH61C7")] // Crockford 16 chars
	public void TryParseRandomBytes_ValidInputs_Returns10Bytes(string input)
	{
		byte[] expected = [0x5D, 0x67, 0x62, 0x47, 0x7A, 0xCB, 0xF9, 0x13, 0x05, 0x87];

		var success = InputParser.TryParseRandomBytes(input, out var randomBytes, out var error);

		Assert.True(success);
		Assert.Null(error);
		Assert.NotNull(randomBytes);
		Assert.Equal(expected, randomBytes);
	}

	[Theory]
	[InlineData("AAAAAAAA")] // Too short (4 bytes)
	[InlineData("AAAAAAAAAAAAAA")] // Too long (7 bytes)
	[InlineData("INVALID_HEX")]
	public void TryParseTimeBytes_InvalidInputs_ReturnsFalse(string input)
	{
		var success = InputParser.TryParseTimeBytes(input, out _, out var error);

		Assert.False(success);
		Assert.NotNull(error);
	}

	[Theory]
	[InlineData("5D6762477A")] // Too short (5 bytes)
	[InlineData("5D6762477ACBF913058700")] // Too long (11 bytes)
	[InlineData("INVALID_RANDOM")]
	public void TryParseRandomBytes_InvalidInputs_ReturnsFalse(string input)
	{
		var success = InputParser.TryParseRandomBytes(input, out _, out var error);

		Assert.False(success);
		Assert.NotNull(error);
	}
}