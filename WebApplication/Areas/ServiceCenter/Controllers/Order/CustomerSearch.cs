using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.PartialViews;

namespace Company.WebApplication.Areas.ServiceCenter.Controllers {

	public partial class OrderController : BaseController<OrderController> {

		[HttpGet]
		public ActionResult CustomerSearch() {
			ViewData.Add("AffiliateIds", Data.Affiliates.ToDictionary(x => x.AffiliateId, y => y.Key));

			if (Request.Cookies["AffiliateIds"] != null
			&& !string.IsNullOrEmpty(Request.Cookies["AffiliateIds"]))
				ViewData.Add("SelectedAffiliateIds", Request.Cookies["AffiliateIds"].Split(',').Select(x => System.Convert.ToInt32(x, System.Globalization.CultureInfo.InvariantCulture)).ToList<int>());
			else
				ViewData.Add("SelectedAffiliateIds", Data.Affiliates.Select(x => x.AffiliateId).ToList<int>());

			ViewData["LocalizedTitle"] = "CustomerSearchTitle";
			ViewData["WebServiceHostName"] = Data.WebServiceHostName;
			SetAffiliateColours(0.25);

			return View();
		}

		[HttpGet]
		[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
		public async Task<ActionResult> CustomerSearchResults(int[] affiliateIds, string customerNumber = null, string name = null, string zip = null, string orderNr = null, string emailAddress = null, string invoiceNumber = null) {

			const int MAX_RESULTS = 20;

			if (Request.Cookies["AffiliateIds"] != null)
				Response.Cookies.Delete("AffiliateIds");

			//Remember the user's choice until midnight, i.e. only for this shift
			CookieOptions option = new CookieOptions() {
				Expires = System.DateTime.Now.Date.AddDays(1)
			};

			Response.Cookies.Append("AffiliateIds", string.Join(",", affiliateIds), option);

			//----

			CustomerSearchResults returnModel = new CustomerSearchResults();

			if (!string.IsNullOrEmpty(customerNumber)
			 || !string.IsNullOrEmpty(name)
			 || !string.IsNullOrEmpty(zip)
			 || !string.IsNullOrEmpty(orderNr)
			 || !string.IsNullOrEmpty(emailAddress)
			 || !string.IsNullOrEmpty(invoiceNumber)
			) {
				DateTime start = DateTime.Now;

				returnModel.Customers.AddRange(await Data.CustomerSearchResults(affiliateIds: affiliateIds, customerNumber: customerNumber, name: name, zip: zip, orderNr: orderNr, emailAddress: emailAddress, invoiceNumber: invoiceNumber, limit: MAX_RESULTS).ConfigureAwait(false));

				ViewData["QueryRunTime"] = (DateTime.Now - start).TotalMilliseconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
			}

			returnModel.NoResults = (returnModel.Customers.Count == 0);
			returnModel.ToManyResults = (returnModel.Customers.Count >= MAX_RESULTS);

			//----

			ViewData["WebServiceHostName"] = Data.WebServiceHostName;
			SetAffiliateColours(0.25);

			return PartialView(returnModel);
		}
	}
}
