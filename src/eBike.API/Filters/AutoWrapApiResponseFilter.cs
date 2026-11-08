using eBike.Application.Common.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace eBike.API.Filters;

public class AutoWrapApiResponseFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Skip Swagger or file downloads or already handled exceptions
        if (context.Exception != null || context.HttpContext.Response.HasStarted)
        {
            return;
        }

        if (context.Result is ObjectResult objectResult)
        {
            // If already ApiResponse, do not double-wrap
            var valueType = objectResult.Value?.GetType();
            if (valueType != null && valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                return;
            }

            var wrapped = ApiResponse<object>.Ok(objectResult.Value ?? new object(), "Success");
            context.Result = new ObjectResult(wrapped)
            {
                StatusCode = objectResult.StatusCode ?? 200
            };
        }
        else if (context.Result is EmptyResult)
        {
            context.Result = new ObjectResult(ApiResponse<string>.Ok(string.Empty, "Success"))
            {
                StatusCode = 200
            };
        }
    }
}
