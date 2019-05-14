using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Views;

namespace Company.WebApplication.Areas.ServiceCenter.Controllers {

	public partial class OrderController : BaseController<OrderController> {

		[HttpGet]
		[Route("ServiceCenter/Order/StartCustomerEdit/{affiliateId?}/{kundenNr?}")]
		public async Task<ActionResult<Invoice>> StartCustomerEdit(int affiliateId = 0, int kundenNr = 0) {
			int customerId = await Data.GetOrCreateCustomerId(kundenNr).ConfigureAwait(false);
			return RedirectToAction("CustomerEdit", new { affiliateId, customerId });
		}

		[HttpGet]
		[Route("ServiceCenter/Order/CustomerEdit/{affiliateId}/{customerId}")]
		public async Task<ActionResult<CustomerEdit>> CustomerEdit(int affiliateId, int customerId) {

			CustomerEdit customerEdit = await Data.GetCustomerEdit(affiliateId, customerId).ConfigureAwait(false);

			if (customerEdit == null) {
				await Data.GetOrCreateInvoice(Request.HttpContext.User.Identity.Name, affiliateId, customerId).ConfigureAwait(false);
				customerEdit = await Data.GetCustomerEdit(affiliateId, customerId).ConfigureAwait(false);
				if (string.IsNullOrEmpty(customerEdit.Country))
					customerEdit.Country = "DE";
				customerEdit.CustomerGroupId = 9;
				ModelState.AddModelError("", "New Customer");  // To make the 'Next Arrow Disappear'
			}

			if (string.IsNullOrEmpty(customerEdit.FullName))
				ViewData["LocalizedTitle"] = "NewCustomerTitle";
			else if (string.IsNullOrEmpty(customerEdit.AffiliateKey))
				ViewData["Title"] = customerEdit.FullName;
			else
				ViewData["Title"] = string.Concat(customerEdit.FullName, " (", customerEdit.AffiliateKey, ")");

			customerEdit.Affiliates.AddRange(Data.Affiliates);
			customerEdit.CustomerGroups.AddRange(Data.CustomerGroups);

			ViewData["WebServiceHostName"] = Data.WebServiceHostName;
			SetAffiliateColours(0.25);

			return View(customerEdit);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("ServiceCenter/Order/CustomerEdit/{affiliateId}/{customerId}")]
		public async Task<ActionResult<CustomerEdit>> CustomerEdit(CustomerEdit customerEdit) {

			if (ModelState.IsValid) {
				if (await Data.UpdateCustomerEdit(Request.HttpContext.User.Identity.Name, customerEdit).ConfigureAwait(false) == 2) {
					ViewData["AddressStreet"] = await Data.GetDeliveryAddressStreet(customerEdit.AffiliateId, customerEdit.CustomerId).ConfigureAwait(false);
				}

				//return RedirectToAction("CustomerEdit", new { customerEdit.AffiliateId, customerEdit.CustomerId } );
			}

			customerEdit.Affiliates.AddRange(Data.Affiliates);
			customerEdit.CustomerGroups.AddRange(Data.CustomerGroups);

			ViewData["WebServiceHostName"] = Data.WebServiceHostName;
			SetAffiliateColours(0.25);

			return View(customerEdit);
		}
	}
}
