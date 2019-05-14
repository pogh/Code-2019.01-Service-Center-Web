using System;
using System.ComponentModel.DataAnnotations;

namespace Company.BusinessObjects.Common
{
    public class Affiliate
    {
		[Key]
		public int AffiliateId {get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
		public string RGB { get; set; }

		public override string ToString()
        {
            return string.Concat(this.AffiliateId, " ", this.Name, " (", this.Key, ")");
        }
    }
}
