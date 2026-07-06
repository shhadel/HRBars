using System.Security.Claims;
using HRBars.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HRBars.WebAPI.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var id = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            return Guid.Parse(id!);
        }
    }
}