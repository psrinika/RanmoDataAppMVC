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
    public class CustomersController : Controller
    {
        private ApplicationDbContext dbMV = new ApplicationDbContext();
        private RanSanDBEntities dbEF = new RanSanDBEntities();

        public ActionResult Index_Original()
        {
            var dbData = dbEF.R_Customer.Select(q => new Customer
            {
                Id = q.Id,
                CustomerName = q.CustomerName,
                ContactPerson = q.ContactPerson,
                Address = q.Address,
                Tel = q.Tel,
                Email = q.Email,
                Balance = q.Balance
            });
            return View(dbData);
        }

        public ActionResult Index(int? noOfRecs, string customerNameContains, string contactPersonNameContains, string telNoContains, string emailContains)
        {
            if (noOfRecs == null || noOfRecs == 0 || noOfRecs > 1000)
            {
                noOfRecs = 10;
            }
            int numberOfRecs = (int)noOfRecs;

            var dataCustNameFiltered = dbEF.R_Customer.Where(q => q.CustomerName.ToLower().Contains(customerNameContains.ToLower()) || customerNameContains == null || customerNameContains.Trim() == string.Empty);
            var dataContactPersonFiltered = dataCustNameFiltered.Where(q => q.ContactPerson.ToLower().Contains(contactPersonNameContains.ToLower()) || contactPersonNameContains == null || contactPersonNameContains.Trim() == string.Empty);
            var dataTelNoFiltered = dataContactPersonFiltered.Where(q => q.Tel.Contains(telNoContains) || telNoContains == null || telNoContains.Trim() == string.Empty);
            var dataEmailFiltered = dataTelNoFiltered.Where(q => q.Email.ToLower().Contains(emailContains.ToLower()) || emailContains == null || emailContains.Trim() == string.Empty);

            var data = dataEmailFiltered;
            var dataSorted = data
                .Select(q => new Customer
                {
                    Id = q.Id,
                    CustomerName = q.CustomerName,
                    ContactPerson = q.ContactPerson,
                    Address = q.Address,
                    Tel = q.Tel,
                    Email = q.Email,
                    Balance = q.Balance
                })
                .Take(numberOfRecs);

            return View(dataSorted);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = dbMV.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CustomerName,ContactPerson,Address,Tel,Email,Balance,TimeStamp")] Customer customer)
        {
            var c = customer.ConvertVwModelToDB(customer);

            if (ModelState.IsValid)
            {
                dbEF.R_Customer.Add(c);
                dbEF.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(customer);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = new Customer().ConvertDBToVwModel(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CustomerName,ContactPerson,Address,Tel,Email,Balance,TimeStamp")] Customer customer)
        {
            var c = customer.ConvertVwModelToDB(customer);
            if (ModelState.IsValid)
            {
                dbEF.Entry(c).State = EntityState.Modified;
                dbEF.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var customerVM = new Customer().ConvertDBToVwModel(id);

            if (customerVM == null)
            {
                return HttpNotFound();
            }
            return View(customerVM);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var customerEF = new Customer().ConvertVwModelToDB(id);
            dbEF.R_Customer.Attach(customerEF);
            dbEF.R_Customer.Remove(customerEF);
            dbEF.SaveChanges();
            return RedirectToAction("Index");
        }

        //public decimal GetCustomerBalance(int customerId, decimal oldInvoiceAmount, decimal newInvoiceAmount, decimal oldPayment, decimal newPayment)
        //{

        //    var previousBalance = dbEF.R_Customer.Where(q => q.Id == customerId).Select(q => q.Balance).FirstOrDefault();
        //    var newBalance = previousBalance + (newInvoiceAmount- oldInvoiceAmount) - (newPayment - oldPayment) ;

        //    return (newBalance == null)?0: (decimal)newBalance;
        //}


        public JsonResult GetCustomers(string term = "")
        {
            var objCustomerlist = dbEF.R_Customer
                .Where(q => q.CustomerName.Contains(term))
                .Select(q => new { CustomerId = q.Id, q.CustomerName }).ToList();
            return Json(objCustomerlist, JsonRequestBehavior.AllowGet);
        }

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
