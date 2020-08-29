using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RanmoDB;
using RanmoDataAppMVC.Models;

namespace RanmoDataAppMVC.Controllers
{
    public class zzzProductionWithAutoCompleteController : Controller
    {
        private RanSanDBEntities db = new RanSanDBEntities();
        // GET: ProductionWithAutoComplete
        public ActionResult Index()
        {
            int numberOfRecs = 10;

            ViewBag.startDate = new DateTime(2020, 1, 1).ToShortDateString();
            ViewBag.endDate = DateTime.Today.ToShortDateString();
            List<Models.Production> prodRecList = ConvertProdRecListFromDB(db.ProductionDatas);
            var data = prodRecList.OrderByDescending(q => q.DateAdded).OrderBy(q => q.FromTime).Take(numberOfRecs).ToList();

            var downTimeReason = db.DownTimeReasons.ToList();
            ViewBag.DownTimeReasonList = new SelectList(downTimeReason);

            

            return View(data);
        }

        private List<Models.Production> ConvertProdRecListFromDB(System.Data.Entity.DbSet<ProductionData> pdl)
        {
            List<Models.Production> prodRecList = new List<Models.Production>();
            foreach (var item in pdl)
            {
                Models.Production pr = ConvertProdRecFromDB(item);
                prodRecList.Add(pr);
            }
            return prodRecList;
        }

        private Models.Production ConvertProdRecFromDB(ProductionData pd)
        {
            Models.Production pr = new Models.Production();
            pr.Id = pd.Id;
            pr.WorkStartDate = pd.WorkStartDate;
            pr.ShiftDN = pd.ShiftDN;
            pr.MachineId = pd.MachineId;
            pr.MachineName = db.Machines.Where(q => q.Id == pd.MachineId).Select(q => q.MachineName).FirstOrDefault();
            pr.ItemId = pd.ItemId;
            pr.ItemName = db.Items.Where(q => q.Id == pd.ItemId).Select(q => q.ItemFor + " - " + q.ItemName).FirstOrDefault();
            pr.ItemObj = ConvertItemFromDB(db.Items.Where(q => q.Id == pd.ItemId).FirstOrDefault());

            pr.EmployeeId = pd.EmployeeId;
            pr.EmployeeName = db.Employees.Where(q => q.Id == pd.EmployeeId).Select(q => q.EmpNo + " - " + q.EmpName).FirstOrDefault();
            pr.FromTime = pd.FromTime;
            pr.MachineCounterFrom = pd.MachineCounterFrom;
            pr.FromTime = pd.FromTime;
            pr.MachineCounterFrom = pd.MachineCounterFrom;
            pr.ToTime = pd.ToTime;
            pr.MachineCounterTo = pd.MachineCounterTo;
            pr.TotalDamaged = pd.TotalDamaged;
            pr.DownTimeMinutes = pd.DownTimeMinutes;
            pr.DownTimeReason = pd.DownTimeReason;
            pr.Notes = pd.Notes;
            pr.DamagesBreakdownAndNotes = "";
            pr.DateAdded = pd.Timestamp;

            return pr;
        }

        private Models.Item ConvertItemFromDB(RanmoDB.Item item)
        {
            Models.Item rtnItem = new Models.Item();
            rtnItem.Id = item.Id;
            rtnItem.ItemFor = item.ItemFor;
            rtnItem.ItemName = item.ItemName;
            rtnItem.IsActive = item.IsActive;
            return rtnItem;
        }

