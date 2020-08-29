using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RanmoDataAppMVC.Models;
using RanmoDataAppMVC.ViewModels;
using RanmoDB;

namespace RanmoDataAppMVC.Controllers
{
    public class PaymentsController : Controller
    {
        private ApplicationDbContext dbMV = new ApplicationDbContext();
        private RanSanDBEntities dbEF = new RanSanDBEntities();

        public ActionResult Index0()
        {


            var paymentsNotFullyPaid = dbEF.R_Invoice
                .Where(i => (!i.FullyPaid))
                .Join(dbEF.R_Payments,
                    i => i.InvoiceNumber,
                    p => p.InvoiceNumber,
                    (i, p) => p);

            var dbData = paymentsNotFullyPaid.Select(paymentVM => new Payments
            {
                Id = paymentVM.Id,
                ReceiptNo = paymentVM.ReceiptNo,
                InvoiceNumber = paymentVM.InvoiceNumber,
                Amount = paymentVM.Amount,
                PaidById = paymentVM.PaidById,
                PaidBy = dbEF.R_PaidBy.Where(c => c.Id == paymentVM.PaidById).Select(n => n.PaidBy).FirstOrDefault(),
                PaymentStatusId = paymentVM.PaymentStatusId,
                PaymentStatus = dbEF.R_PaymentStatus.Where(c => c.Id == paymentVM.PaymentStatusId).Select(n => n.PaymentStatus).FirstOrDefault(),
                ReceivedDate = paymentVM.ReceivedDate,
                ChequeDate = paymentVM.ChequeDate,
                PaidDate = paymentVM.PaidDate,
                Notes = paymentVM.Notes
            });

            SelectList paidByLst = new SelectList(dbEF.R_PaidBy.ToList(), "Id", "PaidBy");
            ViewBag.PaidByList = paidByLst;

            SelectList paymentStatusLst = new SelectList(dbEF.R_PaymentStatus.ToList(), "Id", "PaymentStatus");
            ViewBag.PaymentStatusList = paymentStatusLst;


            return View(dbData);
        }

        public ActionResult Index()
        {
            var dbData = dbEF.R_Payments.Select(q => new Payments
            {
                Id = q.Id,
                ReceiptNo = q.ReceiptNo,
                CustomerId = q.CustomerId,
                CustomerName = dbEF.R_Customer.Where(c => c.Id == q.CustomerId).Select(n => n.CustomerName).FirstOrDefault(),
                Amount = q.Amount,
                PaidById = q.PaidById,
                PaidBy = dbEF.R_PaidBy.Where(c => c.Id == q.PaidById).Select(n => n.PaidBy).FirstOrDefault(),
                PaymentStatusId = q.PaymentStatusId,
                PaymentStatus = dbEF.R_PaymentStatus.Where(c => c.Id == q.PaymentStatusId).Select(n => n.PaymentStatus).FirstOrDefault(),
                ReceivedDate = q.ReceivedDate,
                ChequeDate = q.ChequeDate,
                PaidDate = q.PaidDate,
                Notes = q.Notes
            });

            SelectList paidByLst = new SelectList(dbEF.R_PaidBy.ToList(), "Id", "PaidBy");
            ViewBag.PaidByList = paidByLst;

            SelectList paymentStatusLst = new SelectList(dbEF.R_PaymentStatus.ToList(), "Id", "PaymentStatus");
            ViewBag.PaymentStatusList = paymentStatusLst;

            return View(dbData);
        }


