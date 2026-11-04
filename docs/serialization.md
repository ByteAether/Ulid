# Serialization

`Ulid` serializes as its canonical 26-character string with System.Text.Json, with no setup. Newtonsoft.Json needs the converter below. MessagePack needs the formatter below, which writes the 16 raw bytes.

## System.Text.Json (.NET 5.0+)

The library includes a `JsonConverter`. `Ulid` and `Ulid?` serialize as canonical 26-character strings without extra configuration.

## Newtonsoft.Json

Create a `JsonConverter<Ulid>` that reads and writes the canonical string:

```csharp
using Newtonsoft.Json;
using System;

public class UlidJsonConverter : JsonConverter<Ulid>
{
	public override Ulid ReadJson(
		JsonReader reader,
		Type objectType,
		Ulid existingValue,
		bool hasExistingValue,
		JsonSerializer serializer
	)
	{
		var value = (string)reader.Value;
		return value is null ? default : Ulid.Parse(value);
	}

	public override void WriteJson(
		JsonWriter writer,
		Ulid value,
		JsonSerializer serializer
	)
	{
		writer.WriteValue(value.ToString());
	}
}
```

Register it globally:

```csharp
using Newtonsoft.Json;
using System.Collections.Generic;

JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
	Converters = new List<JsonConverter> { new UlidJsonConverter() }
};
```

Or pass it to individual calls:

```csharp
var settings = new JsonSerializerSettings();
settings.Converters.Add(new UlidJsonConverter());

var json = JsonConvert.SerializeObject(myObject, settings);
var deserializedObject = JsonConvert.DeserializeObject<MyObject>(json, settings);
```

## MessagePack

Serialize a `Ulid` as its 16 raw bytes with a custom formatter:

```csharp
using MessagePack;
using MessagePack.Formatters;

public class UlidFormatter : IMessagePackFormatter<Ulid>
{
	public Ulid Deserialize(
		ref MessagePackReader reader,
		MessagePackSerializerOptions options
	)
	{
		var bytes = reader.ReadByteArray();
		return Ulid.New(bytes);
	}

	public void Serialize(
		ref MessagePackWriter writer,
		Ulid value,
		MessagePackSerializerOptions options
	)
	{
		writer.Write(value.ToByteArray());
	}
}
```

Register the formatter in the default serializer options:

```csharp
MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions
	.WithResolver(MessagePack.Resolvers.CompositeResolver.Create(
		new IMessagePackFormatter[] { new UlidFormatter() },
		MessagePack.Resolvers.StandardResolver.Instance
	));
```
