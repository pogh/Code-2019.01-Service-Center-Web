using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class ArtikelstammName
    {
        public long Id { get; set; }
        public int Artikelnummer { get; set; }
        public short SpracheId { get; set; }
        public string Artikelname { get; set; }
        public string Kurzname { get; set; }
        public string Sortiername { get; set; }
        public string Anzeigename { get; set; }
        public long? Autor { get; set; }
        public DateTime? AutorModifyDate { get; set; }
        public string AutorModifyBy { get; set; }

        public virtual Artikelstamm ArtikelnummerNavigation { get; set; }
    }
}
