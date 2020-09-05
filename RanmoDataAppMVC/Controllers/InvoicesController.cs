using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using RanmoDataAppMVC.Models;
using RanmoDataAppMVC.ViewModels;
using RanmoDB;

namespace RanmoDataAppMVC.Controllers
{
    public class InvoicesController : Controller
    {
        private ApplicationDbContext dbMV = new ApplicationDbContext();
        private RanSanDBEntities dbEF = new RanSanDBEntities();

        public ActionResult Index0()
        {
            var dbData = dbEF.R_Invoice.Select(q => new Invoice
            {
                Id = q.Id,
                InvoiceNumber = q.InvoiceNumber,
                CustomerId = q.CustomerId,
                CustomerName = dbEF.R_Customer.Where(c => c.Id == q.CustomerId).Select(n => n.CustomerName).FirstOrDefault(),
                Amount = q.Amount,
                InvoiceDate = q.InvoiceDate,
                FullyPaid = q.FullyPaid,
                AmountPaid = q.AmountPaid,
                Notes = q.Notes
            }).OrderBy(q => q.InvoiceNumber).Take(10);


            return View(dbData);
        }


        public ActionResult Index(string sortOrder, int? page, int? noOfRecs)
        {
            ViewBag.InvoiceNumberSortParm = String.IsNullOrEmpty(sortOrder) ? "InvoiceNumber_desc" : "";
            ViewBag.InvoiceDateSortParm = sortOrder == "InvoiceDate" ? "InvoiceDate_desc" : "InvoiceDate";
            int numberOfRecs = (noOfRecs == null || noOfRecs == 0 || noOfRecs > 1000) ? 10 : (int)noOfRecs;
            var xx = ViewBag.NumberOfRecords;

            var dbData = dbEF.R_Invoice.Where(q => q.FullyPaid == false).Select(q => new Invoice
            {
                Id = q.Id,
                InvoiceNumber = q.InvoiceNumber,
                CustomerId = q.CustomerId,
                CustomerName = dbEF.R_Customer.Where(c => c.Id == q.CustomerId).Select(n => n.CustomerName).FirstOrDefault(),
                Amount = q.Amount,
                InvoiceDate = q.InvoiceDate,
                FullyPaid = q.FullyPaid,
                AmountPaid = q.AmountPaid,
                Notes = q.Notes
            });

            var sortedData = dbData.OrderBy(q => q.InvoiceNumber);
            switch (sortOrder)
            {
                case "InvoiceNumber_desc":
                    sortedData = dbData.OrderByDescending(q => q.InvoiceNumber);
                    break;
                case "InvoiceDate":
                    sortedData = dbData.OrderBy(q => q.InvoiceDate);
                    break;
                case "InvoiceDate_desc":
                    sortedData = dbData.OrderByDescending(q => q.InvoiceDate);
                    break;
                default:
                    break;
            }
            //            var limiteddData = sortedData.Take(10);

            //https://docs.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/sorting-filtering-and-paging-with-the-entity-framework-in-an-asp-net-mvc-application
            int pageSize = numberOfRecs;
            int pageNumber = (page ?? 1);
            return View(sortedData.ToPagedList(pageNumber, pageSize));
            //return View(limiteddData);
        }


        // https://stackoverflow.com/questions/7265186/how-do-i-specify-the-linq-orderby-argument-dynamically
        //            }).OrderBy(s => Int32.Parse(Regex.Replace(s.InvoiceNumber, @"[^\d]", ""))).Take(10);
        //   }).OrderBy(q => q.InvoiceNumber).Take(10);

