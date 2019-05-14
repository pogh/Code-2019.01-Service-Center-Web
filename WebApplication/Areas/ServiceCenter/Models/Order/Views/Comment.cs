using System.Collections.Generic;
using System.Linq;
using Company.BusinessObjects.Common;

namespace Company.WebApplication.Areas.ServiceCenter.Models.Order.Views {
	public class Comment : OrderBase {

		public Comment(Affiliate affiliate, int customerId) : base(affiliate, customerId) {
		}

		public string CommentText { get; set; }
	}
}
