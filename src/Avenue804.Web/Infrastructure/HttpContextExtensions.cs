using Avenue804.Web.Middleware;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Http;

namespace Avenue804.Web.Infrastructure;

public static class HttpContextExtensions
{
    public static SiteBrandingSnapshot? GetSiteBranding(this HttpContext? context) =>
        context?.Items[SiteBrandingMiddleware.HttpContextItemKey] as SiteBrandingSnapshot;
}
