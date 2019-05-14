using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Company.BusinessObjects.Common;
using Company.WebService.Controllers.WebApplication;
using Company.WebService.Models.MAINDB;

namespace Company.WebService.Controllers.Common {
	public class ArticlesController : BaseController {
		public ArticlesController(MAINDBContext context) : base(context) {
		}

		[HttpGet]
		public ActionResult<List<Article>> Get(int affiliateId, int pzn = 0, string name = "", int limit = 0) {

			if (affiliateId < 1) {
				return new BadRequestResult();
			}

			if (pzn < 0) {
				return new BadRequestResult();
			}

			if (pzn == 0
			&& string.IsNullOrEmpty(name)) {
				return new BadRequestResult();
			}

			List<int> articleIds = new List<int>();

			if (pzn > 0)
				articleIds.AddRange(
						 (from asi in MAINDBContext.ArtikelstammIdentifikation
						  where asi.Pzn == pzn
						  select asi.Artikelnummer
						  ));

			if (string.IsNullOrEmpty(name.Trim())) {
				if (articleIds.Count > 0
				&& limit != 1)
					articleIds.AddRange(GetAssociatedArticleIds(articleIds.First(), limit));
			}
			else {
				var ids = (from asn in MAINDBContext.ArtikelstammName
						   select asn
						 );

				string[] nameParts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

				if (nameParts.Length > 0) {
					foreach (string namePart in nameParts)
						ids = ids.Where(x => x.Anzeigename.Contains(namePart.Trim(), StringComparison.InvariantCultureIgnoreCase));

					if (limit > 0) {
						ids = ids.Take(limit);
					}

					articleIds.AddRange(ids.Select(x => x.Artikelnummer).ToList());
				}
			}

			//--

			var query = (from aſ in MAINDBContext.AffiliateArtikelstamm
						 where aſ.AffiliateId == affiliateId
							&& aſ.Gelistet == true
							&& articleIds.Contains(aſ.Artikelnummer)
						 select new {
							 AffiliateArticleId = System.Convert.ToInt32(aſ.Id),
							 ArticleId = aſ.Artikelnummer,
							 ArticleObject = new Article() {
								 AffiliateArticleId = System.Convert.ToInt32(aſ.Id),
								 ArticleId = aſ.Artikelnummer,
							 }
						 });

			if (limit > 0) {
				query = query.Take(limit);
			}

			var returnValue = query.ToList();

			List<long> affiliateArticleIds = returnValue.Select(x => Convert.ToInt64(x.AffiliateArticleId)).ToList();

			// Add the PZN
			foreach (var item in (from i in MAINDBContext.ArtikelstammIdentifikation
								  where articleIds.Contains(i.Artikelnummer)
								  select new {
									  ArticleId = i.Artikelnummer,
									  PZN = i.Pzn
								  })) {
				foreach (var rv in returnValue.Where(x => x.ArticleId == item.ArticleId)) {
					rv.ArticleObject.PZN = item.PZN.Value;
				}

			}

			// Add the name
			foreach (var item in (from i in MAINDBContext.ArtikelstammName
								  where articleIds.Contains(i.Artikelnummer)
								  select new {
									  ArticleId = i.Artikelnummer,
									  ArticleName = i.Artikelname,
									  ArticleDisplayName = i.Anzeigename
								  })) {
				foreach (var rv in returnValue.Where(x => x.ArticleId == item.ArticleId)) {
					rv.ArticleObject.Name = item.ArticleName;
					rv.ArticleObject.DisplayName = item.ArticleDisplayName;
				}
			}

			foreach (var item in (from aſ in MAINDBContext.Artikelstamm
								  join f in MAINDBContext.ArtikelstammFirma on aſ.AnbieterFirmaId equals f.Id
								  where articleIds.Contains(aſ.Artikelnummer)
								  select new {
									  ArticleId = aſ.Artikelnummer,
									  CompanyName = f.Name
								  }
								 )) {
				foreach (var rv in returnValue.Where(x => x.ArticleId == item.ArticleId)) {
					rv.ArticleObject.DisplayName = string.Concat(rv.ArticleObject.DisplayName, " (", item.CompanyName, ")");
				}
			}

			// Add the availablity
			foreach (var item in (from i in MAINDBContext.AffiliateArtikelstammVerfuegbarkeit
								  where affiliateArticleIds.Contains(i.ArtikelstammId)
								  select new {
									  AffiliateArticleId = i.ArtikelstammId,
									  AvailabilityType = i.Verfuegbarkeit,
									  IsAvailable = i.Online,
									  IsLocked = i.Gesperrt,
									  OrderLimit = i.Mengenbegrenzung
								  })) {
				foreach (var rv in returnValue.Where(x => x.AffiliateArticleId == item.AffiliateArticleId)) {
					rv.ArticleObject.AvailabilityType = item.AvailabilityType;
					rv.ArticleObject.IsOnline = item.IsAvailable;
					rv.ArticleObject.IsLocked = item.IsLocked;
					rv.ArticleObject.OrderLimit = item.OrderLimit;
				}
			}

			// Add the amounts
			foreach (var item in (from m in MAINDBContext.ArtikelstammMenge
								  join e in MAINDBContext.ArtikelstammEinheit on m.EinheitId equals e.Id
								  where articleIds.Contains(m.Artikelnummer)
								  select new {
									  ArticleId = m.Artikelnummer,
									  PackagingAmount = m.Mengenangabe,
									  PackagingAmountTotal = m.Menge,
									  PackagingAmountUnit = e.Schluessel
								  })) {
				foreach (var rv in returnValue.Where(x => x.ArticleId == item.ArticleId)) {
					rv.ArticleObject.PackagingAmount = item.PackagingAmount;
					rv.ArticleObject.PackagingAmountTotal = Convert.ToInt32(item.PackagingAmountTotal);
					rv.ArticleObject.PackagingAmountUnit = item.PackagingAmountUnit;
				}
			}

			// Add the prices
			foreach (var item in (from p in MAINDBContext.AffiliateArtikelstammPreis
								  join m in MAINDBContext.MwstTable on p.MwstId equals m.PkmwstId
								  where affiliateArticleIds.Contains(p.ArtikelstammId)
								  select new {
									  AffiliateArticleId = p.ArtikelstammId,
									  SalePrice = p.Verkaufspreis,
									  RRP = p.Preisempfehlung.HasValue ? p.Preisempfehlung : p.Verkaufspreis,
									  MwSt = m.MwstWert
								  })) {
				foreach (var rv in returnValue.Where(x => x.AffiliateArticleId == item.AffiliateArticleId)) {
					rv.ArticleObject.SalePrice = item.SalePrice;
					rv.ArticleObject.VAT = item.MwSt;
					rv.ArticleObject.RRP = item.RRP.Value;
				}
			}

			// Add the IsPrescriptionOnly
			foreach (var item in (from a in MAINDBContext.ArtikelstammAttributisierung
								  where articleIds.Contains(a.Artikelnummer)
									&& a.AttributId == 41
								  select new {
									  ArticleId = a.Artikelnummer,
									  IsPrescriptionOnly = Convert.ToBoolean(a.Wert, System.Globalization.CultureInfo.InvariantCulture)
								  })) {
				foreach (var rv in returnValue.Where(x => x.ArticleId == item.ArticleId)) {
					rv.ArticleObject.IsPrescriptionOnly = item.IsPrescriptionOnly;
				}
			}

			return returnValue.Select(x => x.ArticleObject).ToList();
		}

