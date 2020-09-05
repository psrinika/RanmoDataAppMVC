using RanmoDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;

namespace RanmoDataAppMVC.ViewModels
{
    public class InvoicePayment
    {
        [Key]
        [Column(Order = 0)]
        public int PaymentId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int InvoiceId { get; set; }

        [DisplayName("Rcpt#")]
        public string ReceiptNo { get; set; }
        public int CustomerId { get; set; }

        [DisplayName("Customer")]
        public string CustomerName { get; set; }

        [DisplayName("Inv#")]
        public string InvoiceNumber { get; set; }

        [DisplayName("Payment")]
        public decimal PaymentAmount { get; set; }

        [DisplayName("Invoice")]
        public decimal InvoiceAmount { get; set; }

        [DisplayName("InvPay")]
        public decimal InvoicePaymentAmount { get; set; }

        [DisplayName("Remain Pay")]
        public decimal RemainingPaymentAmount { get; set; }

        [DisplayName("Remain Inv")]
        public decimal RemainingInvoiceAmount { get; set; }

        public bool PaymentFullyUsed { get; set; }
        public bool InvoiceFullyPaid { get; set; }
        public System.DateTime TimeStamp { get; set; }


        public R_InvoicePayment ConvertVwModelToDB(InvoicePayment invoicePayment)
        {
            var paymentEF = new R_InvoicePayment();
            paymentEF.PaymentId = invoicePayment.PaymentId;
            paymentEF.InvoiceId = invoicePayment.InvoiceId;
            paymentEF.InvoicePaymentAmount = invoicePayment.InvoicePaymentAmount;

            paymentEF.RemainingPaymentAmount = invoicePayment.RemainingPaymentAmount - invoicePayment.InvoicePaymentAmount;
            paymentEF.RemainingInvoiceAmount = invoicePayment.RemainingInvoiceAmount - invoicePayment.InvoicePaymentAmount;
            paymentEF.PaymentFullyUsed = (RemainingPaymentAmount == 0);
            paymentEF.InvoiceFullyPaid = (RemainingInvoiceAmount == 0);

            return paymentEF;
        }

        public InvoicePayment ConvertDBToVwModel(int paymentId, int invoiceId)
        {
            var dbEF = new RanSanDBEntities();

            var invoicePayment = dbEF.R_InvoicePayment
                    .Where(p => p.PaymentId == paymentId && p.InvoiceId == invoiceId)
                    .FirstOrDefault();

            var invoice = dbEF.R_Invoice
                    .Where(p => p.Id == invoiceId)
                    .FirstOrDefault();

            var payment = dbEF.R_Payments
                    .Where(p => p.Id == paymentId)
                    .FirstOrDefault();

            var invoicePaymentEF = new InvoicePayment
            {
                PaymentId = paymentId,
                InvoiceId = invoiceId,
                ReceiptNo = payment.ReceiptNo,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = payment.CustomerId,
                CustomerName = dbEF.R_Customer.Where(c => c.Id == payment.CustomerId).Select(n => n.CustomerName).FirstOrDefault(),

                InvoiceAmount = (invoice.Amount == null) ? 0 : (decimal)invoice.Amount,
                PaymentAmount = (payment.Amount == null) ? 0 : (decimal)payment.Amount,

                //InvoicePaymentAmount = invoicePayment.InvoicePaymentAmount,
                //RemainingPaymentAmount = invoicePayment.RemainingPaymentAmount, // or find it by calc ?
                //RemainingInvoiceAmount = invoicePayment.RemainingInvoiceAmount, // or find it by calc ?
                //PaymentFullyUsed = invoicePayment.PaymentFullyUsed, 
                //InvoiceFullyPaid = invoicePayment.InvoiceFullyPaid

                InvoicePaymentAmount = (invoicePayment == null) ? 0 : invoicePayment.InvoicePaymentAmount,
                RemainingPaymentAmount = (invoicePayment == null) ? 0 : invoicePayment.RemainingPaymentAmount,
                RemainingInvoiceAmount = (invoicePayment == null) ? 0 : invoicePayment.RemainingInvoiceAmount,
                PaymentFullyUsed = (invoicePayment == null) ? false : invoicePayment.PaymentFullyUsed,
                InvoiceFullyPaid = (invoicePayment == null) ? false : invoicePayment.InvoiceFullyPaid

        };


            return invoicePaymentEF;
        }
    }
}