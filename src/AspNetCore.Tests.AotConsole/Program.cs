using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using ByteAether.Ulid;
using ByteAether.Ulid.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable RouteTemplates.RouteParameterConstraintNotResolved

var builder = WebApplication.CreateBuilder();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddUlidRouteConstraint();
builder.Services.ConfigureHttpJsonOptions(options =>
	options.SerializerOptions.TypeInfoResolverChain.Insert(0, UlidJsonContext.Default));

await using var app = builder.Build();
app.MapGet("/ulids/{id:ulid}", (Ulid id) => TypedResults.Ok(id));
await app.StartAsync();

var addresses = app.Services.GetRequiredService<IServer>()
	.Features.Get<IServerAddressesFeature>()?.Addresses;
if (addresses is null)
{
	throw new InvalidOperationException("Could not get the ASP.NET Core server address.");
}

using var client = new HttpClient();
var expected = Ulid.New();
using var response = await client.GetAsync($"{addresses.Single()}/ulids/{expected}");
response.EnsureSuccessStatusCode();
var returned = JsonSerializer.Deserialize(
	await response.Content.ReadAsStringAsync(),
	UlidJsonContext.Default.Ulid);
if (returned != expected)
{
	throw new InvalidOperationException("The ASP.NET Core ULID endpoint returned an unexpected value.");
}

using var invalidResponse = await client.GetAsync($"{addresses.Single()}/ulids/not-a-ulid");
if (invalidResponse.StatusCode != HttpStatusCode.NotFound)
{
	throw new InvalidOperationException("The ASP.NET Core ULID route constraint accepted an invalid value.");
}

[JsonSerializable(typeof(Ulid))]
internal partial class UlidJsonContext : JsonSerializerContext
{
}