		private List<int> GetAssociatedArticleIds(int articleId, int limit) {

			List<int> returnValue = new List<int>();

			foreach (var searchResult in
				(from s in MAINDBContext.ArtikelstammSuche
				 join sb in MAINDBContext.ArtikelstammSucheBegriff on s.Id equals sb.SucheId
				 where s.Artikelnummer == articleId
				 orderby sb.Rang
				 select new {
					 sb.Wert,
					 sb.Rang
				 })) {

				var ids = (from s in MAINDBContext.ArtikelstammSuche
						   join sb in MAINDBContext.ArtikelstammSucheBegriff on s.Id equals sb.SucheId
						   where sb.Rang == searchResult.Rang
							  && sb.Wert == searchResult.Wert
						   select s.Artikelnummer);

				if (limit > 0)
					ids = ids.Take(limit);

				if (returnValue.Count == 0) {
					returnValue = ids.ToList();
				}
				else {
					List<int> newList = returnValue.Intersect(ids).ToList();

					if (limit > 0
					&& newList.Count > limit) {
						returnValue = newList.Take(limit).ToList();
						break;
					}

					if (newList.Count == 1)
						break;

					returnValue = returnValue.Intersect(ids).ToList();
				}
			};

			return returnValue;
		}

		[HttpGet]
		public ActionResult<List<ArticlePictureTemplate>> GetArticlePictureTemplates() {

			var templates = (from x in MAINDBContext.AffiliateArtikelstammBildvorgabe
							 select x);

			List<ArticlePictureTemplate> returnValue = new List<ArticlePictureTemplate>();

			foreach (var template in templates) {
				ArticlePictureTemplate apt = new ArticlePictureTemplate();

				dynamic parameter = JObject.Parse(template.Parameter);

				apt.Id = template.Id;
				apt.Key = template.Schluessel;
				apt.AffiliateId = template.AffiliateId;
				apt.Width = parameter.width;
				apt.Height = parameter.height;
				apt.Filter = parameter.filter;
				apt.Blur = parameter.blur;
				apt.BestFit = parameter.bestfit;
				apt.CompressionQuality = parameter.compressionquality;
				apt.SourceBucket = parameter.sourceBucket;
				apt.TargetBucket = parameter.targetBucket;
				apt.TargetFile = parameter.targetFile;

				returnValue.Add(apt);
			}

			return returnValue;
		}


