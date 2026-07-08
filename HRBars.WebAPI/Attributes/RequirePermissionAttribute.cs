using HRBars.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HRBars.WebAPI.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permissionName;

    public RequirePermissionAttribute(string permissionName)
    {
        _permissionName = permissionName;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var currentUserService = context.HttpContext.RequestServices
            .GetService<ICurrentUserService>();

        if (currentUserService == null || !currentUserService.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!currentUserService.HasPermission(_permissionName))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}