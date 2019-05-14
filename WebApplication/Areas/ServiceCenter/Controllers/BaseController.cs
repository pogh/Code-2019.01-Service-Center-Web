using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;

namespace Company.WebApplication.Areas.ServiceCenter.Controllers {

	[Area("ServiceCenter")]
	[Authorize(Policy = "ServiceCenterUser")]
	public abstract class BaseController<TControllerType> : Controller {
		protected BaseController(IStringLocalizer<TControllerType> localizer, IConfiguration configuration, Data.BaseObject data) {
			_localizer = localizer;
			_configuration = configuration;
			_data = data;
		}

		private readonly IStringLocalizer<TControllerType> _localizer;
		protected IStringLocalizer Localizer {
			get {
				return _localizer;
			}
		}

		/// <summary>
		/// ⚠ Make sure to always use lock(_configuration) {...} 
		/// </summary>
		private readonly IConfiguration _configuration;
		protected string GetConfigurationValue(string key) {
			string returnValue;
			lock (_configuration) {
				returnValue = string.Copy(_configuration[key]);
			}
			return returnValue;
		}

		private readonly Data.BaseObject _data;
		protected Data.BaseObject DataUncast {
			get {
				return _data;
			}
		}

		protected void SetAffiliateColours(double alpha) {
			foreach (var affiliate in _data.Affiliates) {
				ViewData[string.Concat("AffiliateRgba", affiliate.AffiliateId)] = string.Concat("rgba(", 
							int.Parse(affiliate.RGB.Substring(1, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture), ", ", 
							int.Parse(affiliate.RGB.Substring(3, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture), ", ", 
							int.Parse(affiliate.RGB.Substring(5, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture), ", ", 
							alpha.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo), ")", "");
			};

		}
	}
}
