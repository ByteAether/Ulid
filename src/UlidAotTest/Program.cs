using System.Text.Json;
using ByteAether.Ulid;

var serializerOptions = new JsonSerializerOptions
{
};
serializerOptions.TypeInfoResolverChain.Add(UlidJsonContext.Default);

var ulid = Ulid.New();
Console.WriteLine($"Generated ULID: {ulid}");

// Test JSON serialization
var json = JsonSerializer.Serialize(new List<Ulid>() { ulid }, serializerOptions);
Console.WriteLine($"Serialized to JSON: {json}");
var deserializedUlid = JsonSerializer.Deserialize<List<Ulid>>(json, serializerOptions);
Console.WriteLine($"Deserialized from JSON: {deserializedUlid!.First()}");

Console.WriteLine($"ULIDs are equal: {ulid == deserializedUlid!.First()}");
