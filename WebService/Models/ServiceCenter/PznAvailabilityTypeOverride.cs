using System;
using System.Collections.Generic;

namespace Company.WebService.Models.ServiceCenter
{
    public partial class PznAvailabilityTypeOverride
    {
        public int PznAvailabilityTypeOverridePk { get; set; }
        public int Pzn { get; set; }
        public int AvailabilityType { get; set; }
    }
}
