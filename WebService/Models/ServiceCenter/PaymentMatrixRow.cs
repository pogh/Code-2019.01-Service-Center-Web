namespace Company.WebService.Models.MAINDB {
	public partial class PaymentMatrixRow {
		public int RowId { get; set; }
		public int AffiliateId { get; set; }
		public int PaymentTypeId { get; set; }
		public string PaymentTypeName { get; set; }
		public int DeliveryTypeId { get; set; }
		public string DeliveryTypeName { get; set; }
		public decimal ShippingCosts { get; set; }
		public decimal ShippingCostsLimit { get; set; }
	}
}
