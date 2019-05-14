using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class AffiliateArtikelstammBildvorgabe
    {
        public int Id { get; set; }
        public string Schluessel { get; set; }
        public int AffiliateId { get; set; }
        public byte Rang { get; set; }
        public string Parameter { get; set; }
    }
}
