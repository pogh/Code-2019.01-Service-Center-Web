using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.PartialViews;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Views;

namespace Company.WebApplication.Areas.ServiceCenter.Controllers {

	public partial class OrderController : BaseController<OrderController> {

		[HttpGet]
		[Route("ServiceCenter/Order/StartInvoice/{affiliateId}/{kundenNr}")]
		public async Task<ActionResult<Invoice>> StartInvoice(int affiliateId, int kundenNr) {
			int customerId = await Data.GetOrCreateCustomerId(kundenNr).ConfigureAwait(false);
			return RedirectToAction("Invoice", new { affiliateId, customerId });
		}

		[HttpGet]
		[Route("ServiceCenter/Order/Invoice/{affiliateId}/{customerId}")]
		public async Task<ActionResult<Invoice>> Invoice(int affiliateId, int customerId) {

			Invoice invoice = await Data.GetOrCreateInvoice(Request.HttpContext.User.Identity.Name, affiliateId, customerId).ConfigureAwait(false);

			Invoice returnModel = new Invoice(Data.Affiliates.Single(x => x.AffiliateId == affiliateId), customerId) {
				BillingAddress = await Data.GetBillingAddress(affiliateId, customerId).ConfigureAwait(false),
				CustomerGroupId = invoice.CustomerGroupId,
				CustomerGroupName = invoice.CustomerGroupName,
				CustomerGroupDiscount = invoice.CustomerGroupDiscount,
			};

			returnModel.InvoiceItems.Items.AddRange(await Data.GetInvoiceItems(affiliateId, customerId).ConfigureAwait(false));

			ViewData["Title"] = string.Concat(returnModel.BillingAddress.FullName, " (", returnModel.Affiliate.Key, ")");
			ViewData["WebServiceHostName"] = Data.WebServiceHostName;
			SetAffiliateColours(0.25);


			if ((await Data.GetCustomerWarnings(affiliateId, customerId).ConfigureAwait(false)).Count(x => x != "CREDIT_RATING_NONE") > 0)
				ViewData["Message"] = "ForwardCustomerWarning";
			else if(returnModel.CustomerGroupId == 22)  // AMAZON Kunden (BND-1157)
				ViewData["Message"] = "ForwardCustomerWarning";

			return View(returnModel);
		}

		[HttpGet]
		public async Task<ActionResult<ArticleSearchResults>> PznSearchResults(int affiliateId, string searchField) {

			const int MAX_RESULTS = 20;

			ArticleSearchResults returnModel = new ArticleSearchResults();

			if (string.IsNullOrEmpty(searchField)) {
				//returnModel.Articles = new List<Article>();
			}
			else {
				int pzn;
				if (int.TryParse(searchField.Trim(), out pzn)) {
					DateTime start = DateTime.Now;
					returnModel.Articles.AddRange(await Data.PznSearch(affiliateId, pzn, limit: 1).ConfigureAwait(false));
					ViewData["QueryRunTime"] = (DateTime.Now - start).TotalMilliseconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
				}
				else {
					DateTime start = DateTime.Now;
					returnModel.Articles.AddRange(
						(await Data.PznSearch(affiliateId, name: searchField, limit: MAX_RESULTS).ConfigureAwait(false))
						.OrderBy(x => x.IsLocked || !x.IsOnline || x.AvailabilityType == 0 || x.AvailabilityType == 4 || x.AvailabilityType == 6)
						.ThenBy(y => y.DisplayName)
						);
					ViewData["QueryRunTime"] = (DateTime.Now - start).TotalMilliseconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
				}
			}

			returnModel.NoResults = (returnModel.Articles.Count == 0);
			returnModel.ToManyResults = (returnModel.Articles.Count >= MAX_RESULTS);

			return PartialView("InvoiceArticleSearchResults", returnModel);
		}

		[HttpGet]
		public async Task<ActionResult<ArticleSearchResults>> GetOftenOrderedItems(int affiliateId, int customerId) {

			const int MAX_RESULTS = 20;

			ArticleSearchResults returnModel = new ArticleSearchResults();

			if (affiliateId == 0
			 || customerId == 0) {
				//returnModel.Articles = new List<Article>();
			}
			else {
				DateTime start = DateTime.Now;
				returnModel.Articles.AddRange(await Data.GetOftenOrderedItems(affiliateId, customerId).ConfigureAwait(false));
				ViewData["QueryRunTime"] = (DateTime.Now - start).TotalMilliseconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
			}

			returnModel.NoResults = (returnModel.Articles.Count == 0);
			returnModel.ToManyResults = (returnModel.Articles.Count >= MAX_RESULTS);

			return PartialView("InvoiceArticleSearchResults", returnModel);
		}

