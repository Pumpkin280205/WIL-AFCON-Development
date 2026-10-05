namespace WIL_Afcon.Security
{
    public static class SecurityHeadersExtensions
    {
        // Adds browser security headers to every response.
        //
        // The Content-Security-Policy is the main XSS defence-in-depth layer on
        // top of Razor's automatic HTML encoding: scripts may only load from
        // this site (no inline <script> or onclick handlers), so injected markup
        // cannot execute. connect-src also allows the API origin, which the
        // browser talks to directly for the SignalR attendance hub.
        public static IApplicationBuilder UseSecurityHeaders(
            this IApplicationBuilder app,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            var connectSources = new List<string> { "'self'" };

            if (Uri.TryCreate(configuration["ApiSettings:PublicUrl"], UriKind.Absolute, out var apiUrl))
            {
                AddOrigin(connectSources, apiUrl);
            }

            if (environment.IsDevelopment())
            {
                // Local API ports used by attendance-realtime.js's fallback.
                AddOrigin(connectSources, new Uri("http://localhost:5199"));
                AddOrigin(connectSources, new Uri("https://localhost:7285"));
            }

            var csp = string.Join("; ",
                "default-src 'self'",
                "script-src 'self'",
                // Inline style attributes are used for chart bar heights.
                "style-src 'self' 'unsafe-inline'",
                "img-src 'self' data:",
                "font-src 'self'",
                $"connect-src {string.Join(' ', connectSources.Distinct())}",
                "object-src 'none'",
                "base-uri 'self'",
                "form-action 'self'",
                "frame-ancestors 'none'");

            return app.Use(async (context, next) =>
            {
                var headers = context.Response.Headers;

                // Swagger UI (Development only) relies on inline script.
                if (!context.Request.Path.StartsWithSegments("/swagger"))
                {
                    headers["Content-Security-Policy"] = csp;
                }

                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

                await next();
            });
        }

        private static void AddOrigin(List<string> sources, Uri url)
        {
            var origin = url.GetLeftPart(UriPartial.Authority);
            var socketOrigin = url.Scheme == Uri.UriSchemeHttps
                ? "wss://" + url.Authority
                : "ws://" + url.Authority;

            sources.Add(origin);
            sources.Add(socketOrigin);
        }
    }
}
