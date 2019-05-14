using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects {
	public class InvoiceItems {

		public InvoiceItems() {

			Items = new List<InvoiceItem>();
			InitialisePznsGroupings();

		}

		public InvoiceItems(List<InvoiceItem> invoiceItems): this() {
			Items.AddRange(invoiceItems);
		}

		public List<InvoiceItem> Items { get; }

		#region Groupings
		/*
		PZN	Name
		1	Versandkosten
		2	Nachnahmekosten
		3	Express Versandkosten
		7	GoGreen Zuschlag
		8	Zuschlag Eigenhändig

		4	Service Gebühr MAINDB Medikation
		10	Bevorzugte Bestellabwicklung
		23	Lieferung Prüfmediaktion
		8000800	Rezept Einreicher (MAINDB)

		6	Rabat %?
		9	Rabatt
		9500500 Gutschein
	
		17	Gutschriftsbetrag
		20	Gutschriftsbetrag
		21	Retour Ware 16%
		22	Retour Ware 7%

 		*/

		List<int> shippingCostsPzns;
		List<int> additionalFeesPzns;
		List<int> discountPzns;
		List<int> creditPzns;
		List<int> allKnownFees;

		void InitialisePznsGroupings() {
			shippingCostsPzns = new List<int>() { 1, 2, 3, 7, 8 };
			additionalFeesPzns = new List<int>() { 4, 10, 23, 8000800 };
			discountPzns = new List<int>() { 6, 9, 9500500 };
			creditPzns = new List<int>() { 17, 20, 21, 22 };

			allKnownFees = shippingCostsPzns
						.Union(additionalFeesPzns)
						.Union(discountPzns)
						.Union(creditPzns)
						.ToList();
		}

		public List<int> AllKnownFeePzns {
			get {
				return allKnownFees;
			}
		}

		public int OrderItemPznLimit {
			get {
				return 100;
			}
		}

		public decimal OrderItemsTotal {
			get {
				return Items.Where(x => x.PZN >= OrderItemPznLimit).Sum(x => x.TotalPrice);
			}
		}

		public List<InvoiceItem> OrderItems {
			get {
				return Items.Where(x => x.PZN >= OrderItemPznLimit).ToList();
			}
		}

		public decimal ShippingCostsTotal {
			get {
				return Items.Where(x => shippingCostsPzns.Contains(x.PZN)).Sum(x => x.TotalPrice);
			}
		}

		public List<InvoiceItem> ShippingCostItems {
			get {
				return Items.Where(x => shippingCostsPzns.Contains(x.PZN)).ToList();
			}
		}

		public decimal AdditionalFeesTotal {
			get {
				return Items.Where(x => additionalFeesPzns.Contains(x.PZN)).Sum(x => x.TotalPrice);
			}
		}

		public List<InvoiceItem> AdditionalFeeItems {
			get {
				return Items.Where(x => additionalFeesPzns.Contains(x.PZN)).ToList();
			}
		}

		public decimal DiscountsTotal {
			get {
				return Items.Where(x => discountPzns.Contains(x.PZN)).Sum(x => x.TotalPrice);
			}
		}

		public List<InvoiceItem> DiscountItems {
			get {
				return Items.Where(x => discountPzns.Contains(x.PZN)).ToList();
			}
		}

		public decimal CreditsTotal {
			get {
				return Items.Where(x => creditPzns.Contains(x.PZN)).Sum(x => x.TotalPrice);
			}
		}

		public List<InvoiceItem> CreditItems {
			get {
				return Items.Where(x => creditPzns.Contains(x.PZN)).ToList();
			}
		}

		public decimal OthersTotal {
			get {
				return Items.Where(x => x.PZN < OrderItemPznLimit && !allKnownFees.Contains(x.PZN)).Sum(x => x.TotalPrice);
			}
		}

		public List<InvoiceItem> OtherItems {
			get {
				return Items.Where(x => x.PZN < OrderItemPznLimit && !allKnownFees.Contains(x.PZN)).ToList();
			}
		}

		public decimal GrandTotal {
			get {
				return Items.Sum(x => x.TotalPrice);
			}
		}
		#endregion

	}
}