        [HttpPost]
        public ActionResult Index(int? noOfRecs, string invoiceNumberContains, int? customerId, string notesContains,
                int? fullyPaid, decimal? amountFrom, decimal? amountTo, DateTime invFromDate, DateTime invToDate, string sortOrder, int? page)
        {
            int numberOfRecs = (noOfRecs == null || noOfRecs == 0 || noOfRecs > 1000) ? 10 : (int)noOfRecs;
            int custId = (customerId == null) ? 0 : (int)customerId;
            decimal amtFrom = (amountFrom == null) ? 0 : (decimal)amountFrom;
            decimal amtTo = (amountTo == null) ? 10000000 : (decimal)amountTo;
            int fullyPd = (fullyPaid == null) ? -1 : (int)fullyPaid;
            bool fp = (fullyPd == 1);
            DateTime invFromDt = (invFromDate == null) ? DateTime.Today.AddDays(-365) : (DateTime)invFromDate;
            DateTime invToDt = (invToDate == null) ? DateTime.Today.AddDays(100) : (DateTime)invToDate;

            var dbData = dbEF.R_Invoice
                        .Where(q => (q.InvoiceNumber.ToLower().Contains(invoiceNumberContains.ToLower()) || invoiceNumberContains == null || invoiceNumberContains.Trim() == string.Empty)
                        && (customerId == null || q.CustomerId == custId)
                        && (q.Notes.Contains(notesContains) || notesContains == null || notesContains.Trim() == string.Empty)
                        && (q.Amount >= amtFrom && q.Amount <= amtTo)
                        && (fullyPd == -1 || q.FullyPaid == fp)
                        && (q.InvoiceDate >= invFromDt && q.InvoiceDate <= invToDt));



            var sortedData = dbData.OrderBy(q => q.InvoiceNumber);
            switch (sortOrder)
            {
                case "InvoiceNumber_desc":
                    sortedData = dbData.OrderByDescending(q => q.InvoiceNumber);
                    break;
                case "InvoiceDate":
                    sortedData = dbData.OrderBy(q => q.InvoiceDate);
                    break;
                case "InvoiceDate_desc":
                    sortedData = dbData.OrderByDescending(q => q.InvoiceDate);
                    break;
                default:
                    break;
            }

            // var dataLimit = sortedData;
            ViewBag.NumberOfRecords = numberOfRecs;
            var data = sortedData
                .Select(q => new Invoice
                {
                    Id = q.Id,
                    InvoiceNumber = q.InvoiceNumber,
                    CustomerId = q.CustomerId,
                    CustomerName = dbEF.R_Customer.Where(c => c.Id == q.CustomerId).Select(n => n.CustomerName).FirstOrDefault(),
                    Amount = q.Amount,
                    InvoiceDate = q.InvoiceDate,
                    FullyPaid = q.FullyPaid,
                    AmountPaid = q.AmountPaid,
                    Notes = q.Notes
                });

            int pageSize = numberOfRecs;
            int pageNumber = (page ?? 1);
            return View(data.ToPagedList(pageNumber, pageSize));

            //return View(data);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = dbMV.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        public ActionResult Create()
        {
            var invData = new Invoice();
            var custs = dbEF.R_Customer.Select(q => new { q.Id, q.CustomerName });
            invData.CustomersList = new SelectList(custs, "Id", "CustomerName");

            return View(invData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,InvoiceNumber,CustomerId,Amount,InvoiceDate,FullyPaid,AmountPaid,Notes,TimeStamp")] Invoice invoice)
        {
            var i = invoice.ConvertVwModelToDB(invoice);
            var customerId = invoice.CustomerId;
            var invoiceAmount = invoice.Amount;
            var paymentAmount = invoice.AmountPaid;

            ValidateModel(invoice);

            if (ModelState.IsValid)
            {
                try
                {
                    dbEF.R_Invoice.Add(i);
                    var customer = GetCustomerBalance(customerId, 0, invoiceAmount, 0, paymentAmount);
                    dbEF.Entry(customer).State = EntityState.Modified;

                    dbEF.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception excp)
                {
                    var errMsg = excp.InnerException.InnerException.Message;
                    //if (errMsg.Contains("Violation of UNIQUE KEY constraint"))
                    //{
                    //    ModelState.AddModelError("InvoiceNumber", "Invoice Number already exist.");
                    //}
                    //else
                    //{
                    ModelState.AddModelError("InvoiceNumber", errMsg);
                    //}
                }
            }

            var custs = dbEF.R_Customer.Select(q => new { q.Id, q.CustomerName });
            invoice.CustomersList = new SelectList(custs, "Id", "CustomerName");
            return View(invoice);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var invoice = new Invoice().ConvertDBToVwModel(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }

            var custs = dbEF.R_Customer.Select(q => new { q.Id, q.CustomerName });
            invoice.CustomersList = new SelectList(custs, "Id", "CustomerName");

            return View(invoice);
        }

        [HttpPost]
        public ActionResult Edit([Bind(Include = "Id,InvoiceNumber,CustomerId,CustomerName,Amount,InvoiceDate,FullyPaid,AmountPaid,Notes,TimeStamp")] Invoice invoice)
        {
            var invoiceDB = invoice.ConvertVwModelToDB(invoice);

            var invoiceAmountOld = GetOldInvoiceAmount(invoice.Id);
            var invoiceAmountNew = invoice.Amount;
            var paymentAmountOld = GetOldPaymentAmount(invoice.Id);
            var paymentAmountNew = invoice.AmountPaid;

            ValidateModel(invoice);

            if (ModelState.IsValid)
            {
                try
                {
                    dbEF.Entry(invoiceDB).State = EntityState.Modified;
                    var customer = GetCustomerBalance(invoice.CustomerId, invoiceAmountOld, invoiceAmountNew, paymentAmountOld, paymentAmountNew);
                    dbEF.Entry(customer).State = EntityState.Modified;
                    dbEF.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception excp)
                {
                    var errMsg = excp.InnerException.InnerException.Message;
                    ModelState.AddModelError("InvoiceNumber", errMsg);
                }

            }
            var custs = dbEF.R_Customer.Select(q => new { q.Id, q.CustomerName });
            invoice.CustomersList = new SelectList(custs, "Id", "CustomerName");

            return View(invoice);
        }


        private void ValidateModel(Invoice invoice)
        {
            if (invoice.InvoiceNumber == null || invoice.InvoiceNumber.Trim() == string.Empty)
            {
                ModelState.AddModelError("InvoiceNumber", "Invoice Number Cannot be blank.");
            }
            else
            {
                var invoiceExists = dbEF.R_Invoice.Where(q => q.Id != invoice.Id && q.InvoiceNumber == invoice.InvoiceNumber).Count();
                if (invoiceExists > 0)
                {
                    ModelState.AddModelError("InvoiceNumber", "Invoice Number already exist.");
                }
            }

            if (invoice.CustomerId == null || invoice.CustomerId == 0)
            {
                ModelState.AddModelError("CustomerId", "A Customer has to be selected");
            }
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var invoice = new Invoice().ConvertDBToVwModel(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var invoice = dbEF.R_Invoice.Find(id);

            var invoiceAmountOld = GetOldInvoiceAmount(id);
            var paymentAmountOld = GetOldPaymentAmount(id);
            var customer = GetCustomerBalance(invoice.CustomerId, invoiceAmountOld, 0, paymentAmountOld, 0);

            dbEF.R_Invoice.Remove(invoice);
            dbEF.Entry(customer).State = EntityState.Modified;
            dbEF.SaveChanges();
            return RedirectToAction("Index");
        }


        //private decimal? GetNewInvoiceAmount(R_Invoice invoice)
        //{
        //    return invoice.Amount;
        //}

        private decimal? GetOldInvoiceAmount(int invoiceId)
        {
            var invoiceAmount = dbEF.R_Invoice.Where(q => q.Id == invoiceId).Select(q => q.Amount).FirstOrDefault();
            return invoiceAmount;
        }

        private decimal? GetNewPaymentAmount(R_Payments payment)
        {
            return payment.Amount;
        }

        private decimal? GetOldPaymentAmount(int invoiceId)
        {
            var PaymentAmount = dbEF.R_Invoice.Where(q => q.Id == invoiceId).Select(q => q.AmountPaid).FirstOrDefault();
            return PaymentAmount;
        }

        public R_Customer GetCustomerBalance(int? customerId, decimal? oldInvoiceAmount, decimal? newInvoiceAmount, decimal? oldPaymentAmount, decimal? newPaymentAmount)
        {
            var customer = dbEF.R_Customer.Where(q => q.Id == customerId).FirstOrDefault();
            var cusBal = (customer.Balance == null) ? 0 : (decimal)customer.Balance;

            decimal oldInvoiceAmount_i = (oldInvoiceAmount == null) ? 0 : (decimal)oldInvoiceAmount;
            decimal newInvoiceAmount_i = (newInvoiceAmount == null) ? 0 : (decimal)newInvoiceAmount;
            decimal oldPaymentAmount_i = (oldPaymentAmount == null) ? 0 : (decimal)oldPaymentAmount;
            decimal newPaymentAmount_i = (newPaymentAmount == null) ? 0 : (decimal)newPaymentAmount;

            var newBalance = cusBal + (newInvoiceAmount_i - oldInvoiceAmount_i) - (newPaymentAmount_i - oldPaymentAmount_i);
            customer.Balance = newBalance;
            return customer;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbMV.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
