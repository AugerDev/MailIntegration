using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MailboxIntegration.Models
{
    public class MailListDetail
    {
        public List<MailListDetailItems> Items { get; set; }
    }
    public class MailListDetailItems
    {
        public string Display { get; set; }
        public string Id { get; set; }
        public string EmailID { get; set; }
        public string Message { get; set; }
        //public List<Dictionary<string, object>> Properties { get; set; }
        public List<AttachmentProperties> Properties { get; set; }
    }

    public class AttachmentProperties
    {
        public string AttachmentName { get; set; }
        public string AttachmentType { get; set; }
        public int? AttachmentSize { get; set; }
        public string AttachmentId { get; set; }
    }
}