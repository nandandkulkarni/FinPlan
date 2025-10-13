using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FinPlan.Web.Services
{
    // DelegatingHandler that forwards the real client headers from the incoming Blazor Server request
    // to downstream API calls (X-Forwarded-For, X-Forwarded-Proto, X-Forwarded-Host, X-Real-IP if present).
    public class ForwardClientIpHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ForwardClientIpHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = _httpContextAccessor.HttpContext;

            // Try to resolve the scoped ClientConnectionInfo from the current request scope
            ClientConnectionInfo? clientInfo = null;
            if (context != null)
            {
                clientInfo = context.RequestServices.GetService<ClientConnectionInfo>();
            }

            // Resolve values in order of trust: forwarded headers -> scoped info -> cookies -> remote
            string? xff = context?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                          ?? clientInfo?.XForwardedFor
                          ?? TryGetCookie(context, "FP-Client-IP")
                          ?? context?.Connection.RemoteIpAddress?.ToString();

            string? xproto = context?.Request.Headers["X-Forwarded-Proto"].FirstOrDefault()
                             ?? clientInfo?.XForwardedProto
                             ?? TryGetCookie(context, "FP-Client-Proto")
                             ?? context?.Request.Scheme;

            string? xhost = context?.Request.Headers["X-Forwarded-Host"].FirstOrDefault()
                            ?? clientInfo?.XForwardedHost
                            ?? TryGetCookie(context, "FP-Client-Host")
                            ?? context?.Request.Host.Value;

            string? xreal = context?.Request.Headers["X-Real-IP"].FirstOrDefault()
                            ?? clientInfo?.XRealIp
                            ?? TryGetCookie(context, "FP-Client-RealIP");

            // Normalize XFF to first IP
            if (!string.IsNullOrWhiteSpace(xff))
            {
                var parts = xff.Split(',');
                if (parts.Length > 0) xff = parts[0].Trim();
            }

            // Set/replace outgoing headers (plus a dedicated X-Client-IP)
            request.Headers.Remove("X-Forwarded-For");
            request.Headers.Remove("X-Forwarded-Proto");
            request.Headers.Remove("X-Forwarded-Host");
            request.Headers.Remove("X-Real-IP");
            request.Headers.Remove("X-Client-IP");

            if (!string.IsNullOrWhiteSpace(xff))
            {
                request.Headers.TryAddWithoutValidation("X-Forwarded-For", xff);
                request.Headers.TryAddWithoutValidation("X-Client-IP", xff);
            }

            if (!string.IsNullOrWhiteSpace(xproto))
                request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", xproto);

            if (!string.IsNullOrWhiteSpace(xhost))
                request.Headers.TryAddWithoutValidation("X-Forwarded-Host", xhost);

            if (!string.IsNullOrWhiteSpace(xreal))
                request.Headers.TryAddWithoutValidation("X-Real-IP", xreal);

            return await base.SendAsync(request, cancellationToken);
        }

        private static string? TryGetCookie(HttpContext? context, string name)
        {
            if (context?.Request?.Cookies.TryGetValue(name, out var v) == true)
                return v;
            return null;
        }
    }
}
