using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace eBike.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _sender;
    private ICurrentUser? _currentUser;

    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    protected ICurrentUser CurrentUser => _currentUser ??= HttpContext.RequestServices.GetRequiredService<ICurrentUser>();

    protected int CurrentUserId => CurrentUser.UserId ?? 0;
    protected int CurrentConcernId => CurrentUser.ConcernId ?? 0;
}
