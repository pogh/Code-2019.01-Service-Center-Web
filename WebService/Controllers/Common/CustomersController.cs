using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Company.BusinessObjects.Common;
using Company.WebService.Models.MAINDB;
using Company.WebService.Models.Extensions.Comparers;
using Company.WebService.Models.Extensions.WebApplication;

namespace Company.WebService.Controllers.Common {

	public class CustomersController : BaseController {


		readonly MAINDBContext _context;
		public CustomersController(MAINDBContext context) : base(context) {
			_context = context;
		}

#pragma warning disable CA1307 // Specify StringComparison
		[HttpGet]

		public ActionResult<List<Customer>> Search(string affiliateIds, string customerNumber = null, string name = null, string zip = null, string orderNr = null, string emailAddress = null, string invoiceNumber = null, int limit = 0) {

			if (string.IsNullOrEmpty(customerNumber)
			&& string.IsNullOrEmpty(name)
			&& string.IsNullOrEmpty(zip)
			&& string.IsNullOrEmpty(orderNr)
			&& string.IsNullOrEmpty(emailAddress)
			&& string.IsNullOrEmpty(invoiceNumber)
			) {
				return new BadRequestResult();
			}

			var searchQuery = (from k in MAINDBContext.Kunden
							   join b in MAINDBContext.Belege on k.PkkundenId equals b.FkordCusId into groupjoin1
							   from b2 in groupjoin1.DefaultIfEmpty()
							   join l in MAINDBContext.Liefertabelle on b2.PkbelegId equals l.PkbelegId into groupjoin2
							   from l2 in groupjoin2.DefaultIfEmpty()
							   select new {
								   k.PkkundenId,
								   k.KundenNr,
								   k.CusFirstname,
								   k.CusSurname,
								   k.Name1,
								   k.Name2,
								   k.CusStreet,
								   k.CusCity,
								   k.CusZip,
								   k.CusMail,
								   k.CusFon,
								   k.CusMobil,
								   k.CusEbaynick,
								   k.CusBirth,
								   kunden_name1 = k.Name1,
								   kunden_name2 = k.Name2,
								   b2.BelegNr,
								   b2.OrdOrderId,
								   liefer_name1 = l2.Name1,
								   liefer_name2 = l2.Name2,
								   liefer_street = l2.Strasse,
								   liefer_city = l2.Ort,
								   liefer_zip = l2.Plz
							   });

			// ----- customerNumber -------------------------------------------
			if (!string.IsNullOrEmpty(customerNumber)) {
				searchQuery = searchQuery.Where(x => x.KundenNr.StartsWith(customerNumber.PadLeft(7, '0')));
			}

			// ----- name -----------------------------------------------------
			/*
			if (!string.IsNullOrEmpty(name)) {

				string[] nameParts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

				if (nameParts.Length > 0) {
					foreach (string namePart in nameParts)
						searchQuery = searchQuery.Where(x => x.CusFirstname.Contains(namePart.Trim()) || x.CusSurname.Contains(namePart.Trim()) || x.Name1.Contains(namePart.Trim()) || x.Name2.Contains(namePart.Trim()));
				}
			}
			*/

			if (!string.IsNullOrEmpty(name)) {
				List<decimal> customerNamesIds = SearchCustomerName(name, zip);  // "Optimisation"
				if (customerNamesIds.Count == 0)
					searchQuery = searchQuery.Where(x => 1 == 0);  // When nothing is found with a matching name, then no need to return anything
				else
					searchQuery = searchQuery.Where(x => customerNamesIds.Contains(x.PkkundenId));
			}

			// ----- zip -----------------------------------------------------
			if (!string.IsNullOrEmpty(zip)) {
				searchQuery = searchQuery.Where(x => x.CusZip.Equals(zip) || x.liefer_zip.Equals(zip));
			}

			// ----- order number -----------------------------------------------------
			if (!string.IsNullOrEmpty(orderNr)) {
				searchQuery = searchQuery.Where(x => x.OrdOrderId.Equals(orderNr));
			}

			// ----- emailAddress -----------------------------------------------------
			if (!string.IsNullOrEmpty(emailAddress)) {
				searchQuery = searchQuery.Where(x => x.CusMail.Contains(emailAddress));
			}

			// ----- invoiceNumber -----------------------------------------------------
			if (!string.IsNullOrEmpty(invoiceNumber)) {
				searchQuery = searchQuery.Where(x => x.BelegNr.StartsWith(invoiceNumber));
			}

			// ----- limit -----------------------------------------------------
			if (limit > 0) {
				searchQuery = searchQuery.OrderByDescending(x => x.PkkundenId).Take(limit * 10);
			}

			// ------------------------------------------------------------------

			List<decimal> kundenIds = searchQuery.Select(x => x.PkkundenId).Distinct().ToList();

			// ------------------------------------------------------------------

			var returnQuery = (from k in MAINDBContext.Kunden
								   //-- affiliate mapping
							   join µ in MAINDBContext.SchrammOsaffiliateMapping on k.AffiliatePartnerId equals µ.AffiliateAlt
							   join a in MAINDBContext.Affiliates on µ.AffiliateNeu equals a.AffiliateId
							   //-- ugly left joins
							   join b in MAINDBContext.Belege on k.PkkundenId equals b.FkordCusId into groupjoin1
							   from b2 in groupjoin1.DefaultIfEmpty()
							   join l in MAINDBContext.Liefertabelle on b2.PkbelegId equals l.PkbelegId into groupjoin2
							   from l2 in groupjoin2.DefaultIfEmpty()
							   join g in MAINDBContext.KundenGruppen on k.KundenGruppe equals g.PkkundenGruppeId into groupjoin3
							   from g2 in groupjoin3.DefaultIfEmpty()
								   // --
							   select new {
								   k.PkkundenId,
								   k.KundenNr,
								   k.CusBirth,
								   k.CusFirstname,
								   k.CusSurname,
								   k.Name1,
								   k.Name2,
								   k.CusStreet,
								   k.CusZip,
								   k.CusCity,
								   k.CusFon,
								   k.CusMobil,
								   k.CusFax,
								   k.CusMail,
								   k.CusEbaynick,
								   µ.AffiliateNeu,
								   a.Key,
								   k.KundenGruppe,
								   g2.KundenGruppeName,
								   g2.KundenGruppeRabatt,
								   // Search fields
								   kunden_name1 = k.Name1,
								   kunden_name2 = k.Name2,
								   liefer_name1 = l2.Name1,
								   liefer_name2 = l2.Name2,
								   liefer_street = l2.Strasse,
								   liefer_city = l2.Ort,
								   liefer_zip = l2.Plz,
								   b2.BelegNr
							   });

			if (!string.IsNullOrEmpty(affiliateIds)) {
				List<int> ids = JsonConvert.DeserializeObject<int[]>(affiliateIds).ToList<int>();
				returnQuery = returnQuery.Where(x => ids.Contains(x.AffiliateNeu));
			}

			returnQuery = returnQuery.Where(x => kundenIds.Contains(x.PkkundenId));

			List<Customer> customers = returnQuery
				.OrderByDescending(y => y.PkkundenId)
				.Select(x => new Customer() {
					Id = System.Convert.ToInt32(x.PkkundenId),
					CustomerNumber = x.KundenNr,
					DoB = Utils.ConvertSchrammDateTime(x.CusBirth),
					FirstName = string.IsNullOrEmpty(x.CusFirstname) ? x.Name1 : x.CusFirstname,
					LastName = string.IsNullOrEmpty(x.CusSurname) ? x.Name2 : x.CusSurname,
					Street = x.CusStreet,
					Zip = x.CusZip,
					City = x.CusCity,
					TelephoneNumber = x.CusFon,
					MobileNumber = x.CusMobil,
					FaxNumber = x.CusFax,
					EmailAddress = x.CusMail,
					AffiliateId = x.AffiliateNeu,
					AffiliateKey = x.Key,
					CustomerGroupId = Convert.ToInt32(x.KundenGruppe, System.Globalization.CultureInfo.InvariantCulture),
					CustomerGroupName = x.KundenGruppeName,
					CustomerGroupDiscount = x.KundenGruppeRabatt.GetValueOrDefault()
				}
			).ToList();

			List<Customer> returnValue = customers.Distinct<Customer>(new CustomerIdComparer()).Take(limit).ToList();

			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --

			// Sorts the list according to most recent order
			// This doesn't belong here but it was just too tempting and easy to put it here
			SortedList<int, DateTime> sortList = new SortedList<int, DateTime>();

			if (returnValue.Count() > 0) {
				using (var command = _context.Database.GetDbConnection().CreateCommand()) {
					command.CommandText = string.Concat("SELECT FKord_cus_id, MAX(beleg_datum) max_beleg_datum FROM dbo.belege WHERE FKord_cus_id IN (", string.Join(", ", returnValue.Select(x => x.Id)) ,") GROUP BY FKord_cus_id");
					_context.Database.OpenConnection();
					using (DbDataReader reader = command.ExecuteReader()) {
						while (reader.Read())
							sortList.Add(System.Convert.ToInt32(reader.GetDecimal(0)), reader.GetDateTime(1));
						reader.Close();
					}
				}
			}

			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --

			return returnValue.OrderBy(x => sortList.IndexOfKey(x.Id) == -1 ? int.MaxValue : sortList.IndexOfKey(x.Id)).ToList();
		}
#pragma warning restore CA1307 // Specify StringComparison

