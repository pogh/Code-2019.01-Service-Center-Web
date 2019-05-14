using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Company.WebApplication.Areas.Einkauf.Controllers.Home
{
    [Area("Einkauf")]
	[Authorize(Policy = "EinkaufUser")]
	public class DefaultController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
