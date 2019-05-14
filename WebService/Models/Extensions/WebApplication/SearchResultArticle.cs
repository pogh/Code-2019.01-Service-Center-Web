using System;
using System.ComponentModel.DataAnnotations;
using Company.BusinessObjects.Common;

namespace Company.WebService.Models.Extensions.WebApplication {
	public class ArticleSearchResult {

		[Key]
		public int PZN { get; set; }

		public int IntegerField { get; set; }

		public DateTime DateTimeField { get; set; }

	}

	public class ArticleSearchReturn : Article {

		public ArticleSearchReturn(int integerField, DateTime dateTimeField, Article article) : base() {
			IntegerField = integerField;
			DateTimeField = dateTimeField;

			foreach (var articleProperty in article.GetType().GetProperties()) {
				var thisProperty = this.GetType().GetProperty(articleProperty.Name);
				if (thisProperty.CanWrite)
					thisProperty.SetValue(this, articleProperty.GetValue(article));
			}
		}

		public int IntegerField { get; set; }

		public DateTime DateTimeField { get; set; }

	}
}