		// TODO: This needs to be sovled properly
		// Don't hate me
		// If you parameterise the query it takes 4 times as long
		// If you don't set the MAXDOP it takes 4 times as long
		// i.e. a query can take a minute (and I really mean over 60 seconds) without this "Optimisation"
		// Surname / Zip is the most common search
		private List<decimal> SearchCustomerName(string name, string zip) {
			List<decimal> returnValue = new List<decimal>();

			string[] nameParts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			int namePartsCount = nameParts.Length;

			StringBuilder sql = new StringBuilder();

			foreach (string fieldName in new string[] { "cus_firstname", "cus_surname", "name1", "name2" }) {

				sql.AppendLine("SELECT PKkunden_id ");
				sql.AppendLine("FROM dbo.Kunden ");

				for (int n = 0; n < namePartsCount; n += 1) {
					if (n == 0)
						sql.AppendLine(string.Concat("WHERE ", fieldName, " LIKE '%", nameParts[n], "%'"));
					else
						sql.AppendLine(string.Concat("  AND ", fieldName, " LIKE '%", nameParts[n], "'"));
				}

				if (!string.IsNullOrEmpty(zip))
					sql.AppendLine(string.Concat("  AND cus_zip LIKE '%", zip.Trim(), "'"));

				sql.AppendLine("UNION ");
			}

			sql.Remove(sql.Length - 8, 8);
			sql.AppendLine("OPTION (MAXDOP 8)");

			using (var command = _context.Database.GetDbConnection().CreateCommand()) {
				command.CommandText = sql.ToString();
				_context.Database.OpenConnection();
				using (DbDataReader reader = command.ExecuteReader()) {
					while (reader.Read())
						returnValue.Add(reader.GetDecimal(0));
				}
			}

			return returnValue;
		}

