using System.Text.Json;

namespace ByteAether.Ulid.Cli.Tests;

public class InspectorTests
{
	[Fact]
	public void Inspect_CanonicalUlid_ExtractsAllComponentsCorrectly()
	{
		const string ulidString = "01M3PGJEG0BNKP4HVTSFWH61C7";
		var success = Inspector.TryInspect(ulidString, out var result, out _);
		Assert.True(success);

		Assert.Equal(ulidString, result!.Ulid);
		Assert.Equal("01A0ED093A005D6762477ACBF9130587", result.Hex);
		Assert.Equal("01M3PGJEG0", result.Time);
		Assert.Equal("01A0ED093A00", result.TimeHex);
		Assert.Equal("BNKP4HVTSFWH61C7", result.Random);
		Assert.Equal("5D6762477ACBF9130587", result.RandomHex);
		Assert.Equal(1790683200000, result.Timestamp);
		Assert.Equal("2026-09-29T12:00:00.000Z", result.TimeIso);
	}

	[Theory]
	[InlineData(253402300799999, "9999-12-31T23:59:59.999Z")]
	[InlineData(253402300800000, "N/A")]
	public void Inspect_TimestampBoundary_FormatsIsoTimestamp(long timestamp, string expectedIso)
	{
		var result = Inspector.Inspect(Ulid.New(timestamp, new byte[10]));

		Assert.Equal(expectedIso, result.TimeIso);
	}

	[Fact]
	public void Inspect_HexRepresentation_ExtractsAllComponentsCorrectly()
	{
		// 16 bytes = 01A0ED093A00 + 5D6762477ACBF9130587 = 32 hex chars
		const string hex = "01A0ED093A005D6762477ACBF9130587";
		//var result = Inspector.Inspect(hex);
		var success = Inspector.TryInspect(hex, out var result, out _);
		Assert.True(success);

		Assert.Equal("01M3PGJEG0BNKP4HVTSFWH61C7", result!.Ulid);
		Assert.Equal(hex, result.Hex);
		Assert.Equal("01M3PGJEG0", result.Time);
		Assert.Equal("01A0ED093A00", result.TimeHex);
		Assert.Equal("BNKP4HVTSFWH61C7", result.Random);
		Assert.Equal("5D6762477ACBF9130587", result.RandomHex);
	}

	[Fact]
	public void TryInspect_InvalidString_ReturnsFalseWithErrorMessage()
	{
		var success = Inspector.TryInspect("invalid_not_an_ulid", out var result, out var message);
		Assert.False(success);

		Assert.Null(result);
		Assert.NotNull(message);
	}

	[Fact]
	public void ToGreppableText_ContainsAllRequiredLabels()
	{
		var success = Inspector.TryInspect("01M3PGJEG0BNKP4HVTSFWH61C7", out var result, out _);
		Assert.True(success);

		var text = result!.ToGreppableText();

		Assert.Contains("Ulid:                01M3PGJEG0BNKP4HVTSFWH61C7", text);
		Assert.Contains("Hex:                 01A0ED093A005D6762477ACBF9130587", text);
		Assert.Contains("Time (ISO 8601):     2026-09-29T12:00:00.000Z", text);
		Assert.Contains("Timestamp (Unix ms): 1790683200000", text);
		Assert.Contains("Time:                01M3PGJEG0", text);
		Assert.Contains("Time (Hex):          01A0ED093A00", text);
		Assert.Contains("Random:              BNKP4HVTSFWH61C7", text);
		Assert.Contains("Random (Hex):        5D6762477ACBF9130587", text);
	}

	[Fact]
	public void ToJson_ProducesValidDeserializableJson()
	{
		var success = Inspector.TryInspect("01M3PGJEG0BNKP4HVTSFWH61C7", out var result, out _);
		Assert.True(success);

		var json = JsonSerializer.Serialize(result);

		using var doc = JsonDocument.Parse(json);
		var root = doc.RootElement;

		Assert.Equal("01M3PGJEG0BNKP4HVTSFWH61C7", root.GetProperty("ulid").GetString());
		Assert.Equal("01A0ED093A005D6762477ACBF9130587", root.GetProperty("hex").GetString());
		Assert.Equal("01M3PGJEG0", root.GetProperty("time").GetString());
		Assert.Equal("01A0ED093A00", root.GetProperty("timeHex").GetString());
		Assert.Equal(1790683200000, root.GetProperty("timestamp").GetInt64());
		Assert.Equal("2026-09-29T12:00:00.000Z", root.GetProperty("timeIso").GetString());
		Assert.Equal("BNKP4HVTSFWH61C7", root.GetProperty("random").GetString());
		Assert.Equal("5D6762477ACBF9130587", root.GetProperty("randomHex").GetString());
	}

	[Theory]
	[InlineData("time", "01M3PGJEG0")]
	[InlineData("timeHex", "01A0ED093A00")]
	[InlineData("random", "BNKP4HVTSFWH61C7")]
	[InlineData("randomHex", "5D6762477ACBF9130587")]
	[InlineData("timestamp", "1790683200000")]
	[InlineData("timeIso", "2026-09-29T12:00:00.000Z")]
	[InlineData("hex", "01A0ED093A005D6762477ACBF9130587")]
	[InlineData("ulid", "01M3PGJEG0BNKP4HVTSFWH61C7")]
	public void TryGetPart_ReturnsExpectedValues(string partName, string expectedValue)
	{
		var success = Inspector.TryInspect("01M3PGJEG0BNKP4HVTSFWH61C7", out var result, out _);
		Assert.True(success);

		var found = result!.TryGetPart(partName, out var value);

		Assert.True(found);
		Assert.Equal(expectedValue, value);
	}

	[Fact]
	public void TryGetPart_InvalidPartName_ReturnsFalse()
	{
		var success = Inspector.TryInspect("01M3PGJEG0BNKP4HVTSFWH61C7", out var result, out _);
		Assert.True(success);

		var found = result!.TryGetPart("nonexistent_property", out var value);

		Assert.False(found);
		Assert.Null(value);
	}
}