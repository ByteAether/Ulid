using System.Text.Json;

namespace ByteAether.Ulid.AspNetCore.IntegrationTests.OpenApi;

internal static class OpenApiSchemaAssertions
{
	public static void AssertUlidSchemas(string document)
	{
		using var json = JsonDocument.Parse(document);
		var schemas = new List<JsonElement>();
		FindUlidSchemas(json.RootElement, schemas);

		Assert.NotEmpty(schemas);
		foreach (var schema in schemas)
		{
			Assert.Equal("string", schema.GetProperty("type").GetString());
			Assert.Equal("ulid", schema.GetProperty("format").GetString());
			Assert.Equal(Ulid.UlidStringLength, schema.GetProperty("minLength").GetInt32());
			Assert.Equal(Ulid.UlidStringLength, schema.GetProperty("maxLength").GetInt32());
			Assert.False(schema.TryGetProperty("properties", out _));
			Assert.False(schema.TryGetProperty("items", out _));
			Assert.False(schema.TryGetProperty("required", out _));
		}
	}

	private static void FindUlidSchemas(JsonElement element, List<JsonElement> schemas)
	{
		if (element.ValueKind == JsonValueKind.Object)
		{
			if (element.TryGetProperty("format", out var format) && format.GetString() == "ulid")
			{
				schemas.Add(element);
			}

			foreach (var property in element.EnumerateObject())
			{
				FindUlidSchemas(property.Value, schemas);
			}
		}
		else if (element.ValueKind == JsonValueKind.Array)
		{
			foreach (var item in element.EnumerateArray())
			{
				FindUlidSchemas(item, schemas);
			}
		}
	}
}
