using System;
using System.Collections.Generic;

namespace Company.WebService.Models.ServiceCenter
{
    public partial class InvoiceItem
    {
        public int InvoiceItemPk { get; set; }
        public int InvoiceFk { get; set; }
        public int Pzn { get; set; }
        public int Quantity { get; set; }
        public decimal Vat { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal ItemSavings { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? ChangedUtc { get; set; }
        public string ChangedUserName { get; set; }

        public virtual Invoice InvoiceFkNavigation { get; set; }
    }
}
