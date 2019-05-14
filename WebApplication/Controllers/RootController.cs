using System;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Company.WebApplication.Controllers {
	public class RootController : Controller {
		public IActionResult Error() {
			IExceptionHandlerFeature x = HttpContext.Features.Get<IExceptionHandlerFeature>();

			if (x != null) {
				try {
					ViewData["path"] = ((ExceptionHandlerFeature)x).Path;
				}
				catch {
					ViewData["path"] = "Unknown";
				}

				if (x.Error != null) {
					ViewData["message"] = x.Error.Message;
					ViewData["stacktrace"] = x.Error.StackTrace;
				}
			}
			else {
				ViewData["message"] = "I died.";
			}

			var y = View();


			return View();
		}

		[Route("/{code:int}")]
		public IActionResult Info(int code) {
			ViewData["code"] = code;
			ViewData["value"] = System.Convert.ToString(Enum.Parse<Http​Status​Code>(code.ToString(System.Globalization.CultureInfo.InvariantCulture)), System.Globalization.CultureInfo.InvariantCulture);

			if(System.Convert.ToString(ViewData["code"], System.Globalization.CultureInfo.InvariantCulture) == (string)ViewData["value"]) {
				ViewData["code"] = (int)Http​Status​Code.NotFound;
				ViewData["value"] = System.Convert.ToString(Http​Status​Code.NotFound, System.Globalization.CultureInfo.InvariantCulture);
			}

			return View();
		}
	}
}