		[HttpGet]
		public ActionResult<Customer> Get(int customerId) {

			if (customerId < 1)
				return new BadRequestResult();

			var result = (from k in MAINDBContext.Kunden
						  join µ in MAINDBContext.SchrammOsaffiliateMapping on k.AffiliatePartnerId equals µ.AffiliateAlt
						  join a in MAINDBContext.Affiliates on µ.AffiliateNeu equals a.AffiliateId
						  join g in MAINDBContext.KundenGruppen on k.KundenGruppe equals g.PkkundenGruppeId into groupjoin3
						  from g2 in groupjoin3.DefaultIfEmpty()
						  where k.PkkundenId == customerId
						  select new {
							  k.PkkundenId,
							  k.KundenNr,
							  k.CusBirth,
							  k.FkcusGenId,
							  k.CusFirstname,
							  k.CusSurname,
							  k.Name1,
							  k.Name2,
							  k.Name3,
							  k.CusStreet,
							  k.CusZip,
							  k.CusCity,
							  k.CusFon,
							  k.CusMobil,
							  k.CusMail,
							  µ.AffiliateNeu,
							  a.Key,
							  k.KundenGruppe,
							  g2.KundenGruppeName,
							  g2.KundenGruppeRabatt
						  }).Single();


			var (firstName, lastName, companyName, additionalLine) = SplitNameFields(result.CusFirstname, result.CusSurname, result.Name1, result.Name2, result.Name3);

			Customer returnValue = new Customer() {
				Id = System.Convert.ToInt32(result.PkkundenId),
				CustomerNumber = result.KundenNr,
				DoB = Utils.ConvertSchrammDateTime(result.CusBirth),
				Title = result.FkcusGenId,
				FirstName = firstName,
				LastName = lastName,
				Street = result.CusStreet,
				Zip = result.CusZip,
				City = result.CusCity,
				AdditionalLine = additionalLine,
				CompanyName = companyName,
				TelephoneNumber = result.CusFon,
				MobileNumber = result.CusMobil,
				EmailAddress = result.CusMail,
				AffiliateId = result.AffiliateNeu,
				AffiliateKey = result.Key,
				CustomerGroupId = Convert.ToInt32(result.KundenGruppe, System.Globalization.CultureInfo.InvariantCulture),
				CustomerGroupName = result.KundenGruppeName,
				CustomerGroupDiscount = result.KundenGruppeRabatt.GetValueOrDefault()
			};

			return returnValue;
		}

