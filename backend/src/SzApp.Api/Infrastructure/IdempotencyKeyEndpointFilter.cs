using Microsoft.AspNetCore.Mvc;

namespace SzApp.Api.Infrastructure;

public sealed class IdempotencyKeyEndpointFilter : IEndpointFilter
{
    public const string HeaderName = "Idempotency-Key";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var value = context.HttpContext.Request.Headers[HeaderName].ToString().Trim();
        if (value.Length is < 8 or > 128)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Nedostaje ispravan Idempotency-Key",
                extensions: new Dictionary<string, object?> { ["code"] = "idempotency.invalid-key" });
        }

        return await next(context);
    }
}
