using Microsoft.AspNetCore.Builder;
using Shop.PublicApi.Middlewares;

namespace Shop.PublicApi.Extensions;

internal static class MiddlewareExtensions
{
    public static void UseCorrelationId(this IApplicationBuilder builder) =>
        builder.UseMiddleware<CorrelationIdMiddleware>();

    public static void UseErrorHandling(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ErrorHandlingMiddleware>();
}