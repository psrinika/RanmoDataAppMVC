using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RanmoDataAppMVC.ViewModels
{
    public class InvoicePayment
    {
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public System.DateTime TimeStamp { get; set; }
    }
}