		[HttpGet]
		public async Task<ActionResult<ArticleSearchResults>> GetRecentOrderedItems(int affiliateId, int customerId) {

			const int MAX_RESULTS = 20;

			ArticleSearchResults returnModel = new ArticleSearchResults();

			if (affiliateId == 0
			 || customerId == 0) {
				//returnModel.Articles = new List<Article>();
			}
			else {
				DateTime start = DateTime.Now;
				returnModel.Articles.AddRange(await Data.GetRecentOrderedItems(affiliateId, customerId).ConfigureAwait(false));
				ViewData["QueryRunTime"] = (DateTime.Now - start).TotalMilliseconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
			}

			returnModel.NoResults = (returnModel.Articles.Count == 0);
			returnModel.ToManyResults = (returnModel.Articles.Count >= MAX_RESULTS);

			return PartialView("InvoiceArticleSearchResults", returnModel);
		}

		[HttpGet]
		public async Task<ActionResult<List<InvoiceItem>>> GetInvoiceItems(int affiliateId, int customerId) {
			List<InvoiceItem> invoiceItems = await Data.GetInvoiceItems(affiliateId, customerId).ConfigureAwait(false);

			return invoiceItems;
		}

		[HttpPost]
		[AutoValidateAntiforgeryToken]
		public async Task<ActionResult<bool>> AddInvoiceItem(int affiliateId, int customerId, int pzn, int quantity, string vat, string itemPrice, string priceSavings) {

			decimal vatInvariant = decimal.Parse(vat, System.Globalization.CultureInfo.InvariantCulture);
			decimal itemPriceInvariant = decimal.Parse(itemPrice, System.Globalization.CultureInfo.InvariantCulture);
			decimal priceSavingsInvariant = decimal.Parse(priceSavings, System.Globalization.CultureInfo.InvariantCulture);

			await Data.AddInvoiceItem(Request.HttpContext.User.Identity.Name, affiliateId, customerId, pzn, quantity, vatInvariant, itemPriceInvariant, priceSavingsInvariant).ConfigureAwait(false);
			return PartialView("InvoiceItems", new InvoiceItems(await Data.GetInvoiceItems(affiliateId, customerId).ConfigureAwait(false)));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> RemoveInvoiceItem(int affiliateId, int customerId, int invoiceItemId) {
			await Data.RemoveInvoiceItem(Request.HttpContext.User.Identity.Name, affiliateId, customerId, invoiceItemId).ConfigureAwait(false);
			return PartialView("InvoiceItems", new InvoiceItems(await Data.GetInvoiceItems(affiliateId, customerId).ConfigureAwait(false)));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> UpdateInvoiceItemQuantity(int affiliateId, int customerId, int invoiceItemId, int quantity) {
			await Data.UpdateInvoiceItemQuantity(Request.HttpContext.User.Identity.Name, affiliateId, customerId, invoiceItemId, quantity).ConfigureAwait(false);
			return PartialView("InvoiceItems", new InvoiceItems(await Data.GetInvoiceItems(affiliateId, customerId).ConfigureAwait(false)));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> IncrementInvoiceItem(int affiliateId, int customerId, int invoiceItemId) {
			await Data.IncrementInvoiceItem(Request.HttpContext.User.Identity.Name, affiliateId, customerId, invoiceItemId).ConfigureAwait(false);
			return PartialView("InvoiceItems", new InvoiceItems(await Data.GetInvoiceItems(affiliateId, customerId).ConfigureAwait(false)));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DecrementInvoiceItem(int affiliateId, int customerId, int invoiceItemId) {
			await Data.DecrementInvoiceItem(Request.HttpContext.User.Identity.Name, affiliateId, customerId, invoiceItemId).ConfigureAwait(false);
			return PartialView("InvoiceItems", new InvoiceItems(await Data.GetInvoiceItems(affiliateId, customerId).ConfigureAwait(false)));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddSpecialItem(int affiliateId, int customerId, int pzn, decimal itemPrice) {
			await Data.AddSpecialItem(Request.HttpContext.User.Identity.Name, affiliateId, customerId, pzn, itemPrice).ConfigureAwait(false);
			return PartialView("InvoiceItems", new InvoiceItems(await Data.GetInvoiceItems(affiliateId, customerId).ConfigureAwait(false)));
		}

		[HttpGet]
		public async Task<ActionResult<string>> GetComparisonPrices(int affiliateId, int pzn) {
			return await Data.GetComparisonPrices(affiliateId, pzn).ConfigureAwait(false);
		}
	}
}
