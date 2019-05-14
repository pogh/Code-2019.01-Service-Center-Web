using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.BusinessObjects.Common;

namespace Company.WebApplication.Areas.ServiceCenter.Data {
	public partial class Order : BaseObject {

		public async Task<Customer> GetCustomer(int customerId) {
			if (customerId < 1) {
				return null;
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getcustomer", new Dictionary<string, object>() { { "customerid", customerId } }).ConfigureAwait(false);
				return JsonConvert.DeserializeObject<Customer>(json);
			}
		}

		public async Task<string> GetCustomerComment(int customerId) {

			if (customerId < 1) {
				return null;
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getcustomercomment", new Dictionary<string, object>() { { "customerid", customerId } }).ConfigureAwait(false);
				return JsonConvert.DeserializeObject<string>(json);
			}
		}

		public async Task<bool> SetCustomerComment(string userName, int customerId, string commentText) {

			if (string.IsNullOrEmpty(userName)
			 || customerId < 1)
				return false;

			using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

				httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
				httpClientHandler.UseDefaultCredentials = true;

				using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
					httpClient.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/setcustomercomment"));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					var response = await httpClient.PostAsJsonAsync("",
										new {
											userName,
											customerId,
											commentText
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
