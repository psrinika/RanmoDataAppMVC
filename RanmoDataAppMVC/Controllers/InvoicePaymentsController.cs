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
    public class InvoicePaymentsController : Controller
    {
        private RanSanDBEntities dbEF = new RanSanDBEntities();
        private ApplicationDbContext dbMV = new ApplicationDbContext();

        // GET: InvoicePayments
        public ActionResult Index()
        {

            List<InvoicePayment> dataPaymentInvoices = new List<InvoicePayment>();

            var dbOldPayments = dbEF.R_Payments
                    .Where(p => p.PaymentStatusId != 3) // chk whether 3 = Settled
                    .OrderBy(q => q.PaidDate); // chk ascending / descending
            //        .Take(10);

            var dbOldPaymentCustomerInvoices = dbEF.R_Invoice
                .Where(i => !i.FullyPaid) // when fully paid this has to b updated
                .Join(dbOldPayments, i => i.CustomerId, p => p.CustomerId, (i, p) => new { I = i, P = p });

            foreach (var item in dbOldPaymentCustomerInvoices)
            {
                InvoicePayment pi = new InvoicePayment();
                pi.PaymentId = item.P.Id;
                pi.InvoiceId = item.I.Id;
                pi.ReceiptNo = item.P.ReceiptNo;
                pi.InvoiceNumber = item.I.InvoiceNumber;
                pi.CustomerId = item.P.CustomerId;
                pi.CustomerName = dbEF.R_Customer.Where(c => c.Id == item.P.CustomerId).Select(n => n.CustomerName).FirstOrDefault();
                pi.InvoiceAmount = (item.I.Amount == null) ? 0 : (decimal)item.I.Amount;
                pi.PaymentAmount = (item.P.Amount == null) ? 0 : (decimal)item.P.Amount;

                var ip = dbEF.R_InvoicePayment
                    .Where(p => p.PaymentId == item.P.Id && p.InvoiceId == item.I.Id)
                    .FirstOrDefault();

                //pi.InvoicePaymentAmount = ip.InvoicePaymentAmount;
                //pi.RemainingPaymentAmount = ip.RemainingPaymentAmount; 
                //pi.RemainingInvoiceAmount = ip.RemainingInvoiceAmount;
                //pi.PaymentFullyUsed = ip.PaymentFullyUsed;
                //pi.InvoiceFullyPaid = ip.InvoiceFullyPaid;

                pi.InvoicePaymentAmount = (ip == null) ? 0 : ip.InvoicePaymentAmount;
                pi.RemainingPaymentAmount = (ip == null) ? 0 : ip.RemainingPaymentAmount;
                pi.RemainingInvoiceAmount = (ip == null) ? 0 : ip.RemainingInvoiceAmount;
                pi.PaymentFullyUsed = (ip == null) ? false : ip.PaymentFullyUsed;
                pi.InvoiceFullyPaid = (ip == null) ? false : ip.InvoiceFullyPaid;

                dataPaymentInvoices.Add(pi);
            }

            //var dbOldInvoices = dbEF.R_Invoice
            //        .Where(p => !p.FullyPaid) // when fully paid this has to b updated
            //        .OrderBy(q => q.InvoiceDate); // chk ascending / descending


            //var dbInvoicePayment_Payment = dbEF.R_InvoicePayment
            //    .Join(dbOldPayments, ip => ip.PaymentId, p => p.Id, (pi, p) => new { PI = pi, P = p });

            //var viewData = 

            return View(dataPaymentInvoices);
        }


        public ActionResult Edit(int PaymentId, int InvoiceId)

        // [Bind(Include = "PaymentId,InvoiceId,ReceiptNo,CustomerId,CustomerName,InvoiceNumber,PaymentAmount,InvoiceAmount,InvoicePaymentAmount,RemainingPaymentAmount,RemainingInvoiceAmount,PaymentFullyUsed,InvoiceFullyPaid,TimeStamp")] InvoicePayment ip)
        //  int PaymentId, int InvoiceId, string ReceiptNo, int CustomerId, string CustomerName,
        //        string InvoiceNumber, decimal? PaymentAmount, decimal? InvoiceAmount, decimal? InvoicePaymentAmount,
        //        decimal? RemainingPaymentAmount, decimal? RemainingInvoiceAmount, bool PaymentFullyUsed, bool InvoiceFullyPaid)

        {
            //if (PaymentId == null || InvoiceId == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            InvoicePayment invoicePayment = new InvoicePayment().ConvertDBToVwModel(PaymentId, InvoiceId);
            if (invoicePayment == null)
            {
                return HttpNotFound();
            }
            return View(invoicePayment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PaymentId,InvoiceId,ReceiptNo,CustomerId,CustomerName,InvoiceNumber,PaymentAmount,InvoiceAmount,InvoicePaymentAmount,RemainingPaymentAmount,RemainingInvoiceAmount,PaymentFullyUsed,InvoiceFullyPaid,TimeStamp")] InvoicePayment invoicePayment)
        {
            var x = invoicePayment;
            // get invoicePayment --> depending on existence, Add or Modify
            var invoicePaymentEF = dbEF.R_InvoicePayment
                                    .Where(q => q.PaymentId == invoicePayment.PaymentId && q.InvoiceId == invoicePayment.InvoiceId).FirstOrDefault();

            if (invoicePaymentEF == null)
            {
                invoicePaymentEF.PaymentId = invoicePayment.PaymentId;
                invoicePaymentEF.InvoiceId = invoicePayment.InvoiceId;
                invoicePaymentEF.InvoicePaymentAmount = invoicePayment.InvoicePaymentAmount;
                invoicePaymentEF.RemainingPaymentAmount = invoicePayment.RemainingPaymentAmount; // function to get the remaining payment amount
                invoicePaymentEF.RemainingInvoiceAmount = invoicePayment.RemainingInvoiceAmount; // function to get the remaining invoice amount

                invoicePaymentEF.PaymentFullyUsed = invoicePayment.PaymentFullyUsed; // if remaining = 0 or more
                invoicePaymentEF.InvoiceFullyPaid = invoicePayment.InvoiceFullyPaid;

                dbEF.R_InvoicePayment.Add(invoicePaymentEF);
            }
            // dbEF.InvoicePayments.Add(invoicePayment);


            // get invoice / payment / customer update, 


            if (ModelState.IsValid)
            {
                dbEF.Entry(invoicePayment).State = EntityState.Modified;
                dbEF.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(invoicePayment);
        }

        private decimal? GetInvoiceAmount(int invoiceId)
        {
            var invoiceAmount = dbEF.R_Invoice.Where(q => q.Id == invoiceId).Select(q => q.Amount).FirstOrDefault();
            return invoiceAmount;
        }

        private decimal? GetPaymentAmount(int invoiceId)
        {
            var PaymentAmount = dbEF.R_Invoice.Where(q => q.Id == invoiceId).Select(q => q.AmountPaid).FirstOrDefault();
            return PaymentAmount;
        }

        /*

          [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "PaymentId,InvoiceId,ReceiptNo,CustomerId,CustomerName,InvoiceNumber,PaymentAmount,InvoiceAmount,InvoicePaymentAmount,RemainingPaymentAmount,RemainingInvoiceAmount,PaymentFullyUsed,InvoiceFullyPaid,TimeStamp")] InvoicePayment invoicePayment)
        {
            var x = invoicePayment;

            if (ModelState.IsValid)
            {
                dbEF.Entry(invoicePayment).State = EntityState.Modified;
                dbEF.SaveChanges();
            }
            //            return View(invoicePayment);
            return RedirectToAction("Index");

        }          
            
            
            // GET: InvoicePayments/Details/5
                public ActionResult Details(int? id)
                {
                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    InvoicePayment invoicePayment = dbEF.InvoicePayments.Find(id);
                    if (invoicePayment == null)
                    {
                        return HttpNotFound();
                    }
                    return View(invoicePayment);
                }

                // GET: InvoicePayments/Create
                public ActionResult Create()
                {
                    return View();
                }

                // POST: InvoicePayments/Create
                // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
                // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
                [HttpPost]
                [ValidateAntiForgeryToken]
                public ActionResult Create([Bind(Include = "PaymentId,InvoiceId,ReceiptNo,CustomerId,CustomerName,InvoiceNumber,PaymentAmount,InvoiceAmount,InvoicePaymentAmount,RemainingPaymentAmount,RemainingInvoiceAmount,PaymentFullyUsed,InvoiceFullyPaid,TimeStamp")] InvoicePayment invoicePayment)
                {
                    if (ModelState.IsValid)
                    {
                        dbEF.InvoicePayments.Add(invoicePayment);
                        dbEF.SaveChanges();
                        return RedirectToAction("Index");
                    }

                    return View(invoicePayment);
                }



                // POST: InvoicePayments/Edit/5
                // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
                // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
                [HttpPost]
                [ValidateAntiForgeryToken]
                public ActionResult Edit([Bind(Include = "PaymentId,InvoiceId,ReceiptNo,CustomerId,CustomerName,InvoiceNumber,PaymentAmount,InvoiceAmount,InvoicePaymentAmount,RemainingPaymentAmount,RemainingInvoiceAmount,PaymentFullyUsed,InvoiceFullyPaid,TimeStamp")] InvoicePayment invoicePayment)
                {
                    if (ModelState.IsValid)
                    {
                        dbEF.Entry(invoicePayment).State = EntityState.Modified;
                        dbEF.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    return View(invoicePayment);
                }

                // GET: InvoicePayments/Delete/5
                public ActionResult Delete(int? id)
                {
                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    InvoicePayment invoicePayment = dbEF.InvoicePayments.Find(id);
                    if (invoicePayment == null)
                    {
                        return HttpNotFound();
                    }
                    return View(invoicePayment);
                }

                // POST: InvoicePayments/Delete/5
                [HttpPost, ActionName("Delete")]
                [ValidateAntiForgeryToken]
                public ActionResult DeleteConfirmed(int id)
                {
                    InvoicePayment invoicePayment = dbEF.InvoicePayments.Find(id);
                    dbEF.InvoicePayments.Remove(invoicePayment);
                    dbEF.SaveChanges();
                    return RedirectToAction("Index");
                }
        */
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbEF.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
