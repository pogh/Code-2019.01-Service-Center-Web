using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class Liefertabelle
    {
        public decimal PkliefertabelleId { get; set; }
        public decimal PkcudId { get; set; }
        public decimal? PkbelegId { get; set; }
        public string Anrede { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Strasse { get; set; }
        public string Plz { get; set; }
        public string Ort { get; set; }
        public bool Gesperrt { get; set; }
        public string Sperrbeschreibung { get; set; }
        public string Länderkürzel { get; set; }
    }
}
