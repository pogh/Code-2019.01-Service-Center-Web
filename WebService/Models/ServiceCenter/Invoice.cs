using System;
using System.Collections.Generic;

namespace Company.WebService.Models.ServiceCenter
{
    public partial class Invoice
    {
        public Invoice()
        {
            InvoiceItem = new HashSet<InvoiceItem>();
        }

        public int InvoicePk { get; set; }
        public int AffiliateId { get; set; }
        public int CustomerId { get; set; }
        public int InvoiceId { get; set; }
        public int CustomerGroupId { get; set; }
        public decimal CustomerGroupDiscount { get; set; }
        public int? DeliveryTypeId { get; set; }
        public int? PaymentTypeId { get; set; }
        public string Iban { get; set; }
        public string Bic { get; set; }
        public string AccountOwner { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? ChangedUtc { get; set; }
        public string ChangedUserName { get; set; }

        public virtual InvoiceBillingAddress InvoiceBillingAddress { get; set; }
        public virtual InvoiceDeliveryAddress InvoiceDeliveryAddress { get; set; }
        public virtual ICollection<InvoiceItem> InvoiceItem { get; set; }
    }
}
