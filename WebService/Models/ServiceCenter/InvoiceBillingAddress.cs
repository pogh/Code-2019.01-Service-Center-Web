using System;
using System.Collections.Generic;

namespace Company.WebService.Models.ServiceCenter
{
    public partial class InvoiceBillingAddress
    {
        public int InvoiceBillingAddressPk { get; set; }
        public int InvoiceFk { get; set; }
        public string CustomerNumber { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string AdditionalLine { get; set; }
        public string EmailAddress { get; set; }
        public string TelephoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public string FaxNumber { get; set; }
        public DateTime? DoB { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? ChangedUtc { get; set; }
        public string ChangedUserName { get; set; }

        public virtual Invoice InvoiceFkNavigation { get; set; }
    }
}