        /*
        > filtering part 
            - Date x 3 into a drop down
            - Customer & Inv num - auto complete
            - Code to fill status / paid by to drop down
            - implement filter

        > In create & edit --> show the 
                amount, date, total paid ... details of invoice in a seperate panel
                by invoice / by customer
        > add fields as chk number or let them put in notes ...

https://www.google.com/search?rlz=1C1NHXL_enCA736CA736&sxsrf=ALeKk02KED6s3y1-yQbEGzw9JnhsDh-RAw%3A1595723481117&ei=2c4cX5PGBs_j_AbE7pyoAg&q=mvc+autocomplete+ajax+query+based+on+selection&oq=mvc+autocomplete+ajax+query+based+on+selection&gs_lcp=CgZwc3ktYWIQAzoECAAQRzoCCAA6BggAEBYQHjoFCCEQoAE6CAgAEAgQDRAeOgcIIRAKEKABOgQIIRAVUKmdlgRYl_iYBGCh-pgEaApwAXgAgAGqAogB5SSSAQcxNC4yMi4zmAEAoAECoAEBqgEHZ3dzLXdpesABAQ&sclient=psy-ab&ved=0ahUKEwjTloGa1enqAhXPMd8KHUQ3ByUQ4dUDCAw&uact=5
https://stackoverflow.com/questions/35753808/how-to-make-another-ajax-call-upon-selection-of-autocomplete-text-field-value-in

            https://stackoverflow.com/questions/376644/setting-ajax-url-for-jquery-in-js-file-using-asp-net-mvc
        */
        [HttpPost]
        public ActionResult Index(int? noOfRecs, string ReceiptNo, int? CustomerId, string DateType,
                 int? PaidById, int? PaymentStatusId, decimal? amountFrom, decimal? amountTo, DateTime fromDate, DateTime toDate)
        {
            int numberOfRecs = (noOfRecs == null || noOfRecs == 0 || noOfRecs > 1000) ? 10 : (int)noOfRecs;
            //var custId = (customerId == null) ? 0 : (int)customerId;
            decimal amtFrom = (amountFrom == null) ? 0 : (decimal)amountFrom;
            decimal amtTo = (amountTo == null) ? 10000000 : (decimal)amountTo;
            //var paidById = (PaidById == null) ? 0 : (int)PaidById;


            //DateTime fromDtDefault = (fromDate == null) ? DateTime.Today.AddDays(-365) : (DateTime)fromDate;
            //DateTime toDtDefault = (toDate == null) ? DateTime.Today.AddDays(100) : (DateTime)toDate;

            DateTime paidFromDt = DateTime.Today.AddDays(-365);
            DateTime paidToDt = DateTime.Today.AddDays(100);

            DateTime chequeFromDt = DateTime.Today.AddDays(-365);
            DateTime chequeToDt = DateTime.Today.AddDays(100);

            DateTime receivedFromDt = DateTime.Today.AddDays(-365);
            DateTime receivedToDt = DateTime.Today.AddDays(100);

            if (fromDate != null)
            {
                if (DateType == "PaidDate")
                {
                    paidFromDt = fromDate;
                }
                else if (DateType == "ChequeDate")
                {
                    chequeFromDt = fromDate;
                }
                else if (DateType == "ReceivedDate")
                {
                    receivedFromDt = fromDate;
                }
            }
            if (toDate != null)
            {
                if (DateType == "PaidDate")
                {
                    paidToDt = toDate;
                }
                else if (DateType == "ChequeDate")
                {
                    chequeToDt = toDate;
                }
                else if (DateType == "ReceivedDate")
                {
                    receivedToDt = toDate;
                }
            }

            //var paymentsByCust = dbEF.R_Invoice
            //   .Where(i => (!i.FullyPaid))
            //   .Join(dbEF.R_Payments,
            //      i => i.InvoiceNumber,
            //      p => p.InvoiceNumber,
            //      (i, p) => p);

            var dataInvNoFiltered = dbEF.R_Payments
                        .Where(q => (ReceiptNo == null || ReceiptNo.Trim() == string.Empty ||
                            q.ReceiptNo.ToLower().Contains(ReceiptNo.ToLower()))
                        && (CustomerId == null || q.CustomerId == CustomerId)
                        && (PaidById == null ||  q.PaidById == PaidById)
                        && (q.Amount >= amtFrom && q.Amount <= amtTo)
                        && ((q.PaidDate == null && DateType != "PaidDate") || (q.PaidDate >= paidFromDt && q.PaidDate <= paidToDt))
                        && ((q.ChequeDate == null && DateType != "ChequeDate") || (q.ChequeDate >= chequeFromDt && q.ChequeDate <= chequeToDt))
                        && ((q.ReceivedDate == null && DateType != "ReceivedDate") || (q.ReceivedDate >= receivedFromDt && q.ReceivedDate <= receivedToDt))
                        )
                        .OrderBy(q => q.ReceivedDate)
                        .Take(numberOfRecs);


            var dataSorted = dataInvNoFiltered
                .Select(q => new Payments
                {
                    Id = q.Id,
                    ReceiptNo = q.ReceiptNo,
                    CustomerId = q.CustomerId,
                    CustomerName = dbEF.R_Customer.Where(c => c.Id == q.CustomerId).Select(n => n.CustomerName).FirstOrDefault(),
                    Amount = q.Amount,
                    PaidById = q.PaidById,
                    PaidBy = dbEF.R_PaidBy.Where(c => c.Id == q.PaidById).Select(n => n.PaidBy).FirstOrDefault(),
                    PaymentStatusId = q.PaymentStatusId,
                    PaymentStatus = dbEF.R_PaymentStatus.Where(c => c.Id == q.PaymentStatusId).Select(n => n.PaymentStatus).FirstOrDefault(),
                    ReceivedDate = q.ReceivedDate,
                    ChequeDate = q.ChequeDate,
                    PaidDate = q.PaidDate,
                    Notes = q.Notes
                });

            SelectList paidByLst = new SelectList(dbEF.R_PaidBy.ToList(), "Id", "PaidBy");
            ViewBag.PaidByList = paidByLst;

            SelectList paymentStatusLst = new SelectList(dbEF.R_PaymentStatus.ToList(), "Id", "PaymentStatus");
            ViewBag.PaymentStatusList = paymentStatusLst;
            return View(dataSorted);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payments payments = dbMV.Payments.Find(id);
            if (payments == null)
            {
                return HttpNotFound();
            }
            return View(payments);
        }

