using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Company.BusinessObjects.Common;
using Company.WebService.Controllers.Common;
using Company.WebService.Models.MAINDB;
using Company.WebService.Models.Extensions.WebApplication;
using Company.WebService.Models.ServiceCenter;

namespace Company.WebService.Controllers.WebApplication {

	public class ServiceCenterController : BaseController {

		public ServiceCenterController(MAINDBContext context1, ServiceCenterContext context2, IConfiguration configuration) : base(context1, context2, configuration) {
			string nonOrderItemPznsString = Configuration("NonOrderItemPzns");
			NonOrderItemPzns = nonOrderItemPznsString.Split(',', System.StringSplitOptions.RemoveEmptyEntries).Select(x => System.Convert.ToInt32(x, System.Globalization.CultureInfo.InvariantCulture)).ToList();
		}

		/// <summary>
		/// Pzns less than 100 cannot be ordered, but there are also 'not real items' which shouldn't be ordered but have a Pzn greater than 100, e.g. Vouchers
		/// </summary>
		private List<int> NonOrderItemPzns { get; }

		readonly Dictionary<string, decimal> _pznVat = new Dictionary<string, decimal>();
		private decimal GetVAT(int affiliateId, int pzn) {

			decimal returnValue = 0;
			string key = string.Concat(affiliateId, ".", pzn);

			lock (_pznVat) {
				if (_pznVat.ContainsKey(key))
					returnValue = _pznVat[key];
			}

			if (returnValue == 0) {

				ArticlesController articlesController = new ArticlesController(MAINDBContext);
				ActionResult<decimal> actionResult = articlesController.GetVAT(affiliateId, pzn);

				returnValue = actionResult.Value;

				lock (_pznVat) {
					_pznVat[key] = returnValue;
				}

				articlesController = null;
			}
			return returnValue;
		}

		[HttpGet]
		public ActionResult<int> GetOrCreateCustomerId(int kundenNr) {
			int returnValue = 0;

			SqlParameter returnSqlParameter = new SqlParameter() {
				ParameterName = "@returnValue",
				SqlDbType = System.Data.SqlDbType.Int,
				Direction = System.Data.ParameterDirection.Output
			};

			object result;
			ServiceCenterContext.Database.ExecuteSqlCommand("SELECT @returnValue = CustomerId FROM [ServiceCenter].[CustomerMapping] WHERE KundenNr = {0};", kundenNr, returnSqlParameter);
			result = returnSqlParameter.Value;

			if (result is int) {
				returnValue = (int)result;
			}
			else {
				ServiceCenterContext.Database.ExecuteSqlCommand("SELECT @returnValue = NEXT VALUE FOR [ServiceCenter].[CustomerId];", returnSqlParameter);
				returnValue = (int)returnSqlParameter.Value;

				if (kundenNr > 0) {
					ServiceCenterContext.Database.ExecuteSqlCommand("INSERT INTO [ServiceCenter].[CustomerMapping](CustomerId, KundenNr) VALUES({0}, {1});", returnValue, kundenNr);
				}
			}

			return returnValue;
		}

		[HttpGet]
		public ActionResult<int> GetKundenNr(int customerId) {

			SqlParameter returnSqlParameter = new SqlParameter() {
				ParameterName = "@returnValue",
				SqlDbType = System.Data.SqlDbType.Int,
				Direction = System.Data.ParameterDirection.Output
			};

			ServiceCenterContext.Database.ExecuteSqlCommand("SELECT @returnValue = KundenNr FROM [ServiceCenter].[CustomerMapping] WHERE CustomerId = {0}", customerId, returnSqlParameter);

			if (returnSqlParameter.Value is int)
				return (int)returnSqlParameter.Value;
			else
				return -1;
		}

		/// <summary>
		/// Returns items that the customer has most often ordered
		/// </summary>
		[HttpGet]
		public ActionResult<List<ArticleSearchReturn>> GetOftenOrderedItems(int affiliateId, int customerId, int limit = 5) {

			if (affiliateId < 1
			|| customerId < 1)
				return new BadRequestResult();

			StringBuilder sql = new StringBuilder();

			sql.AppendLine("SELECT oit_pro_pzn PZN, MAX(B.beleg_datum) DateTimeField, SUM(I.oit_amount) IntegerField ");
			sql.AppendLine("FROM dbo.Belege B ");
			sql.AppendLine("JOIN dbo.orders_items I ON B.PKbeleg_id = I.FKoit_ord_id ");
			sql.AppendLine("JOIN SchrammOsaffiliateMapping µ on B.AffiliatePartnerId = µ.AffiliateAlt ");
			sql.AppendLine("WHERE B.FKord_cus_id = @FKord_cus_id ");
			sql.AppendLine("  AND µ.affiliateNEU = @affiliateId ");
			sql.AppendLine("  AND I.oit_price != 0 ");
			sql.AppendLine("  AND B.beleg_datum > DATEADD(MONTH, -6, GETDATE()) ");
			sql.AppendLine("  AND I.oit_pro_pzn > 100 ");
			sql.AppendLine("GROUP BY I.oit_pro_pzn, I.oit_pro_name ");
			sql.AppendLine("ORDER BY SUM(I.oit_amount) DESC, MAX(B.beleg_datum) DESC, MAX(I.oit_price) DESC ");

			var results = (MAINDBContext.ArticleSearchResults.FromSql(
						sql.ToString(),
						new SqlParameter("FKord_cus_id", GetKundenNr(customerId).Value),
						new SqlParameter("affiliateId", affiliateId)
						));

			ArticlesController articlesController = new ArticlesController(MAINDBContext);
			List<ArticleSearchReturn> returnValue = new List<ArticleSearchReturn>();

			foreach (var result in results) {

				if (NonOrderItemPzns.Contains(result.PZN))
					continue;

				List<Article> articleDetails = articlesController.Get(affiliateId, result.PZN, limit: 1).Value;

				if (articleDetails.Count() == 1) {
					returnValue.Add(new ArticleSearchReturn(result.IntegerField, result.DateTimeField, articleDetails.Single()));
				}
			}

			articlesController = null;

			return returnValue.Where(x => x.SalePrice != 0).Take(limit).ToList();
		}

		/// <summary>
		/// Gets the most recently used back info
		/// </summary>
		[HttpGet]
		public ActionResult<BankInfo> GetLastBankInfo(int affiliateId, int customerId) {

			if (affiliateId < 1
			|| customerId < 1)
				return new BadRequestResult();

			StringBuilder sql = new StringBuilder();

			sql.AppendLine("SELECT TOP 1 B.PKbeleg_id BankInfoId, B.IBAN, B.BIC, B.ord_account_owner AccountOwner ");
			sql.AppendLine("FROM dbo.Belege B ");
			sql.AppendLine("JOIN SchrammOsaffiliateMapping µ on B.AffiliatePartnerId = µ.AffiliateAlt ");
			sql.AppendLine("WHERE B.FKord_cus_id = @customerId ");
			sql.AppendLine("  AND µ.affiliateNEU = @affiliateId ");
			sql.AppendLine("ORDER BY B.PKbeleg_id DESC ");

			var results = (MAINDBContext.BankInfos.FromSql(
						sql.ToString(),
						new SqlParameter("customerId", GetKundenNr(customerId).Value),
						new SqlParameter("affiliateId", affiliateId)
						));

			if (results.Count() == 1)
				return results.Single();
			else
				return null;
		}

