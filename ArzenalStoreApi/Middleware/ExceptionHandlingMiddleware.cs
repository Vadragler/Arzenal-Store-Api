using Arzenal.Store.Api.Service.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

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
                _logger.LogWarning(ex, "Resource not found. TraceId: {TraceId}", context.TraceIdentifier);
                await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Ressource introuvable.");
            }
            catch (DuplicateException ex)
            {
                _logger.LogWarning(ex, "Conflicting resource. TraceId: {TraceId}", context.TraceIdentifier);
                await WriteProblemAsync(context, StatusCodes.Status409Conflict, "La ressource existe déjà.");
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Invalid request. TraceId: {TraceId}", context.TraceIdentifier);
                await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "La requête est invalide.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized request. TraceId: {TraceId}", context.TraceIdentifier);
                await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Authentification invalide ou expirée.");
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Database unavailable. TraceId: {TraceId}", context.TraceIdentifier);
                await WriteProblemAsync(context, StatusCodes.Status503ServiceUnavailable, "Le service est temporairement indisponible.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Database update rejected. TraceId: {TraceId}", context.TraceIdentifier);
                await WriteProblemAsync(context, StatusCodes.Status409Conflict, "La modification est en conflit avec l'état actuel.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error. TraceId: {TraceId}", context.TraceIdentifier);
                await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Une erreur interne est survenue.");
            }
        }

        private static async Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
        {
            if (context.Response.HasStarted)
                return;

            context.Response.StatusCode = statusCode;
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode switch
                {
                    StatusCodes.Status400BadRequest => "Requête invalide",
                    StatusCodes.Status401Unauthorized => "Non autorisé",
                    StatusCodes.Status404NotFound => "Introuvable",
                    StatusCodes.Status409Conflict => "Conflit",
                    StatusCodes.Status503ServiceUnavailable => "Service indisponible",
                    _ => "Erreur interne"
                },
                Detail = detail,
                Instance = context.Request.Path
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
