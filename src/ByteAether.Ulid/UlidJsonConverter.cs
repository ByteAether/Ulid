#if NETCOREAPP3_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ByteAether.Ulid;

/// <summary>
/// A custom JSON converter for the <see cref="Ulid"/> type.
/// </summary>
public class UlidJsonConverter : JsonConverter<Ulid>
{
	/// <summary>
	/// Reads and converts the JSON representation of the object.
	/// </summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">The serializer options.</param>
	/// <returns>The converted <see cref="Ulid"/> object.</returns>
	/// <exception cref="ArgumentException">Thrown when the JSON value is null.</exception>
	/// <exception cref="JsonException">Thrown when the JSON value is not a valid ULID.</exception>
	public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var json = reader.GetString() ?? throw new ArgumentException($"The JSON value is null.", nameof(reader));

		try
		{
			return Ulid.Parse(json);
		}
		catch (FormatException ex)
		{
			throw new JsonException($"'{json}' is not a valid ULID.", ex);
		}
	}

	/// <summary>
	/// Writes the JSON representation of the object.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value to write.</param>
	/// <param name="options">The serializer options.</param>
	public override void Write(Utf8JsonWriter writer, Ulid value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
#endif