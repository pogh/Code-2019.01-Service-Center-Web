using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class ArtikelstammMenge
    {
        public int Artikelnummer { get; set; }
        public string Mengenangabe { get; set; }
        public double Menge { get; set; }
        public byte EinheitId { get; set; }

        public virtual Artikelstamm ArtikelnummerNavigation { get; set; }
        public virtual ArtikelstammEinheit Einheit { get; set; }
    }
}
