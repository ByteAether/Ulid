using System.Text.Json.Serialization;

namespace ByteAether.Ulid;

[JsonSerializable(typeof(Ulid))]
[JsonSerializable(typeof(List<Ulid>))]
public partial class UlidJsonContext : JsonSerializerContext
{ }