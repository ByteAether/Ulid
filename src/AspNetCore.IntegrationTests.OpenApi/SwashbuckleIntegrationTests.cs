using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable RouteTemplates.RouteParameterConstraintNotResolved

namespace ByteAether.Ulid.AspNetCore.IntegrationTests.OpenApi;

public class SwashbuckleTests
{
	// ReSharper disable NotAccessedPositionalProperty.Local
	private sealed record UlidResponse(Ulid Id, Ulid? OptionalId);
	// ReSharper restore NotAccessedPositionalProperty.Local

	[Fact]
	public async Task UlidAndNullableUlidSchemasAreGeneratedAs26CharacterStrings()
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseTestServer();
		builder.Services.AddUlidRouteConstraint();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(options => options.SchemaFilter<UlidSchemaFilter>());

		await using var app = builder.Build();
		app.UseSwagger();
		app.MapGet("/ulids/{id:ulid}", (Ulid id) => TypedResults.Ok(new UlidResponse(id, id)));

		await app.StartAsync(TestContext.Current.CancellationToken);
		var client = app.GetTestClient();

		using var response = await client.GetAsync("/swagger/v1/swagger.json", TestContext.Current.CancellationToken);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var document = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		Assert.Contains("\"optionalId\":", document, StringComparison.Ordinal);
		OpenApiSchemaAssertions.AssertUlidSchemas(document);
	}
}