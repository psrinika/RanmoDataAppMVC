//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RanmoDB
{
    using System;
    using System.Collections.Generic;
    
    public partial class R_InvoicePayment
    {
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public System.DateTime TimeStamp { get; set; }
        public Nullable<decimal> AmountPaid { get; set; }
        public bool PaymentFullyUsed { get; set; }
        public bool InvoiceFullyPaid { get; set; }
    }
}
