using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class AffiliateArtikelstamm
    {
        public AffiliateArtikelstamm()
        {
            AffiliateArtikelstammAlternativPreis = new HashSet<AffiliateArtikelstammAlternativPreis>();
        }

        public long Id { get; set; }
        public int AffiliateId { get; set; }
        public int Artikelnummer { get; set; }
        public bool? Suchbar { get; set; }
        public int Rang { get; set; }
        public bool? Gelistet { get; set; }
        public byte GruppeId { get; set; }
        public byte TypId { get; set; }

        public virtual Artikelstamm ArtikelnummerNavigation { get; set; }
        public virtual AffiliateArtikelstammPreis AffiliateArtikelstammPreis { get; set; }
        public virtual AffiliateArtikelstammVerfuegbarkeit AffiliateArtikelstammVerfuegbarkeit { get; set; }
        public virtual ICollection<AffiliateArtikelstammAlternativPreis> AffiliateArtikelstammAlternativPreis { get; set; }
    }
}
