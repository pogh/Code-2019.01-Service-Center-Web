using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Company.BusinessObjects.Common {

	/// This is used in random places around the database
	public enum CountryKey {
		DE = 1,
		AT = 2
	}
	public abstract class AddressBase {

		[Key]
		public int Id { get; set; }

		[Required(ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ValueRequired")]
		[StringLength(50, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string Title { get; set; }

		[Required(ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ValueRequired")]
		[StringLength(100, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string FirstName { get; set; }

		[Required(ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ValueRequired")]
		[StringLength(255, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string LastName { get; set; }

		[StringLength(255, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string CompanyName { get; set; }

		[StringLength(255, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string FullName => string.Concat(Title, " ", FirstName, " ", LastName).Trim();

		[Required(ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ValueRequired")]
		[StringLength(255, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string Street { get; set; }

		[Required(ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ValueRequired")]
		[StringLength(50, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string Zip { get; set; }

		[Required(ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ValueRequired")]
		[StringLength(255, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string City { get; set; }

		[StringLength(2, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string Country { get; set; }

		[StringLength(255, ErrorMessageResourceType = typeof(Resources.GeneralValidation), ErrorMessageResourceName = "ExceedsLength")]
		public string AdditionalLine { get; set; }

		public string FullAddress {
			get {
				StringBuilder sb = new StringBuilder(Street);

				if (!string.IsNullOrEmpty(AdditionalLine))
					sb.Append(string.Concat(", ", AdditionalLine));

				if (!string.IsNullOrEmpty(Zip)
				|| !string.IsNullOrEmpty(City)) {
					if (sb.Length > 0)
						sb.Append(", ");

					sb.Append(string.Concat(Zip, " ", City).Trim());
				}

				return sb.ToString();
			}
		}

		public override string ToString() {
			if (string.IsNullOrEmpty(FullName) && string.IsNullOrEmpty(FullAddress))
				return null;

			if (!string.IsNullOrEmpty(FullName) && string.IsNullOrEmpty(FullAddress))
				return FullName;

			if (string.IsNullOrEmpty(FullName) && !string.IsNullOrEmpty(FullAddress))
				return FullAddress;

			return string.Concat(FullName, ", ", FullAddress);
		}
	}
}
