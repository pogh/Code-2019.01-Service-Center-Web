using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class SchrammOsaffiliateMapping
    {
        public int Id { get; set; }
        public int AffiliateAlt { get; set; }
        public int AffiliateNeu { get; set; }
        public string Kostenstelle { get; set; }
        public string AffiliateName { get; set; }
        public string StdDruckdaten { get; set; }
        public string Schlusssatz1 { get; set; }
        public string Schlusssatz2 { get; set; }
        public string Widerruf { get; set; }
        public string Zollerklärung { get; set; }
        public string Dhlteilnahme { get; set; }
        public string EmailFooter { get; set; }
        public string WiderrufPdf { get; set; }
    }
}
