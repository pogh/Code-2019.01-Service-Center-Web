using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class ArtikelstammEinheit
    {
        public ArtikelstammEinheit()
        {
            ArtikelstammMenge = new HashSet<ArtikelstammMenge>();
        }

        public byte Id { get; set; }
        public string Schluessel { get; set; }
        public byte Gruppierung { get; set; }
        public bool Bezug { get; set; }
        public double Faktor { get; set; }

        public virtual ICollection<ArtikelstammMenge> ArtikelstammMenge { get; set; }
    }
}
