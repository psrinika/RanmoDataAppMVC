using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
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

        public ActionResult Index1()
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

        public ActionResult Index(int? noOfRecs, string ReceiptNo, int? CustomerId, string CustomerName, string DateType, 
            int? PaidById, int? PaymentStatusId, decimal? amountFrom, decimal? amountTo, DateTime? fromDate, 
            DateTime? toDate, string sortOrder, int? page, string CurrentFilter)
        {
            ViewBag.CurrentFilter = CurrentFilter;
            ViewBag.sortOrder = sortOrder;

            ViewBag.ReceiptNoSortParm = String.IsNullOrEmpty(sortOrder) ? "ReceiptNo_desc" : "";
            ViewBag.PaidDateSortParm = sortOrder == "PaidDate" ? "PaidDate_desc" : "PaidDate";
            ViewBag.CustomerNameSortParm = sortOrder == "CustomerName" ? "CustomerName_desc" : "CustomerName";



            int numberOfRecs = (noOfRecs == null || noOfRecs == 0 || noOfRecs > 1000) ? 10 : (int)noOfRecs;

            decimal amtFrom = (amountFrom == null) ? 0 : (decimal)amountFrom;
            decimal amtTo = (amountTo == null) ? 10000000 : (decimal)amountTo;

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
                    paidFromDt = (DateTime)fromDate;
                }
                else if (DateType == "ChequeDate")
                {
                    chequeFromDt = (DateTime)fromDate;
                }
                else if (DateType == "ReceivedDate")
                {
                    receivedFromDt = (DateTime)fromDate;
                }
            }
            if (toDate != null)
            {
                if (DateType == "PaidDate")
                {
                    paidToDt = (DateTime)toDate;
                }
                else if (DateType == "ChequeDate")
                {
                    chequeToDt = (DateTime)toDate;
                }
                else if (DateType == "ReceivedDate")
                {
                    receivedToDt = (DateTime)toDate;
                }
            }

            var dbData = dbEF.R_Payments
                        .Where(q => (ReceiptNo == null || ReceiptNo.Trim() == string.Empty ||
                            q.ReceiptNo.ToLower().Contains(ReceiptNo.ToLower()))
                        && (CustomerId == null || q.CustomerId == CustomerId)
                        && (PaidById == null || q.PaidById == PaidById)
                        && (q.Amount >= amtFrom && q.Amount <= amtTo)
                        && ((q.PaidDate == null && DateType != "PaidDate") || (q.PaidDate >= paidFromDt && q.PaidDate <= paidToDt))
                        && ((q.ChequeDate == null && DateType != "ChequeDate") || (q.ChequeDate >= chequeFromDt && q.ChequeDate <= chequeToDt))
                        && ((q.ReceivedDate == null && DateType != "ReceivedDate") || (q.ReceivedDate >= receivedFromDt && q.ReceivedDate <= receivedToDt))
                        )
                        .Join(dbEF.R_Customer, i => i.CustomerId, c => c.Id, (i, c) => new { I = i, c.CustomerName })
                      .Where(c => (string.IsNullOrEmpty(CustomerName) || c.CustomerName.Contains(CustomerName)));
            ;
            // .OrderBy(q => q.ReceivedDate);
            //.Take(numberOfRecs);

            var sortedData = dbData.OrderBy(q => SqlFunctions.Replicate("0", 20 - q.I.ReceiptNo.Length) + q.I.ReceiptNo);
            switch (sortOrder)
            {
                case "ReceiptNo_desc":
                    sortedData = dbData.OrderByDescending(q => SqlFunctions.Replicate("0", 20 - q.I.ReceiptNo.Length) + q.I.ReceiptNo);
                    break;
                case "PaidDate":
                    sortedData = dbData.OrderBy(q => q.I.PaidDate);
                    break;
                case "PaidDate_desc":
                    sortedData = dbData.OrderByDescending(q => q.I.PaidDate);
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

            ViewBag.NumberOfRecords = noOfRecs;
            ViewBag.ReceiptNo = ReceiptNo;
            CustomerName = ViewBag.CustomerName;
            ViewBag.PaidById = PaidById;
            ViewBag.PaymentStatusId = PaymentStatusId;
            ViewBag.amountFrom = amountFrom;
            fromDate = ViewBag.fromDate;
            //ViewBag.notesContains = notesContains;
            ViewBag.CustomerId = CustomerId;
            ViewBag.amountTo = amountTo;
            ViewBag.toDate = toDate;

            var data = sortedData
                .Select(q => new Payments
                {
                    Id = q.I.Id,
                    ReceiptNo = q.I.ReceiptNo,
                    CustomerId = q.I.CustomerId,
                    CustomerName = dbEF.R_Customer.Where(c => c.Id == q.I.CustomerId).Select(n => n.CustomerName).FirstOrDefault(),
                    Amount = q.I.Amount,
                    PaidById = q.I.PaidById,
                    PaidBy = dbEF.R_PaidBy.Where(c => c.Id == q.I.PaidById).Select(n => n.PaidBy).FirstOrDefault(),
                    PaymentStatusId = q.I.PaymentStatusId,
                    PaymentStatus = dbEF.R_PaymentStatus.Where(c => c.Id == q.I.PaymentStatusId).Select(n => n.PaymentStatus).FirstOrDefault(),
                    ReceivedDate = q.I.ReceivedDate,
                    ChequeDate = q.I.ChequeDate,
                    PaidDate = q.I.PaidDate,
                    Notes = q.I.Notes
                });



            SelectList paidByLst = new SelectList(dbEF.R_PaidBy.ToList(), "Id", "PaidBy");
            ViewBag.PaidByList = paidByLst;

            SelectList paymentStatusLst = new SelectList(dbEF.R_PaymentStatus.ToList(), "Id", "PaymentStatus");
            ViewBag.PaymentStatusList = paymentStatusLst;
            //return View(dataSorted);

            int pageSize = numberOfRecs;
            int pageNumber = (page ?? 1);
            return View(data.ToPagedList(pageNumber, pageSize));

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


        */
        [HttpPost]
        public ActionResult Index(int? noOfRecs, string ReceiptNo, int? CustomerId, string DateType, int? PaidById, 
            int? PaymentStatusId, decimal? amountFrom, decimal? amountTo, DateTime fromDate, DateTime toDate)
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
        public ActionResult Create([Bind(Include = "Id,ReceiptNo,CustomerId,Amount,PaidDate,PaidById,PaymentStatusId,ReceivedDate,ChequeDate,ChequeNum,Notes,TimeStamp")] Payments payments)
        {
            var p = payments.ConvertVwModelToDB(payments);

            ValidateModel(payments);

            if (ModelState.IsValid)
            {
                try
                {
                    dbEF.R_Payments.Add(p);
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
        public ActionResult Edit([Bind(Include = "Id,ReceiptNo,CustomerId,Amount,PaidDate,PaidById,PaymentStatusId,ReceivedDate,ChequeDate,ChequeNum,Notes,TimeStamp")] Payments payments)
        {
            var paymentDB = payments.ConvertVwModelToDB(payments);
            ValidateModel(payments);
            if (ModelState.IsValid)
            {
                try
                {
                    dbEF.Entry(paymentDB).State = EntityState.Modified;
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


        public ActionResult InvoicePayment(int? paymentId)  // , int? invoiceId
        {
            

            //if (paymentId == null || invoiceId == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}

            // can chk for Payment Status ... 
            // if not the Check Realized - redirect to Create/Edit 
            // - if possible with an error messaage

            // if paymentId  == null or  0 (as in Create new payment?
            // if coming from Edit --> find the customer ... and from that find the invoice with oldest date and the status is not fully paid
            /* chk if there is a record in invoicePayment
             * 
             * 
             */
            var payment = dbEF.R_Payments
                    .Where(p => p.Id == paymentId)
                    .FirstOrDefault();

            var customerId = (payment == null)?0: payment.CustomerId;

            var invoice = dbEF.R_Invoice
                                .Where(p => p.CustomerId == customerId)
                                .OrderBy(x => x.InvoiceDate)
                                .FirstOrDefault();
                                

            var invoicePayment = dbEF.R_InvoicePayment
                    .Where(p => p.PaymentId == paymentId && p.InvoiceId == invoice.Id)
                    .FirstOrDefault();


            return View(invoicePayment);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InvoicePayment([Bind(Include = "Id,ReceiptNo,CustomerId,Amount,PaidDate,PaidById,ReceivedDate,ChequeDate,ChequeNum,Notes,TimeStamp")] Payments payments)
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
