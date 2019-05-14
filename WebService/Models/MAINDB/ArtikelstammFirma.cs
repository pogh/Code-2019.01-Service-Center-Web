using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class ArtikelstammFirma
    {
        public ArtikelstammFirma()
        {
            ArtikelstammAnbieterFirma = new HashSet<Artikelstamm>();
            ArtikelstammHerstellerFirma = new HashSet<Artikelstamm>();
        }

        public int Id { get; set; }
        public string Schluessel { get; set; }
        public string Name { get; set; }
        public string Alternativname { get; set; }
        public string Sortiername { get; set; }
        public string Strasse { get; set; }
        public string Hausnummer { get; set; }
        public string Plz { get; set; }
        public string Ort { get; set; }
        public bool? Hersteller { get; set; }
        public bool? Anbieter { get; set; }
        public bool Virtuell { get; set; }
        public string Logo { get; set; }

        public virtual ICollection<Artikelstamm> ArtikelstammAnbieterFirma { get; set; }
        public virtual ICollection<Artikelstamm> ArtikelstammHerstellerFirma { get; set; }
    }
}
