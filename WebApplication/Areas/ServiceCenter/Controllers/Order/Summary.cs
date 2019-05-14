using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Views;

namespace Company.WebApplication.Areas.ServiceCenter.Controllers {

	public partial class OrderController : BaseController<OrderController> {

		[HttpGet]
		[Route("ServiceCenter/Order/Summary/{affiliateId}/{customerId}")]
		public async Task<ActionResult<Summary>> Summary(int affiliateId, int customerId) {
			Summary returnModel = new Summary(Data.Affiliates.Single(x => x.AffiliateId == affiliateId), customerId);

			Summary summary = await Data.GetSummary(affiliateId, customerId).ConfigureAwait(false);
			if (summary != null) {
				returnModel.PaymentTypeId = summary.PaymentTypeId;
				returnModel.DeliveryTypeId = summary.DeliveryTypeId;
				returnModel.Bic = summary.Bic;
				returnModel.Iban = summary.Iban;
				returnModel.AccountOwner = summary.AccountOwner;
				returnModel.CustomerGroupId = summary.CustomerGroupId;
				returnModel.CustomerGroupName = summary.CustomerGroupName;
				returnModel.CustomerGroupDiscount = summary.CustomerGroupDiscount;
				summary = null;
			}

			returnModel.BillingAddress = await Data.GetBillingAddress(affiliateId, customerId).ConfigureAwait(false);
			returnModel.DeliveryAddress = await Data.GetDeliveryAddress(affiliateId, customerId).ConfigureAwait(false);

			returnModel.InvoiceItems.Items.AddRange(await Data.GetInvoiceItems(affiliateId, customerId).ConfigureAwait(false));

			returnModel.PaymentTypes.AddRange(Data.GetPaymentTypes(affiliateId));

			if (returnModel.PaymentTypeId.HasValue)
				returnModel.DeliveryTypes.AddRange(Data.GetDeliveryTypes(affiliateId, returnModel.PaymentTypeId.Value));

			ViewData["Title"] = string.Concat(returnModel.BillingAddress.FullName, " (", returnModel.Affiliate.Key, ")");
			ViewData["WebServiceHostName"] = Data.WebServiceHostName;
			SetAffiliateColours(0.25);

			await ValidateInvoiceModel(returnModel).ConfigureAwait(false);

			return View(returnModel);
		}

		[HttpGet]
		[Route("ServiceCenter/Order/SendInvoice/{affiliateId}/{customerId}")]
		public async Task<ActionResult<Invoice>> SendInvoice(int affiliateId, int customerId) {

			int result = await Data.SendInvoice(Request.HttpContext.User.Identity.Name, affiliateId, customerId).ConfigureAwait(false);

			if (result > 10000) {
				return RedirectToAction("Confirmation", new {
					affiliateId,
					customerId,
					invoiceId = result
				});
			}
			else {
				return RedirectToAction("Summary");
			}
		}

		[HttpGet]
		[Route("ServiceCenter/Order/SendCustomerComment/{affiliateId}/{customerId}")]
		public async Task<ActionResult<Invoice>> SendCustomerComment(int affiliateId, int customerId) {

			System.Threading.Thread.Sleep((new Random()).Next(1500, 2500));

			bool result = await Data.SendCustomerComment(Request.HttpContext.User.Identity.Name, affiliateId, customerId).ConfigureAwait(false);

			if (result) {
				return RedirectToAction("CustomerSearch");
			}
			else {
				// If there was nothing to the customer in the database, then the ajax call was probably too slow.
				// Let's wait again until showing the page to the user again in the hope that it's there.
				System.Threading.Thread.Sleep((new Random()).Next(1500, 2500));

				return RedirectToAction("Comment");
			}
		}

		[HttpGet]
		public async Task<ActionResult<bool>> ValidateInvoiceModel(OrderBase orderBase) {

			ModelState.Clear();

			foreach (int validationErrorNumber in await Data.ValidateInvoice(orderBase.AffiliateId, orderBase.CustomerId).ConfigureAwait(false)) {

				string key = string.Empty;

				if (validationErrorNumber < 1999) {
					key = string.Empty;
				}
				else if (validationErrorNumber < 2999) {
					key = "Customer";
				}
				else if (validationErrorNumber < 3999) {
					key = "DeliveryAddress";
				}
				else if (validationErrorNumber < 4999) {
					key = "Payment";
				}
				else if (validationErrorNumber < 5999) {
					key = "Delivery";
				}
				else if (validationErrorNumber < 6999) {
					key = "Items";
				}

				ModelState.AddModelError(key, Convert.ToString(validationErrorNumber, System.Globalization.CultureInfo.InvariantCulture));
			}

			return (ModelState.ErrorCount == 0);
		}
	}
}
