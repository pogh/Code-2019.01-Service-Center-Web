using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Company.WebService.Models.MAINDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Company.BusinessObjects.Common;
using System.Data.Common;

namespace Company.WebService.Controllers.Common {

	public class LookupsController : BaseController {

		readonly MAINDBContext _context;
		public LookupsController(MAINDBContext context) : base(context) {
			_context = context;
		}
		[HttpGet]
		public ActionResult<List<Affiliate>> GetAffiliates() {

			StringBuilder sql = new StringBuilder();

			sql.AppendLine("SELECT a.id, a.name, a.Schluessel, b.RGB ");
			sql.AppendLine("FROM dbo.Affiliate a ");
			sql.AppendLine("LEFT JOIN( ");
			sql.AppendLine("SELECT AffiliateId, [28] RGB ");
			sql.AppendLine("FROM( ");
			sql.AppendLine("SELECT * ");
			sql.AppendLine("FROM dbo.AffiliateKonfiguration ");
			sql.AppendLine("WHERE KonfigurationstypID = 28 ");
			sql.AppendLine(") P ");
			sql.AppendLine("PIVOT( ");
			sql.AppendLine("		MAX(Wert) ");
			sql.AppendLine("		FOR KonfigurationstypID IN([28]) ");
			sql.AppendLine("		) Q ");
			sql.AppendLine("	) b ON a.ID = b.AffiliateID ");
			sql.AppendLine("WHERE a.Aktiv = 1 ");

			var result = MAINDBContext.Affiliates.FromSql(sql.ToString());

			return result.Select(x => new Affiliate() {
				AffiliateId = x.AffiliateId,
				Name = x.Name,
				Key = x.Key,
				RGB = x.RGB
			}).ToList();
		}


		[HttpGet]
		public ActionResult<List<PaymentMatrixRow>> GetPaymentMatrix() {

			StringBuilder sql = new StringBuilder();

			sql.AppendLine("SELECT ");
			sql.AppendLine("	m.PKVersandkostenBerechnungID RowId, ");
			sql.AppendLine("	k.Affiliate AffiliateId, ");
			sql.AppendLine("	m.ZahlungsweiseID PaymentTypeId, ");
			sql.AppendLine("	z.Zahlungsweise PaymentTypeName, ");
			sql.AppendLine("	m.LieferartID DeliveryTypeId, ");
			sql.AppendLine("	la.Lieferart_Sprachwert DeliveryTypeName, ");
			sql.AppendLine("	m.Versandkosten ShippingCosts, ");
			sql.AppendLine("	m.Versandkostengrenze ShippingCostsLimit ");
			sql.AppendLine("FROM dbo.VersandkostenMatrix m ");
			sql.AppendLine("JOIN dbo.Absatzkanal k ON m.AbsatzkanalID = k.PKAbsatzkanalID ");
			sql.AppendLine("JOIN dbo.Zahlungsweise z ON m.ZahlungsweiseID = z.PKdvt_id ");
			sql.AppendLine("JOIN dbo.Lieferart la ON m.LieferartID = la.PKLieferart_ID ");
			sql.AppendLine("WHERE la.aktiv = 1 ");
			sql.AppendLine("  AND k.Absatzkanal LIKE 'SERVICE-CENTER %' ");
			sql.AppendLine("  AND m.ZahlungsweiseID IN(1, 5, 4) ");
			sql.AppendLine("ORDER BY k.Affiliate, k.Absatzkanal, z.Zahlungsweise, la.Lieferart_Sprachwert ");

			var results = MAINDBContext.PaymentMatrix.FromSql(sql.ToString());

			return results.ToList();
		}

		[HttpGet]
		public ActionResult<string> GetBestGuestCityFromZip(string zip, string street) {
			string returnValue = null;

			if (string.IsNullOrEmpty(zip)
			|| zip.Length < 4)
				return null;

			if (string.IsNullOrEmpty(street)
			|| street.Length < 3)
				return null;

			StringBuilder sql = new StringBuilder();
			sql.AppendLine("SELECT DISTINCT p.Ort ");
			sql.AppendLine("FROM dbo.strasse s ");
			sql.AppendLine("JOIN dbo.plz p ON s.Alort = p.Alort AND s.PLZ = p.PLZ ");
			sql.AppendLine("WHERE s.PLZ = @zip ");
			sql.AppendLine("  AND s.Strasse LIKE @street ");

			using (var command = _context.Database.GetDbConnection().CreateCommand()) {
				command.CommandText = sql.ToString();

				DbParameter p1 = command.CreateParameter();
				p1.ParameterName = "@zip";
				p1.DbType = System.Data.DbType.String;
				p1.Value = zip;
				command.Parameters.Add(p1);

				DbParameter p2 = command.CreateParameter();
				p2.ParameterName = "@street";
				p2.DbType = System.Data.DbType.String;
				command.Parameters.Add(p2);

				bool keepTrying = true;
				int i = 3;

				_context.Database.OpenConnection();

				while (keepTrying) {
					returnValue = string.Empty;
					p2.Value = string.Concat(street.Substring(0, i), "%");

					using (DbDataReader reader = command.ExecuteReader()) {
						if (reader.Read())
							returnValue = reader.GetString(0);
						keepTrying = reader.Read();
						reader.Close();
					}
					i += 1;
				}
			}
			return returnValue;
		}

