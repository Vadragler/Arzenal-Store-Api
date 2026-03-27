using System.Net;
using Arzenal.Store.Api.Service.Exceptions;

namespace ArzenalStoreApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Not Found: {Message}", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.NotFound; // 404
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (DuplicateException ex)
            {
                _logger.LogWarning(ex, "Conflict: {Message}", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.Conflict; // 409
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401
                await context.Response.WriteAsJsonAsync(new { error = "Accès non autorisé" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
                await context.Response.WriteAsJsonAsync(new { error = $"Une erreur interne est survenue : {ex.Message}" });
            }
        }
    }
}
