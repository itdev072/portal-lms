using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Portal.Api.Controllers;

[ApiController]
[AllowAnonymous]
public class ErrorController : ControllerBase
{
    [Route("/api/error")]
    public IActionResult Error() => Problem(title: "An unexpected error occurred.");
}
