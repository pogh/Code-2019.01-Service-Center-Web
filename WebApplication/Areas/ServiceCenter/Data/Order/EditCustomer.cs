using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.BusinessObjects.Common;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Views;

namespace Company.WebApplication.Areas.ServiceCenter.Data {

	public partial class Order : BaseObject {
		private class CustomerUpdate : Customer {
			public CustomerUpdate(Customer customer) {
				foreach (PropertyInfo propertyInfo in GetType().GetProperties()) {
					var thisProperty = customer.GetType().GetProperty(propertyInfo.Name);
					if (thisProperty != null
					 && thisProperty.CanWrite)
						propertyInfo.SetValue(this, thisProperty.GetValue(customer));
				}
			}

			public string UserName { get; set; }
		}

		public async Task<int> UpdateCustomerEdit(string userName, Customer customer) {
			if (customer.Id < 1) {
				return -1;
			}
			else {

				using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

					httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
					httpClientHandler.UseDefaultCredentials = true;

					using (HttpClient client = new HttpClient(httpClientHandler)) {
						client.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/setcustomeredit"));

						CustomerUpdate customerUpdate = new CustomerUpdate(customer) {
							UserName = userName
						};

						var response = await client.PostAsJsonAsync("", customerUpdate).ConfigureAwait(false);

						int result = await response.Content.ReadAsAsync<int>().ConfigureAwait(false);

						if (response.IsSuccessStatusCode)
							return result;
						else
							return 0;
					}
				}
			}
		}

		public async Task<CustomerEdit> GetCustomerEdit(int affiliateId, int customerId) {
			if (customerId < 1) {
				return null;
			}
			else {
				string json = await GetJsonAsync("/webapplication/servicecenter/getcustomeredit", new Dictionary<string, object>() { { "affiliateId", affiliateId }, { "customerid", customerId } }).ConfigureAwait(false);
				CustomerEdit customerEdit = JsonConvert.DeserializeObject<CustomerEdit>(json);

				return customerEdit;
			}
		}
	}
}
