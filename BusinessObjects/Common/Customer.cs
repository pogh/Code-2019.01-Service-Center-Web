using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Company.BusinessObjects.Common {
	public class Customer : BillingAddress {

		#region CustomerGroup
		public int CustomerGroupId { get; set; }
		public string CustomerGroupName { get; set; }
		public decimal CustomerGroupDiscount { get; set; }
		#endregion

		#region Affiliate
		[Required(ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ValueRequired")]
		[Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ValueRequired")]

		public int AffiliateId { get; set; }
		public string AffiliateKey { get; set; }
		#endregion

		public override string ToString() {
			return string.Concat(FullName, " ", CustomerNumber, " (", Id, ")").Trim();
		}
	}
}