		[HttpGet]
		public ActionResult<List<DeliveryAddress>> GetDeliveryAddresses(int customerId) {

			if (customerId < 1)
				return new BadRequestResult();

			StringBuilder sql = new StringBuilder();

			sql.AppendLine("SELECT DISTINCT CAST(MAX(l.PKLiefertabelle_ID) AS INT) AddressId, k.cus_firstname FirstName, k.cus_surname LastName, l.Anrede Title, l.Name1, l.Name2, l.Name3 Name3, ISNULL(l.Strasse, '') Street, ISNULL(l.plz, '') Zip, ISNULL(l.Ort, '') City, CAST(b.Laenderschluessel AS NVARCHAR(MAX)) Country, null AdditionalLine, null CompanyName ");
			sql.AppendLine("FROM dbo.belege b ");
			sql.AppendLine("JOIN dbo.Liefertabelle l ON b.PKbeleg_id = l.PKbeleg_id ");
			sql.AppendLine("JOIN dbo.Kunden k ON b.FKord_cus_id = K.PKkunden_id ");
			sql.AppendLine("WHERE b.FKord_cus_id = @customerId ");
			sql.AppendLine("GROUP BY k.cus_firstname, k.cus_surname, l.Anrede, l.Name1, l.Name2, l.Name3, l.Strasse, l.plz, l.Ort, b.Laenderschluessel ");
			sql.AppendLine("UNION ALL ");
			sql.AppendLine("SELECT DISTINCT -CAST(FKord_cus_id AS INT) AddressId, k.cus_firstname FirstName, k.cus_surname LastName, FKcus_gen_id Title, k.Name1, k.Name2, k.Name3 Name3, ISNULL(k.cus_street, '') Street, ISNULL(k.cus_zip, '') Zip, ISNULL(k.cus_city, '') City, CAST(b.Laenderschluessel AS NVARCHAR(MAX)) Country, null AdditionalLine, null CompanyName  ");
			sql.AppendLine("FROM dbo.belege b ");
			sql.AppendLine("JOIN dbo.Kunden k ON b.FKord_cus_id = K.PKkunden_id ");
			sql.AppendLine("WHERE b.FKord_cus_id = @customerId ");

			var addresses = MAINDBContext.Addresses.FromSql(sql.ToString(), new SqlParameter("@customerId", customerId));

			List<DeliveryAddress> returnValue = new List<DeliveryAddress>();
			foreach (WebService.Models.Extensions.WebApplication.Address address in addresses)
				try {
					CountryKey countryKey;

					if (Enum.TryParse<CountryKey>(address.Country, out countryKey)) {
						address.Country = System.Convert.ToString(countryKey, System.Globalization.CultureInfo.InvariantCulture);
					}

					var (firstName, lastName, companyName, additionalLine) = SplitNameFields(address.FirstName, address.LastName, address.Name1, address.Name2, address.Name3);

					returnValue.Add(new DeliveryAddress() {
						Title = string.IsNullOrEmpty(address.Title) ? null : address.Title.Trim(),
						FirstName = string.IsNullOrEmpty(firstName) ? null : firstName.Trim(),
						LastName = string.IsNullOrEmpty(lastName) ? null : lastName.Trim(),
						CompanyName = string.IsNullOrEmpty(companyName) ? null : companyName.Trim(),
						Street = string.IsNullOrEmpty(address.Street) ? null : address.Street.Trim(),
						Zip = string.IsNullOrEmpty(address.Zip) ? null : address.Zip.Trim(),
						City = string.IsNullOrEmpty(address.City) ? null : address.City.Trim(),
						Country = string.IsNullOrEmpty(address.Country) ? null : address.Country.Trim(),
						AdditionalLine = string.IsNullOrEmpty(additionalLine) ? null : additionalLine.Trim()
					});
				}
				catch {
					continue;
				}

			return returnValue.Distinct<DeliveryAddress>(new AddressComparer()).ToList();
		}

