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

        //        public ActionResult Index(int? noOfRecs, string CustomerName, string ContactPerson, string TelNo, string Email)
        public ActionResult Index(string sortOrder, int? page, int? noOfRecs, string CustomerName, int? CustomerId, 
            string ContactPerson, string TelNo, string Email, string Notes, int? Active )
        {
            ViewBag.CustomerNameSortParm = String.IsNullOrEmpty(sortOrder) ? "CustomerName_desc" : "";

            int numberOfRecs = (noOfRecs == null || noOfRecs == 0 || noOfRecs > 1000) ? 10 : (int)noOfRecs;

            CustomerName = (CustomerName == "" || CustomerName == null) ? null : CustomerName.ToLower();
            ContactPerson = (ContactPerson == "" || ContactPerson == null) ? null : ContactPerson.ToLower();
            Email = (Email == "" || Email == null) ? null : Email.ToLower();
            TelNo = (TelNo == "") ? null : TelNo;
            int actv = (Active == null) ? 1 : (int)Active;
            bool active = (actv == 1);
            //CustomerId = (CustomerId == null) ? 0 : CustomerId;

            var dbData = dbEF.R_Customer
                    .Where(q => (CustomerName == null || q.CustomerName.ToLower().Contains(CustomerName))
                    && (CustomerId == null || q.Id == CustomerId)
                    && (ContactPerson == null || q.ContactPerson.ToLower().Contains(ContactPerson))
                    && (Email == null || q.Email.ToLower().Contains(Email))
                    && (TelNo == null || q.Tel.Contains(TelNo))
                    && (Active == -1 || q.Active == active)
                    && (string.IsNullOrEmpty(Notes )|| q.Notes.Contains(Notes))
                    );

            var sortedData = dbData.OrderBy(q => q.CustomerName.Trim());
            switch (sortOrder)
            {
                case "CustomerName_desc":
                    sortedData = dbData.OrderByDescending(q => q.CustomerName.Trim());
                    break;
                default:
                    break;
            }

            ViewBag.NumberOfRecords = numberOfRecs;
            ViewBag.CustomerName = CustomerName;
            ViewBag.CustomerId = CustomerId;
            ViewBag.ContactPerson = ContactPerson;
            ViewBag.TelNo = TelNo;
            ViewBag.Email = Email;
           


            var data = sortedData
                .Select(q => new Customer
                {
                    Id = q.Id,
                    CustomerName = q.CustomerName,
                    ContactPerson = q.ContactPerson,
                    Address = q.Address,
                    Tel = q.Tel,
                    Email = q.Email,
                    Balance = q.Balance
                });
                //.Take(numberOfRecs);

            int pageSize = numberOfRecs;
            int pageNumber = (page ?? 1);

            var x = data.ToList();
            var y = sortedData.ToList();


            return View(data.ToPagedList(pageNumber, pageSize));
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
        public ActionResult Create([Bind(Include = "Id,CustomerName,ContactPerson,Address,Tel,Email,Balance,Active,Notes")] Customer customer)
        {
            customer.Active = true;
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
        public ActionResult Edit([Bind(Include = "Id,CustomerName,ContactPerson,Address,Tel,Email,Balance,Active,Notes")] Customer customer)
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