		/// <summary>
		/// Gets the most recent orders and associated items
		/// </summary>
		[HttpGet]
		public ActionResult<List<ArticleSearchReturn>> GetRecentOrderedItems(int affiliateId, int customerId, int limit = 10) {

			if (affiliateId < 1
			|| customerId < 1)
				return new BadRequestResult();

			StringBuilder sql = new StringBuilder();

			sql.AppendLine("SELECT oit_pro_pzn Pzn, B.beleg_datum DateTimeField, SUM(I.oit_amount) IntegerField ");
			sql.AppendLine("FROM dbo.Belege B ");
			sql.AppendLine("JOIN dbo.orders_items I ON B.PKbeleg_id = I.FKoit_ord_id ");
			sql.AppendLine("JOIN SchrammOsaffiliateMapping µ on B.AffiliatePartnerId = µ.AffiliateAlt ");
			sql.AppendLine("WHERE B.FKord_cus_id = @FKord_cus_id ");
			sql.AppendLine("  AND µ.affiliateNEU = @affiliateId ");
			sql.AppendLine("  AND B.beleg_datum > DATEADD(MONTH, -6, GETDATE()) ");
			sql.AppendLine("  AND I.oit_price != 0 ");
			sql.AppendLine("  AND I.oit_pro_pzn > 100 ");
			sql.AppendLine("GROUP BY B.beleg_datum, oit_pro_pzn ");
			sql.AppendLine("ORDER BY B.beleg_datum DESC, SUM(I.oit_amount) DESC, MAX(I.oit_price) DESC ");

			var results = (MAINDBContext.ArticleSearchResults.FromSql(
						sql.ToString(),
						new SqlParameter("FKord_cus_id", GetKundenNr(customerId).Value),
						new SqlParameter("affiliateId", affiliateId)
						));

			ArticlesController articlesController = new ArticlesController(MAINDBContext);
			List<ArticleSearchReturn> returnValue = new List<ArticleSearchReturn>();

			foreach (var result in results) {
				if (NonOrderItemPzns.Contains(result.PZN))
					continue;

				List<Article> articleDetails = articlesController.Get(affiliateId, result.PZN, limit: 1).Value;

				if (articleDetails.Count() == 1) {
					returnValue.Add(new ArticleSearchReturn(result.IntegerField, result.DateTimeField, articleDetails.Single()));
				}
			}

			articlesController = null;

			return returnValue.Where(x => x.SalePrice != 0).Take(limit).ToList();
		}


		[HttpGet]
		public ActionResult<Invoice> GetInvoice(int affiliateId, int customerId) {

			if (affiliateId < 0
			 || customerId < 1)
				return new BadRequestResult();

			var invoices = (from i in ServiceCenterContext.Invoice
							where i.AffiliateId == affiliateId
							   && i.CustomerId == customerId
							   && i.InvoiceId == 0
							select i);

			if (invoices.Count() == 1)
				return invoices.Single();
			else
				return null;
		}

		[HttpPost]
		public async Task<ActionResult<Invoice>> CreateInvoice(string userName, int affiliateId, int customerId) {

			if (string.IsNullOrWhiteSpace(userName)
			 || affiliateId < 0
			 || customerId < 1)
				return new BadRequestResult();

			Invoice invoice = new Invoice {
				InvoiceBillingAddress = new InvoiceBillingAddress() {
					ChangedUserName = userName
				},
				InvoiceDeliveryAddress = new InvoiceDeliveryAddress() {
					ChangedUserName = userName
				},
				ChangedUserName = userName
			};
			InvoiceBillingAddress invoiceBillingAddress = invoice.InvoiceBillingAddress;
			InvoiceDeliveryAddress invoiceDeliveryAddress = invoice.InvoiceDeliveryAddress;

			int kundennr = GetKundenNr(customerId).Value;

			if (kundennr < 0) {
				invoice.AffiliateId = affiliateId;
				invoice.CustomerId = customerId;
			}
			else {
				CustomersController customers = new CustomersController(MAINDBContext);
				ActionResult<Customer> actionResultCustomer = customers.Get(kundennr);
				customers = null;

				if (actionResultCustomer.Value == null)
					return new BadRequestResult();

				Customer customer = actionResultCustomer.Value;

				invoice.AffiliateId = affiliateId;
				invoice.CustomerId = customerId;
				invoice.CustomerGroupId = customer.CustomerGroupId;
				invoice.CustomerGroupDiscount = customer.CustomerGroupDiscount;
				invoice.InvoiceBillingAddress = invoiceBillingAddress;
				invoice.InvoiceDeliveryAddress = invoiceDeliveryAddress;
				invoice.ChangedUserName = userName;

				foreach (PropertyInfo propertyInfo in invoiceBillingAddress.GetType().GetProperties()) {
					var thisProperty = customer.GetType().GetProperty(propertyInfo.Name);
					if (thisProperty != null
					 && thisProperty.CanWrite)
						propertyInfo.SetValue(invoiceBillingAddress, thisProperty.GetValue(customer));
				}
				LookupsController lookups = new LookupsController(MAINDBContext);

				string bestGuessBillingStreet = lookups.GetBestGuestStreetFromZip(invoiceBillingAddress.Zip, invoiceBillingAddress.Street).Value;
				if (!string.IsNullOrEmpty(bestGuessBillingStreet))
					invoiceBillingAddress.Street = bestGuessBillingStreet;

				if (string.IsNullOrEmpty(invoiceBillingAddress.Country))
					invoiceBillingAddress.Country = "DE";

				foreach (PropertyInfo propertyInfo in invoiceDeliveryAddress.GetType().GetProperties()) {
					var thisProperty = customer.GetType().GetProperty(propertyInfo.Name);
					if (thisProperty != null
					 && thisProperty.CanWrite)
						propertyInfo.SetValue(invoiceDeliveryAddress, thisProperty.GetValue(customer));
				}
				string bestGuessDeliveryStreet = lookups.GetBestGuestStreetFromZip(invoiceDeliveryAddress.Zip, invoiceDeliveryAddress.Street).Value;
				if (!string.IsNullOrEmpty(bestGuessDeliveryStreet))
					invoiceDeliveryAddress.Street = bestGuessDeliveryStreet;
				lookups = null;
				if (string.IsNullOrEmpty(invoiceDeliveryAddress.Country))
					invoiceDeliveryAddress.Country = "DE";
			}

			ServiceCenterContext.Invoice.Add(invoice);

			await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);

			invoice.InvoiceBillingAddress = null;
			invoice.InvoiceDeliveryAddress = null;

			return invoice;
		}

		[HttpGet]
		public async Task<ActionResult<Invoice>> GetOrCreateInvoice(string userName, int affiliateId, int customerId) {

			if (string.IsNullOrWhiteSpace(userName)
			 || affiliateId < 0
			 || customerId < 1)
				return new BadRequestResult();

			ActionResult<Invoice> result = GetInvoice(affiliateId, customerId);

			Invoice invoice = null;
			if (result != null)
				invoice = result.Value;

			if (invoice == null) {
				invoice = (await CreateInvoice(userName, affiliateId, customerId).ConfigureAwait(false)).Value;
				invoice.InvoiceBillingAddress = null;
			}

			return invoice;
		}

		[HttpGet]
		public ActionResult<BillingAddress> GetBillingAddress(int affiliateId, int customerId) {

			BusinessObjects.Common.BillingAddress address = new BusinessObjects.Common.BillingAddress();

			if (affiliateId < 1
			 || customerId < 1)
				return new BadRequestResult();

			var addresses = (from i in ServiceCenterContext.Invoice
							 join da in ServiceCenterContext.InvoiceBillingAddress on i.InvoicePk equals da.InvoiceFk
							 where i.AffiliateId == affiliateId
								&& i.CustomerId == customerId
								&& i.InvoiceId == 0
							 select da);

			BillingAddress returnValue = new BillingAddress(); ;

			if (addresses.Count() == 1) {
				InvoiceBillingAddress invoiceBillingAddress = addresses.Single();

				foreach (PropertyInfo propertyInfo in returnValue.GetType().GetProperties()) {
					var thisProperty = invoiceBillingAddress.GetType().GetProperty(propertyInfo.Name);
					if (thisProperty != null
					 && thisProperty.CanWrite)
						propertyInfo.SetValue(returnValue, thisProperty.GetValue(invoiceBillingAddress));
				}
			}
			return returnValue;
		}

