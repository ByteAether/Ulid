using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ByteAether.Ulid.AspNetCore;

/// <summary>
/// Constrains a route parameter to a valid ULID.
/// </summary>
internal sealed class RouteConstraint : IRouteConstraint
{
	/// <inheritdoc/>
	public bool Match(
		HttpContext? httpContext,
		IRouter? route,
		string routeKey,
		RouteValueDictionary values,
		RouteDirection routeDirection)
		=> values.TryGetValue(routeKey, out var value)
			&& (
				value is string ulidString && Ulid.TryParse(ulidString, provider: null, out _)
				|| routeDirection == RouteDirection.UrlGeneration && value is Ulid
			);
}