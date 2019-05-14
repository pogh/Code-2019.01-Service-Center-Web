using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.BusinessObjects.Common {

	/// <summary>
	/// Benutzt die Bilder aus den Websites zu klauen
	/// </summary>
	public class ArticlePictureTemplate {
		public int Id { get; set; }
		public string Key { get; set; }
		public int AffiliateId { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public string Filter { get; set; }
		public decimal Blur { get; set; }
		public bool BestFit { get; set; }
		public int CompressionQuality { get; set; }
		public string SourceBucket { get; set; }
		public string TargetBucket { get; set; }
		public string TargetFile { get; set; }

		public override string ToString() {
			return string.Concat(Id, " ", Key, " (", AffiliateId, ") ", Width , "×", Height, " ", TargetFile);
		}
	}
}