        public JsonResult GetItems(string term = "")
        {
            var objCustomerlist = db.Items
                            .Where(c => c.ItemName.ToUpper()
                            .Contains(term.ToUpper()))
                            .Select(c => new { ItemName = c.ItemFor + " - " + c.ItemName, c.Id })
                            .Distinct().ToList();
            return Json(objCustomerlist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Test(int Id)
        {

            var items = db.Items.Where(c => c.Id == Id).FirstOrDefault();
            return View("Index", items);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductionData productionData = db.ProductionDatas.Find(id);
            if (productionData == null)
            {
                return HttpNotFound();
            }

            //var machines = db.Machines.ToList();
            //ViewBag.Machines = new SelectList(machines);

            SelectList machines = new SelectList(db.Machines.ToList(), "Id", "MachineName");
            ViewBag.MachineNames = machines;

            var data = ConvertProdRecFromDB(productionData);


            return View(data);
        }
    }
}
/*
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
    public class InvoicesController : Controller
    {
        private ApplicationDbContext dbMV = new ApplicationDbContext();
        private RanSanDBEntities dbEF = new RanSanDBEntities();

        public ActionResult Index()
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
            });
            return View(dbData);
        }


        [HttpPost]
        public ActionResult Index(int? noOfRecs, string invoiceNumberContains, int? customerId, string notesContains,
                int? fullyPaid, decimal? amountFrom, decimal? amountTo, DateTime invFromDate, DateTime invToDate)
        {
            int numberOfRecs = (noOfRecs == null || noOfRecs == 0 || noOfRecs > 1000) ? 10 : (int)noOfRecs;
            int custId = (customerId == null) ? 0 : (int)customerId;
            decimal amtFrom = (amountFrom == null) ? 0 : (decimal)amountFrom;
            decimal amtTo = (amountTo == null) ? 10000000 : (decimal)amountTo;
            int fullyPd = (fullyPaid == null) ? -1 : (int)fullyPaid;
            bool fp = (fullyPd == 1);
            DateTime invFromDt = (invFromDate == null) ? DateTime.Today.AddDays(-365) : (DateTime)invFromDate;
            DateTime invToDt = (invToDate == null) ? DateTime.Today.AddDays(100) : (DateTime)invToDate;

            var dataInvNoFiltered = dbEF.R_Invoice
                        .Where(q => (q.InvoiceNumber.ToLower().Contains(invoiceNumberContains.ToLower()) || invoiceNumberContains == null || invoiceNumberContains.Trim() == string.Empty)
                        && (customerId == null || q.CustomerId == custId)
                        && (q.Notes.Contains(notesContains) || notesContains == null || notesContains.Trim() == string.Empty)
                        && (q.Amount >= amtFrom && q.Amount <= amtTo)
                        && (fullyPd == -1 || q.FullyPaid == fp)
                        && (q.InvoiceDate >= invFromDt && q.InvoiceDate <= invToDt))
                        .OrderBy(q => q.InvoiceDate)
                        .Take(numberOfRecs);


            var dataSorted = dataInvNoFiltered
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

            return View(dataSorted);
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
        public ActionResult Create([Bind(Include = "Id,InvoiceNumber,CustomerId,Amount,InvoiceDate,FullyPaid,Notes,AmountPaid,TimeStamp")] Invoice invoice)
        {
            var i = invoice.ConvertVwModelToDB(invoice);
            invoice.AmountPaid = 0;
            var customerId = invoice.CustomerId;
            var amount = invoice.Amount;
            var customer = GetCustomerBalance(customerId, amount, 0);

            if (ModelState.IsValid)
            {
                try
                {
                    dbEF.R_Invoice.Add(i);
                    dbEF.Entry(customer).State = EntityState.Modified;
                    dbEF.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception excp)
                {
                    var errMsg = excp.InnerException.InnerException.Message;
                    if (errMsg.Contains("Violation of UNIQUE KEY constraint"))
                    {
                        ModelState.AddModelError("InvoiceNumber", "Invoice Number already exist.");
                    }
                    else
                    {
                        ModelState.AddModelError("InvoiceNumber", "Error occured");
                    }
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
            var amountNew = invoice.Amount;
            var invoiceEF = dbEF.R_Invoice.Where(q => q.Id == invoice.Id).FirstOrDefault();
            var amountOld = (invoiceEF.Amount == null)? 0: invoiceEF.Amount;
            var customer = GetCustomerBalance(invoice.CustomerId, amountOld, amountNew);

            if (ModelState.IsValid)
            {
                try
                {
                    dbEF.Entry(invoiceDB).State = EntityState.Modified;
                    dbEF.Entry(customer).State = EntityState.Modified;
                    dbEF.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception excp)
                {
                    var errMsg = excp.InnerException.InnerException.Message;
                    if (errMsg.Contains("Violation of UNIQUE KEY constraint"))
                    {
                        ModelState.AddModelError("InvoiceNumber", "Invoice Number already exist.");
                    }
                    else
                    {
                        ModelState.AddModelError("InvoiceNumber", "Error occured");
                    }
                }

            }
            var custs = dbEF.R_Customer.Select(q => new { q.Id, q.CustomerName });
            invoice.CustomersList = new SelectList(custs, "Id", "CustomerName");

            return View(invoice);
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
            var amount = (invoice.Amount == null) ? 0 : (decimal)invoice.Amount;
            var customer = GetCustomerBalance((int)invoice.CustomerId, -amount, 0);
            dbEF.R_Invoice.Remove(invoice);
            dbEF.Entry(customer).State = EntityState.Modified;
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

        public R_Customer GetCustomerBalance(int? customerId, decimal? oldInvoiceAmount, decimal? newInvoiceAmount) // , decimal oldPayment, decimal newPayment
        {
            var customer = dbEF.R_Customer.Where(q => q.Id == customerId).FirstOrDefault();
            var customerCurrentBalance = (customer.Balance == null) ? 0 : customer.Balance;
            var newBalance = customerCurrentBalance + (newInvoiceAmount - oldInvoiceAmount);
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


*/
