#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ByteAether.Ulid.AspNetCore;

/// <summary>
/// Represents ULID values as 26-character strings in Swashbuckle-generated OpenAPI documents.
/// </summary>
public sealed class UlidSchemaFilter : ISchemaFilter
{
	/// <inheritdoc/>
#if NET10_0_OR_GREATER
	public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
#else
	public void Apply(OpenApiSchema schema, SchemaFilterContext context)
#endif
	{
		if (!SchemaHelper.IsUlid(context.Type))
		{
			return;
		}

#if NET10_0_OR_GREATER
		if (schema is OpenApiSchemaReference)
		{
			// The component schema was filtered before Swashbuckle returned this reference.
			return;
		}

		if (schema is not OpenApiSchema mutableSchema)
		{
			throw new InvalidOperationException($"Expected a mutable {nameof(OpenApiSchema)} for {nameof(Ulid)}, but received {schema.GetType().FullName}.");
		}
#else
		var mutableSchema = schema;
#endif

		SchemaHelper.ApplyUlidSchema(mutableSchema);
	}
}
