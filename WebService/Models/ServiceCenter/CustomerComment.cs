using System;
using System.Collections.Generic;

namespace Company.WebService.Models.ServiceCenter
{
    public partial class CustomerComment
    {
        public int CustomerCommentPk { get; set; }
        public int CustomerId { get; set; }
        public string CommentText { get; set; }
        public bool CommentSent { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? ChangedUtc { get; set; }
        public string ChangedUserName { get; set; }

        public virtual CustomerMapping Customer { get; set; }
    }
}
