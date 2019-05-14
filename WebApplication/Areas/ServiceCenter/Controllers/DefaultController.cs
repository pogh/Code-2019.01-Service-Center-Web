using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Company.WebApplication.Areas.ServiceCenter.Controllers {

	[Area("ServiceCenter")]
	[Authorize(Policy = "ServiceCenterUser")]
	public class DefaultController : Controller{
        public IActionResult Index()
        {
			return RedirectToAction("CustomerSearch", "Order");
		}
    }
}
