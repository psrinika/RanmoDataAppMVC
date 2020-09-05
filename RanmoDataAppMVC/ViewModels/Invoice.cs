using RanmoDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RanmoDataAppMVC.ViewModels
{
    public partial class Invoice
    {
        public int Id { get; set; }

        //[MaxLength(127)]
        //[Index(IsUnique = true)] (string invoiceNumber)
        //[Key]
        //[Remote("IsInvoiceNumberExist", "Invoices", ErrorMessage = "Invoice Number already exist.")]
        //[Index("IX_R_Invoice", IsUnique = true)]
        [Required]
        //[Index(IsUnique = true)]
        // 
        //https://www.c-sharpcorner.com/blogs/remote-validation-in-mvc-5-using-remote-attribute
        // none of the above didn't work
        public string InvoiceNumber { get; set; }

        public Nullable<int> CustomerId { get; set; }

        [DisplayName("Customer Name")]
        public string CustomerName { get; set; }

        [RegularExpression(@"^\d+.\d{0,2}", ErrorMessage = "Amount Should be a valid number with 2 decimals. ex: 2345.50 ")]
        public Nullable<decimal> Amount { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public bool FullyPaid { get; set; }

        [RegularExpression(@"^\d+.\d{0,2}", ErrorMessage = "Amount Should be a valid number with 2 decimals. ex: 2345.50 ")]
        public Nullable<decimal> AmountPaid { get; set; }

        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        public DateTime? TimeStamp { get; set; }

        public SelectList CustomersList { get; set; }


        public R_Invoice ConvertVwModelToDB(Invoice invoice)
        {
            var i = new R_Invoice();
            i.Id = invoice.Id;
            i.InvoiceNumber = invoice.InvoiceNumber;
            i.CustomerId = invoice.CustomerId;
            i.Amount = invoice.Amount;
            i.InvoiceDate = invoice.InvoiceDate;
            i.FullyPaid = invoice.FullyPaid;
            i.AmountPaid = invoice.AmountPaid;
            i.Notes = invoice.Notes;
            return i;
        }

        public Invoice ConvertDBToVwModel(int? Id)
        {
            var dbEF = new RanSanDBEntities();
            var invoiceVM = dbEF.R_Invoice
            .Where(p => p.Id == Id)
            .FirstOrDefault();

            var invData = new Invoice
            {
                Id = invoiceVM.Id,
                InvoiceNumber = invoiceVM.InvoiceNumber,
                CustomerId = invoiceVM.CustomerId,
                CustomerName = dbEF.R_Customer.Where(c => c.Id == invoiceVM.CustomerId).Select(n => n.CustomerName).FirstOrDefault(),
                Amount = invoiceVM.Amount,
                InvoiceDate = invoiceVM.InvoiceDate,
                FullyPaid = invoiceVM.FullyPaid,
                AmountPaid = invoiceVM.AmountPaid,
                Notes = invoiceVM.Notes
            };

            invData.CustomersList = new SelectList(dbEF.R_Customer);
            return invData;
        }

        public R_Invoice ConvertVwModelToDB(int? Id)
        {
            var dbEF = new RanSanDBEntities();
            var invoiceEF = dbEF.R_Invoice
                                .Where(p => p.Id == Id)
                                .FirstOrDefault();

            return invoiceEF;
        }


    }



}