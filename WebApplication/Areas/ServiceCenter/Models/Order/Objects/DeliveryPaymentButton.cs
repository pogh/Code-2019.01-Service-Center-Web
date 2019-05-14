using Newtonsoft.Json;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects {
	public class DeliveryPaymentButton {

		public const int DISCOUNT_PZN = 9;

		[JsonProperty("DeliveryPaymentButtonPk")]
		public int DeliveryPaymentButtonId { get; set; }
		public int Pzn { get; set; }
		public string DisplayText { get; set; }
		public decimal ItemPrice { get; set; }
		public decimal DisplayFromTotal { get; set; }
		public override string ToString() {
			return string.Concat(DeliveryPaymentButtonId, " ", DisplayText, " ", ItemPrice.ToString("0.00 €", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat));
		}
	}
}
