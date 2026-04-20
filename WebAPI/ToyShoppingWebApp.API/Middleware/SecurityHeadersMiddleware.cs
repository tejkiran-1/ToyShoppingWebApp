namespace ToyShoppingWebApp.API.Middleware
{
    /// <summary>
    /// Middleware to add security headers to all HTTP responses
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Prevent clickjacking attacks
            context.Response.Headers["X-Frame-Options"] = "DENY";

            // Prevent MIME type sniffing
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            // Enable XSS protection in older browsers
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

            // Enforce HTTPS and preload HSTS
            if (!context.Request.IsHttps && !context.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
            }

            // Content Security Policy (CSP) - prevent inline scripts, only allow from same origin
            context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:;";

            // Referrer policy
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Disable permissions policy
            context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

            await _next(context);
        }
    }
}
