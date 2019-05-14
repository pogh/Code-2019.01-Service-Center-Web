using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects {
	public class InvoiceItem {
		[JsonProperty("InvoiceItemPk")]
		public int InvoiceItemId { get; set; }
		public int PZN { get; set; }
		public string DisplayName { get; set; }
		public string Amount { get; set; }
		public int Quantity { get; set; }
		public decimal VAT { get; set; }
		[JsonProperty("ItemPrice")]
		public decimal Price { get; set; }
		[JsonProperty("ItemSavings")]
		public decimal PriceSavings { get; set; }

		public decimal TotalPrice {
			get {
				return Price * Quantity;
			}
		}
		public decimal TotalSaving {
			get {
				return PriceSavings * Quantity;
			}
		}

		public override string ToString() {
			return string.Concat(PZN, " ", DisplayName, " ", Price.ToString("0.00 €", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat));
		}
	}
}