		[HttpGet]
		public ActionResult<string> GetBestGuestStreetFromZip(string zip, string street) {
			if (string.IsNullOrEmpty(zip)
			|| zip.Length < 4)
				return street;

			if (string.IsNullOrEmpty(street)
			|| street.Length < 3)
				return street;

			//--------------------------------------------------------------------------

			int houseNumberIndex = street.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
			string streetNamePart = string.Empty;
			string houseNumberPart = string.Empty;

			if (houseNumberIndex > 1) {
				streetNamePart = street.Substring(0, houseNumberIndex).Trim();
				houseNumberPart = street.Substring(houseNumberIndex, street.Length - houseNumberIndex).Trim();
			}
			else {
				streetNamePart = street;
			}

			//--------------------------------------------------------------------------

			StringBuilder sql = new StringBuilder();
			sql.AppendLine("SELECT s.Strasse ");
			sql.AppendLine("FROM dbo.strasse s ");
			sql.AppendLine("WHERE s.PLZ = @zip ");
			sql.AppendLine("  AND s.Strasse LIKE @street ");

			//--------------------------------------------------------------------------

			List<string> guesses = new List<string>();

			if (streetNamePart.Length > 7 && streetNamePart.EndsWith("strasse", System.StringComparison.InvariantCultureIgnoreCase))
				streetNamePart = streetNamePart.Replace("strasse", "str.", System.StringComparison.InvariantCultureIgnoreCase);

			if (streetNamePart.Length > 6 && streetNamePart.EndsWith("straße", System.StringComparison.InvariantCultureIgnoreCase))
				streetNamePart = streetNamePart.Replace("straße", "str.", System.StringComparison.InvariantCultureIgnoreCase);

			if (streetNamePart.Length > 3 && streetNamePart.EndsWith("str", System.StringComparison.InvariantCultureIgnoreCase))
				streetNamePart = string.Concat(streetNamePart, ".");

			if (!streetNamePart.Equals(streetNamePart.Replace("ae", "ä", System.StringComparison.InvariantCultureIgnoreCase), System.StringComparison.InvariantCultureIgnoreCase))
				guesses.Add(streetNamePart.Replace("ae", "ä", System.StringComparison.InvariantCultureIgnoreCase));

			if (!streetNamePart.Equals(streetNamePart.Replace("oe", "ö", System.StringComparison.InvariantCultureIgnoreCase), System.StringComparison.InvariantCultureIgnoreCase))
				guesses.Add(streetNamePart.Replace("oe", "ö", System.StringComparison.InvariantCultureIgnoreCase));

			if (!streetNamePart.Equals(streetNamePart.Replace("ue", "ü", System.StringComparison.InvariantCultureIgnoreCase), System.StringComparison.InvariantCultureIgnoreCase))
				guesses.Add(streetNamePart.Replace("ue", "ü", System.StringComparison.InvariantCultureIgnoreCase));

			if (!streetNamePart.Equals(streetNamePart.Replace(" - ", "-", System.StringComparison.InvariantCultureIgnoreCase), System.StringComparison.InvariantCultureIgnoreCase))
				guesses.Add(streetNamePart.Replace(" - ", "-", System.StringComparison.InvariantCultureIgnoreCase));

			if (streetNamePart.Contains(" str", System.StringComparison.InvariantCultureIgnoreCase))
				guesses.Add(streetNamePart.Replace(" str", "str", System.StringComparison.InvariantCultureIgnoreCase));

			if (streetNamePart.Contains("str", System.StringComparison.InvariantCultureIgnoreCase))
				guesses.Add(streetNamePart.Replace("str", " str", System.StringComparison.InvariantCultureIgnoreCase));

			int j = streetNamePart.Length;
			int i = j - 5;
			if (i > 3)
				for (int m = j; i < m; m -= 1)
					guesses.Add(streetNamePart.Substring(0, m));
			else
				guesses.Add(streetNamePart);

			//--------------------------------------------------------------------------

			string correctedStreetNamePart = streetNamePart;

			using (var command = _context.Database.GetDbConnection().CreateCommand()) {
				command.CommandText = sql.ToString();

				DbParameter p1 = command.CreateParameter();
				p1.ParameterName = "@zip";
				p1.DbType = System.Data.DbType.String;
				p1.Value = zip;
				command.Parameters.Add(p1);

				DbParameter p2 = command.CreateParameter();
				p2.ParameterName = "@street";
				p2.DbType = System.Data.DbType.String;
				command.Parameters.Add(p2);

				_context.Database.OpenConnection();

				foreach (string guess in guesses) {
					correctedStreetNamePart = streetNamePart;
					p2.Value = string.Concat(guess, "%");

					using (DbDataReader reader = command.ExecuteReader()) {
						if (reader.Read()) {
							correctedStreetNamePart = reader.GetString(0);
							if (!reader.Read())
								break;
						}
						reader.Close();
					}
				}
			}

			//--------------------------------------------------------------------------

			return string.Concat(correctedStreetNamePart, " ", houseNumberPart).Trim();
		}
	}
}
