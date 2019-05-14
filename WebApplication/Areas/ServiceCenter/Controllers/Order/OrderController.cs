using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;

namespace Company.WebApplication.Areas.ServiceCenter.Controllers {

	[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
	public partial class OrderController : BaseController<OrderController> {

		public OrderController(IStringLocalizer<OrderController> localizer, IConfiguration configuration, IMemoryCache memoryCache) : base(localizer, configuration, new Data.Order(configuration, memoryCache)) {
		}

		protected Data.Order Data {
			get {
				return (Data.Order)base.DataUncast;
			}
		}

		[HttpGet]
		public ActionResult Index() {
			return RedirectToAction("CustomerSearch");
		}

	}
}
