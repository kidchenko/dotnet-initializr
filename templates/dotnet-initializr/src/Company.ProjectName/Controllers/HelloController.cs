using Microsoft.AspNetCore.Mvc;

namespace Company.ProjectName.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { Message = "Hello from Company.ProjectName!" });
}
