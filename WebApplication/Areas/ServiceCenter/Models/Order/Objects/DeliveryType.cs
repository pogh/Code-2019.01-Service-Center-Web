using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects {
	public class DeliveryType {

		public DeliveryType() {

		}

		public DeliveryType(int deliveryTypeId, string deliveryTypeName, decimal shippingCosts, decimal shippingLimits) {
			DeliveryTypeId = deliveryTypeId;
			DeliveryTypeName = deliveryTypeName;
			ShippingCosts = shippingCosts;
			ShippingLimits = shippingLimits;
		}

		public int DeliveryTypeId { get; set; }
		public string DeliveryTypeName { get; set; }
		public decimal ShippingCosts { get; set; }
		public decimal ShippingLimits { get; set; }

		public override string ToString() {
			return string.Concat(DeliveryTypeId, " ", DeliveryTypeName, " ", ShippingCosts.ToString("0.00 €", System.Globalization.CultureInfo.InvariantCulture));
		}
	}
}
