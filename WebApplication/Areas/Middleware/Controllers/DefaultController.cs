using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Company.WebApplication.Areas.Middleware.Controllers.Home
{
    [Area("Middleware")]
	[Authorize(Policy = "MiddlewareUser")]
	public class DefaultController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
