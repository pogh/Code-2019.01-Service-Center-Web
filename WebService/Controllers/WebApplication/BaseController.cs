using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Company.WebService.Models.MAINDB;
using Company.WebService.Models.ServiceCenter;

namespace Company.WebService.Controllers.WebApplication {
	[Route("api/webapplication/[controller]/[action]")]
	public abstract class BaseController : WebService.Controllers.BaseController {
		public BaseController(MAINDBContext context1, ServiceCenterContext context2, IConfiguration configuration) : base(context1) {
			_serviceCenterContext = context2;
			_configuration = configuration;
		}

		private readonly ServiceCenterContext _serviceCenterContext;
		protected ServiceCenterContext ServiceCenterContext {
			get { return _serviceCenterContext; }
		}

		/// <summary>
		/// ⚠ Make sure to always use lock(_configuration) {...} 
		/// </summary>
		private readonly IConfiguration _configuration;
		protected string Configuration(string key) {
			string returnValue;
			lock (_configuration) {
				returnValue = string.Copy(_configuration[key]);
			}
			return returnValue;
		}
	}
}
