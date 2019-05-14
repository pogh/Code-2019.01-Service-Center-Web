using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.PartialViews {
	public class DeliveryTypesPartial {

		public DeliveryTypesPartial() {
			DeliveryTypes = new List<DeliveryType>();
		}

		public int SelectedDeliveryTypeId { get; set; }
		public decimal OrderTotal { get; set; }
		public List<DeliveryType> DeliveryTypes { get;  }

	}
}
