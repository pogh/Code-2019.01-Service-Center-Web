using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class ArtikelstammIdentifikation
    {
        public int Artikelnummer { get; set; }
        public string Sku { get; set; }
        public string Gtin { get; set; }
        public string Isbn { get; set; }
        public int? Pzn { get; set; }
        public string Parameter { get; set; }
        public byte? KennzeichnungId { get; set; }
        public byte? LandId { get; set; }
        public string Lsin { get; set; }

        public virtual Artikelstamm ArtikelnummerNavigation { get; set; }
    }
}
