using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.BusinessObjects.Common;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.PartialViews;

namespace Company.WebApplication.Areas.ServiceCenter.Data {
	public partial class Order : BaseObject {
		public async Task<List<ArticleSearchResultArticle>> PznSearch(int affiliateId, int pzn = 0, string name = "", int limit = 0) {
			Dictionary<string, object> parameters = new Dictionary<string, object>();

			if (affiliateId > 0) {
				parameters.Add("affiliateId", affiliateId);
			}

			if (pzn > 0) {
				parameters.Add("pzn", pzn);
			}

			if (!string.IsNullOrEmpty(name)) {
				parameters.Add("name", name);
			}


			if (limit != 0) {
				parameters.Add("limit", limit);
			}

			if (parameters.Count == 0) {
				return new List<ArticleSearchResultArticle>();
			}
			else {
				string json = await base.GetJsonAsync("/common/articles/get", parameters).ConfigureAwait(false);
				List<ArticleSearchResultArticle> returnValue = JsonConvert.DeserializeObject<List<ArticleSearchResultArticle>>(json);
				AddUrlsToArticleSearchResults(affiliateId, ref returnValue);

				foreach (KeyValuePair<int, int> keyValuePair in PznAvailabilityTypeOverride) {
					foreach (ArticleSearchResultArticle article in returnValue.Where(x => x.PZN == keyValuePair.Key)) {
						article.AvailabilityType = keyValuePair.Value;
					}
				}

				return returnValue;
			}
		}

		public async Task<List<ArticleSearchResultArticle>> GetOftenOrderedItems(int affiliateId, int customerId) {
			if (customerId < 1) {
				return null;
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getoftenordereditems", new Dictionary<string, object>() { { "affiliateId", affiliateId }, { "customerid", customerId } }).ConfigureAwait(false);
				List<ArticleSearchResultArticle> returnValue = JsonConvert.DeserializeObject<List<ArticleSearchResultArticle>>(json);
				AddUrlsToArticleSearchResults(affiliateId, ref returnValue);
				return returnValue;
			}
		}

		public async Task<List<ArticleSearchResultArticle>> GetRecentOrderedItems(int affiliateId, int customerId) {
			if (customerId < 1) {
				return null;
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getrecentordereditems", new Dictionary<string, object>() { { "affiliateId", affiliateId }, { "customerid", customerId } }).ConfigureAwait(false);
				List<ArticleSearchResultArticle> returnValue = JsonConvert.DeserializeObject<List<ArticleSearchResultArticle>>(json);
				AddUrlsToArticleSearchResults(affiliateId, ref returnValue);
				return returnValue;
			}
		}

		private void AddUrlsToArticleSearchResults(int affiliateId, ref List<ArticleSearchResultArticle> articles) {
			var articlePictureTemplates = ArticlePictureTemplates.Where(x => x.AffiliateId == affiliateId);

			if (articlePictureTemplates.Count() == 0) {
				return;
			}
			else {

				Dictionary<int, string> w = ShopProductUrls;
				Dictionary<int, string> e = ShopProductPictureUrls;

				ArticlePictureTemplate apt = articlePictureTemplates.OrderByDescending(x => x.Height).First();

				foreach (ArticleSearchResultArticle article in articles) {

					StringBuilder sb = new StringBuilder(apt.TargetFile);
					sb.Replace("{pzn}", article.PZN.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(8, '0'));
					sb.Replace("{urlname}", article.BaseUrl);
					sb.Replace("{rang}", "1");
					sb.Insert(0, "/");
					sb.Insert(0, ShopProductPictureUrls[affiliateId]);
					sb.Replace("//", "/");
					sb.Insert(0, "https://");
					sb.Replace("--", "-");

					article.PictureUrl = sb.ToString();

					article.WebsiteUrl = string.Concat("https://", ShopProductUrls[affiliateId].TrimEnd('/'), "/", article.PZN.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(8, '0'), "-", article.BaseUrl.TrimStart('/'), ".html");
				}
			}
		}

		public async Task<bool> AddInvoiceItem(string userName, int affiliateId, int customerId, int pzn, int quantity, decimal vat, decimal itemPrice, decimal itemSavings) {

			if (string.IsNullOrEmpty(userName)
			 || affiliateId < 1
			 || customerId < 1
			 || pzn < 1)
				return false;

			using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

				httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
				httpClientHandler.UseDefaultCredentials = true;

				using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
					httpClient.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/addinvoiceitem"));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					var response = await httpClient.PostAsJsonAsync("",
										new {
											userName,
											affiliateId,
											customerId,
											pzn,
											quantity,
											vat,
											itemPrice,
											itemSavings
										}).ConfigureAwait(false);

					bool result = await response.Content.ReadAsAsync<bool>().ConfigureAwait(false);

