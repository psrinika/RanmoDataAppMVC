using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RanmoDB;

namespace RanmoDataAppMVC.Controllers
{
    public class EmployeesController : Controller
    {
        private RanSanDBEntities db = new RanSanDBEntities();

        // GET: Employees
        public ActionResult Index( int? noOfRecs, string empNoContains, string empNameContains, int? activeStatus)
        {
            if (noOfRecs == null || noOfRecs == 0 || noOfRecs > 1000)
            {
                noOfRecs = 10;
            }
            int numberOfRecs = (int)noOfRecs;

            string empNo = empNoContains;
            string empName = empNameContains;
            int isActive = (activeStatus == null ) ? -1: (int)activeStatus ;

            var dataEmpNoFiltered = db.Employees.Where(q => q.EmpNo.ToLower().Contains(empNo.ToLower()) || empNo == null || empNo.Trim() == string.Empty);
            var dataEmpNameFiltered = dataEmpNoFiltered.Where(q => q.EmpName.ToLower().Contains(empName.ToLower()) || empName == null || empName.Trim() == string.Empty);
            var dataisActiveFiltered = dataEmpNameFiltered.Where(q => q.IsActive == (isActive==1) || isActive == -1);
            var data = dataisActiveFiltered;
            var dataSorted = data.OrderByDescending(q => q.DateAdded).Take(numberOfRecs).ToList();

            return View(dataSorted);
        }

        // GET: Employees/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // GET: Employees/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // .
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,EmpNo,EmpName,isActive,DateAdded")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                employee.DateAdded = DateTime.Now;
                db.Employees.Add(employee);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(employee);
        }

        // GET: Employees/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // .
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,EmpNo,EmpName,isActive,DateAdded")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(employee).State = EntityState.Modified;
                employee.DateAdded = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(employee);
        }

        // GET: Employees/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Employee employee = db.Employees.Find(id);
            db.Employees.Remove(employee);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
