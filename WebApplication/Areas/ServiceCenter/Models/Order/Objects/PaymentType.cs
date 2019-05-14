using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects {
	public class PaymentType {
		public PaymentType() {
		}

		public PaymentType(int paymentTypeId, string paymentTypeName) : base() {
			PaymentTypeId = paymentTypeId;
			PaymentTypeName = paymentTypeName;
		}

		public int PaymentTypeId { get; set; }
		public string PaymentTypeName { get; set; }

		public override string ToString() {
			return string.Concat(PaymentTypeId, " ", PaymentTypeName);
		}
	}
}
