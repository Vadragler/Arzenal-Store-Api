namespace ArzenalStoreApi.Middleware
{
    public class GuidValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GuidValidationMiddleware> _logger;

        public GuidValidationMiddleware(RequestDelegate next, ILogger<GuidValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.RouteValues != null)
            {
                foreach (var kvp in context.Request.RouteValues)
                {
                    if (Guid.TryParse(kvp.Value?.ToString(), out var guid))
                    {
                        if (guid == Guid.Empty)
                        {
                            _logger.LogWarning("GUID invalide pour la route {Route} : {Param}={Value}",
                                context.Request.Path, kvp.Key, kvp.Value);
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            await context.Response.WriteAsJsonAsync(new { error = $"Paramètre '{kvp.Key}' invalide" });
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
