using Microsoft.AspNetCore.Mvc;

namespace BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing.Tests.Controllers
{

    public class TestController : Controller
    {

        [HttpGet( "" )]
        public IActionResult Index( )
            => Content( "Hello, World!" );

        [HttpGet( "test" )]
        public IActionResult Test( )
            => Content( "Test!" );

    }

}
