using Microsoft.AspNetCore.Mvc;

namespace NuGetServer.Extensions;

public static class ProblemDetailsExtensions
{
    public static ProblemDetails ToProblem(this string message, int statusCode = 400, string? title = null)
    {
        return new ProblemDetails
        {
            Title = title ?? "Error",
            Detail = message,
            Status = statusCode
        };
    }
}
