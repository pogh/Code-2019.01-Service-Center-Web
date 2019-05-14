using System;
using System.Collections.Generic;

namespace Company.WebService.Models.ServiceCenter
{
    public partial class DeliveryPaymentButton
    {
        public int DeliveryPaymentButtonPk { get; set; }
        public int AffiliateId { get; set; }
        public int Pzn { get; set; }
        public string DisplayText { get; set; }
        public decimal DisplayFromTotal { get; set; }
        public decimal ItemPrice { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime UntilDate { get; set; }
    }
}
