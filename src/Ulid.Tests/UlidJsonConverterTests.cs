#if NETCOREAPP
using System.Text.Json;

namespace ByteAether.Ulid.Tests;

public class UlidJsonConverterTests
{
	private class TestDto
	{
		public Ulid UlidProperty { get; set; }
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
}
#endif
