using ArzenalStoreApi.Controllers.Store;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ArzenalStoreApi.Documentation;

public sealed class StoreUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(StoreAdminController) || context.MethodInfo.Name != nameof(StoreAdminController.Upload)) return;
        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/octet-stream"] = new()
                {
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "binary" }
                }
            }
        };
    }
}