		// Name and address are saved in 27 different fields
		private (string FirstName, string LastName, string CompanyName, string AdditionalLine) SplitNameFields(string currentFirstName, string currentLastName, string name1, string name2, string name3) {

			if (string.IsNullOrEmpty(name1)
			 || string.IsNullOrWhiteSpace(name1)) {
				name1 = name2.Trim();
				name2 = name3.Trim();
				name3 = string.Empty;
			}

			if (name1 != null)
				name1 = Regex.Replace(name1, @"\s+", " ");

			if (name2 != null)
				name2 = Regex.Replace(name2, @"\s+", " ");

			if (name3 != null)
				name3 = Regex.Replace(name3, @"\s+", " ");

			string companyName = null;
			string fullName = null;
			string additionalLine = null;

			if (string.IsNullOrEmpty(name1)
			 && string.IsNullOrEmpty(name2)
			 && string.IsNullOrEmpty(name3)) {
				return (FirstName: null, LastName: null, CompanyName: null, AdditionalLine: null);
			}
			else if (string.IsNullOrEmpty(name2)
				  && string.IsNullOrEmpty(name3)) {
				fullName = name1.Trim();
			}
			else if (string.IsNullOrEmpty(name3)) {

				string testFullname = string.Concat(currentFirstName, " ", currentLastName).Trim();
				testFullname = Regex.Replace(testFullname, @"\s+", " ");

				if (name2.Equals(testFullname, StringComparison.InvariantCultureIgnoreCase)) {
					companyName = name1.Trim();
					fullName = name2.Trim();
				}
				else {
					fullName = name1.Trim();
					additionalLine = name2.Trim();
				}
			}
			else {
				companyName = name1.Trim();
				fullName = name2.Trim();
				additionalLine = name3.Trim();
			}

			string[] fullnameSplit = fullName.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
			int n = fullnameSplit.Length;
			string[] firstLastname = new string[n];

			int m = System.Convert.ToInt32(Math.Floor(n / 2.0));

			Array.Copy(fullnameSplit, 0, firstLastname, 0, m);
			string firstName = string.Join(' ', firstLastname).Trim();

			Array.Copy(fullnameSplit, m, firstLastname, 0, n - m);
			string lastName = string.Join(' ', firstLastname).Trim();

			return (FirstName: firstName, LastName: lastName, CompanyName: companyName, AdditionalLine: additionalLine);
		}

