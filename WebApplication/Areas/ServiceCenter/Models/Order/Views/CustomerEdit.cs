using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Company.BusinessObjects.Common;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.Views {
	public class CustomerEdit : Customer {
		public CustomerEdit() {
			CustomerGroups = new List<CustomerGroupResult>();
			Affiliates = new List<Affiliate>();
		}

		public CustomerEdit(Customer customer) : this() {
			foreach (PropertyInfo propertyInfo in this.GetType().GetProperties()) {
				var thisProperty = customer.GetType().GetProperty(propertyInfo.Name);
				if (thisProperty != null
				 && thisProperty.CanWrite)
					propertyInfo.SetValue(this, thisProperty.GetValue(customer));
			}
		}

		public int CustomerId { get; set; }  // Id for this object = InvoicePk

		public List<Affiliate> Affiliates { get; }
		public List<CustomerGroupResult> CustomerGroups { get; }
	}
}
