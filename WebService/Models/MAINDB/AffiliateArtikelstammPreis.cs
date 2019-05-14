using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class AffiliateArtikelstammPreis
    {
        public long ArtikelstammId { get; set; }
        public byte WaehrungId { get; set; }
        public decimal Verkaufspreis { get; set; }
        public decimal? Preis { get; set; }
        public byte MwstId { get; set; }
        public decimal? Preisempfehlung { get; set; }
        public bool Gesperrt { get; set; }
        public int? SperrgrundId { get; set; }
        public DateTime Datum { get; set; }
        public bool? Rabattfaehig { get; set; }
        public int? ManuellerRabatt { get; set; }
        public bool Überwachung { get; set; }
        public decimal? Kanalverkaufspreis1 { get; set; }

        public virtual AffiliateArtikelstamm Artikelstamm { get; set; }
    }
}
