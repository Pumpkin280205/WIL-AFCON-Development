namespace WIL_Afcon.API.Security
{
    public static class SecurityHeadersExtensions
    {
        // Baseline response headers for a JSON API: stop browsers sniffing
        // content types, framing responses, or leaking URLs via Referer.
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var headers = context.Response.Headers;
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["Referrer-Policy"] = "no-referrer";

                await next();
            });
        }
    }
}
