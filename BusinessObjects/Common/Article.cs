using System.ComponentModel.DataAnnotations;

namespace Company.BusinessObjects.Common {
	public class Article {

		[Key]
		public int AffiliateArticleId { get; set; }

		public int ArticleId { get; set; }

		public int PZN { get; set; }

		public string Name { get; set; }
		public string DisplayName { get; set; }

		public bool IsOnline { get; set; }

		public bool IsLocked { get; set; }

		/// <summary>
		/// Rezeptpflichtig
		/// </summary>
		public bool IsPrescriptionOnly { get; set; } 

		/// <summary>
		/// Wert kommt aus [dbo].[ArtikelstammVerfuegbarkeitStatus]
		/// </summary>
		public int AvailabilityType { get; set; }

		public int OrderLimit { get; set; }

		public string PackagingAmount { get; set; }

		public string PackagingAmountUnit { get; set; }

		public int PackagingAmountTotal { get; set; }

		public decimal SalePrice { get; set; }

		public decimal VAT { get; set; }

		/// <summary>
		///  Recommended Retail Price (Unverbindliche Preisempfehlung)
		/// </summary>
		public decimal RRP { get; set; }

		public decimal PriceSavings {
			get {
				return RRP - SalePrice;
			}
		}
		public override string ToString() {
			return string.Concat(this.Name, " (", this.PZN.ToString(new string('0', 8), System.Globalization.CultureInfo.InvariantCulture), ")");
		}
	}
}
