using Arzenal.Store.Api.Service.Exceptions;
using ArzenalStoreApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arzenal.Store.Api.TestUnitaire.Unit.Middleware;

public class StoreProblemDetailsTests
{
    [Fact]
    public async Task InvalidStoreRequest_ReturnsProblemDetailsWithTheCorrectMediaType()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlingMiddleware(_ => throw new ValidationException("private detail"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.Invoke(context);

        Assert.Equal(400, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.DoesNotContain("private detail", body);
    }
}
