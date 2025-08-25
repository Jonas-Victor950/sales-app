using Microsoft.AspNetCore.Mvc;

namespace SalesApp.WebApi.Controllers;

[ApiController]
[Route("api/hello")]
public class HelloController : ControllerBase
{
  [HttpGet]
  public IActionResult Get() => Ok(new { message = "SalesApp API up!" });
}
