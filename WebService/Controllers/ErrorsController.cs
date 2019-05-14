using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Company.WebService.Controllers {

	[ApiExplorerSettings(IgnoreApi = true)]
	public class ErrorsController : Controller {

		public ErrorsController(ILogger<ErrorsController> logger) {
			_logger = logger;
		}

		private readonly ILogger _logger;

		[HttpGet("/Error")]
		public ActionResult Index() {

			IExceptionHandlerPathFeature exceptionData = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

			string message;

			try {
				message = exceptionData.Error.Message;
			}
			catch {
				message = "I died";
			}

			try {
				_logger.Log(LogLevel.Error, message);
			}
			catch {}

			return Ok(message);
		}
	}
}
