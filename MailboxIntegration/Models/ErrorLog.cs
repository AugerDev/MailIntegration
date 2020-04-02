using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MailboxIntegration.Models
{
    public class ErrorLog
    {
        public const string ErrorKey = "TempDataErrorMessage";
        public string Message { get; set; }
        public string Debug { get; set; }
    }
}