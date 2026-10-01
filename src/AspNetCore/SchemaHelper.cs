#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif

namespace ByteAether.Ulid.AspNetCore;

internal static class SchemaHelper
{
	internal static bool IsUlid(Type type)
		=> (Nullable.GetUnderlyingType(type) ?? type) == typeof(Ulid);

	internal static void ApplyUlidSchema(OpenApiSchema schema)
	{
#if NET10_0_OR_GREATER
		schema.Type = JsonSchemaType.String;
#else
		schema.Type = "string";
#endif
		schema.Format = "ulid";
		schema.MinLength = Ulid.UlidStringLength;
		schema.MaxLength = Ulid.UlidStringLength;
		schema.Properties?.Clear();
		schema.Required?.Clear();
		schema.Items = null;
	}
}
