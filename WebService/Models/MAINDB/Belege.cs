using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class Belege
    {
        public decimal PkbelegId { get; set; }
        public decimal FkbelegTyp { get; set; }
        public string BelegNr { get; set; }
        public decimal? PkordId { get; set; }
        public string OrdOrderId { get; set; }
        public decimal? FkordCusId { get; set; }
        public decimal? FkordCudId { get; set; }
        public decimal? FkordDvtId { get; set; }
        public DateTime? OrdTimestamp { get; set; }
        public DateTime? BelegDatum { get; set; }
        public decimal? FkordOstId { get; set; }
        public string OrdTrackingNr { get; set; }
        public decimal? OrdExported { get; set; }
        public DateTime? OrdExportedAt { get; set; }
        public bool? Versandkostenfrei { get; set; }
        public decimal? OrdAccountKtonr { get; set; }
        public decimal? OrdAccountBlz { get; set; }
        public string OrdAccountOwner { get; set; }
        public int? Ausgebucht { get; set; }
        public decimal? Gesendet { get; set; }
        public decimal? IaChecked { get; set; }
        public decimal? StatusIntern { get; set; }
        public decimal? StatusExtern { get; set; }
        public decimal? GhSend { get; set; }
        public decimal? EkWert { get; set; }
        public decimal? VkWert { get; set; }
        public decimal? Db1Wert { get; set; }
        public decimal LexwareExport { get; set; }
        public decimal Express { get; set; }
        public decimal? SendGhWanne { get; set; }
        public decimal? SendLagerWanne { get; set; }
        public decimal? Gesperrt { get; set; }
        public decimal Lieferart { get; set; }
        public bool Eigenhändig { get; set; }
        public bool SchufaOk { get; set; }
        public long? ShopPk { get; set; }
        public bool Affiliate { get; set; }
        public int AffiliatePartnerId { get; set; }
        public string AffiliateBestellId { get; set; }
        public bool BetrügerOk { get; set; }
        public bool? ApothekerInfo { get; set; }
        public bool? CallcenterInfo { get; set; }
        public bool BeratungAngeboten { get; set; }
        public bool MehrfachbestellungOk { get; set; }
        public bool DoppelbestellungOk { get; set; }
        public int BxTimeslotId { get; set; }
        public string BxMobilenumber { get; set; }
        public string Iban { get; set; }
        public string Bic { get; set; }
        public string Mandatsreferenz { get; set; }
        public DateTime? Mandatdatum { get; set; }
        public bool Zeitung { get; set; }
        public int ChannelAdvisorId { get; set; }
        public DateTime? Wunschtag { get; set; }
        public DateTime? Abendtermin { get; set; }
        public bool MwStfrei { get; set; }
        public bool Geschäftskunde { get; set; }
        public bool NettoRechnung { get; set; }
        public decimal Grundrabatt { get; set; }
        public DateTime? HangoverDate { get; set; }
        public bool AmazonFulfilled { get; set; }
        public bool NavÜbergabe { get; set; }
        public string Absatzkanal { get; set; }
        public bool Nachlieferung { get; set; }
        public int? Laenderschluessel { get; set; }
        public bool Pillbox { get; set; }
        public string PsmId { get; set; }
        public bool Neukunde { get; set; }
        public bool DhlSendungsverfolgung { get; set; }
        public bool OptPrio { get; set; }
        public bool DhlexpressSamstag { get; set; }
    }
}
