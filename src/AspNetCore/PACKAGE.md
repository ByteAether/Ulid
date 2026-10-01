# ULID Integration for ASP.NET Core
*from ByteAether*

[![License](https://img.shields.io/github/license/ByteAether/Ulid?logo=github&label=License)](https://github.com/ByteAether/Ulid/blob/main/LICENSE)
![.NET 10.0](https://img.shields.io/badge/.NET-10.0-brightgreen)
![.NET 9.0](https://img.shields.io/badge/.NET-9.0-brightgreen)
![.NET 8.0](https://img.shields.io/badge/.NET-8.0-brightgreen)
![.NET 7.0](https://img.shields.io/badge/.NET-7.0-brightgreen)
![.NET 6.0](https://img.shields.io/badge/.NET-6.0-brightgreen)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.Ulid.AspNetCore?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.Ulid.AspNetCore/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.Ulid.AspNetCore?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.Ulid.AspNetCore/)

Adds ASP.NET Core support for `ByteAether.Ulid`.

## ✨ Features

- **Route constraints**: register a `:ulid` route parameter policy that accepts only valid ULID values.
- **JSON support**: `Ulid` and `Ulid?` serialize as canonical 26-character strings without extra configuration in reflection-enabled applications.
- **OpenAPI support**: expose ULIDs as strings with the `ulid` format in Swashbuckle and ASP.NET Core OpenAPI.
- **AOT-friendly**: compatible with Native AOT; ASP.NET Core source-generated JSON metadata is required for application types.

For the core library and full details, see the [GitHub repository](https://github.com/ByteAether/Ulid).

## 💾 Install

```sh
dotnet add package ByteAether.Ulid.AspNetCore
```

The package depends on `ByteAether.Ulid`. OpenAPI packages are optional and are not added transitively.

## 🚀 Routes and JSON

No extra packages are required for route constraints or JSON serialization.

```csharp
using ByteAether.Ulid;
using ByteAether.Ulid.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddUlidRouteConstraint();

var app = builder.Build();
app.MapGet("/users/{id:ulid}", (Ulid id) => Results.Ok(new UserResponse(id)));
app.Run();

public record UserResponse(Ulid Id);
```

Behavior:

- route parameters must match a valid ULID, otherwise the route does not match
- Minimal API and MVC model binding accept valid ULID route and query values
- link generation accepts `Ulid` values for `:ulid` route parameters
- `Ulid` values in JSON bodies and responses use canonical 26-character strings; nullable values remain JSON `null`

### Native AOT JSON

For Native AOT Minimal APIs, include `Ulid` and every request/response type in a source-generated `JsonSerializerContext`, then register that context:

```csharp
using ByteAether.Ulid;
using System.Text.Json.Serialization;

builder.Services.ConfigureHttpJsonOptions(options =>
	options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

[JsonSerializable(typeof(Ulid))]
[JsonSerializable(typeof(UserResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
```

## 📘 Optional OpenAPI support

Choose the OpenAPI integration you already use. Install only that package; this package does not add either dependency automatically.

### Built-in ASP.NET Core OpenAPI (.NET 9 and 10)

```sh
dotnet add package Microsoft.AspNetCore.OpenApi
```

```csharp
builder.Services.AddOpenApi(options => options.AddUlidSchemaTransformer());
app.MapOpenApi();
```

For built-in OpenAPI, use the matching ASP.NET Core version:

- .NET 9: `Microsoft.AspNetCore.OpenApi` 9.0.0 or newer
- .NET 10: `Microsoft.AspNetCore.OpenApi` 10.0.0 or newer

### Swashbuckle (.NET 6–10)

```sh
dotnet add package Swashbuckle.AspNetCore
```

```csharp
builder.Services.AddSwaggerGen(options => options.SchemaFilter<UlidSchemaFilter>());
app.UseSwagger();
```

For Swashbuckle, use a version compatible with your target framework:

- .NET 6–9: 6.0.0 or newer
- .NET 10: 10.0.0 or newer

## 📜 License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/ByteAether/Ulid/blob/main/LICENSE) file for details.
