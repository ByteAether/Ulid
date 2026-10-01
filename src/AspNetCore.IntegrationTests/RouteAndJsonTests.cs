using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable RouteTemplates.RouteParameterConstraintNotResolved

namespace ByteAether.Ulid.AspNetCore.IntegrationTests;

public class RouteAndJsonTests
{
	private sealed record UlidPayload(Ulid Id, Ulid? OptionalId);
	private sealed record UlidResponse(Ulid RouteId, Ulid Id, Ulid? OptionalId);

	[Fact]
	public async Task RoutesAndJsonWorkWithoutOpenApiPackages()
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseTestServer();
		builder.Services.AddUlidRouteConstraint();

		await using var app = builder.Build();
		app.MapPost("/ulids/{id:ulid}", (Ulid id, UlidPayload payload) =>
			TypedResults.Ok(new UlidResponse(id, payload.Id, payload.OptionalId)));

		await app.StartAsync(TestContext.Current.CancellationToken);
		var client = app.GetTestClient();
		var expected = Ulid.New();

		using var validResponse = await client.PostAsJsonAsync(
			$"/ulids/{expected}",
			new UlidPayload(expected, null),
			TestContext.Current.CancellationToken);
		Assert.Equal(HttpStatusCode.OK, validResponse.StatusCode);
		var returnedPayload = await validResponse.Content.ReadFromJsonAsync<UlidResponse>(TestContext.Current.CancellationToken);
		Assert.Equal(expected, returnedPayload?.RouteId);
		Assert.Equal(expected, returnedPayload?.Id);
		Assert.Null(returnedPayload?.OptionalId);
		using var responseJson = JsonDocument.Parse(await validResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
		Assert.Equal(expected.ToString(), responseJson.RootElement.GetProperty("id").GetString());
		Assert.Equal(JsonValueKind.Null, responseJson.RootElement.GetProperty("optionalId").ValueKind);

		using var invalidResponse = await client.PostAsJsonAsync(
			"/ulids/not-a-ulid",
			new UlidPayload(expected, null),
			TestContext.Current.CancellationToken);
		Assert.Equal(HttpStatusCode.NotFound, invalidResponse.StatusCode);

		using var invalidJsonResponse = await client.PostAsJsonAsync(
			$"/ulids/{expected}",
			new { Id = "not-a-ulid", OptionalId = (string?)null },
			TestContext.Current.CancellationToken);
		Assert.Equal(HttpStatusCode.BadRequest, invalidJsonResponse.StatusCode);
	}

	[Fact]
	public async Task UlidRouteValuesAndQueryParametersAreSupported()
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseTestServer();
		builder.Services.AddUlidRouteConstraint();

		await using var app = builder.Build();
		app.MapGet("/ulids/{id:ulid}", (Ulid id) => TypedResults.Ok(id))
			.WithName("GetUlid");
		app.MapGet("/lookup", (Ulid id) => TypedResults.Ok(id));

		await app.StartAsync(TestContext.Current.CancellationToken);
		var client = app.GetTestClient();
		var expected = Ulid.New();
		var linkGenerator = app.Services.GetRequiredService<LinkGenerator>();

		var path = linkGenerator.GetPathByName("GetUlid", new { id = expected });
		Assert.Equal($"/ulids/{expected}", path);

		using var routeResponse = await client.GetAsync(path!, TestContext.Current.CancellationToken);
		Assert.Equal(expected, await routeResponse.Content.ReadFromJsonAsync<Ulid>(TestContext.Current.CancellationToken));

		using var queryResponse = await client.GetAsync($"/lookup?id={expected}", TestContext.Current.CancellationToken);
		Assert.Equal(expected, await queryResponse.Content.ReadFromJsonAsync<Ulid>(TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task MvcModelBindingAcceptsUlidStrings()
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseTestServer();
		builder.Services.AddControllers().AddApplicationPart(typeof(UlidModelBindingController).Assembly);

		await using var app = builder.Build();
		app.MapControllers();

		await app.StartAsync(TestContext.Current.CancellationToken);
		var expected = Ulid.New();
		using var response = await app.GetTestClient().GetAsync(
			$"/mvc-ulids/{expected}",
			TestContext.Current.CancellationToken);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(expected, await response.Content.ReadFromJsonAsync<Ulid>(TestContext.Current.CancellationToken));
	}
}

[ApiController]
[Route("mvc-ulids")]
public sealed class UlidModelBindingController : ControllerBase
{
	[HttpGet("{id}")]
	public Ulid Get(Ulid id) => id;
}