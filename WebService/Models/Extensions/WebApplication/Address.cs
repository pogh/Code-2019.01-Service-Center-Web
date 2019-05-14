using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Company.WebService.Models.Extensions.WebApplication {
	public class Address {

		[Key]
		public int AddressId { get; set; }

		[StringLength(50)]
		public string Title { get; set; }

		[StringLength(150)]
		public string Name1 { get; set; }

		[StringLength(150)]
		public string Name2 { get; set; }

		[StringLength(150)]
		public string Name3 { get; set; }

		[StringLength(150)]
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string CompanyName { get; set; }
		public string Street { get; set; }

		[StringLength(10)]
		public string Zip { get; set; }

		[StringLength(100)]
		public string City { get; set; }

		[StringLength(2)]
		public string Country { get; set; }
		public string AdditionalLine { get; set; }

		public override string ToString() {
			return string.Concat(string.Concat(Title, " ", FirstName, " ", LastName).Trim(), ", ", string.IsNullOrEmpty(CompanyName) ? "" : string.Concat("(", CompanyName, ")"), " ", Street, ", ", string.IsNullOrEmpty(AdditionalLine) ? "" : string.Concat(AdditionalLine, ","), string.Concat(Zip, " ", City).Trim(), " ", Country).Trim();
		}
	}
}