		[HttpPost]
		public async Task<ActionResult<bool>> UpdateBillingAddress() {

			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			if (string.IsNullOrWhiteSpace((string)parameters.userName)
			 || parameters.affiliateId < 1
			 || parameters.customerId < 1)
				return new BadRequestResult();

			Invoice invoice = (await GetOrCreateInvoice((string)parameters.userName, System.Convert.ToInt32(parameters.affiliateId), System.Convert.ToInt32(parameters.customerId))).Value;

			var invoiceBillingAddresses = (from da in ServiceCenterContext.InvoiceBillingAddress
										   where da.InvoiceFk == invoice.InvoicePk
										   select da
									 );

			InvoiceBillingAddress invoiceBillingAddress;

			if (invoiceBillingAddresses.Count() == 0) {
				invoiceBillingAddress = new InvoiceBillingAddress() {
					InvoiceFk = invoice.InvoicePk
				};
				ServiceCenterContext.InvoiceBillingAddress.Add(invoiceBillingAddress);
			}
			else {
				invoiceBillingAddress = invoiceBillingAddresses.Single();
				ServiceCenterContext.InvoiceBillingAddress.Update(invoiceBillingAddress);
			}

			// Transpose parameter values to delivery object
			foreach (JProperty child in ((JObject)parameters).Children()) {
				PropertyInfo propertyInfo = invoiceBillingAddress.GetType().GetProperty(child.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				if (propertyInfo != null) {
					if (child.Value.Type == JTokenType.Null) {
						propertyInfo.SetValue(invoiceBillingAddress, null);
					}
					else {
						propertyInfo.SetValue(invoiceBillingAddress, Convert.ChangeType(child.Value, propertyInfo.PropertyType, System.Globalization.CultureInfo.InvariantCulture));
					}
				}
			}
			invoiceBillingAddress.ChangedUserName = parameters.userName;

			LookupsController lookups = new LookupsController(MAINDBContext);
			string bestGuessBillingStreet = lookups.GetBestGuestStreetFromZip(invoiceBillingAddress.Zip, invoiceBillingAddress.Street).Value;
			if (!string.IsNullOrEmpty(bestGuessBillingStreet))
				invoiceBillingAddress.Street = bestGuessBillingStreet;
			lookups = null;

			await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);

			return true;
		}

		[HttpGet]
		public ActionResult<DeliveryAddress> GetDeliveryAddress(int affiliateId, int customerId) {

			BusinessObjects.Common.DeliveryAddress address = new BusinessObjects.Common.DeliveryAddress();

			if (affiliateId < 1
			 || customerId < 1)
				return new BadRequestResult();

			var addresses = (from i in ServiceCenterContext.Invoice
							 join da in ServiceCenterContext.InvoiceDeliveryAddress on i.InvoicePk equals da.InvoiceFk
							 where i.AffiliateId == affiliateId
								&& i.CustomerId == customerId
								&& i.InvoiceId == 0
							 select da);

			DeliveryAddress returnValue = new DeliveryAddress(); ;

			if (addresses.Count() == 1) {
				InvoiceDeliveryAddress invoiceDeliveryAddress = addresses.Single();

				foreach (PropertyInfo propertyInfo in returnValue.GetType().GetProperties()) {
					var thisProperty = invoiceDeliveryAddress.GetType().GetProperty(propertyInfo.Name);
					if (thisProperty != null
					 && thisProperty.CanWrite)
						propertyInfo.SetValue(returnValue, thisProperty.GetValue(invoiceDeliveryAddress));
				}
			}
			return returnValue;
		}

		[HttpPost]
		public async Task<ActionResult<int>> UpdateDeliveryAddress() {
			int returnValue = 0;

			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			if (string.IsNullOrWhiteSpace((string)parameters.userName)
			 || parameters.affiliateId < 1
			 || parameters.customerId < 1)
				return new BadRequestResult();

			Invoice invoice = (await GetOrCreateInvoice((string)parameters.userName, System.Convert.ToInt32(parameters.affiliateId), System.Convert.ToInt32(parameters.customerId))).Value;

			var invoiceDeliveryAddresses = (from da in ServiceCenterContext.InvoiceDeliveryAddress
											where da.InvoiceFk == invoice.InvoicePk
											select da
									 );

			InvoiceDeliveryAddress invoiceDeliveryAddress;

			if (invoiceDeliveryAddresses.Count() == 0) {
				invoiceDeliveryAddress = new InvoiceDeliveryAddress() {
					InvoiceFk = invoice.InvoicePk
				};
				ServiceCenterContext.InvoiceDeliveryAddress.Add(invoiceDeliveryAddress);
			}
			else {
				invoiceDeliveryAddress = invoiceDeliveryAddresses.Single();
				ServiceCenterContext.InvoiceDeliveryAddress.Update(invoiceDeliveryAddress);
			}

			// Transpose parameter values to delivery object
			foreach (JProperty child in ((JObject)parameters).Children()) {
				PropertyInfo propertyInfo = invoiceDeliveryAddress.GetType().GetProperty(child.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				if (propertyInfo != null) {
					if (child.Value.Type == JTokenType.Null) {
						propertyInfo.SetValue(invoiceDeliveryAddress, null);
					}
					else {
						propertyInfo.SetValue(invoiceDeliveryAddress, Convert.ChangeType(child.Value, propertyInfo.PropertyType, System.Globalization.CultureInfo.InvariantCulture));
					}
				}
			}
			invoiceDeliveryAddress.ChangedUserName = parameters.userName;

			LookupsController lookups = new LookupsController(MAINDBContext);
			string correctedStreet = lookups.GetBestGuestStreetFromZip(invoiceDeliveryAddress.Zip, invoiceDeliveryAddress.Street).Value;
			lookups = null;

			if (string.IsNullOrEmpty(correctedStreet)
			 || invoiceDeliveryAddress.Street == correctedStreet) {
				returnValue = 1;
			}
			else {
				invoiceDeliveryAddress.Street = correctedStreet;
				returnValue = 2;
			}

			if (await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false) == 0)
				returnValue = 0;

			return returnValue;
		}

		[HttpGet]
		public ActionResult<string> GetDeliveryAddressStreet(int affiliateId, int customerId) {

			if (affiliateId < 1
			 || customerId < 1)
				return new BadRequestResult();

			string returnValue = (from i in ServiceCenterContext.Invoice
								  join d in ServiceCenterContext.InvoiceDeliveryAddress on i.InvoicePk equals d.InvoiceFk
								  where i.AffiliateId == affiliateId
									 && i.CustomerId == customerId
									 && i.InvoiceId == 0
								  select d.Street).Single();

			return returnValue;
		}

		[HttpPost]
		public async Task<ActionResult<int>> UpdatePaymentType() {
			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			if (string.IsNullOrWhiteSpace((string)parameters.userName)
			 || parameters.affiliateId < 1
			 || parameters.customerId < 1
			 || parameters.paymentTypeId < 1)
				return new BadRequestResult();

			Invoice invoice = (await GetOrCreateInvoice((string)parameters.userName, System.Convert.ToInt32(parameters.affiliateId), System.Convert.ToInt32(parameters.customerId))).Value;

			bool setDeliveryTypeId = invoice.PaymentTypeId != System.Convert.ToInt32(parameters.paymentTypeId);

			string bic = (string)parameters.bic;
			if (!string.IsNullOrEmpty(bic))
				bic = bic.Replace(" ", "", System.StringComparison.InvariantCulture).ToUpper(System.Globalization.CultureInfo.InvariantCulture);

			string iban = (string)parameters.iban;
			if (!string.IsNullOrEmpty(iban))
				iban = iban.Replace(" ", "", System.StringComparison.InvariantCulture).ToUpper(System.Globalization.CultureInfo.InvariantCulture);

			invoice.PaymentTypeId = System.Convert.ToInt32(parameters.paymentTypeId);
			invoice.DeliveryTypeId = setDeliveryTypeId ? 0 : invoice.DeliveryTypeId;
			invoice.Bic = bic;
			invoice.Iban = iban;
			invoice.AccountOwner = (string)parameters.accountOwner;

			ServiceCenterContext.Invoice.Update(invoice);
			await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);

