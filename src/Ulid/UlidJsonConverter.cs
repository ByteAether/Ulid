#if NETCOREAPP
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ByteAether.Ulid;

/// <summary>
/// A custom JSON converter for the <see cref="Ulid"/> type.
/// </summary>
public class UlidJsonConverter : JsonConverter<Ulid>
{
	/// <inheritdoc/>
	public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		try
		{
			if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.PropertyName)
			{
				throw new JsonException("Expected string or property name");
			}

			if (reader.HasValueSequence)
			{
				var byteSequence = reader.ValueSequence;
				if (byteSequence.Length != Ulid.UlidStringLength)
				{
					throw new JsonException($"Ulid invalid: length must be {Ulid.UlidStringLength}");
				}

				Span<byte> byteSpan = stackalloc byte[Ulid.UlidStringLength];
				byteSequence.CopyTo(byteSpan);
				return Ulid.Parse(byteSpan);
			}
			else
			{
				var byteSpan = reader.ValueSpan;
				return Ulid.Parse(byteSpan);
			}
		}
		catch (FormatException ex)
		{
			throw new JsonException($"Ulid invalid: length must be {Ulid.UlidStringLength}", ex);
		}
	}

	/// <inheritdoc/>
	public override void Write(Utf8JsonWriter writer, Ulid ulid, JsonSerializerOptions options)
	{
		Span<byte> ulidString = stackalloc byte[Ulid.UlidStringLength];
		ulid.TryFormat(ulidString, out _, []);
		writer.WriteStringValue(ulidString);
	}

#if NET6_0_OR_GREATER
	/// <inheritdoc/>
	public override void WriteAsPropertyName(Utf8JsonWriter writer, Ulid ulid, JsonSerializerOptions options)
	{
		Span<byte> ulidString = stackalloc byte[Ulid.UlidStringLength];
		ulid.TryFormat(ulidString, out _, []);
		writer.WritePropertyName(ulidString);
	}

	/// <inheritdoc/>
	public override Ulid ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> Read(ref reader, typeToConvert, options);
#endif
}
#endif