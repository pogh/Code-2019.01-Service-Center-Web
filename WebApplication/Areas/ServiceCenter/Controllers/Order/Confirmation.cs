using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Views;

namespace Company.WebApplication.Areas.ServiceCenter.Controllers {

	public partial class OrderController : BaseController<OrderController> {

		[HttpGet]
		[Route("ServiceCenter/Order/Confirmation/{affiliateId}/{customerId}/{invoiceId}")]
		public async Task<ActionResult<Confirmation>> Confirmation(int affiliateId, int customerId, int invoiceId) {

			Confirmation returnModel = new Confirmation(Data.Affiliates.Single(x => x.AffiliateId == affiliateId), customerId) {
				BillingAddress = await Data.GetBillingAddress(affiliateId, customerId).ConfigureAwait(false)
			};
			returnModel.InvoiceId = invoiceId;

			ViewData["Title"] = string.Concat(returnModel.BillingAddress.FullName, " (", returnModel.Affiliate.Key, ")");
			ViewData["WebServiceHostName"] = Data.WebServiceHostName;
			SetAffiliateColours(0.25);

			return View(returnModel);
		}
	}
}
