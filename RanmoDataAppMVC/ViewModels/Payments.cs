using RanmoDB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RanmoDataAppMVC.ViewModels
{
    public class Payments
    {
        public int Id { get; set; }
        public string ReceiptNo { get; set; }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNumber { get; set; }

        [RegularExpression(@"^\d+.\d{0,2}", ErrorMessage = "Amount Should be a valid number with 2 decimals. ex: 2345.50 ")]
        public Nullable<decimal> Amount { get; set; }

        public Nullable<int> PaidById { get; set; }
        public string PaidBy { get; set; }
        public string PaidBySelected { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public Nullable<System.DateTime> ReceivedDate { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public Nullable<System.DateTime> ChequeDate { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public Nullable<System.DateTime> PaidDate { get; set; }

        public Nullable<int> PaymentStatusId { get; set; }
        public string PaymentStatus { get; set; }

        public string PaymentStatusSelected { get; set; }

        public string ChequeNum { get; set; }

        public bool PayAnyUnPaid { get; set; }

        [RegularExpression(@"^\d+.\d{0,2}", ErrorMessage = "Amount Should be a valid number with 2 decimals. ex: 2345.50 ")]
        public Nullable<decimal> AmountRemaining { get; set; }

        public string Notes { get; set; }
        public Nullable<System.DateTime> TimeStamp { get; set; }

        public SelectList PaymentStatusList { get; set; }
        public SelectList PaidByList { get; set; }


        public R_Payments ConvertVwModelToDB(Payments payments)
        {
            var paymentEF = new R_Payments();
            paymentEF.Id = payments.Id;
            paymentEF.ReceiptNo = payments.ReceiptNo;
            paymentEF.CustomerId = payments.CustomerId;
            paymentEF.Amount = payments.Amount;
            paymentEF.PaidById = payments.PaidById;
            paymentEF.PaymentStatusId = payments.PaymentStatusId;
            paymentEF.ReceivedDate = payments.ReceivedDate;
            paymentEF.ChequeDate = payments.ChequeDate;
            paymentEF.ChequeNum = payments.ChequeNum;
            paymentEF.PaidDate = payments.PaidDate;
            paymentEF.Notes = payments.Notes;
            return paymentEF;
        }

        public Payments ConvertDBToVwModel(int? Id)
        {
            var dbEF = new RanSanDBEntities();
            var paymentVM = dbEF.R_Payments
            .Where(p => p.Id == Id)
            .FirstOrDefault();

            var paymentEF = new Payments
            {
                Id = paymentVM.Id,
                ReceiptNo = paymentVM.ReceiptNo,
                CustomerId = paymentVM.CustomerId,
                CustomerName = dbEF.R_Customer.Where(c => c.Id == paymentVM.CustomerId).Select(n => n.CustomerName).FirstOrDefault(),
                Amount = paymentVM.Amount,
                PaidById = paymentVM.PaidById,
                PaidBy = dbEF.R_PaidBy.Where(c => c.Id == paymentVM.PaidById).Select(n => n.PaidBy).FirstOrDefault(),
                PaymentStatusId = paymentVM.PaymentStatusId,
                PaymentStatus = dbEF.R_PaymentStatus.Where(c => c.Id == paymentVM.PaymentStatusId).Select(n => n.PaymentStatus).FirstOrDefault(),
                ReceivedDate = paymentVM.ReceivedDate,
                ChequeDate = paymentVM.ChequeDate,
                ChequeNum = paymentVM.ChequeNum,
                PaidDate = paymentVM.PaidDate,
                Notes = paymentVM.Notes
            };

            paymentEF.PaidByList = new SelectList(dbEF.R_PaidBy);
            paymentEF.PaymentStatusList = new SelectList(dbEF.R_PaymentStatus);

            return paymentEF;
        }

        public R_Payments ConvertVwModelToDB(int? Id)
        {
            var dbEF = new RanSanDBEntities();
            var invoiceEF = dbEF.R_Payments
                                .Where(p => p.Id == Id)
                                .FirstOrDefault();

            return invoiceEF;
        }



    }

    public class InvoiceDetails
    {
        public string InvoiceNumber { get; set; }

        public int? CustomerId { get; set; }

        public string CustomerName { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public Nullable<DateTime> InvoiceDate { get; set; }

        public Nullable<decimal> InvoiceAmount { get; set; }

        public Nullable<decimal> TotalAlreadyPaid { get; set; }

    }


    public class CustomerDetails
    {

        public Nullable<decimal> TotalPendingAmount { get; set; }

        public Nullable<decimal> TotalAlreadyPaid { get; set; }

    }
}