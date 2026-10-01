using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ByteAether.Ulid.AspNetCore;

/// <summary>
/// Provides ASP.NET Core registration for <see cref="Ulid"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the <c>ulid</c> route constraint.
	/// </summary>
	public static IServiceCollection AddUlidRouteConstraint(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.Configure<RouteOptions>(options =>
		{
#if NET7_0_OR_GREATER
			options.SetParameterPolicy<RouteConstraint>("ulid");
#else
#pragma warning disable IL2026
			options.ConstraintMap["ulid"] = typeof(RouteConstraint);
#pragma warning restore IL2026
#endif
		});

		return services;
	}
}