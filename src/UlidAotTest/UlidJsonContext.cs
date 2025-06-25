using System.Text.Json.Serialization;

namespace ByteAether.Ulid;

[JsonSerializable(typeof(Ulid))]
[JsonSerializable(typeof(MyClassWithUlid))]
internal partial class UlidJsonContext : JsonSerializerContext
{ }