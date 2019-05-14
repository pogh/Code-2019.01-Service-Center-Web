using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class AffiliateArtikelstammVerfuegbarkeit
    {
        public long ArtikelstammId { get; set; }
        public byte Verfuegbarkeit { get; set; }
        public int Lieferdauer { get; set; }
        public bool Online { get; set; }
        public bool Gesperrt { get; set; }
        public int? SperrgrundId { get; set; }
        public DateTime Datum { get; set; }
        public int Mengenbegrenzung { get; set; }
        public int MengenbegrenzungVerkauf { get; set; }

        public virtual AffiliateArtikelstamm Artikelstamm { get; set; }
    }
}
