using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Company.BusinessObjects.Common {
	public class BillingAddress : DeliveryAddress {

		[StringLength(16, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string CustomerNumber { get; set; }

		[StringLength(20, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		[Required(ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ValueRequired")]
		public string TelephoneNumber { get; set; }

		[StringLength(255, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string MobileNumber { get; set; }

		[StringLength(20, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string FaxNumber { get; set; }

		[EmailAddress]
		[StringLength(100, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string EmailAddress { get; set; }

		[Required(ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ValueRequired")]
		[DataType(DataType.Date)]
		[Range(typeof(DateTime), "1900-01-01", "2019-12-31", ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ValueInvalid")]
		public DateTime? DoB { get; set; }
	}
}
