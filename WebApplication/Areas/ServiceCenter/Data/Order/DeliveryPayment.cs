using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.BusinessObjects.Common;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.PartialViews;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Views;

namespace Company.WebApplication.Areas.ServiceCenter.Data {
	public partial class Order : BaseObject {

		public async Task<DeliveryPayment> GetDeliveryPayment(int affiliateId, int customerId) {
			if (affiliateId < 1
			 || customerId < 1) {
				return null;
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getinvoice", new Dictionary<string, object>() { { "affiliateId", affiliateId }, { "customerid", customerId } }).ConfigureAwait(false);
				DeliveryPayment deliveryPayment = JsonConvert.DeserializeObject<DeliveryPayment>(json);

				if (deliveryPayment.CustomerGroupId != 0)
					deliveryPayment.CustomerGroupName = CustomerGroups.Single(x => x.CustomerGroupId == deliveryPayment.CustomerGroupId).CustomerGroupName;

				return deliveryPayment;
			}
		}

		public async Task<List<DeliveryAddress>> GetDeliveryAddresses(int customerId) {
			if (customerId < 1) {
				return null;
			}
			else {

				string json = await base.GetJsonAsync("/webapplication/servicecenter/getdeliveryaddresses", new Dictionary<string, object>() { { "customerid", customerId } }).ConfigureAwait(false);
				List<DeliveryAddress> deliveryAddresses = JsonConvert.DeserializeObject<List<DeliveryAddress>>(json);

				if (deliveryAddresses == null)
					deliveryAddresses = new List<DeliveryAddress>();

				return deliveryAddresses;
			}
		}

		public async Task<string> GetDeliveryAddressStreet(int affiliateId, int customerId) {
			if (affiliateId < 0
			 || customerId < 1) {
				return null;
			}
			else {

				string json = await base.GetJsonAsync("/webapplication/servicecenter/getdeliveryaddressstreet", new Dictionary<string, object>() { { "affiliateId", affiliateId }, { "customerid", customerId } }).ConfigureAwait(false);
				return JsonConvert.DeserializeObject<string>(json);
			}
		}

		public async Task<int> UpdateDeliveryAddress(string userName, int affiliateId, int customerId, string title = null, string firstName = null, string lastName = null, string companyName = null, string street = null, string city = null, string zip = null, string country = null, string additionalLine = null) {

			if (string.IsNullOrEmpty(userName)
			 || affiliateId < 1
			 || customerId < 1)
				return -1;

			using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

				httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
				httpClientHandler.UseDefaultCredentials = true;

				using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
					httpClient.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/updatedeliveryaddress"));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					var response = await httpClient.PostAsJsonAsync("",
										new {
											userName,
											affiliateId,
											customerId,
											title,
											firstName,
											lastName,
											companyName,
											street,
											city,
											zip,
											country,
											additionalLine
										}).ConfigureAwait(false);

					int result = await response.Content.ReadAsAsync<int>().ConfigureAwait(false);

					if (response.IsSuccessStatusCode)
						return result;
					else
						return 0;
				}
			}
		}

		public async Task<int> UpdatePaymentType(string userName, int affiliateId, int customerId, int paymentTypeId, string bic, string iban, string accountOwner) {

			if (string.IsNullOrEmpty(userName)
			 || affiliateId < 1
			 || customerId < 1
			 || paymentTypeId < 1)
				return 0;

			using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

				httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
				httpClientHandler.UseDefaultCredentials = true;

				using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
					httpClient.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/updatepaymenttype"));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					var response = await httpClient.PostAsJsonAsync("",
										new {
											userName,
											affiliateId,
											customerId,
											paymentTypeId,
											bic,
											iban,
											accountOwner
										}).ConfigureAwait(false);

					int result = await response.Content.ReadAsAsync<int>().ConfigureAwait(false);

					if (response.IsSuccessStatusCode)
						return result;
					else
						return 0;
				}
			}
		}

		public async Task<bool> AddDeliveryPaymentButtonItem(string userName, int affiliateId, int customerId, int deliveryPaymentButtonId) {

			if (string.IsNullOrEmpty(userName)
			 || affiliateId < 1
			 || customerId < 1
			 || deliveryPaymentButtonId < 1)
				return false;

			using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

				httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
				httpClientHandler.UseDefaultCredentials = true;

				using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
					httpClient.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/adddeliverypaymentbuttonitem"));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					var response = await httpClient.PostAsJsonAsync("",
										new {
											userName,
											affiliateId,
											customerId,
											deliveryPaymentButtonId
										}).ConfigureAwait(false);

					bool result = await response.Content.ReadAsAsync<bool>().ConfigureAwait(false);

					if (response.IsSuccessStatusCode)
						return result;
					else
						return false;
				}
			}
		}

		public DeliveryTypesPartial GetDeliveryTypePartial(int affiliateId, int paymentTypeId, int selectedDeliveryTypeId, decimal orderTotal) {

			DeliveryTypesPartial returnModel = new DeliveryTypesPartial();

			returnModel.DeliveryTypes.AddRange(GetDeliveryTypes(affiliateId, paymentTypeId));
			returnModel.SelectedDeliveryTypeId = selectedDeliveryTypeId;
			returnModel.OrderTotal = orderTotal;

			return returnModel;
		}


		public async Task<bool> UpdateDeliveryType(string userName, int affiliateId, int customerId, int deliveryTypeId, decimal shippingCosts) {

			if (string.IsNullOrEmpty(userName)
			|| affiliateId < 1
			|| customerId < 0
			|| deliveryTypeId < 0) {
				return false;
			}
			else {

				using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

					httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
					httpClientHandler.UseDefaultCredentials = true;

					using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
						httpClient.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/updatedeliverytype"));
						httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

						var response = await httpClient.PostAsJsonAsync("",
											new {
												userName,
												affiliateId,
												customerId,
												deliveryTypeId,
												shippingCosts
											}).ConfigureAwait(false);

						bool result = await response.Content.ReadAsAsync<bool>().ConfigureAwait(false);

						if (response.IsSuccessStatusCode)
							return result;
						else
							return false;
					}
				}
			}
		}

		public async Task<List<DeliveryPaymentButton>> GetDeliveryPaymentButtons(int affiliateId) {
			if (affiliateId < 1) {
				return null;
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getdeliverypaymentbuttons", new Dictionary<string, object>() { { "affiliateId", affiliateId } }).ConfigureAwait(false);
				return JsonConvert.DeserializeObject<List<DeliveryPaymentButton>>(json);
			}
		}

		public async Task<string> GetBankAccountDetails(int affiliateId, int customerId) {
			if (affiliateId < 1
			|| customerId < 1) {
				return null;
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getlastbankinfo", new Dictionary<string, object>() { { "affiliateId", affiliateId }, { "customerId", customerId } }).ConfigureAwait(false);
				return json;
			}
		}
	}
}
