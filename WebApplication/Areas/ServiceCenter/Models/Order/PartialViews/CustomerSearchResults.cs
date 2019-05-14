using System.Collections.Generic;
using Company.BusinessObjects.Common;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.PartialViews {
	public class CustomerSearchResults
    {
		public CustomerSearchResults() {
			Customers = new List<Customer>();
		}
        public List<Customer> Customers { get; }
        public bool NoResults { set; get; }
        public bool ToManyResults { set; get; }
    }
}
