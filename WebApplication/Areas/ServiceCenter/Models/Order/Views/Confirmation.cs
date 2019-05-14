using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company.BusinessObjects.Common;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.Views {
	public class Confirmation : OrderBase {
		public Confirmation(Affiliate affiliate, int customerId) : base(affiliate, customerId) {
		}
	}
}
