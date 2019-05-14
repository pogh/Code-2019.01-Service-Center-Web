using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class ArtikelstammAttributisierung
    {
        public long Id { get; set; }
        public int AttributId { get; set; }
        public int Artikelnummer { get; set; }
        public object Wert { get; set; }

        public virtual Artikelstamm ArtikelnummerNavigation { get; set; }
    }
}
