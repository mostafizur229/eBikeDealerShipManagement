using eBike.Application.Common.Interfaces;
using eBike.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace eBike.API.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class HasPermissionAttribute : TypeFilterAttribute
{
    public HasPermissionAttribute(string module, PermissionAction action)
        : base(typeof(PermissionFilter))
    {
        Arguments = new object[] { module, action.ToString() };
    }
}

public class PermissionFilter : IAsyncAuthorizationFilter
{
    private readonly string _module;
    private readonly string _action;

    public PermissionFilter(string module, string action)
    {
        _module = module;
        _action = action;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
        if (!currentUser.UserId.HasValue)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IRolePermissionCacheService>();
        bool hasPermission = await permissionService.HasPermissionAsync(currentUser.UserId.Value, _module, _action);

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}
