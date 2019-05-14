using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class ArtikelstammSucheBegriff
    {
        public long Id { get; set; }
        public long SucheId { get; set; }
        public byte BegrifftypId { get; set; }
        public string Wert { get; set; }
        public byte Rang { get; set; }

        public virtual ArtikelstammSuche Suche { get; set; }
    }
}
