using System;
using System.ComponentModel.DataAnnotations;
using Company.BusinessObjects.Common;

namespace Company.WebService.Models.Extensions.WebApplication {
	public class BankInfo {

		[Key]
		public int BankInfoId { get; set; }

		public string IBAN { get; set; }

		public string BIC { get; set; }

		public string AccountOwner { get; set; }

	}
}
