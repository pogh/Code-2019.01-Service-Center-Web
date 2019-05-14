using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class Kunden
    {
        public decimal PkkundenId { get; set; }
        public string KundenNr { get; set; }
        public decimal? FkcusId { get; set; }
        public string CusPasswd { get; set; }
        public string CusNewpasswd { get; set; }
        public string CusBirth { get; set; }
        public string CusEbaynick { get; set; }
        public string FkcusGenId { get; set; }
        public string CusFirstname { get; set; }
        public string CusSurname { get; set; }
        public string CusFirm { get; set; }
        public string CusStreet { get; set; }
        public string CusZip { get; set; }
        public string CusCity { get; set; }
        public decimal? FkcusCucId { get; set; }
        public string CusFon { get; set; }
        public string CusMobil { get; set; }
        public string CusFax { get; set; }
        public string CusMail { get; set; }
        public decimal? FkcusCstId { get; set; }
        public decimal? CusIsWholesaler { get; set; }
        public decimal? CusIsAdmin { get; set; }
        public string CusSince { get; set; }
        public string CusTmp { get; set; }
        public decimal? CusTmpLoginstamp { get; set; }
        public decimal? CusBonusAccount { get; set; }
        public decimal? CusMAINDBDollar { get; set; }
        public string CusPayback { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public decimal? KundenGruppe { get; set; }
        public bool Geschäftskunde { get; set; }
        public bool Affiliate { get; set; }
        public int AffiliatePartnerId { get; set; }
        public string AffiliateKundenId { get; set; }
        public bool MwStbefreit { get; set; }
        public bool NettoRechnung { get; set; }
        public decimal Grundrabatt { get; set; }
        public int OutboundCallId { get; set; }
        public int? PtkundenId { get; set; }
        public bool Gesperrt { get; set; }
        public bool Auslandsversand { get; set; }
    }
}
