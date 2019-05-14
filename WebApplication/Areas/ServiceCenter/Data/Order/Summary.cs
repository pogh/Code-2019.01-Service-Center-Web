using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Views;

namespace Company.WebApplication.Areas.ServiceCenter.Data {
	public partial class Order : BaseObject {

		public async Task<Summary> GetSummary(int affiliateId, int customerId) {
			if (affiliateId < 1
			 || customerId < 1) {
				return null;
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getinvoice", new Dictionary<string, object>() { { "affiliateId", affiliateId }, { "customerid", customerId } }).ConfigureAwait(false);
				Summary summary = JsonConvert.DeserializeObject<Summary>(json);

				if (summary != null
				 && summary.CustomerGroupId != 0)
					summary.CustomerGroupName = CustomerGroups.Single(x => x.CustomerGroupId == summary.CustomerGroupId).CustomerGroupName;

				return summary;
			}
		}


		public async Task<List<int>> ValidateInvoice(int affiliateId, int customerId) {

			if (affiliateId < 1
			|| customerId < 1) {
				return new List<int>() { 1000 };
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/validateinvoice", new Dictionary<string, object>() { { "affiliateId", affiliateId }, { "customerid", customerId } }).ConfigureAwait(false);
				return JsonConvert.DeserializeObject<List<int>>(json);
			}
		}

		public async Task<int> SendInvoice(string userName, int affiliateId, int customerId) {

			if (string.IsNullOrEmpty(userName)
			 || affiliateId < 1
			 || customerId < 1)
				return 0;

			using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

				httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
				httpClientHandler.UseDefaultCredentials = true;

				using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
					httpClient.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/sendinvoice"));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					var response = await httpClient.PostAsJsonAsync("",
										new {
											userName,
											affiliateId,
											customerId
										}).ConfigureAwait(false);

					int result = await response.Content.ReadAsAsync<int>().ConfigureAwait(false);

					if (response.IsSuccessStatusCode)
						return result;
					else
						return 1000;
				}
			}
		}


		public async Task<bool> SendCustomerComment(string userName, int affiliateId, int customerId) {

			if (string.IsNullOrEmpty(userName)
			 || affiliateId < 1
			 || customerId < 1)
				return false;

			using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

				httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
				httpClientHandler.UseDefaultCredentials = true;

				using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
					httpClient.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/sendcustomercomment"));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					var response = await httpClient.PostAsJsonAsync("",
										new {
											userName,
											affiliateId,
											customerId
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
}
