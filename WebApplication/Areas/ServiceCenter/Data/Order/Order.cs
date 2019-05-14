using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Company.BusinessObjects.Common;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.PartialViews;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Views;

namespace Company.WebApplication.Areas.ServiceCenter.Data {

	public partial class Order : BaseObject {
		public Order(IConfiguration configuration, IMemoryCache memoryCache) : base(configuration, memoryCache) {
		}

		public async Task<int> GetOrCreateCustomerId(int kundenNr) {
			if (kundenNr < 0) {
				return 0;
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getorcreatecustomerid", new Dictionary<string, object>() { { "kundenNr", kundenNr } }).ConfigureAwait(false);
				int customerId = JsonConvert.DeserializeObject<int>(json);
				return customerId;
			}
		}

		public async Task<Invoice> GetOrCreateInvoice(string userName, int affiliateId, int customerId) {
			if (string.IsNullOrEmpty(userName)
			 || affiliateId < 0
			 || customerId < 1) {
				return null;
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getorcreateinvoice", new Dictionary<string, object>() { { "userName", userName }, { "affiliateId", affiliateId }, { "customerid", customerId } }).ConfigureAwait(false);
				Invoice invoice = JsonConvert.DeserializeObject<Invoice>(json);

				if (invoice.CustomerGroupId != 0)
					invoice.CustomerGroupName = CustomerGroups.Single(x => x.CustomerGroupId == invoice.CustomerGroupId).CustomerGroupName;

				return invoice;
			}
		}

		public async Task<DeliveryAddress> GetDeliveryAddress(int affiliateId, int customerId) {

			if (affiliateId < 1
			|| customerId < 1) {
				return null;
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getdeliveryaddress", new Dictionary<string, object>() { { "affiliateId", affiliateId }, { "customerid", customerId } }).ConfigureAwait(false);
				DeliveryAddress deliveryAddress = JsonConvert.DeserializeObject<DeliveryAddress>(json);
				return deliveryAddress;
			}
		}

		public async Task<BillingAddress> GetBillingAddress(int affiliateId, int customerId) {

			if (affiliateId < 1
			|| customerId < 1) {
				return null;
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getbillingaddress", new Dictionary<string, object>() { { "affiliateId", affiliateId }, { "customerid", customerId } }).ConfigureAwait(false);
				BillingAddress billingAddress = JsonConvert.DeserializeObject<BillingAddress>(json);
				return billingAddress;
			}
		}

		public async Task<List<InvoiceItem>> GetInvoiceItems(int affiliateId, int customerId) {

			if (affiliateId < 1
			|| customerId < 1) {
				return null;
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getinvoiceitems", new Dictionary<string, object>() { { "affiliateId", affiliateId }, { "customerid", customerId } }).ConfigureAwait(false);
				List<InvoiceItem> returnValue = JsonConvert.DeserializeObject<List<InvoiceItem>>(json);

				Dictionary<string, object> parameters = new Dictionary<string, object> {
					{ "affiliateId", affiliateId },
					{ "pzn", 0 },
					{ "limit", 1 }
				};

				foreach (int pzn in returnValue.Select(x => x.PZN)) {

					parameters["pzn"] = pzn;

					json = await base.GetJsonAsync("/common/articles/get", parameters).ConfigureAwait(false);
					List<ArticleSearchResultArticle> articles = JsonConvert.DeserializeObject<List<ArticleSearchResultArticle>>(json);

					if (articles.Count() == 1) {
						ArticleSearchResultArticle resultArticle = articles.Single();

						foreach (InvoiceItem invoiceItem in returnValue.Where(x => x.PZN == pzn)) {
							invoiceItem.DisplayName = resultArticle.DisplayName;
							StringBuilder amount = new StringBuilder();
							amount.Append(resultArticle.PackagingAmount);
							amount.Append(" ");
							amount.Append(resultArticle.PackagingAmountUnit);

							if (resultArticle.PackagingAmount != resultArticle.PackagingAmountTotal.ToString(System.Globalization.CultureInfo.InvariantCulture)) {
								amount.Append(" (");
								amount.Append(resultArticle.PackagingAmountTotal);
								amount.Append(" ");
								amount.Append(resultArticle.PackagingAmountUnit);
								amount.Append(")");
							}

							invoiceItem.Amount = amount.ToString();
						}
					}
				}

				return returnValue;
			}
		}

		public List<PaymentType> GetPaymentTypes(int affiliateId) {

			List<PaymentType> returnValue = new List<PaymentType>();

			var paymentMatrixRows = PaymentMatrix.Where(x => x.AffiliateId == affiliateId);

			foreach (int paymentTypeId in paymentMatrixRows.Select(x => x.PaymentTypeId).Distinct())
				returnValue.Add(new PaymentType(paymentTypeId, paymentMatrixRows.First(x => x.PaymentTypeId == paymentTypeId).PaymentTypeName));

			return returnValue;
		}

		public List<DeliveryType> GetDeliveryTypes(int affiliateId, int paymentTypeId) {

			List<DeliveryType> returnValue = new List<DeliveryType>();

			var paymentMatrixRows = PaymentMatrix.Where(x => x.AffiliateId == affiliateId && x.PaymentTypeId == paymentTypeId);

			foreach (var deliveryType in paymentMatrixRows)
				returnValue.Add(new DeliveryType(deliveryType.DeliveryTypeId, deliveryType.DeliveryTypeName, deliveryType.ShippingCosts, deliveryType.ShippingCostsLimit));

			return returnValue;
		}

		public async Task<string> GetBestGuestCityFromZip(string zip, string street) {
			if (string.IsNullOrEmpty(zip)
			|| zip.Length < 4)
				return null;

			if (string.IsNullOrEmpty(street)
			|| street.Length < 3)
				return null;

			string json = await base.GetJsonAsync("/common/lookups/getbestguestcityfromzip", new Dictionary<string, object>() { { "zip", zip }, { "street", street } }).ConfigureAwait(false);
			return JsonConvert.DeserializeObject<string>(json);
		}
	}
}
