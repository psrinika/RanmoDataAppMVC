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
    public class PaymentStatusController : Controller
    {
        private ApplicationDbContext dbMV = new ApplicationDbContext();
        private RanSanDBEntities dbEF = new RanSanDBEntities();

        public ActionResult Index()
        {
            var dbData = dbEF.R_PaymentStatus.Select(q => new PaymentStatus { Id = q.Id, Payment_Status = q.PaymentStatus });
            //var psl = dbData.ToList();
            PaymentStatusList psl = new PaymentStatusList();
            psl.psLst = dbData.ToList();

            return View(psl);
        }


        [HttpPost]
        public ActionResult Index(PaymentStatusList m)
        {
            var dbData = m;
            return View(dbData);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Payment_Status")] PaymentStatus paymentStatus)
        {
            var ps = paymentStatus.ConvertVwModelToDB(paymentStatus);
            if (ModelState.IsValid)
            {
                dbEF.R_PaymentStatus.Add(ps);
                dbEF.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(paymentStatus);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var paymentStatusVM = new PaymentStatus().ConvertDBToVwModel(id);
            if (paymentStatusVM == null)
            {
                return HttpNotFound();
            }
            return View(paymentStatusVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Payment_Status")] PaymentStatus paymentStatus)
        {
            var ps = paymentStatus.ConvertVwModelToDB(paymentStatus);
            if (ModelState.IsValid)
            {
                dbEF.Entry(ps).State = EntityState.Modified;
                dbEF.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(paymentStatus);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var paymentStatusVM = new PaymentStatus().ConvertDBToVwModel(id);
            if (paymentStatusVM == null)
            {
                return HttpNotFound();
            }
            return View(paymentStatusVM);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var paymentStatusEF = new PaymentStatus().ConvertVwModelToDB(id);
            dbEF.R_PaymentStatus.Attach(paymentStatusEF);
            dbEF.R_PaymentStatus.Remove(paymentStatusEF);
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
