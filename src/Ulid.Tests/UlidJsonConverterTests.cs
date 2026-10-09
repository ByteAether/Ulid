#if NETCOREAPP
using System.Text.Json;

namespace ByteAether.Ulid.Tests;

public class UlidJsonConverterTests
{
	private class TestDto
	{
		public Ulid UlidProperty { get; init; }
	}

	private static JsonSerializerOptions _jsonOptions => new()
	{
		Converters = { new UlidJsonConverter() }
	};

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Serialize_UlidToJsonString(bool useCustomOptions)
	{
		// Arrange
		var ulid = Ulid.New();
		var options = useCustomOptions ? _jsonOptions : null;

		// Act
		var jsonString = JsonSerializer.Serialize(ulid, options);

		// Assert
		Assert.Equal($"\"{ulid}\"", jsonString);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Deserialize_JsonStringToUlid(bool useCustomOptions)
	{
		// Arrange
		var ulid = Ulid.New();
		var jsonString = $"\"{ulid}\"";
		var options = useCustomOptions ? _jsonOptions : null;

		// Act
		var deserializedUlid = JsonSerializer.Deserialize<Ulid>(jsonString, options);

		// Assert
		Assert.Equal(ulid, deserializedUlid);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Deserialize_BadUlidString_ThrowsJsonException(bool useCustomOptions)
	{
		// Arrange
		var ulid = Ulid.New();
		var invalidJsonString = $"\"{ulid.ToString()[1..]}\""; // Remove the first character to make it invalid
		var options = useCustomOptions ? _jsonOptions : null;

		// Act & Assert
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Ulid>(invalidJsonString, options));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Deserialize_EscapedJsonStringToUlid(bool useCustomOptions)
	{
		// Arrange
		var ulid = Ulid.New();
		var ulidString = ulid.ToString();
		var escapedJsonString = $"\"\\u{(int)ulidString[0]:X4}{ulidString[1..^1]}\\u{(int)ulidString[^1]:x4}\"";
		var options = useCustomOptions ? _jsonOptions : null;

		// Act
		var deserializedUlid = JsonSerializer.Deserialize<Ulid>(escapedJsonString, options);

		// Assert
		Assert.Equal(ulid, deserializedUlid);
	}

	[Theory]
	[InlineData("\"\\u0030\"")]
	[InlineData("\"01AN4Z07BY79KA1307SR9X4MV\\u0021\"")]
	[InlineData("\"\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\\u0030\"")]
	public void Deserialize_BadEscapedJsonString_ThrowsJsonException(string invalidJsonString)
	{
		// Act & Assert
		Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Ulid>(invalidJsonString, _jsonOptions));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Read_SegmentedJsonString_ShouldReturnUlid(bool escaped)
	{
		// Arrange
		var ulid = Ulid.New();
		var ulidString = ulid.ToString();
		var json = escaped
			? $"\"\\u{(int)ulidString[0]:X4}{ulidString[1..]}\""
			: $"\"{ulidString}\"";
		var bytes = System.Text.Encoding.UTF8.GetBytes(json);

		// Split the value into two segments to exercise the ValueSequence path
		var first = new Segment(bytes.AsMemory(0, 10));
		var last = first.Append(bytes.AsMemory(10));
		var reader = new Utf8JsonReader(new System.Buffers.ReadOnlySequence<byte>(first, 0, last, last.Memory.Length));
		reader.Read();

		// Act
		var result = new UlidJsonConverter().Read(ref reader, typeof(Ulid), _jsonOptions);

		// Assert
		Assert.Equal(ulid, result);
	}

	private sealed class Segment : System.Buffers.ReadOnlySequenceSegment<byte>
	{
		public Segment(ReadOnlyMemory<byte> memory) => Memory = memory;

		public Segment Append(ReadOnlyMemory<byte> memory)
		{
			var next = new Segment(memory) { RunningIndex = RunningIndex + Memory.Length };
			Next = next;
			return next;
		}
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Serialize_DtoToJson(bool useCustomOptions)
	{
		// Arrange
		var dto = new TestDto { UlidProperty = Ulid.New() };
		var expectedJson = $"{{\"UlidProperty\":\"{dto.UlidProperty}\"}}";
		var options = useCustomOptions ? _jsonOptions : null;

		// Act
		var resultJson = JsonSerializer.Serialize(dto, options);

		// Assert
		Assert.Equal(expectedJson, resultJson);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Deserialize_GoodJsonToDto(bool useCustomOptions)
	{
		// Arrange
		var ulid = Ulid.New();
		var jsonString = $"{{\"UlidProperty\":\"{ulid}\"}}";
		var options = useCustomOptions ? _jsonOptions : null;

		// Act
		var resultDto = JsonSerializer.Deserialize<TestDto>(jsonString, options);

		// Assert
		Assert.NotNull(resultDto);
		Assert.Equal(ulid, resultDto.UlidProperty);
	}

#if NET6_0_OR_GREATER
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Serialize_UlidAsPropertyName(bool useCustomOptions)
	{
		// Arrange
		var ulid = Ulid.New();
		var dictionary = new Dictionary<Ulid, string>
		{
			{ ulid, "value" }
		};
		var options = useCustomOptions ? _jsonOptions : null;

		// Act
		var jsonString = JsonSerializer.Serialize(dictionary, options);

		// Assert
		Assert.Contains($"\"{ulid}\":\"value\"", jsonString);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Deserialize_UlidAsPropertyName(bool useCustomOptions)
	{
		// Arrange
		var ulid = Ulid.New();
		var jsonString = $"{{\"{ulid}\":\"value\"}}";
		var options = useCustomOptions ? _jsonOptions : null;

		// Act
		var deserializedDictionary = JsonSerializer.Deserialize<Dictionary<Ulid, string>>(jsonString, options);

		// Assert
		Assert.NotNull(deserializedDictionary);
		Assert.True(deserializedDictionary.ContainsKey(ulid));
		Assert.Equal("value", deserializedDictionary[ulid]);
	}
#endif
}
#endif