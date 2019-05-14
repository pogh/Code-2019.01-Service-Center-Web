using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Company.BusinessObjects.Common;
using Company.WebApplication.Areas.ServiceCenter.Models.Order;
using Company.WebApplication.Areas.ServiceCenter.Models.Order.Objects;

namespace Company.WebApplication.Areas.ServiceCenter.Data {

	[Authorize(Policy = "ServiceCenterUser")]
	public abstract class BaseObject {

		protected BaseObject(IConfiguration configuration, IMemoryCache memoryCache) {
			_configuration = configuration;
			_memoryCache = memoryCache;
		}

		/// <summary>
		/// ⚠ Make sure to always use lock(_configuration) {...} 
		/// </summary>
		private readonly IConfiguration _configuration;
		protected string GetConfigurationValue(string key) {
			string returnValue;
			lock (_configuration) {
				returnValue = string.Copy(_configuration[key]);
			}
			return returnValue;
		}

		/// <summary>
		/// ⚠ Make sure to always use lock(_memoryCache) {...} 
		/// </summary>
		private readonly IMemoryCache _memoryCache;

#pragma warning disable CA1055 // Uri return values should not be strings

		public string WebServiceHostName {
			get {

				string cacheKey = "WebServiceHostName";
				string cacheValue;

				bool inCache = false;
				lock (_memoryCache) {
					inCache = _memoryCache.TryGetValue(cacheKey, out cacheValue);
				}

				if (!inCache) {
					Uri baseUri = new Uri(GetConfigurationValue("WebServiceUrl"));
					cacheValue = baseUri.Host;
					lock (_memoryCache) {
						_memoryCache.Set(cacheKey, cacheValue);
					}
				}
				return cacheValue;
			}
		}


		protected string GetWebServiceUrl(string pathSegment) {
			string baseUrl;
			baseUrl = GetConfigurationValue("WebServiceUrl");
			return string.Concat(baseUrl.TrimEnd('/'), "/", pathSegment.TrimStart('/'));
		}
#pragma warning restore CA1055 // Uri return values should not be strings

		protected async Task<string> GetJsonAsync(string path, Dictionary<string, object> parameters = null) {
			string returnValue = string.Empty;

			using (HttpClientHandler httpClientHandler = new HttpClientHandler()) {

				httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
				httpClientHandler.UseDefaultCredentials = true;

				using (HttpClient httpClient = new HttpClient(httpClientHandler)) {
					Dictionary<string, string> queryParameters = new Dictionary<string, string>();

					if (parameters != null) {
						foreach (KeyValuePair<string, object> parameter in parameters) {
							if (parameter.Value != null) {
								string parameterValue = null;

								if (parameter.Value.GetType().IsArray) {
									parameterValue = JsonConvert.SerializeObject(parameter.Value, Formatting.None);
								}
								else {
									parameterValue = Convert.ToString(parameter.Value, System.Globalization.CultureInfo.InvariantCulture);
								}

								queryParameters.Add(parameter.Key, parameterValue);
							}
						}
					}

					httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

					returnValue = await httpClient.GetStringAsync(new Uri(QueryHelpers.AddQueryString(GetWebServiceUrl(path), queryParameters))).ConfigureAwait(false);
				}
			}
			return returnValue;
		}

		public List<Affiliate> Affiliates {
			get {
				string cacheKey = "Affiliates";
				string cacheValue;

				bool inCache = false;
				lock (_memoryCache) {
					inCache = _memoryCache.TryGetValue(cacheKey, out cacheValue);
				}

				if (!inCache) {
					string baseUri;
					baseUri = GetConfigurationValue("WebServiceUrl");

					cacheValue = GetJsonAsync("common/lookups/getaffiliates").Result;

					lock (_memoryCache) {
						_memoryCache.Set(cacheKey, cacheValue);
					}
				}

				return JsonConvert.DeserializeObject<List<Affiliate>>(cacheValue);
			}
		}

