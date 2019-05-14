using Company.BusinessObjects.Common;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.Views {

	public abstract class OrderBase {

		public OrderBase(Affiliate affiliate, int customerId) {
			Affiliate = affiliate;
			CustomerId = customerId;
		}

		public static string PZNStringFormat {
			get {
				return new string('0', 8);
			}
		}

		public int InvoicePk { get; set; }
		public int InvoiceId { get; set; }
		public int AffiliateId {
			get {
				if (Affiliate == null)
					return 0;
				else
					return Affiliate.AffiliateId;
			}
		}

		public int CustomerId { get; private set; }

		public string CustomerNumber {
			get {
				if (BillingAddress != null)
					return BillingAddress.CustomerNumber;
				else
					return string.Empty;
			}
		}

		public Affiliate Affiliate { get; private set; }

		public int CustomerGroupId { get; set; }
		public string CustomerGroupName { get; set; }
		public decimal CustomerGroupDiscount { get; set; }
		public BillingAddress BillingAddress { get; set; }
	}
}
