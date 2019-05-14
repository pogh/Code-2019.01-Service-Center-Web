using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class AffiliateArtikelstammAlternativPreis
    {
        public int Id { get; set; }
        public long ArtikelstammId { get; set; }
        public int SuchanbieterId { get; set; }
        public decimal Verkaufspreis { get; set; }
        public int Mengenbegrenzung { get; set; }
        public DateTime Startdatum { get; set; }
        public DateTime Enddatum { get; set; }
        public DateTime Aenderungsdatum { get; set; }
        public string GriffinUser { get; set; }

        public virtual AffiliateArtikelstamm Artikelstamm { get; set; }
        public virtual ArtikelstammSuchanbieter Suchanbieter { get; set; }
    }
}
