using System.Collections.Generic;
using System.Linq;
using Company.BusinessObjects.Common;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.Views {
	public class DeliveryPayment : OrderBase {

		public DeliveryPayment(Affiliate affiliate, int customerId) : base(affiliate, customerId) {
			InvoiceItems = new InvoiceItems();
			DeliveryTypes = new List<DeliveryType>();
			PaymentTypes = new List<PaymentType>();
			DeliveryAddresses = new List<DeliveryAddress>();
			DeliveryPaymentButtons = new List<DeliveryPaymentButton>();
		}

		public InvoiceItems InvoiceItems { get; }

		public List<DeliveryType> DeliveryTypes { get; }
		public int? DeliveryTypeId { get; set; }

		public List<PaymentType> PaymentTypes { get; }
		public int? PaymentTypeId { get; set; }
		public string Iban { get; set; }
		public string Bic { get; set; }
		public string AccountOwner { get; set; }

		public DeliveryAddress DeliveryAddress { get; set; }
		public List<DeliveryAddress> DeliveryAddresses { get; }

		public List<DeliveryPaymentButton> DeliveryPaymentButtons { get; }
	}
}
