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
	// Every character of an escaped string is at most 6 bytes long ("\uXXXX")
	private const int _maxEscapedLength = Ulid.UlidStringLength * 6;

	/// <inheritdoc/>
	public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.PropertyName)
		{
			throw new JsonException("Expected string or property name");
		}

		if (!TryRead(ref reader, out var ulid))
		{
			throw new JsonException($"Ulid invalid: expected a {Ulid.UlidStringLength} character Crockford's Base32 string");
		}

		return ulid;
	}

	private static bool TryRead(ref Utf8JsonReader reader, out Ulid ulid)
	{
#if NET7_0_OR_GREATER
		if (reader.ValueIsEscaped)
		{
			return TryReadEscaped(ref reader, out ulid);
		}
#endif

		if (reader.HasValueSequence)
		{
			var byteSequence = reader.ValueSequence;
			if (byteSequence.Length == Ulid.UlidStringLength)
			{
				Span<byte> byteSpan = stackalloc byte[Ulid.UlidStringLength];
				byteSequence.CopyTo(byteSpan);
				if (Ulid.TryParse(byteSpan, null, out ulid))
				{
					return true;
				}
			}
		}
		else if (Ulid.TryParse(reader.ValueSpan, null, out ulid))
		{
			return true;
		}

#if NET7_0_OR_GREATER
		ulid = default;
		return false;
#else
		// The escaped flag is not available, so unescape only after the raw value failed to parse
		return TryReadEscaped(ref reader, out ulid);
#endif
	}

	private static bool TryReadEscaped(ref Utf8JsonReader reader, out Ulid ulid)
	{
		var length = reader.HasValueSequence ? reader.ValueSequence.Length : reader.ValueSpan.Length;
		if (length > _maxEscapedLength)
		{
			ulid = default;
			return false;
		}

#if NET7_0_OR_GREATER
		Span<char> chars = stackalloc char[_maxEscapedLength];
		var charsWritten = reader.CopyString(chars);
		return Ulid.TryParse(chars[..charsWritten], null, out ulid);
#else
		return Ulid.TryParse(reader.GetString(), null, out ulid);
#endif
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