		[HttpGet]
		public ActionResult<List<CustomerGroupResult>> GetCustomerGroups() {

			var result = (from g in MAINDBContext.KundenGruppen
						  select new CustomerGroupResult() {
							  CustomerGroupId = g.PkkundenGruppeId,
							  CustomerGroupName = g.KundenGruppeName,
							  CustomerGroupDiscount = g.KundenGruppeRabatt ?? 0
						  });

			return result.ToList();
		}

		[HttpGet]
		public ActionResult<List<string>> GetCustomerWarnings(int kundenNr) {
			List<string> returnValue = new List<string>();

			if (kundenNr < 1)
				return returnValue;


			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --

			StringBuilder sql = new StringBuilder();
			sql.AppendLine("SELECT p.ScoreAmpel ");
			sql.AppendLine("FROM dbo.BoniversumRequest q ");
			sql.AppendLine("JOIN dbo.BoniversumResponse p ON q.AnfrageID = p.RequestID ");
			sql.AppendLine("WHERE q.KundenID = @kundenId ");
			sql.AppendLine("ORDER BY q.Anfragedatum DESC ");

			using (var command = _context.Database.GetDbConnection().CreateCommand()) {
				command.CommandText = sql.ToString();

				DbParameter p1 = command.CreateParameter();
				p1.ParameterName = "@kundenId";
				p1.DbType = System.Data.DbType.Int32;
				p1.Value = kundenNr;
				command.Parameters.Add(p1);

				_context.Database.OpenConnection();
				using (DbDataReader reader = command.ExecuteReader()) {
					if (reader.Read())
						switch (reader.GetString(0)) {

							case "grün":
								//No warning
								break;

							case "gelb":
								returnValue.Add("CREDIT_RATING_ORANGE");
								break;

							case "rot":
								returnValue.Add("CREDIT_RATING_RED");
								break;

							default:
								returnValue.Add("CREDIT_RATING_NONE");
								break;
						}
					else
						returnValue.Add("CREDIT_RATING_NONE");
					reader.Close();
				}
			}

			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --

			sql.Clear();
			sql.AppendLine("SELECT ");
			sql.AppendLine("	CASE WHEN K.FKcus_cst_id = 9 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS Leifersperre, ");
			sql.AppendLine("	CASE WHEN K.cus_tmp = 1 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END Mahnsperre ");
			sql.AppendLine("FROM dbo.Kunden K ");
			sql.AppendLine("WHERE K.PKkunden_id = @kundenId ");

			using (var command = _context.Database.GetDbConnection().CreateCommand()) {
				command.CommandText = sql.ToString();

				DbParameter p1 = command.CreateParameter();
				p1.ParameterName = "@kundenId";
				p1.DbType = System.Data.DbType.Int32;
				p1.Value = kundenNr;
				command.Parameters.Add(p1);

				_context.Database.OpenConnection();
				using (DbDataReader reader = command.ExecuteReader()) {
					if (reader.Read()) {
						if (reader.GetBoolean(0))
							returnValue.Add("DELIVERY_STOP");
						if (reader.GetBoolean(1))
							returnValue.Add("PAYMENT_REMINDER_STOP");
					}
					reader.Close();
				}
			}

			//  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --  --

			return returnValue;
		}
	}
}
