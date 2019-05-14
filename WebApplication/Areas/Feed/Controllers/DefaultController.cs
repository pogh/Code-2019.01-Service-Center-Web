using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Company.WebApplication.Areas.Feed.Controllers.Home
{
    [Area("Feed")]
	[Authorize(Policy = "FeedUser")]
	public class DefaultController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
