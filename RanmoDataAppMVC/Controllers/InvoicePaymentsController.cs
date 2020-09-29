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
    public class InvoicePaymentsController : Controller
    {
        private RanSanDBEntities dbEF = new RanSanDBEntities();
        private ApplicationDbContext dbMV = new ApplicationDbContext();

        // GET: InvoicePayments
        public ActionResult Index()
        {

            List<InvoicePayment> dataPaymentInvoices = new List<InvoicePayment>();

            var dbOldPayments0 = dbEF.R_Payments
                    .Where(p => p.PaymentStatusId != 3) // chk whether 3 = Settled
                    .OrderBy(q => q.PaidDate); // chk ascending / descending

            var dbOldPayments = dbEF.R_Payments
                    .Where(p => p.PaymentStatusId != 3 && p.ReceivedDate != null) // chk whether 3 = Settled
                    .GroupBy(g => g.CustomerId)
                    .Select(q => q.OrderBy(r => r.ReceivedDate).ThenByDescending(r =>r.Amount).FirstOrDefault());

            var dbOldInvoices = dbEF.R_Invoice
                    .Where(i => !i.FullyPaid && i.InvoiceDate != null) // chk whether 3 = Settled
                    .GroupBy(g => g.CustomerId)
                    .Select(q => q.OrderBy(r => r.InvoiceDate).ThenByDescending(r => r.Amount).FirstOrDefault());

            var dbOldPaymentCustomerInvoices = dbOldInvoices
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

                pi.InvoicePaymentAmount = (ip == null) ? 0 : ip.InvoicePaymentAmount;
                pi.RemainingPaymentAmount = (ip == null) ? 0 : ip.RemainingPaymentAmount;
                pi.RemainingInvoiceAmount = (ip == null) ? 0 : ip.RemainingInvoiceAmount;
                pi.PaymentFullyUsed = (ip == null) ? false : ip.PaymentFullyUsed;
                pi.InvoiceFullyPaid = (ip == null) ? false : ip.InvoiceFullyPaid;

                dataPaymentInvoices.Add(pi);
            }

            InvoicePaymentList ipl = new InvoicePaymentList();
            ipl.IPLst = dataPaymentInvoices;

            return View(ipl);
        }


        [HttpPost]
        public ActionResult Index(InvoicePaymentList ipl)
        {



            foreach (var ipLstItem in ipl.IPLst.Where(q=>q.InvoicePaymentAmount > 0))
            {
                var dbOldPayments = dbEF.R_Payments
                    .Where(p => p.PaymentStatusId != 3 && p.Id == ipLstItem.PaymentId); // chk ascending / descending

                var PCI_Item = dbEF.R_Invoice
                    .Where(i => !i.FullyPaid && i.Id == ipLstItem.InvoiceId) // when fully paid this has to b updated
                    .Join(dbOldPayments, i => i.CustomerId, p => p.CustomerId, (i, p) => new { I = i, P = p }).FirstOrDefault();

                InvoicePayment invoicePayment = new InvoicePayment();
                invoicePayment.PaymentId = PCI_Item.P.Id;
                invoicePayment.InvoiceId = PCI_Item.I.Id;
                invoicePayment.ReceiptNo = PCI_Item.P.ReceiptNo;
                invoicePayment.InvoiceNumber = PCI_Item.I.InvoiceNumber;
                invoicePayment.CustomerId = PCI_Item.P.CustomerId;
                invoicePayment.CustomerName = dbEF.R_Customer.Where(c => c.Id == PCI_Item.P.CustomerId).Select(n => n.CustomerName).FirstOrDefault();
                invoicePayment.InvoiceAmount = (PCI_Item.I.Amount == null) ? 0 : (decimal)PCI_Item.I.Amount;
                invoicePayment.PaymentAmount = (PCI_Item.P.Amount == null) ? 0 : (decimal)PCI_Item.P.Amount;

                var invoicePaymentEF = dbEF.R_InvoicePayment
                    .Where(p => p.PaymentId == PCI_Item.P.Id && p.InvoiceId == PCI_Item.I.Id)
                    .FirstOrDefault();

                bool invoicePaymentEFRecExists = (invoicePaymentEF != null);
                if (!invoicePaymentEFRecExists)
                {
                    invoicePaymentEF = new R_InvoicePayment();
                }

                //invoicePayment.InvoicePaymentAmount = (invoicePaymentEF == null) ? 0 : invoicePaymentEF.InvoicePaymentAmount;
                invoicePayment.InvoicePaymentAmount = ipLstItem.InvoicePaymentAmount;
                invoicePayment.RemainingPaymentAmount = (!invoicePaymentEFRecExists) ? invoicePayment.PaymentAmount : invoicePaymentEF.RemainingPaymentAmount;
                invoicePayment.RemainingInvoiceAmount = (!invoicePaymentEFRecExists) ? invoicePayment.InvoiceAmount : invoicePaymentEF.RemainingInvoiceAmount;
                invoicePayment.PaymentFullyUsed = (!invoicePaymentEFRecExists) ? false : invoicePaymentEF.PaymentFullyUsed;
                invoicePayment.InvoiceFullyPaid = (!invoicePaymentEFRecExists) ? false : invoicePaymentEF.InvoiceFullyPaid;


                // get invoicePayment --> depending on existence, Add or Modify


                var invoiceEF = dbEF.R_Invoice
                            .Where(q => q.Id == invoicePayment.InvoiceId).FirstOrDefault();

                var paymentEF = dbEF.R_Payments
                                        .Where(q => q.Id == invoicePayment.PaymentId).FirstOrDefault();

                var customerEF = dbEF.R_Customer
                                        .Where(q => q.Id == invoicePayment.CustomerId).FirstOrDefault();

                if (invoicePayment.RemainingPaymentAmount < invoicePayment.InvoicePaymentAmount)
                {
                    ModelState.AddModelError("InvoicePaymentAmount", "Invoice Payment Amount Cannot be more than the Remaining Payment Amount.");
                    
                    return View(invoicePayment);
                }



                var cal = new InvoicePaymentCalcValues().GetAmountChanges(invoicePayment, invoicePaymentEF, 
                        invoiceEF, paymentEF, customerEF, invoicePaymentEFRecExists);

                if (ModelState.IsValid)
                {
                    if (!invoicePaymentEFRecExists)
                    {
                        dbEF.R_InvoicePayment.Add(invoicePaymentEF);
                    }
                    else
                    {
                        dbEF.Entry(invoicePaymentEF).State = EntityState.Modified;
                    }
                    dbEF.Entry(invoiceEF).State = EntityState.Modified;
                    dbEF.Entry(paymentEF).State = EntityState.Modified;
                    dbEF.Entry(customerEF).State = EntityState.Modified;

                    dbEF.SaveChanges();
                    //return RedirectToAction("Index");
                }


            }

            return RedirectToAction("Index");

        }



        public ActionResult Index0()
        {

            List<InvoicePayment> dataPaymentInvoices = new List<InvoicePayment>();

            var dbOldPayments = dbEF.R_Payments
                    .Where(p => p.PaymentStatusId != 3) // chk whether 3 = Settled
                    .OrderBy(q => q.PaidDate); // chk ascending / descending

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

        //[HttpPost]

        public ActionResult Index00(InvoicePayment invoicePayment)
        {
            var x = invoicePayment;

            return View(invoicePayment);
        }


        public ActionResult Edit(int PaymentId, int InvoiceId)
        {
            // [Bind(Include = "PaymentId,InvoiceId,ReceiptNo,CustomerId,CustomerName,InvoiceNumber,PaymentAmount,InvoiceAmount,InvoicePaymentAmount,RemainingPaymentAmount,RemainingInvoiceAmount,PaymentFullyUsed,InvoiceFullyPaid,TimeStamp")] InvoicePayment ip)
            //  int PaymentId, int InvoiceId, string ReceiptNo, int CustomerId, string CustomerName,
            //        string InvoiceNumber, decimal? PaymentAmount, decimal? InvoiceAmount, decimal? InvoicePaymentAmount,
            //        decimal? RemainingPaymentAmount, decimal? RemainingInvoiceAmount, bool PaymentFullyUsed, bool InvoiceFullyPaid)

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
            if (invoicePayment.PaymentAmount < invoicePayment.InvoicePaymentAmount)
            {
                ModelState.AddModelError("InvoicePaymentAmount", "Invoice Payment Amount Cannot be more than the Payment Amount.");
                return View(invoicePayment);
            }

            // get invoicePayment --> depending on existence, Add or Modify
            var invoicePaymentEF = dbEF.R_InvoicePayment
                                    .Where(q => q.PaymentId == invoicePayment.PaymentId && q.InvoiceId == invoicePayment.InvoiceId).FirstOrDefault();

            var invoiceEF = dbEF.R_Invoice
                        .Where(q => q.Id == invoicePayment.InvoiceId).FirstOrDefault();

            var paymentEF = dbEF.R_Payments
                                    .Where(q => q.Id == invoicePayment.PaymentId).FirstOrDefault();

            var customerEF = dbEF.R_Customer
                                    .Where(q => q.Id == invoicePayment.CustomerId).FirstOrDefault();

            var o = new InvoicePaymentCalcValues();

           //// var cal = o.GetAmountChanges(invoicePayment, invoicePaymentEF, invoiceEF, paymentEF, customerEF, false);

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

            if (ModelState.IsValid)
            {
                dbEF.Entry(invoicePayment).State = EntityState.Modified;
                dbEF.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(invoicePayment);
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
                // .
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
                // .
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


    public class InvoicePaymentCalcValues
    {
        private RanSanDBEntities dbEF = new RanSanDBEntities();

        //public int InvoiceId { get; set; }
        //public int PaymentId { get; set; }

        //public decimal RemainingPaymentAmount { get; set; }
        //public decimal RemainingInvoiceAmount { get; set; }

        //public bool PaymentFullyUsed { get; set; }

        //public decimal InvoiceAmountPaid { get; set; }
        //public bool InvoiceFullyPaid { get; set; }

        //public decimal PaymentAmountPaid { get; set; }
        //public int PaymentStatusId { get; set; }
        //public decimal CustomerBalance { get; set; }
        public InvoicePayment InvoicePaymentVM { get; set; }

        public R_InvoicePayment InvoicePaymentEF { get; set; }
        public R_Invoice InvoiceEF { get; set; }
        public R_Payments PaymentEF { get; set; }
        public R_Customer CustomerEF { get; set; }

        public InvoicePaymentCalcValues GetAmountChanges(InvoicePayment invoicePayment, R_InvoicePayment invoicePaymentEF, 
            R_Invoice invoice, R_Payments payment, R_Customer customer, bool invoicePaymentEFRecExists)
        {
            InvoicePaymentCalcValues rtnCalc = new InvoicePaymentCalcValues();

            if (!invoicePaymentEFRecExists)
            {
                invoicePaymentEF.PaymentId = invoicePayment.PaymentId;
                invoicePaymentEF.InvoiceId = invoicePayment.InvoiceId;
                invoicePaymentEF.InvoicePaymentAmount = invoicePayment.InvoicePaymentAmount;
            }

            var invoiceAmountPaid_Existing = (invoice.AmountPaid == null) ? 0 : (decimal)invoice.AmountPaid;
            //invoice.AmountPaid ==> Accumilated Total Payment
            //invoice.AmountPaid = (invoicePaymentEF.InvoiceFullyPaid) ? invoice.Amount : invoiceAmountPaid_Existing + invoicePayment.InvoicePaymentAmount;
            //invoice.FullyPaid = (invoicePaymentEF.InvoiceFullyPaid);
            invoice.AmountPaid = (invoiceAmountPaid_Existing + invoicePayment.InvoicePaymentAmount > invoice.Amount) ? invoice.Amount : invoiceAmountPaid_Existing + invoicePayment.InvoicePaymentAmount;
            invoice.FullyPaid = (invoice.Amount == invoice.AmountPaid);

            var paymentAmountPaid_Existing = (payment.AmountPaid == null) ? 0 : (decimal)payment.AmountPaid;
            //payment.AmountPaid = (invoicePaymentEF.PaymentFullyUsed) ? payment.Amount : paymentAmountPaid_Existing + invoicePayment.InvoicePaymentAmount;
            //payment.PaymentStatusId = (invoicePaymentEF.PaymentFullyUsed) ? 3 : (invoicePayment.InvoicePaymentAmount == 0) ? 1 : 2;
            
            // no need to chk whether Remaining < payment, as it is checked earlier and raised an exception
            payment.AmountPaid = paymentAmountPaid_Existing + invoicePayment.InvoicePaymentAmount; 
            payment.PaymentStatusId = (payment.Amount == payment.AmountPaid) ? 3 : 2;


            var cusBal = (customer.Balance == null) ? 0 : (decimal)customer.Balance;
            customer.Balance = cusBal - invoicePayment.InvoicePaymentAmount;

            invoicePaymentEF.RemainingPaymentAmount = invoicePayment.RemainingPaymentAmount - invoicePayment.InvoicePaymentAmount;
            invoicePaymentEF.RemainingInvoiceAmount = (invoicePayment.RemainingInvoiceAmount < invoicePayment.InvoicePaymentAmount) ? 0 : invoicePayment.RemainingInvoiceAmount - invoicePayment.InvoicePaymentAmount;
            invoicePaymentEF.PaymentFullyUsed = (invoicePaymentEF.RemainingPaymentAmount == 0);
            invoicePaymentEF.InvoiceFullyPaid = (invoicePaymentEF.RemainingInvoiceAmount == 0);
            invoicePaymentEF.TimeStamp = DateTime.Now;
            // Invoice : Amount Paid, Fully Paid
            // Payment : AmountPaid   Status
            // Customer: Balance  

            /*
             Id	PaymentStatus
            1	Pending
            2	Partially Paid
            3	Settled
            */
            return rtnCalc;


        }




    }

}