		[HttpGet]
		public ActionResult<decimal> GetVAT(int affiliateId, int pzn) {

			var vats = (from i in MAINDBContext.ArtikelstammIdentifikation
						join s in MAINDBContext.AffiliateArtikelstamm on i.Artikelnummer equals s.Artikelnummer
						join p in MAINDBContext.AffiliateArtikelstammPreis on s.Id equals p.ArtikelstammId
						join m in MAINDBContext.MwstTable on p.MwstId equals m.PkmwstId
						where i.Pzn == pzn
							&& s.AffiliateId == affiliateId
						select m.MwstWert);

			switch (vats.Count()) {
				case 0:
					return 19.0M;               // Best guess

				case 1:
					return vats.Single();

				default:
					return vats.First();        // Hopefully doesn't happen
			}
		}

		[HttpGet]
		public ActionResult<List<KeyValuePair<string, decimal>>> GetComparisonPrices(int affiliateId, int pzn) {

				var prices = (from i in MAINDBContext.ArtikelstammIdentifikation
						  join s in MAINDBContext.Artikelstamm on i.Artikelnummer equals s.Artikelnummer
						  join a in MAINDBContext.AffiliateArtikelstamm on s.Artikelnummer equals a.Artikelnummer
						  join ap in MAINDBContext.AffiliateArtikelstammAlternativPreis on a.Id equals ap.ArtikelstammId
						  join sa in MAINDBContext.ArtikelstammSuchanbieter on ap.SuchanbieterId equals sa.Id
						  where a.AffiliateId == affiliateId
							 && i.Pzn == pzn
						  select new {
							  VendorName = sa.Name,
							  Price = ap.Verkaufspreis
						  });

			return prices.Select(x => new KeyValuePair<string, decimal>(x.VendorName, x.Price)).ToList();
		}
	}
}