        public ActionResult Create()
        {
            var paymentData = new Payments();
            var paidBys = dbEF.R_PaidBy.Select(q => new { q.Id, q.PaidBy });
            var paymentStatuses = dbEF.R_PaymentStatus.Select(q => new { q.Id, q.PaymentStatus });
            paymentData.PaidByList = new SelectList(paidBys, "Id", "PaidBy");
            paymentData.PaidBySelected = "Cheque";
            paymentData.PaymentStatusList = new SelectList(paymentStatuses, "Id", "PaymentStatus");
            paymentData.PaymentStatusSelected = "Pending";
            return View(paymentData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ReceiptNo,CustomerId,Amount,PaidDate,PaidById,ReceivedDate,ChequeDate,ChequeNum,Notes,TimeStamp")] Payments payments)
        {
            var i = payments.ConvertVwModelToDB(payments);

            ValidateModel(payments);

            if (ModelState.IsValid)
            {
                try
                {
                    dbEF.R_Payments.Add(i);
                    dbEF.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception excp)
                {
                    var errMsg = excp.InnerException.InnerException.Message;
                    ModelState.AddModelError("InvoiceNumber", errMsg);
                }
            }

            var paidBys = dbEF.R_PaidBy.Select(q => new { q.Id, q.PaidBy });
            var paymentStatuses = dbEF.R_PaymentStatus.Select(q => new { q.Id, q.PaymentStatus });
            payments.PaidByList = new SelectList(paidBys, "Id", "PaidBy");
            payments.PaymentStatusList = new SelectList(paymentStatuses, "Id", "PaymentStatus");
            return View(payments);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var payments = new Payments().ConvertDBToVwModel(id);
            if (payments == null)
            {
                return HttpNotFound();
            }


            var paidBys = dbEF.R_PaidBy.Select(q => new { q.Id, q.PaidBy });
            var paymentStatuses = dbEF.R_PaymentStatus.Select(q => new { q.Id, q.PaymentStatus });
            payments.PaidByList = new SelectList(paidBys, "Id", "PaidBy");
            payments.PaymentStatusList = new SelectList(paymentStatuses, "Id", "PaymentStatus");
            return View(payments);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ReceiptNo,CustomerId,Amount,PaidDate,PaidById,ReceivedDate,ChequeDate,ChequeNum,Notes,TimeStamp")] Payments payments)
        {
            var invoiceDB = payments.ConvertVwModelToDB(payments);
            ValidateModel(payments);
            if (ModelState.IsValid)
            {
                try
                {
                    dbEF.Entry(invoiceDB).State = EntityState.Modified;
                    dbEF.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception excp)
                {
                    var errMsg = excp.InnerException.InnerException.Message;
                    ModelState.AddModelError("InvoiceNumber", errMsg);
                }
            }

            var paidBys = dbEF.R_PaidBy.Select(q => new { q.Id, q.PaidBy });
            var paymentStatuses = dbEF.R_PaymentStatus.Select(q => new { q.Id, q.PaymentStatus });
            payments.PaidByList = new SelectList(paidBys, "Id", "PaidBy");
            payments.PaymentStatusList = new SelectList(paymentStatuses, "Id", "PaymentStatus");
            return View(payments);
        }

        private void ValidateModel(Payments payment)
        {
            if (payment.ReceiptNo == null || payment.ReceiptNo.Trim() == string.Empty)
            {
                ModelState.AddModelError("ReceiptNo", "Receipt Number Cannot be blank.");
            }
            else
            {
                var receiptExists = dbEF.R_Payments.Where(q => q.Id != payment.Id && q.ReceiptNo == payment.ReceiptNo).Count();
                if (receiptExists > 0)
                {
                    ModelState.AddModelError("ReceiptNo", "Receipt Number already exist.");
                }
            }

            if (payment.CustomerId == 0)
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
            var payments = new Payments().ConvertDBToVwModel(id);
            if (payments == null)
            {
                return HttpNotFound();
            }
            return View(payments);
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var payments = dbEF.R_Payments.Find(id);
            dbEF.R_Payments.Remove(payments);
            dbEF.SaveChanges();
            return RedirectToAction("Index");
        }

        public JsonResult GetCustomers(string term = "")
        {
            var objCustomerlist = dbEF.R_Customer
                .Where(q => q.CustomerName.Contains(term))
                .Select(q => new { CustomerId = q.Id, q.CustomerName }).ToList();
            return Json(objCustomerlist, JsonRequestBehavior.AllowGet);
        }



        public JsonResult GetInvoices(string term = "")
        {
            var objCustomerlist = dbEF.R_Invoice
                .Where(q => q.InvoiceNumber.Contains(term))
                .Select(q => new { InvoiceId = q.Id, q.InvoiceNumber }).ToList();
            return Json(objCustomerlist, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetInvoiceDetails(int id = 0)
        {
            var invoiceDetails = dbEF.R_Invoice
                .Where(q => q.Id == id)
                .Select(q => new InvoiceDetails { InvoiceNumber = q.InvoiceNumber, InvoiceAmount = q.Amount,
                    InvoiceDate = q.InvoiceDate, CustomerId = q.CustomerId }).FirstOrDefault();


            var customer = dbEF.R_Customer.Where(q => q.Id == invoiceDetails.CustomerId).FirstOrDefault();
            var sumPaid = dbEF.R_Payments
                    .Where(q => q.InvoiceNumber == invoiceDetails.InvoiceNumber)
                    .Sum(q => q.Amount);

            invoiceDetails.CustomerId = customer.Id;
            invoiceDetails.CustomerName = customer.CustomerName;
            invoiceDetails.TotalAlreadyPaid = sumPaid;

            return Json(invoiceDetails, JsonRequestBehavior.AllowGet);
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
