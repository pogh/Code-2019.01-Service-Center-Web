using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class ArtikelstammSuche
    {
        public ArtikelstammSuche()
        {
            ArtikelstammSucheBegriff = new HashSet<ArtikelstammSucheBegriff>();
        }

        public long Id { get; set; }
        public int Artikelnummer { get; set; }
        public short SpracheId { get; set; }
        public string Schluessel { get; set; }
        public string Name { get; set; }
        public string AnbieterFirma { get; set; }
        public string HerstellerFirma { get; set; }
        public string Mengenangabe { get; set; }
        public string Einheit { get; set; }
        public string Darreichung { get; set; }
        public string Warengruppe { get; set; }

        public virtual Artikelstamm ArtikelnummerNavigation { get; set; }
        public virtual ICollection<ArtikelstammSucheBegriff> ArtikelstammSucheBegriff { get; set; }
    }
}
