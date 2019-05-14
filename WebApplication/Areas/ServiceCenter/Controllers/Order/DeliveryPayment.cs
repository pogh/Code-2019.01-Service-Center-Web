using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.PartialViews;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Views;

namespace Company.WebApplication.Areas.ServiceCenter.Controllers {

	public partial class OrderController : BaseController<OrderController> {

		[HttpGet]
		[Route("ServiceCenter/Order/DeliveryPayment/{affiliateId}/{customerId}")]
		public async Task<ActionResult<DeliveryPayment>> DeliveryPayment(int affiliateId, int customerId) {
			DeliveryPayment returnModel = new DeliveryPayment(Data.Affiliates.Single(x => x.AffiliateId == affiliateId), customerId);

			DeliveryPayment deliveryPayment = await Data.GetDeliveryPayment(affiliateId, customerId).ConfigureAwait(false);
			if (deliveryPayment != null) {
				returnModel.PaymentTypeId = deliveryPayment.PaymentTypeId;
				returnModel.DeliveryTypeId = deliveryPayment.DeliveryTypeId;
				returnModel.Bic = deliveryPayment.Bic;
				returnModel.Iban = deliveryPayment.Iban;
				returnModel.AccountOwner = deliveryPayment.AccountOwner;
				returnModel.CustomerGroupId = deliveryPayment.CustomerGroupId;
				returnModel.CustomerGroupName = deliveryPayment.CustomerGroupName;
				returnModel.CustomerGroupDiscount = deliveryPayment.CustomerGroupDiscount;
				deliveryPayment = null;

				returnModel.InvoiceItems.Items.AddRange(await Data.GetInvoiceItems(affiliateId, customerId).ConfigureAwait(false));
			}

			returnModel.BillingAddress = await Data.GetBillingAddress(affiliateId, customerId).ConfigureAwait(false);

			returnModel.DeliveryAddress = await Data.GetDeliveryAddress(affiliateId, customerId).ConfigureAwait(false);
			if (string.IsNullOrEmpty(returnModel.DeliveryAddress.Country))
				returnModel.DeliveryAddress.Country = returnModel.BillingAddress.Country;
			returnModel.DeliveryAddresses.AddRange(await Data.GetDeliveryAddresses(customerId).ConfigureAwait(false));

			returnModel.PaymentTypes.AddRange(Data.GetPaymentTypes(affiliateId));

			if (returnModel.PaymentTypeId.HasValue)
				returnModel.DeliveryTypes.AddRange(Data.GetDeliveryTypes(affiliateId, returnModel.PaymentTypeId.Value));

			returnModel.DeliveryPaymentButtons.AddRange(await Data.GetDeliveryPaymentButtons(affiliateId).ConfigureAwait(false));

			ViewData["Title"] = string.Concat(returnModel.BillingAddress.FullName, " (", returnModel.Affiliate.Key, ")");
			ViewData["WebServiceHostName"] = Data.WebServiceHostName;
			SetAffiliateColours(0.25);

			return View(returnModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult<int>> UpdateDeliveryAddress(int affiliateId, int customerId, string title = null, string firstName = null, string lastName = null, string companyName = null, string street = null, string city = null, string zip = null, string country = null, string additionalLine = null) {
			return await Data.UpdateDeliveryAddress(Request.HttpContext.User.Identity.Name, affiliateId, customerId, title, firstName, lastName, companyName, street, city, zip, country, additionalLine).ConfigureAwait(false);
		}

		[HttpGet]
		public async Task<ActionResult<string>> GetDeliveryAddressStreet(int affiliateId, int customerId) {
			return await Data.GetDeliveryAddressStreet(affiliateId, customerId).ConfigureAwait(false);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> UpdatePaymentType(int affiliateId, int customerId, int paymentTypeId, string bic, string iban, string accountOwner, int selectedDeliveryTypeId, string orderItemsTotal) {
			int deliveryTypeId = selectedDeliveryTypeId;

			if (await Data.UpdatePaymentType(Request.HttpContext.User.Identity.Name, affiliateId, customerId, paymentTypeId, bic, iban, accountOwner).ConfigureAwait(false) == 2)
				deliveryTypeId = 0;

			DeliveryTypesPartial returnValue = Data.GetDeliveryTypePartial(affiliateId, paymentTypeId, deliveryTypeId, System.Convert.ToDecimal(orderItemsTotal, System.Globalization.CultureInfo.InvariantCulture));

			return PartialView("DeliveryPaymentDeliveryTypes", returnValue);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult<bool>> UpdateDeliveryType(int affiliateId, int customerId, int deliveryTypeId, string shippingCosts) {
			return await Data.UpdateDeliveryType(Request.HttpContext.User.Identity.Name, affiliateId, customerId, deliveryTypeId, Convert.ToDecimal(shippingCosts, System.Globalization.CultureInfo.InvariantCulture)).ConfigureAwait(false);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult<bool>> AddDeliveryPaymentButtonItem(int affiliateId, int customerId, int deliveryPaymentButtonId) {

			await Data.AddDeliveryPaymentButtonItem(Request.HttpContext.User.Identity.Name, affiliateId, customerId, deliveryPaymentButtonId).ConfigureAwait(false);
			return PartialView("DeliveryPaymentSummary", new InvoiceItems(await Data.GetInvoiceItems(affiliateId, customerId).ConfigureAwait(false)));
		}

		[HttpGet]
		public async Task<ActionResult<InvoiceItems>> GetPaymentSummaryPartial(int affiliateId, int customerId) {
			return PartialView("DeliveryPaymentSummary", new InvoiceItems(await Data.GetInvoiceItems(affiliateId, customerId).ConfigureAwait(false)));
		}

		[HttpGet]
		public async Task<ActionResult<string>> GetBankAccountDetails(int affiliateId, int customerId) {
			return await Data.GetBankAccountDetails(affiliateId, customerId).ConfigureAwait(false);
		}

		[HttpGet]
		public async Task<ActionResult<string>> GetBestGuestCityFromZip(string zip, string street) {
			return await Data.GetBestGuestCityFromZip(zip, street).ConfigureAwait(false);
		}
	}
}
