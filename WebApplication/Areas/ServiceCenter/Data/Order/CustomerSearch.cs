using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.BusinessObjects.Common;

namespace Company.WebApplication.Areas.ServiceCenter.Data {
	public partial class Order : BaseObject {
		public async Task<List<Customer>> CustomerSearchResults(int[] affiliateIds, string customerNumber = null, string name = null, string zip = null, string orderNr = null, string emailAddress = null, string invoiceNumber = null, int limit = 0) {
			           
			Dictionary<string, object> parameters = new Dictionary<string, object>();

			if (affiliateIds != null && affiliateIds.Length > 0) {
				parameters.Add("affiliateIds", affiliateIds);
			}

			if (!string.IsNullOrEmpty(customerNumber)) {
				parameters.Add("customernumber", customerNumber);
			}

			if (!string.IsNullOrEmpty(name)) {
				parameters.Add("name", name);
			}

			if (!string.IsNullOrEmpty(zip)) {
				parameters.Add("zip", zip);
			}

			if (!string.IsNullOrEmpty(orderNr)) {
				parameters.Add("orderNr", orderNr);
			}

			if (!string.IsNullOrEmpty(emailAddress)) {
				parameters.Add("emailAddress", emailAddress);
			}

			if (!string.IsNullOrEmpty(invoiceNumber)) {
				parameters.Add("invoiceNumber", invoiceNumber);
			}

			if (limit != 0) {
				parameters.Add("limit", limit);
			}

			if (parameters.Count == 0) {
				return new List<Customer>();
			}
			else {
				string json = await base.GetJsonAsync("/common/customers/search", parameters).ConfigureAwait(false);
				return JsonConvert.DeserializeObject<List<Customer>>(json);
			}
		}
	}
}