		public List<ArticlePictureTemplate> ArticlePictureTemplates {
			get {
				string cacheKey = "ArticlePictureTemplates";
				string cacheValue;

				bool inCache = false;
				lock (_memoryCache) {
					inCache = _memoryCache.TryGetValue(cacheKey, out cacheValue);
				}

				if (!inCache) {
					string baseUri;
					baseUri = GetConfigurationValue("WebServiceUrl");

					cacheValue = GetJsonAsync("common/articles/getarticlepicturetemplates").Result;

					lock (_memoryCache) {
						_memoryCache.Set(cacheKey, cacheValue);
					}
				}

				return JsonConvert.DeserializeObject<List<ArticlePictureTemplate>>(cacheValue);
			}
		}

		public Dictionary<int, string> ShopProductUrls {
			get {
				string cacheKey = "ShopProductUrls";
				string cacheValue;

				bool inCache = false;
				lock (_memoryCache) {
					inCache = _memoryCache.TryGetValue(cacheKey, out cacheValue);
				}

				if (!inCache) {
					cacheValue = GetConfigurationValue("ShopProductUrls");

					lock (_memoryCache) {
						_memoryCache.Set(cacheKey, cacheValue);
					}
				}

				int n = 0;
				Dictionary<int, string> returnValue = cacheValue.Split(";", StringSplitOptions.None).ToDictionary(x => n += 1, y => y);

				return returnValue;
			}
		}
		public Dictionary<int, string> ShopProductPictureUrls {
			get {
				string cacheKey = "ShopProductPictureUrls";
				string cacheValue;

				bool inCache = false;
				lock (_memoryCache) {
					inCache = _memoryCache.TryGetValue(cacheKey, out cacheValue);
				}

				if (!inCache) {

					cacheValue = GetConfigurationValue("ShopProductPictureUrls");

					lock (_memoryCache) {
						_memoryCache.Set(cacheKey, cacheValue);
					}
				}

				int n = 0;
				Dictionary<int, string> returnValue = cacheValue.Split(";", StringSplitOptions.None).ToDictionary(x => n += 1, y => y);

				return returnValue;
			}
		}

		public List<PaymentMatrixRow> PaymentMatrix {
			get {
				string cacheKey = "PaymentMatrix";
				string cacheValue;

				bool inCache = false;
				lock (_memoryCache) {
					inCache = _memoryCache.TryGetValue(cacheKey, out cacheValue);
				}

				if (!inCache) {
					string baseUri;
					baseUri = GetConfigurationValue("WebServiceUrl");

					cacheValue = GetJsonAsync("common/lookups/getpaymentmatrix").Result;

					lock (_memoryCache) {
						_memoryCache.Set(cacheKey, cacheValue);
					}
				}

				return JsonConvert.DeserializeObject<List<PaymentMatrixRow>>(cacheValue);
			}
		}

		public List<CustomerGroupResult> CustomerGroups {
			get {
				string cacheKey = "CustomerGroups";
				string cacheValue;

				bool inCache = false;
				lock (_memoryCache) {
					inCache = _memoryCache.TryGetValue(cacheKey, out cacheValue);
				}

				if (!inCache) {
					string baseUri;
					baseUri = GetConfigurationValue("WebServiceUrl");

					cacheValue = GetJsonAsync("/common/customers/getcustomergroups").Result;

					lock (_memoryCache) {
						_memoryCache.Set(cacheKey, cacheValue);
					}
				}

				return JsonConvert.DeserializeObject<List<CustomerGroupResult>>(cacheValue);
			}
		}

		public Dictionary<int, int> PznAvailabilityTypeOverride {
			get {
				string cacheKey = "PznAvailabilityTypeOverride";
				string cacheValue;

				bool inCache = false;
				lock (_memoryCache) {
					inCache = _memoryCache.TryGetValue(cacheKey, out cacheValue);
				}

				if (!inCache) {
					string baseUri;
					baseUri = GetConfigurationValue("WebServiceUrl");

					cacheValue = GetJsonAsync("/webapplication/servicecenter/getpznavailabilitytypeoverrides").Result;

					lock (_memoryCache) {
						_memoryCache.Set(cacheKey, cacheValue);
					}
				}

				return JsonConvert.DeserializeObject<Dictionary<int, int>>(cacheValue);
			}
		}
	}
}
