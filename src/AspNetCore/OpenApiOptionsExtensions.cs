#if NET9_0_OR_GREATER
using Microsoft.AspNetCore.OpenApi;

namespace ByteAether.Ulid.AspNetCore;

/// <summary>
/// Provides OpenAPI configuration for <see cref="Ulid"/>.
/// </summary>
public static class OpenApiOptionsExtensions
{
	/// <summary>
	/// Adds the ULID schema transformer to the OpenAPI document configuration.
	/// </summary>
	public static OpenApiOptions AddUlidSchemaTransformer(this OpenApiOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		options.AddSchemaTransformer<SchemaTransformer>();
		return options;
	}
}
#endif