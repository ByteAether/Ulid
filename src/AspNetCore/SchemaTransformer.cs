#if NET9_0_OR_GREATER
using Microsoft.AspNetCore.OpenApi;
#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif

namespace ByteAether.Ulid.AspNetCore;

/// <summary>
/// Represents ULID values as 26-character strings in OpenAPI documents.
/// </summary>
internal sealed class SchemaTransformer : IOpenApiSchemaTransformer
{
	/// <inheritdoc/>
	public Task TransformAsync(
		OpenApiSchema schema,
		OpenApiSchemaTransformerContext context,
		CancellationToken cancellationToken)
	{
		if (!SchemaHelper.IsUlid(context.JsonTypeInfo.Type))
		{
			return Task.CompletedTask;
		}

		SchemaHelper.ApplyUlidSchema(schema);
		return Task.CompletedTask;
	}
}
#endif