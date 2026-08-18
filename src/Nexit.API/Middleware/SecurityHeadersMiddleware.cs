namespace Nexit.API.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["Cache-Control"] = "no-store";

        // Content-Security-Policy: la API es JSON puro, así que "default-src 'none'" es seguro.
        // En Development se omite en /swagger para no romper la UI de Swagger (que sí carga CSS/JS propios).
        var esSwaggerEnDesarrollo = environment.IsDevelopment() && context.Request.Path.StartsWithSegments("/swagger");
        if (!esSwaggerEnDesarrollo)
        {
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";
        }

        await next(context);
    }
}
