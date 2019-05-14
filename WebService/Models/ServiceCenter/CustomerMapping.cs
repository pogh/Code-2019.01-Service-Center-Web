using System;
using System.Collections.Generic;

namespace Company.WebService.Models.ServiceCenter
{
    public partial class CustomerMapping
    {
        public CustomerMapping()
        {
            CustomerComment = new HashSet<CustomerComment>();
        }

        public int CustomerId { get; set; }
        public int KundenNr { get; set; }

        public virtual ICollection<CustomerComment> CustomerComment { get; set; }
    }
}