					if (response.IsSuccessStatusCode)
						return result;
					else
						return false;
				}
			}
		}
		public async Task<bool> RemoveInvoiceItem(string userName, int affiliateId, int customerId, int invoiceItemId) {

			if (string.IsNullOrEmpty(userName)
			 || affiliateId < 1
			 || customerId < 1
			 || invoiceItemId < 1)
				return false;

			using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

				httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
				httpClientHandler.UseDefaultCredentials = true;

				using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
					httpClient.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/removeinvoiceitem"));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					var response = await httpClient.PostAsJsonAsync("",
										new {
											userName,
											affiliateId,
											customerId,
											invoiceItemId
										}).ConfigureAwait(false);

					bool result = await response.Content.ReadAsAsync<bool>().ConfigureAwait(false);

					if (response.IsSuccessStatusCode)
						return result;
					else
						return false;
				}
			}
		}

		public async Task<bool> UpdateInvoiceItemQuantity(string userName, int affiliateId, int customerId, int invoiceItemId, int quantity) {

			if (string.IsNullOrEmpty(userName)
			 || affiliateId < 1
			 || customerId < 1
			 || invoiceItemId < 1
			 || quantity < 0) {
				return false;
			}
			else {

				using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

					httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
					httpClientHandler.UseDefaultCredentials = true;

					using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
						httpClient.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/updateinvoiceitemquantity"));
						httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

						var response = await httpClient.PostAsJsonAsync("",
											new {
												userName,
												affiliateId,
												customerId,
												invoiceItemId,
												quantity
											}).ConfigureAwait(false);

						bool result = await response.Content.ReadAsAsync<bool>().ConfigureAwait(false);

						if (response.IsSuccessStatusCode)
							return result;
						else
							return false;
					}
				}
			}
		}

		public async Task<bool> IncrementInvoiceItem(string userName, int affiliateId, int customerId, int invoiceItemId) {

			if (string.IsNullOrEmpty(userName)
			 || affiliateId < 1
			 || customerId < 1
			 || invoiceItemId < 1) {
				return false;
			}
			else {

				using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

					httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
					httpClientHandler.UseDefaultCredentials = true;

					using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
						httpClient.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/incrementinvoiceitem"));
						httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

						var response = await httpClient.PostAsJsonAsync("",
											new {
												userName,
												affiliateId,
												customerId,
												invoiceItemId
											}).ConfigureAwait(false);

						bool result = await response.Content.ReadAsAsync<bool>().ConfigureAwait(false);

						if (response.IsSuccessStatusCode)
							return result;
						else
							return false;
					}
				}
			}
		}

		public async Task<bool> DecrementInvoiceItem(string userName, int affiliateId, int customerId, int invoiceItemId) {

			if (string.IsNullOrEmpty(userName)
			 || affiliateId < 1
			 || customerId < 1
			 || invoiceItemId < 1) {
				return false;
			}
			else {

				using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

					httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
					httpClientHandler.UseDefaultCredentials = true;

					using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
						httpClient.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/decrementinvoiceitem"));
						httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

						var response = await httpClient.PostAsJsonAsync("",
											new {
												userName,
												affiliateId,
												customerId,
												invoiceItemId
											}).ConfigureAwait(false);

						bool result = await response.Content.ReadAsAsync<bool>().ConfigureAwait(false);

						if (response.IsSuccessStatusCode)
							return result;
						else
							return false;
					}
				}
			}
		}

		public async Task<string> GetComparisonPrices(int affiliateId, int pzn) {
			if (affiliateId < 1
			 || pzn < 1) {
				return null;
			}
			else {
				string json = await base.GetJsonAsync("/common/articles/getcomparisonprices", new Dictionary<string, object>() { { "affiliateId", affiliateId }, { "pzn", pzn } }).ConfigureAwait(false);
				List<KeyValuePair<string, decimal>> prices = JsonConvert.DeserializeObject<List<KeyValuePair<string, decimal>>>(json);

				StringBuilder returnValue = new StringBuilder();

				foreach (KeyValuePair<string, decimal> pair in prices.OrderBy(x => x.Key))
					returnValue.Append(string.Concat(pair.Key, " ", pair.Value.ToString("0.00 €", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat), " – "));

				if (returnValue.Length > 3)
					returnValue.Remove(returnValue.Length - 3, 3);

				return returnValue.ToString();
			}
		}


		public async Task<List<string>> GetCustomerWarnings(int affiliateId, int customerId) {
			if (affiliateId < 0
			 || customerId < 1) {
				return new List<string>();
			}
			else {
				string json = await base.GetJsonAsync("/webapplication/servicecenter/getcustomerwarnings", new Dictionary<string, object>() { { "affiliateId", affiliateId }, { "customerId", customerId } }).ConfigureAwait(false);
				List<string> warnings = JsonConvert.DeserializeObject<List<string>>(json);
				return warnings;
			}
		}

		public async Task<bool> AddSpecialItem(string userName, int affiliateId, int customerId, int pzn, decimal itemPrice) {

			if (string.IsNullOrEmpty(userName)
			 || affiliateId < 1
			 || customerId < 1
			 || pzn < 1)
				return false;

			using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

				httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
				httpClientHandler.UseDefaultCredentials = true;

				using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
					httpClient.BaseAddress = new Uri(GetWebServiceUrl("/webapplication/servicecenter/addspecialitem"));
					httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					var response = await httpClient.PostAsJsonAsync("",
										new {
											userName,
											affiliateId,
											customerId,
											pzn,
											itemPrice,
										}).ConfigureAwait(false);

					bool result = await response.Content.ReadAsAsync<bool>().ConfigureAwait(false);

					if (response.IsSuccessStatusCode)
						return result;
					else
						return false;
				}
			}
		}

	}
}
