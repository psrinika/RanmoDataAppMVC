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
    public class PaidBiesController : Controller
    {
        private ApplicationDbContext dbMV = new ApplicationDbContext();
        private RanSanDBEntities dbEF = new RanSanDBEntities();

        public ActionResult Index()
        {
            var dbData = dbEF.R_PaidBy.Select(q => new PaidBy { Id = q.Id, Paid_By = q.PaidBy });
            return View(dbData);
        }


        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Paid_By")] PaidBy paidBy)
        {
            var pb = paidBy.ConvertVwModelToDB(paidBy);
            if (ModelState.IsValid)
            {
                dbEF.R_PaidBy.Add(pb);
                dbEF.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(paidBy);
        }


        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var PaidByVM = new PaidBy().ConvertDBToVwModel(id);
            if (PaidByVM == null)
            {
                return HttpNotFound();
            }
            return View(PaidByVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Paid_By")] PaidBy paidBy)
        {
            var pb = paidBy.ConvertVwModelToDB(paidBy);
            if (ModelState.IsValid)
            {
                dbEF.Entry(pb).State = EntityState.Modified;
                dbEF.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(paidBy);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var paidByVM = new PaidBy().ConvertDBToVwModel(id);
            if (paidByVM == null)
            {
                return HttpNotFound();
            }
            return View(paidByVM);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var paidByEF = new PaidBy().ConvertVwModelToDB(id);
            dbEF.R_PaidBy.Attach(paidByEF);
            dbEF.R_PaidBy.Remove(paidByEF);
            dbEF.SaveChanges();
            return RedirectToAction("Index");
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
