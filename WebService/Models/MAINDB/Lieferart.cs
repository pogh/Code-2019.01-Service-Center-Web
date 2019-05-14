using System;
using System.Collections.Generic;

namespace Company.WebService.Models.MAINDB
{
    public partial class Lieferart
    {
        public decimal PklieferartId { get; set; }
        public string LieferartSprachwert { get; set; }
        public bool? Aktiv { get; set; }
        public decimal Versandkostengrenze { get; set; }
        public int Prioritaet { get; set; }
        public TimeSpan EndUhrzeit { get; set; }
        public string Länderkürzel { get; set; }
        public string Land { get; set; }
        public string LandEasylog { get; set; }
        public bool MwstFrei { get; set; }
        public decimal OrderPrioFaktor { get; set; }
        public int DefaultPrioritaet { get; set; }
    }
}
