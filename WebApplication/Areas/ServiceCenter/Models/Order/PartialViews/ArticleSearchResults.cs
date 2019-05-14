using System;
using System.Collections.Generic;
using System.Text;
using Company.BusinessObjects.Common;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.PartialViews {
	public class ArticleSearchResults {

		public ArticleSearchResults() {
			Articles = new List<ArticleSearchResultArticle>();
		}

		public List<ArticleSearchResultArticle> Articles { get; }
		public bool NoResults { set; get; }
		public bool ToManyResults { set; get; }
	}

	public class ArticleSearchResultArticle : Article {

		public string BaseUrl {
			get {
				StringBuilder returnValue = new StringBuilder(Name.ToLowerInvariant());

				returnValue.Replace("?", "");

				returnValue.Replace("'", "");
				returnValue.Replace("\"", "");
				returnValue.Replace(")", "");
				returnValue.Replace("(", "");
				returnValue.Replace("%", "");
				returnValue.Replace("!", "");
				returnValue.Replace("`", "");
				returnValue.Replace("´", "");

				returnValue.Replace("ú", "u");
				returnValue.Replace("à", "a");
				returnValue.Replace("é", "e");
				returnValue.Replace("á", "a");
				returnValue.Replace("µ", "u");
				returnValue.Replace("=", "-");
				returnValue.Replace("ä", "ae");
				returnValue.Replace("á", "a");
				returnValue.Replace("°", "grad");
				returnValue.Replace("ö", "oe");
				returnValue.Replace(":", "-");
				returnValue.Replace("ü", "ue");
				returnValue.Replace("ß", "ss");
				returnValue.Replace("&uuml;", "ue");
				returnValue.Replace("&ouml;", "oe");
				returnValue.Replace("&auml;", "ae");
				returnValue.Replace("&amp", "und");
				returnValue.Replace("&", "und");
				returnValue.Replace("+", "plus");
				returnValue.Replace("/", "-");
				returnValue.Replace(" ", "-");
				returnValue.Replace(".", "-");
				returnValue.Replace(",", "-");
				returnValue.Replace("---", "-");
				returnValue.Replace("--", "-");

				return returnValue.ToString();
			}
		}


		public string WebsiteUrl { get; set; }
		public string PictureUrl { get; set; }
		public int IntegerField { get; set; }

		public DateTime DateTimeField { get; set; }

	}
}
