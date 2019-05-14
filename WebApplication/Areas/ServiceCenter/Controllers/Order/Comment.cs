using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Company.BusinessObjects.Common;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Views;

namespace Company.WebApplication.Areas.ServiceCenter.Controllers {

	public partial class OrderController : BaseController<OrderController> {

		[HttpGet]
		[Route("ServiceCenter/Order/StartComment/{affiliateId}/{kundenNr}")]
		public async Task<ActionResult<Invoice>> StartComment(int affiliateId, int kundenNr) {
			int customerId = await Data.GetOrCreateCustomerId(kundenNr).ConfigureAwait(false);
			return RedirectToAction("Comment", new { affiliateId, customerId });
		}

		[HttpGet]
		[Route("ServiceCenter/Order/Comment/{affiliateId}/{customerId}")]
		public async Task<ActionResult> Comment(int affiliateId, int customerId) {

			Comment comment = new Comment(Data.Affiliates.Single(x => x.AffiliateId == affiliateId), customerId) {
				BillingAddress = new BillingAddress(),
			};

			Customer customer = await Data.GetCustomer(customerId).ConfigureAwait(false);
			if (customer != null) {
				foreach (PropertyInfo propertyInfo in comment.BillingAddress.GetType().GetProperties()) {
					var thisProperty = customer.GetType().GetProperty(propertyInfo.Name);
					if (thisProperty != null
					 && thisProperty.CanWrite)
						propertyInfo.SetValue(comment.BillingAddress, thisProperty.GetValue(customer));
				}

				comment.CommentText = await Data.GetCustomerComment(customerId).ConfigureAwait(false);
				comment.CustomerGroupId = customer.CustomerGroupId;
				comment.CustomerGroupName = customer.CustomerGroupName;
				comment.CustomerGroupDiscount = customer.CustomerGroupDiscount;

				comment.InvoicePk = 1;  // Abused to report whether we have found a customer
			}
			else {
				comment.InvoicePk = 0;
			}

			ViewData["Title"] = string.Concat(comment.BillingAddress.FullName, " (", comment.Affiliate.Key, ")");
			ViewData["WebServiceHostName"] = Data.WebServiceHostName;
			SetAffiliateColours(0.25);

			return View(comment);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult<bool>> SetCustomerComment(int affiliateId, int customerId, string commentText) {
			return await Data.SetCustomerComment(Request.HttpContext.User.Identity.Name, customerId, commentText).ConfigureAwait(false);
		}
	}
}
