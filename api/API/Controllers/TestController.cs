using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        [HttpGet("test", Name="Test")]
        public IActionResult GetTest()
        {
            if(HttpContext.User.Identity.IsAuthenticated){
                Console.WriteLine("-------------------------------------");
                Console.WriteLine(HttpContext.User.IsInRole(Roles.Admin));
            }
            Console.WriteLine("TestController");
            return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
        }
    }
}