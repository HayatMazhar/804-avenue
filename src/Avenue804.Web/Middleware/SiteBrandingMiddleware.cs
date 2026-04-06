using Microsoft.Extensions.DependencyInjection;

namespace Avenue804.Web.Middleware;

public static class SiteBrandingMiddleware
{
    public const string HttpContextItemKey = "SiteBrandingSnapshot";

    public static IApplicationBuilder UseSiteBranding(this IApplicationBuilder app) =>
        app.Use(async (ctx, next) =>
        {
            var svc = ctx.RequestServices.GetRequiredService<Services.ISiteBrandingService>();
            ctx.Items[HttpContextItemKey] = await svc.GetSnapshotAsync(ctx.RequestAborted);
            await next();
        });
}