			if (setDeliveryTypeId)
				return 2;
			else
				return 1;
		}

		[HttpGet]
		public ActionResult<List<InvoiceItem>> GetInvoiceItems(int affiliateId, int customerId) {

			if (affiliateId < 1
			 || customerId < 1)
				return new BadRequestResult();

			List<InvoiceItem> returnValue = new List<InvoiceItem>();

			ActionResult<Invoice> result = GetInvoice(System.Convert.ToInt32(affiliateId), System.Convert.ToInt32(customerId));

			Invoice invoice = null;
			if (result != null)
				invoice = result.Value;

			if (invoice != null) {
				var invoiceItems = (from i in ServiceCenterContext.InvoiceItem
									where i.InvoiceFk == invoice.InvoicePk
									select i);

				foreach (InvoiceItem invoiceItem in invoiceItems) {
					invoiceItem.InvoiceFkNavigation = null;
					returnValue.Add(invoiceItem);
				}
			}
			return returnValue.ToList();
		}

		/// <summary>
		/// Adds an item to an order, or increments the number of items order.  Also calculates and add a discount item (if needed)
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<bool>> AddInvoiceItem() {

			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			if (string.IsNullOrWhiteSpace((string)parameters.userName)
			 || parameters.affiliateId < 1
			 || parameters.customerId < 1
			 || parameters.pzn < 1)
				return new BadRequestResult();

			Invoice invoice = (await GetOrCreateInvoice((string)parameters.userName, System.Convert.ToInt32(parameters.affiliateId), System.Convert.ToInt32(parameters.customerId))).Value;

			int affiliateId = (int)parameters.affiliateId;
			int customerId = (int)parameters.customerId;
			int pzn = (int)parameters.pzn;
			int maxOrderAmount = GetMaxOrderAmount((int)parameters.affiliateId, (int)parameters.pzn);
			int quantity = (int)parameters.quantity;

			if (quantity > maxOrderAmount)
				quantity = maxOrderAmount;

			var items = (from ii in ServiceCenterContext.InvoiceItem
						 join i in ServiceCenterContext.Invoice on ii.InvoiceFk equals i.InvoicePk
						 where i.AffiliateId == affiliateId
							&& i.CustomerId == customerId
							&& i.InvoiceId == 0
							&& ii.Pzn == pzn
						 select ii);

			InvoiceItem invoiceItem;

			if (items.Count() == 1) {
				invoiceItem = items.Single();

				if ((invoiceItem.Quantity + quantity) > maxOrderAmount)
					invoiceItem.Quantity = maxOrderAmount;
				else
					invoiceItem.Quantity += quantity;
			}
			else {
				invoiceItem = new InvoiceItem() {
					InvoiceFk = invoice.InvoicePk,
					Pzn = parameters.pzn,
					Quantity = parameters.quantity,
					Vat = parameters.vat,
					ItemPrice = parameters.itemPrice,
					ItemSavings = parameters.itemSavings,
					ChangedUserName = (string)parameters.userName
				};
			}

			ServiceCenterContext.InvoiceItem.Update(invoiceItem);
			await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);

			await UpdateGroupDiscountItem(affiliateId, invoice.InvoicePk, invoice.CustomerGroupDiscount, (string)parameters.userName).ConfigureAwait(false);

			return true;
		}

		/// <summary>
		/// Articles can only be ordered in a certain amount
		/// </summary>
		private int GetMaxOrderAmount(int affiliateId, int pzn) {

			var mengenbegrenzung = (from i in MAINDBContext.ArtikelstammIdentifikation
									join s in MAINDBContext.AffiliateArtikelstamm on i.Artikelnummer equals s.Artikelnummer
									join v in MAINDBContext.AffiliateArtikelstammVerfuegbarkeit on s.Id equals v.ArtikelstammId
									where i.Pzn == pzn
									   && s.AffiliateId == affiliateId
									select v.Mengenbegrenzung);

			return mengenbegrenzung.Single();
		}

		[HttpPost]
		public async Task<ActionResult<bool>> RemoveInvoiceItem() {

			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			if (string.IsNullOrWhiteSpace((string)parameters.userName)
			 || parameters.affiliateId < 1
			 || parameters.customerId < 1
			 || parameters.invoiceItemId < 1)
				return new BadRequestResult();

			return await RemoveInvoiceItem((string)parameters.userName, (int)parameters.affiliateId, (int)parameters.customerId, (int)parameters.invoiceItemId).ConfigureAwait(false); ;
		}

		private async Task<bool> RemoveInvoiceItem(string userName, int affiliateId, int customerId, int invoiceItemId) {

			ActionResult<Invoice> actionResult = GetInvoice(affiliateId, customerId);

			if (actionResult == null)
				return false;

			var invoiceItems = (from i in ServiceCenterContext.InvoiceItem
								where i.InvoiceFk == actionResult.Value.InvoicePk
								   && i.InvoiceItemPk == invoiceItemId
								select i);

			if (invoiceItems.Count() == 1) {
				ServiceCenterContext.InvoiceItem.Remove(invoiceItems.Single());
				await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);
				await UpdateGroupDiscountItem(affiliateId, actionResult.Value.InvoicePk, actionResult.Value.CustomerGroupDiscount, userName).ConfigureAwait(false);
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>
		/// A special item is something can only added once, e.g. shipping or discount
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<bool>> AddSpecialItem() {

			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			if (string.IsNullOrWhiteSpace((string)parameters.userName)
			 || parameters.affiliateId < 1
			 || parameters.customerId < 1
			 || parameters.pzn < 1)
				return new BadRequestResult();

			int affiliateId = parameters.affiliateId;
			int customerId = parameters.customerId;
			int pzn = parameters.pzn;
			decimal itemPrice = parameters.itemPrice;
			string userName = parameters.userName;

			Invoice invoice = (await GetOrCreateInvoice(userName, System.Convert.ToInt32(parameters.affiliateId), System.Convert.ToInt32(parameters.customerId))).Value;

			if (pzn == DeliveryPaymentButton.DISCOUNT_PZN) {
				await UpdateGroupDiscountItem(affiliateId, invoice.InvoicePk, itemPrice, userName).ConfigureAwait(false);
			}
			else {

				var invoiceItems = (from i in ServiceCenterContext.InvoiceItem
									where i.InvoiceFk == invoice.InvoicePk
									   && i.Pzn == pzn
									select i);

				if (invoiceItems.Count() == 0) {
					InvoiceItem invoiceItem = new InvoiceItem() {
						InvoiceFk = invoice.InvoicePk,
						Pzn = pzn,
						Quantity = 1,
						Vat = GetVAT(affiliateId, pzn),
						ItemPrice = itemPrice,
						ChangedUserName = userName
					};
					ServiceCenterContext.InvoiceItem.Update(invoiceItem);
				}
				else if (invoiceItems.Count() == 1) {
					InvoiceItem invoiceItem = invoiceItems.Single();
					invoiceItem.Quantity = 1;
					invoiceItem.Vat = GetVAT(affiliateId, pzn);
					invoiceItem.ItemPrice = itemPrice;
					invoiceItem.ChangedUserName = userName;

					ServiceCenterContext.InvoiceItem.Update(invoiceItem);
				}
				else if (invoiceItems.Count() > 1) {
					ServiceCenterContext.InvoiceItem.RemoveRange(invoiceItems);
					InvoiceItem invoiceItem = new InvoiceItem() {
						InvoiceFk = invoice.InvoicePk,
						Pzn = pzn,
						Quantity = 1,
						Vat = GetVAT(affiliateId, pzn),
						ItemPrice = itemPrice,
						ChangedUserName = userName
					};
					ServiceCenterContext.InvoiceItem.Update(invoiceItem);
				}

				await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);
			}

			return true;
		}

		[HttpPost]
		public async Task<ActionResult<bool>> AddDeliveryPaymentButtonItem() {

			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			if (string.IsNullOrWhiteSpace((string)parameters.userName)
			 || parameters.affiliateId < 1
			 || parameters.customerId < 1
			 || parameters.deliveryPaymentButtonPk < 1)
				return new BadRequestResult();

			int affiliateId = parameters.affiliateId;
			int customerId = parameters.customerId;
			int deliveryPaymentButtonPk = parameters.deliveryPaymentButtonId;

			var deliveryPaymentButtons = (from dpb in ServiceCenterContext.DeliveryPaymentButton
										  where dpb.AffiliateId == affiliateId
											 && dpb.DeliveryPaymentButtonPk == deliveryPaymentButtonPk
										  select dpb);

			if (deliveryPaymentButtons.Count() != 1) {
				return false;
			}

			DeliveryPaymentButton deliveryPaymentButton = deliveryPaymentButtons.Single();

			int pzn = deliveryPaymentButton.Pzn;
			decimal itemPrice = deliveryPaymentButton.ItemPrice;
			string userName = parameters.userName;

			Invoice invoice = (await GetOrCreateInvoice(userName, System.Convert.ToInt32(parameters.affiliateId), System.Convert.ToInt32(parameters.customerId))).Value;

			if (pzn == DeliveryPaymentButton.DISCOUNT_PZN) {
				await UpdateGroupDiscountItem(affiliateId, invoice.InvoicePk, itemPrice, userName).ConfigureAwait(false);
			}
			else {

				var invoiceItems = (from i in ServiceCenterContext.InvoiceItem
									where i.InvoiceFk == invoice.InvoicePk
									   && i.Pzn == pzn
									select i);

				if (invoiceItems.Count() == 0) {
					InvoiceItem invoiceItem = new InvoiceItem() {
						InvoiceFk = invoice.InvoicePk,
						Pzn = pzn,
						Quantity = 1,
						Vat = GetVAT(affiliateId, pzn),
						ItemPrice = itemPrice,
						ChangedUserName = userName
					};
					ServiceCenterContext.InvoiceItem.Update(invoiceItem);
				}
				else if (invoiceItems.Count() == 1) {
					InvoiceItem invoiceItem = invoiceItems.Single();
					invoiceItem.Quantity = 1;
					invoiceItem.Vat = GetVAT(affiliateId, pzn);
					invoiceItem.ItemPrice = itemPrice;
					invoiceItem.ChangedUserName = userName;

					ServiceCenterContext.InvoiceItem.Update(invoiceItem);
				}
				else if (invoiceItems.Count() > 1) {
					ServiceCenterContext.InvoiceItem.RemoveRange(invoiceItems);
					InvoiceItem invoiceItem = new InvoiceItem() {
						InvoiceFk = invoice.InvoicePk,
						Pzn = pzn,
						Quantity = 1,
						Vat = GetVAT(affiliateId, pzn),
						ItemPrice = itemPrice,
						ChangedUserName = userName
					};
					ServiceCenterContext.InvoiceItem.Update(invoiceItem);
				}

				await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);
			}

			return true;
		}

		[HttpPost]
		public async Task<ActionResult<bool>> IncrementInvoiceItem() {

			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			if (string.IsNullOrWhiteSpace((string)parameters.userName)
			 || parameters.affiliateId < 1
			 || parameters.customerId < 1
			 || parameters.invoiceItemId < 1)
				return new BadRequestResult();


			int affiliateId = (int)parameters.affiliateId;
			int customerId = (int)parameters.customerId;
			int invoiceItemId = (int)parameters.invoiceItemId;

			var invoiceItem = (from i in ServiceCenterContext.Invoice
							   join ii in ServiceCenterContext.InvoiceItem on i.InvoicePk equals ii.InvoiceFk
							   where i.AffiliateId == affiliateId
								  && i.CustomerId == customerId
								  && ii.InvoiceItemPk == invoiceItemId
							   select new {
								   i.InvoicePk,
								   Item = ii,
								   i.CustomerGroupDiscount
							   }).Single();

			int maxOrderAmount = GetMaxOrderAmount(affiliateId, invoiceItem.Item.Pzn);

			invoiceItem.Item.Quantity += 1;

			if (invoiceItem.Item.Quantity > maxOrderAmount)
				invoiceItem.Item.Quantity = maxOrderAmount;

			ServiceCenterContext.InvoiceItem.Update(invoiceItem.Item);

			await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);

			await UpdateGroupDiscountItem(affiliateId, invoiceItem.InvoicePk, invoiceItem.CustomerGroupDiscount, (string)parameters.userName).ConfigureAwait(false);

			return true;
		}

		[HttpPost]
		public async Task<ActionResult<bool>> DecrementInvoiceItem() {

			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			if (string.IsNullOrWhiteSpace((string)parameters.userName)
			 || parameters.affiliateId < 1
			 || parameters.customerId < 1
			 || parameters.invoiceItemId < 1)
				return new BadRequestResult();


			int affiliateId = (int)parameters.affiliateId;
			int customerId = (int)parameters.customerId;
			int invoiceItemId = (int)parameters.invoiceItemId;

			var invoiceItem = (from i in ServiceCenterContext.Invoice
							   join ii in ServiceCenterContext.InvoiceItem on i.InvoicePk equals ii.InvoiceFk
							   where i.AffiliateId == affiliateId
								  && i.CustomerId == customerId
								  && ii.InvoiceItemPk == invoiceItemId
							   select new {
								   i.InvoicePk,
								   Item = ii,
								   i.CustomerGroupDiscount
							   }).Single();

			invoiceItem.Item.Quantity -= 1;

			if (invoiceItem.Item.Quantity < 1) {
				ServiceCenterContext.InvoiceItem.Remove(invoiceItem.Item);
			}
			else {
				ServiceCenterContext.InvoiceItem.Update(invoiceItem.Item);
			}

			await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);

			await UpdateGroupDiscountItem(affiliateId, invoiceItem.InvoicePk, invoiceItem.CustomerGroupDiscount, (string)parameters.userName).ConfigureAwait(false);

			return true;
		}

		private async Task UpdateGroupDiscountItem(int affiliateId, int invoicePk, decimal groupDiscount, string userName) {
			var invoiceItems = (from i in ServiceCenterContext.InvoiceItem
								where i.InvoiceFk == invoicePk
								select i);

			int discountItemCount = invoiceItems.Count(x => x.Pzn == DeliveryPaymentButton.DISCOUNT_PZN);
			decimal discountSum = -invoiceItems.Where(x => x.Pzn > 100 && !NonOrderItemPzns.Contains(x.Pzn)).Sum(y => y.ItemPrice * y.Quantity) * (groupDiscount / 100M);

			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --
			// No discount, but discount items
			if (discountSum == 0 && discountItemCount > 0) {
				ServiceCenterContext.InvoiceItem.RemoveRange(invoiceItems.Where(x => x.Pzn == DeliveryPaymentButton.DISCOUNT_PZN));
				await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);
			}
			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --
			// Items, but no discount item
			else if (discountSum != 0 && discountItemCount == 0) {
				InvoiceItem discountInvoiceItem = new InvoiceItem() {
					InvoiceFk = invoicePk,
					Pzn = DeliveryPaymentButton.DISCOUNT_PZN,
					Quantity = 1,
					Vat = GetVAT(affiliateId, DeliveryPaymentButton.DISCOUNT_PZN),
					ItemPrice = discountSum,
					ChangedUserName = userName
				};

				ServiceCenterContext.InvoiceItem.Add(discountInvoiceItem);
				await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);
			}
			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --
			// Items, and discount item
			else if (discountSum != 0 && discountItemCount == 1) {
				InvoiceItem discountInvoiceItem = invoiceItems.Single(x => x.Pzn == DeliveryPaymentButton.DISCOUNT_PZN);

				discountInvoiceItem.Quantity = 1;
				discountInvoiceItem.Vat = GetVAT(affiliateId, discountInvoiceItem.Pzn);
				discountInvoiceItem.ItemPrice = discountSum;

				ServiceCenterContext.InvoiceItem.Update(discountInvoiceItem);
				await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);
			}
			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --
			// Items, and several discount items (shouldn't happen)
			else if (discountSum != 0 && discountItemCount > 1) {
				ServiceCenterContext.InvoiceItem.RemoveRange(invoiceItems.Where(x => x.Pzn == DeliveryPaymentButton.DISCOUNT_PZN));

				InvoiceItem discountInvoiceItem = new InvoiceItem() {
					InvoiceFk = invoicePk,
					Pzn = DeliveryPaymentButton.DISCOUNT_PZN,
					Quantity = 1,
					Vat = GetVAT(affiliateId, DeliveryPaymentButton.DISCOUNT_PZN),
					ItemPrice = discountSum,
					ChangedUserName = userName
				};

				ServiceCenterContext.InvoiceItem.Add(discountInvoiceItem);
				await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		[HttpGet]
		public ActionResult<string> GetCustomerComment(int customerId) {

			if (customerId < 1)
				return new BadRequestResult();

			var comments = (from c in ServiceCenterContext.CustomerComment
							where c.CustomerId == customerId
							   && c.CommentSent == false
							select c.CommentText);

			if (comments.Count() == 1)
				return comments.Single();
			else
				return string.Empty;

		}

		[HttpPost]
		public async Task<ActionResult<bool>> SetCustomerComment() {
			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			int customerId = (int)parameters.customerId;
			string commentText = (string)parameters.commentText;
			string userName = (string)parameters.userName;

			if (customerId < 1
			 || string.IsNullOrWhiteSpace(commentText))
				return new BadRequestResult();

			var comments = (from c in ServiceCenterContext.CustomerComment
							where c.CustomerId == customerId
							   && c.CommentSent == false
							select c);

			CustomerComment comment;

			if (comments.Count() == 0) {
				comment = new CustomerComment {
					CustomerId = customerId,
					CommentText = commentText,
					ChangedUserName = userName
				};

				ServiceCenterContext.CustomerComment.Add(comment);
			}
			else {
				comment = comments.Single();
				comment.CommentText = commentText;
				comment.ChangedUserName = userName;
				ServiceCenterContext.CustomerComment.Update(comment);
			}

			await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);

			return true;
		}

		[HttpPost]
		public async Task<ActionResult<bool>> UpdateInvoiceItemQuantity() {

			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			if (string.IsNullOrWhiteSpace((string)parameters.userName)
			 || parameters.affiliateId < 1
			 || parameters.customerId < 1
			 || parameters.invoiceItemId < 1
			 || parameters.quantity < 0)
				return new BadRequestResult();

			int affiliateId = parameters.affiliateId;
			int customerId = parameters.customerId;
			int invoiceItemId = parameters.invoiceItemId;
			int quantity = (int)parameters.quantity;

			if (quantity <= 0) {
				return await RemoveInvoiceItem((string)parameters.userName, affiliateId, customerId, invoiceItemId).ConfigureAwait(false);
			}
			else {

				var items = (from ii in ServiceCenterContext.InvoiceItem
							 join i in ServiceCenterContext.Invoice on ii.InvoiceFk equals i.InvoicePk
							 where i.AffiliateId == affiliateId
								&& i.CustomerId == customerId
								&& ii.InvoiceItemPk == invoiceItemId
							 select ii);

				InvoiceItem invoiceItem;

				if (items.Count() == 1)
					invoiceItem = items.Single();
				else
					return new BadRequestResult();

				int maxOrderAmount = GetMaxOrderAmount(affiliateId, invoiceItem.Pzn);

				if (quantity > maxOrderAmount)
					quantity = maxOrderAmount;

				invoiceItem.Quantity = quantity;
				invoiceItem.ChangedUserName = parameters.userName;

				ServiceCenterContext.Update(invoiceItem);
				await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);

				Invoice invoice = (from i in ServiceCenterContext.Invoice
								   where i.InvoicePk == invoiceItem.InvoiceFk
								   select i).Single();

				await UpdateGroupDiscountItem(affiliateId, invoice.InvoicePk, invoice.CustomerGroupDiscount, (string)parameters.userName).ConfigureAwait(false);

			}
			return true;
		}

		[HttpPost]
		public async Task<ActionResult<bool>> UpdateDeliveryType() {
			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			if (string.IsNullOrWhiteSpace((string)parameters.userName)
			 || parameters.affiliateId < 1
			 || parameters.customerId < 1
			 || parameters.deliveryTypeId < 1)
				return new BadRequestResult();

			int affiliateId = System.Convert.ToInt32(parameters.affiliateId);
			int customerId = System.Convert.ToInt32(parameters.customerId);

			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  -- 

			Invoice invoice = (await GetOrCreateInvoice((string)parameters.userName, affiliateId, customerId).ConfigureAwait(false)).Value;

			invoice.DeliveryTypeId = System.Convert.ToInt32(parameters.deliveryTypeId);
			ServiceCenterContext.Invoice.Update(invoice);

			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  -- 

			ServiceCenterContext.Invoice.Include(x => x.InvoiceItem).Load();

			InvoiceItem shippingCostItem;

			if (invoice.InvoiceItem.Count(x => x.Pzn == 1) == 0) {
				shippingCostItem = new InvoiceItem() {
					InvoiceFk = invoice.InvoicePk,
					Pzn = 1,
				};
			}
			else {
				shippingCostItem = invoice.InvoiceItem.First(x => x.Pzn == 1);
			}

			shippingCostItem.Quantity = 1;
			shippingCostItem.Vat = GetVAT(affiliateId, 1);
			shippingCostItem.ItemPrice = System.Convert.ToDecimal(parameters.shippingCosts);
			shippingCostItem.ChangedUserName = (string)parameters.userName;

			ServiceCenterContext.InvoiceItem.Update(shippingCostItem);

			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  -- 

			await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false);

			return true;
		}

		[HttpGet]
		public ActionResult<List<DeliveryPaymentButton>> GetDeliveryPaymentButtons(int affiliateId) {

			if (affiliateId < 1)
				return new BadRequestResult();

			var deliveryPaymentButtons = (from d in ServiceCenterContext.DeliveryPaymentButton
										  where d.AffiliateId == affiliateId
											 && d.FromDate < System.DateTime.Now.Date
											 && d.UntilDate > System.DateTime.Now.AddDays(1).Date
										  select d);

			return deliveryPaymentButtons.ToList();
		}

		[HttpGet]
		public ActionResult<List<int>> ValidateInvoice(int affiliateId, int customerId) {
			List<int> returnValue = new List<int>();

			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  -- 

			ActionResult<Invoice> invoiceResult = GetInvoice(affiliateId, customerId);

			//  -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
			//																   1000s General Error
			if (invoiceResult == null) {
				returnValue.Add(1001);                                      // 1001 - Invoice not found
			}
			else {

				Invoice invoice = invoiceResult.Value;

				//  -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
				//															   2000s Customer
				if (invoice.CustomerId == 0)
					returnValue.Add(2001);                                  // 2001 - No customer id

				var invoiceBillingAddresses = (from a in ServiceCenterContext.InvoiceBillingAddress
											   where a.InvoiceFk == invoice.InvoicePk
											   select a);

				if (invoiceBillingAddresses.Count() != 1) {
					returnValue.Add(2002);                                  // 2002 - No delivery address
				}
				else {
					InvoiceBillingAddress invoiceBillingAddress = invoiceBillingAddresses.Single();
					if (string.IsNullOrEmpty(invoiceBillingAddress.FirstName)
					&& string.IsNullOrEmpty(invoiceBillingAddress.LastName)
						)
						returnValue.Add(2003);                              // 2003 - Billing address name missing

					if (string.IsNullOrEmpty(invoice.InvoiceBillingAddress.Street))
						returnValue.Add(2004);                              // 2004 - Billing address street missing

					if (string.IsNullOrEmpty(invoice.InvoiceBillingAddress.City))
						returnValue.Add(2005);                              // 2005 - Billing city street missing

					if (string.IsNullOrEmpty(invoice.InvoiceBillingAddress.Zip))
						returnValue.Add(2006);                              // 2006 - Billing zip street missing
				}


				//  -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
				//															   3000s DeliveryAddress

				var invoiceDeliveryAddresses = (from a in ServiceCenterContext.InvoiceDeliveryAddress
												where a.InvoiceFk == invoice.InvoicePk
												select a);

				if (invoiceDeliveryAddresses.Count() != 1) {
					returnValue.Add(3001);                                  // 3001 - No delivery address
				}
				else {
					InvoiceDeliveryAddress invoiceDeliveryAddress = invoiceDeliveryAddresses.Single();

					if (string.IsNullOrEmpty(invoiceDeliveryAddress.FirstName)
					&& string.IsNullOrEmpty(invoiceDeliveryAddress.LastName)
						)
						returnValue.Add(3002);                                  // 3002 - Delivery address name missing

					if (string.IsNullOrEmpty(invoiceDeliveryAddress.Street))
						returnValue.Add(3003);                                  // 3003 - Delivery address street missing

					if (string.IsNullOrEmpty(invoiceDeliveryAddress.City))
						returnValue.Add(3004);                                  // 3004 - Delivery city street missing

					if (string.IsNullOrEmpty(invoiceDeliveryAddress.Zip))
						returnValue.Add(3005);                                  // 3005 - Delivery zip street missing

					if (string.IsNullOrEmpty(invoiceDeliveryAddress.Country))
						returnValue.Add(3006);                                  // 3006 - Delivery country street missing

				}

				//  -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
				//															   4000s Payment
				if (!invoice.PaymentTypeId.HasValue
				 || invoice.PaymentTypeId == 0)
					returnValue.Add(4001);                                  // 4001 - No payment type

				if (invoice.PaymentTypeId.HasValue
				&& invoice.PaymentTypeId == 5
				&& (string.IsNullOrEmpty(invoice.Iban) || string.IsNullOrEmpty(invoice.Bic) || string.IsNullOrEmpty(invoice.AccountOwner)))
					returnValue.Add(4002);                                  // 4002 - Bank details missing

				//  -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
				//															   5000s Delivery
				if (!invoice.DeliveryTypeId.HasValue
				 || invoice.DeliveryTypeId == 0)
					returnValue.Add(5001);                                  // 5001 - No delivery type

				//  -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
				//																6000s Items
				List<InvoiceItem> invoiceItems = GetInvoiceItems(affiliateId, customerId).Value;

				if (invoiceItems.Count(x => x.Pzn > 100 && !NonOrderItemPzns.Contains(x.Pzn)) == 0) {
					returnValue.Add(6001);                                  // 6001 - No items
				}
				else {
					if (invoiceItems.Sum(x => x.ItemPrice * x.Quantity) <= 0)
						returnValue.Add(6002);                              // 6002 - Order sum less than zero
				}
			}

			return returnValue;
		}

		/// <summary>
		/// Moves the order from the ServiceCenter to the Einlese tables with the usp_InvoiceInsertIntoEinlesen stored proc
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<int>> SendInvoice() {
			int returnValue = 0;

			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			if (string.IsNullOrWhiteSpace((string)parameters.userName)
			 || parameters.affiliateId < 1
			 || parameters.customerId < 1)
				return new BadRequestResult();

			int affiliateId = System.Convert.ToInt32(parameters.affiliateId);
			int customerId = System.Convert.ToInt32(parameters.customerId);

			ActionResult<List<int>> validateInvoiceResult = ValidateInvoice(affiliateId, customerId);

			if (validateInvoiceResult.Value.Count != 0)
				returnValue = 0;

			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  -- 

			ActionResult<Invoice> getInvoiceResult = GetInvoice(affiliateId, customerId);

			if (getInvoiceResult == null) {
				returnValue = 1001;
			}
			else {

				SqlParameter returnSqlParameter = new SqlParameter() {
					ParameterName = "@returnValue",
					SqlDbType = System.Data.SqlDbType.Int,
					Direction = System.Data.ParameterDirection.Output
				};

				await ServiceCenterContext.Database.ExecuteSqlCommandAsync(
					"EXEC @returnValue = ServiceCenter.usp_InvoiceInsertIntoEinlesen @invoicePk, @changedUserName",
					returnSqlParameter,
					new SqlParameter("@invoicePk", getInvoiceResult.Value.InvoicePk),
					new SqlParameter("@changedUserName", (string)parameters.userName)
					).ConfigureAwait(false);

				returnValue = (int)returnSqlParameter.Value;
			}

			return returnValue;
		}

		/// <summary>
		/// Moves the comment to the Kunden table with the usp_InsertCommentInCustomer stored proc
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<bool>> SendCustomerComment() {
			bool returnValue = false;

			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			if (string.IsNullOrWhiteSpace((string)parameters.userName)
			 || parameters.affiliateId < 1
			 || parameters.customerId < 1)
				return new BadRequestResult();

			int customerId = System.Convert.ToInt32(parameters.customerId);

			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  -- 

			SqlParameter returnSqlParameter = new SqlParameter() {
				ParameterName = "@returnValue",
				SqlDbType = System.Data.SqlDbType.Int,
				Direction = System.Data.ParameterDirection.Output
			};

			await ServiceCenterContext.Database.ExecuteSqlCommandAsync(
				"EXEC @returnValue = ServiceCenter.usp_InsertCommentInCustomer @customerId, @changedUserName",
				returnSqlParameter,
				new SqlParameter("@customerId", customerId),
				new SqlParameter("@changedUserName", (string)parameters.userName)
				).ConfigureAwait(false);

			returnValue = (int)returnSqlParameter.Value != 0;

			return returnValue;
		}

		[HttpGet]
		public ActionResult<List<DeliveryAddress>> GetDeliveryAddresses(int customerId) {

			int kundennr = GetKundenNr(customerId).Value;

			CustomersController customers = new CustomersController(MAINDBContext);
			ActionResult<List<DeliveryAddress>> actionResult = customers.GetDeliveryAddresses(kundennr);
			customers = null;

			return actionResult.Value;
		}

		[HttpGet]
		public ActionResult<Customer> GetCustomer(int customerId) {

			int kundennr = GetKundenNr(customerId).Value;

			if (kundennr > 0) {
				CustomersController customers = new CustomersController(MAINDBContext);
				ActionResult<Customer> actionResult = customers.Get(kundennr);
				customers = null;
				return actionResult.Value;
			}
			else {
				return null;
			}
		}

		[HttpGet]
		public ActionResult<object> GetCustomerEdit(int affiliateId, int customerId) {

			var returnValue = (from i in ServiceCenterContext.Invoice
							   join b in ServiceCenterContext.InvoiceBillingAddress on i.InvoicePk equals b.InvoiceFk
							   where i.AffiliateId == affiliateId
								  && i.CustomerId == customerId
								  && i.InvoiceId == 0
							   select new {
								   id = i.InvoicePk,
								   i.CustomerId,
								   b.CustomerNumber,
								   i.AffiliateId,
								   i.CustomerGroupId,
								   i.CustomerGroupDiscount,
								   b.Title,
								   b.FirstName,
								   b.LastName,
								   b.CompanyName,
								   b.Street,
								   b.City,
								   b.Zip,
								   b.Country,
								   b.AdditionalLine,
								   b.EmailAddress,
								   b.TelephoneNumber,
								   b.MobileNumber,
								   b.FaxNumber,
								   b.DoB
							   });


			if (returnValue.Count() == 1) {
				var customerEdit = returnValue.Single();

				return customerEdit;
			}
			else
				return null;
		}

		[HttpPost]
		public async Task<ActionResult<int>> SetCustomerEdit() {
			int returnValue = 0;

			dynamic parameters;

			using (StreamReader reader = new StreamReader(Request.Body)) {
				parameters = JObject.Parse(reader.ReadToEnd());
			}

			// -------------------------------------------------------------------------

			int invoicePk = (int)parameters.Id;
			string userName = (string)parameters.UserName;

			Invoice invoice = (from i in ServiceCenterContext.Invoice
							   where i.InvoicePk == invoicePk
							   select i).Single();

			int customerGroupId = (int)parameters.CustomerGroupId;
			decimal customerGroupDiscount;

			if (customerGroupId == 0) {
				customerGroupDiscount = 0;
			}
			else {

				CustomersController customers = new CustomersController(MAINDBContext);
				List<CustomerGroupResult> customerGroups = customers.GetCustomerGroups().Value;
				customers = null;

				CustomerGroupResult customerGroupResult = customerGroups.Where(x => x.CustomerGroupId == customerGroupId).Single();
				customerGroupDiscount = customerGroupResult.CustomerGroupDiscount;
			}

			invoice.CustomerGroupId = customerGroupId;
			invoice.CustomerGroupDiscount = customerGroupDiscount;
			invoice.AffiliateId = (int)parameters.AffiliateId;

			invoice.ChangedUserName = userName;

			ServiceCenterContext.Invoice.Update(invoice);

			// -------------------------------------------------------------------------

			var invoiceBillingAddresses = (from b in ServiceCenterContext.InvoiceBillingAddress
										   where b.InvoiceFk == invoicePk
										   select b);

			InvoiceBillingAddress invoiceBillingAddress;

			if (invoiceBillingAddresses.Count() == 0) {
				invoiceBillingAddress = new InvoiceBillingAddress() {
					InvoiceFk = invoice.InvoicePk
				};
				ServiceCenterContext.InvoiceBillingAddress.Add(invoiceBillingAddress);
			}
			else {
				invoiceBillingAddress = invoiceBillingAddresses.Single();
				ServiceCenterContext.InvoiceBillingAddress.Update(invoiceBillingAddress);
			}

			// Transpose parameter values to delivery object
			foreach (JProperty child in ((JObject)parameters).Children()) {
				PropertyInfo propertyInfo = invoiceBillingAddress.GetType().GetProperty(child.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				if (propertyInfo != null) {
					if (child.Value.Type == JTokenType.Null) {
						propertyInfo.SetValue(invoiceBillingAddress, null);
					}
					else {
						propertyInfo.SetValue(invoiceBillingAddress, Convert.ChangeType(child.Value, propertyInfo.PropertyType, System.Globalization.CultureInfo.InvariantCulture));
					}
				}
			}

			LookupsController lookups = new LookupsController(MAINDBContext);
			string correctedStreet = lookups.GetBestGuestStreetFromZip(invoiceBillingAddress.Zip, invoiceBillingAddress.Street).Value;

			if (string.IsNullOrEmpty(correctedStreet)
			 || invoiceBillingAddress.Street == correctedStreet) {
				returnValue = 1;
			}
			else {
				invoiceBillingAddress.Street = correctedStreet;
				returnValue = 2;
			}

			invoiceBillingAddress.ChangedUserName = userName;

			// -------------------------------------------------------------------------

			var invoiceDeliveryAddresses = (from b in ServiceCenterContext.InvoiceDeliveryAddress
											where b.InvoiceFk == invoicePk
											select b);

			InvoiceDeliveryAddress invoiceDeliveryAddress;

			if (invoiceDeliveryAddresses.Count() == 0) {
				invoiceDeliveryAddress = new InvoiceDeliveryAddress() {
					InvoiceFk = invoice.InvoicePk
				};
				ServiceCenterContext.InvoiceDeliveryAddress.Add(invoiceDeliveryAddress);
			}
			else {
				invoiceDeliveryAddress = invoiceDeliveryAddresses.Single();
				ServiceCenterContext.InvoiceDeliveryAddress.Update(invoiceDeliveryAddress);
			}

			// Transpose parameter values to delivery object
			foreach (JProperty child in ((JObject)parameters).Children()) {
				PropertyInfo propertyInfo = invoiceDeliveryAddress.GetType().GetProperty(child.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				if (propertyInfo != null) {
					if (child.Value.Type == JTokenType.Null) {
						propertyInfo.SetValue(invoiceDeliveryAddress, null);
					}
					else {
						propertyInfo.SetValue(invoiceDeliveryAddress, Convert.ChangeType(child.Value, propertyInfo.PropertyType, System.Globalization.CultureInfo.InvariantCulture));
					}
				}
			}

			string bestGuessDeliveryStreet = lookups.GetBestGuestStreetFromZip(invoiceDeliveryAddress.Zip, invoiceDeliveryAddress.Street).Value;
			if (!string.IsNullOrEmpty(bestGuessDeliveryStreet))
				invoiceDeliveryAddress.Street = bestGuessDeliveryStreet;

			lookups = null;

			invoiceDeliveryAddress.ChangedUserName = userName;

			// -------------------------------------------------------------------------

			if (await ServiceCenterContext.SaveChangesAsync().ConfigureAwait(false) == 0)
				returnValue = 0;

			return returnValue;
		}

		[HttpGet]
		public ActionResult<Dictionary<int, int>> GetPznAvailabilityTypeOverrides() {

			var returnValue = (from o in ServiceCenterContext.PznAvailabilityTypeOverride
							   select new {
								   o.Pzn,
								   o.AvailabilityType
							   });

			return returnValue.ToDictionary(x => x.Pzn, y => y.AvailabilityType);
		}

		[HttpGet]
		public ActionResult<List<string>> GetCustomerWarnings(int affiliateId, int customerId) {
			List<string> returnValue = new List<string>();

			if (affiliateId < 0
			 || customerId < 1)
				return returnValue;

			int kundenNr = GetKundenNr(customerId).Value;

			if (kundenNr < 0)
				return returnValue;

			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --

			foreach (string warning in (from w in ServiceCenterContext.CustomerWarning
										where w.KundenNr == kundenNr
										select w.WarningType)
				) {
				returnValue.Add(warning);
			}
			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --

			CustomersController customers = new CustomersController(MAINDBContext);
			ActionResult<List<string>> warnings = customers.GetCustomerWarnings(kundenNr);

			if (warnings.Value != null)
				foreach (string warning in warnings.Value)
					returnValue.Add(warning);

			customers = null;


			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --

			return returnValue;
		}
	}
}
