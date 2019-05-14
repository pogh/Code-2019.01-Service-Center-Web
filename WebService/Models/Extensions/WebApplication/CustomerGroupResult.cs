using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.WebService.Models.Extensions.WebApplication {
	public class CustomerGroupResult {
		public int CustomerGroupId { get; set;  }
		public string CustomerGroupName { get; set; }
		public decimal CustomerGroupDiscount { get; set; }
	}
}
