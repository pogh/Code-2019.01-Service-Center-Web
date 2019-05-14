using System;
using System.Collections.Generic;
using System.Text;

namespace Company.BusinessObjects.Common {
	public class DeliveryAddress : AddressBase {
		public int OrderId { get; set; }
	}
}
