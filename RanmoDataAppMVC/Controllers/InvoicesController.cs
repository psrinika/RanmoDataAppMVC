using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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

        public ActionResult Index(int? noOfRecs, string invoiceNumberContains, int? customerId, string CustomerName, string notesContains,
                int? fullyPaid, decimal? amountFrom, decimal? amountTo, DateTime? invFromDate, DateTime? invToDate, string sortOrder, int? page, string CurrentFilter)
        {

            ViewBag.CurrentFilter = CurrentFilter;
            ViewBag.sortOrder = sortOrder;

            ViewBag.InvoiceNumberSortParm = String.IsNullOrEmpty(sortOrder) ? "InvoiceNumber_desc" : "";
            ViewBag.CustomerNameSortParm = sortOrder == "CustomerName" ? "CustomerName_desc" : "CustomerName";

            ViewBag.InvoiceDateSortParm = sortOrder == "InvoiceDate" ? "InvoiceDate_desc" : "InvoiceDate";

            int numberOfRecs = (noOfRecs == null || noOfRecs == 0 || noOfRecs > 1000) ? 10 : (int)noOfRecs;
            CustomerName = (CustomerName == "" || CustomerName == null) ? null : CustomerName.ToLower();
            customerId = (CustomerName == null || CustomerName == "") ? null : customerId;
            int custId = (customerId == null) ? 0 : (int)customerId;
            decimal amtFrom = (amountFrom == null) ? 0 : (decimal)amountFrom;
            decimal amtTo = (amountTo == null) ? 10000000 : (decimal)amountTo;
            int fullyPd = (fullyPaid == null) ? 0 : (int)fullyPaid;
            bool fp = (fullyPd == 1);
            DateTime invFromDt = (invFromDate == null) ? DateTime.Today.AddDays(-365) : (DateTime)invFromDate;
            DateTime invToDt = (invToDate == null) ? DateTime.Today.AddDays(100) : (DateTime)invToDate;

            var dbData = dbEF.R_Invoice
                        .Where(i => (i.InvoiceNumber.ToLower().Contains(invoiceNumberContains.ToLower()) || invoiceNumberContains == null || invoiceNumberContains.Trim() == string.Empty)
                        && (customerId == null || i.CustomerId == custId)
                        && (string.IsNullOrEmpty(notesContains) || i.Notes.Contains(notesContains))
                        && (i.Amount >= amtFrom && i.Amount <= amtTo)
                        && (fullyPd == -1 || i.FullyPaid == fp)
                        && (i.InvoiceDate >= invFromDt && i.InvoiceDate <= invToDt))
                      .Join(dbEF.R_Customer, i => i.CustomerId, c => c.Id, (i, c) => new { I = i, c.CustomerName })
                      .Where(c => (string.IsNullOrEmpty(CustomerName) || c.CustomerName.Contains(CustomerName)));

            //var sortedData = dbData.OrderBy(q => q.I.InvoiceNumber, new NaturalStringComparer());
            var sortedData = dbData.OrderBy(q => SqlFunctions.Replicate("0", 20 - q.I.InvoiceNumber.Length) + q.I.InvoiceNumber);
         //   var x = sortedData.ToList();
            switch (sortOrder)
            {
                case "InvoiceNumber_desc":
                    sortedData = dbData.OrderByDescending(q => SqlFunctions.Replicate("0", 20 - q.I.InvoiceNumber.Length) + q.I.InvoiceNumber);
                    break;
                case "InvoiceDate":
                    sortedData = dbData.OrderBy(q => q.I.InvoiceDate);
                    break;
                case "InvoiceDate_desc":
                    sortedData = dbData.OrderByDescending(q => q.I.InvoiceDate);
                    break;
                case "CustomerName":
                    sortedData = dbData.OrderBy(q => q.CustomerName);
                    break;
                case "CustomerName_desc":
                    sortedData = dbData.OrderByDescending(q => q.CustomerName);
                    break;
                default:
                    break;
            }

            ViewBag.NumberOfRecords = numberOfRecs;
            ViewBag.invoiceNumberContains = invoiceNumberContains;
            ViewBag.fullyPaid = fullyPaid;
            ViewBag.amountFrom = amountFrom;
            ViewBag.invFromDate = invFromDate;
            ViewBag.notesContains = notesContains;
            ViewBag.CustomerId = customerId;
            ViewBag.amountTo = amountTo;
            ViewBag.invToDate = invToDate;

            var data = sortedData
                .Select(q => new Invoice
                {
                    Id = q.I.Id,
                    InvoiceNumber = q.I.InvoiceNumber,
                    CustomerId = q.I.CustomerId,
                    CustomerName = q.CustomerName,
                    Amount = q.I.Amount,
                    InvoiceDate = q.I.InvoiceDate,
                    FullyPaid = q.I.FullyPaid,
                    AmountPaid = q.I.AmountPaid,
                    Notes = q.I.Notes
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
 //           var paymentAmount = invoice.AmountPaid;

            ValidateModel(invoice);

            if (ModelState.IsValid)
            {
                try
                {
                    dbEF.R_Invoice.Add(i);
 //                   var customer = GetCustomerBalance(customerId, 0, invoiceAmount, 0, paymentAmount);
                    var customer = GetCustomerBalance(customerId, 0, invoiceAmount);
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
//            var paymentAmountOld = GetOldPaymentAmount(invoice.Id);
//            var paymentAmountNew = invoice.AmountPaid;

            ValidateModel(invoice);

            if (ModelState.IsValid)
            {
                try
                {
                    dbEF.Entry(invoiceDB).State = EntityState.Modified;
                    //var customer = GetCustomerBalance(invoice.CustomerId, invoiceAmountOld, invoiceAmountNew, paymentAmountOld, paymentAmountNew);
                    var customer = GetCustomerBalance(invoice.CustomerId, invoiceAmountOld, invoiceAmountNew);
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
            //           var paymentAmountOld = GetOldPaymentAmount(id);
            //var customer = GetCustomerBalance(invoice.CustomerId, invoiceAmountOld, 0, paymentAmountOld, 0);
            var customer = GetCustomerBalance(invoice.CustomerId, invoiceAmountOld, 0);

            dbEF.R_Invoice.Remove(invoice);
            dbEF.Entry(customer).State = EntityState.Modified;
            dbEF.SaveChanges();
            return RedirectToAction("Index");
        }

        private decimal? GetOldInvoiceAmount(int invoiceId)
        {
            var invoiceAmount = dbEF.R_Invoice.Where(q => q.Id == invoiceId).Select(q => q.Amount).FirstOrDefault();
            return invoiceAmount;
        }

        //public  string PadNumbers(string input)
        //{
        //    return Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
        //}

        public R_Customer GetCustomerBalance(int? customerId, decimal? oldInvoiceAmount, decimal? newInvoiceAmount)
        {
            var customer = dbEF.R_Customer.Where(q => q.Id == customerId).FirstOrDefault();
            var cusBal = (customer.Balance == null) ? 0 : (decimal)customer.Balance;

            decimal oldInvoiceAmount_i = (oldInvoiceAmount == null) ? 0 : (decimal)oldInvoiceAmount;
            decimal newInvoiceAmount_i = (newInvoiceAmount == null) ? 0 : (decimal)newInvoiceAmount;
            //decimal oldPaymentAmount_i = (oldPaymentAmount == null) ? 0 : (decimal)oldPaymentAmount;
            //decimal newPaymentAmount_i = (newPaymentAmount == null) ? 0 : (decimal)newPaymentAmount;

            var newBalance = cusBal + (newInvoiceAmount_i - oldInvoiceAmount_i);// - (newPaymentAmount_i - oldPaymentAmount_i);
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

    public static class SafeNativeMethods
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }

    public sealed class NaturalStringComparer : IComparer<string>
    {
        #region IComparer<string> Members

        public int Compare(string x, string y)
        {
            return SafeNativeMethods.StrCmpLogicalW(x, y);
        }

        #endregion
    }
}
