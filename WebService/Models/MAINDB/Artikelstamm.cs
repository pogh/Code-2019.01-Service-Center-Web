using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class Artikelstamm
    {
        public Artikelstamm()
        {
            AffiliateArtikelstamm = new HashSet<AffiliateArtikelstamm>();
            ArtikelstammAttributisierung = new HashSet<ArtikelstammAttributisierung>();
            ArtikelstammName = new HashSet<ArtikelstammName>();
            ArtikelstammSuche = new HashSet<ArtikelstammSuche>();
        }

        public int Artikelnummer { get; set; }
        public byte GruppeId { get; set; }
        public byte TypId { get; set; }
        public int AnbieterFirmaId { get; set; }
        public int? HerstellerFirmaId { get; set; }
        public int? WarengruppeId { get; set; }
        public int? DarreichungId { get; set; }
        public bool Virtuell { get; set; }
        public bool? Aktiv { get; set; }
        public bool Bundle { get; set; }

        public virtual ArtikelstammFirma AnbieterFirma { get; set; }
        public virtual ArtikelstammFirma HerstellerFirma { get; set; }
        public virtual ArtikelstammIdentifikation ArtikelstammIdentifikation { get; set; }
        public virtual ArtikelstammMenge ArtikelstammMenge { get; set; }
        public virtual ICollection<AffiliateArtikelstamm> AffiliateArtikelstamm { get; set; }
        public virtual ICollection<ArtikelstammAttributisierung> ArtikelstammAttributisierung { get; set; }
        public virtual ICollection<ArtikelstammName> ArtikelstammName { get; set; }
        public virtual ICollection<ArtikelstammSuche> ArtikelstammSuche { get; set; }
    }
}
