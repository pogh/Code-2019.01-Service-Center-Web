using System;
using System.Collections.Generic;

namespace Company.WebService.Models.ServiceCenter
{
    public partial class CustomerWarning
    {
        public int CustomerWarningPk { get; set; }
        public int KundenNr { get; set; }
        public string WarningType { get; set; }
    }
}
