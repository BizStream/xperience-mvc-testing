using Microsoft.AspNetCore.Mvc;

namespace BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing.Tests.Controllers;

public class TestController : Controller
{
    [HttpGet( "" )]
    public IActionResult Index( )
    {
        return Content( "Hello, World!" );
    }

    [HttpGet( "test" )]
    public IActionResult Test( )
    {
        return Content( "Test!" );
    }
}