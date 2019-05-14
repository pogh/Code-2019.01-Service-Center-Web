using System.Collections.Generic;
using System.Linq;
using Company.BusinessObjects.Common;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.Views {
	public class Invoice : OrderBase {

		public Invoice(Affiliate affiliate, int customerId) : base(affiliate, customerId) {
			InvoiceItems = new InvoiceItems();
		}

		public InvoiceItems InvoiceItems { get; }
	}
}
