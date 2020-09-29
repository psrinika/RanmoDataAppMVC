using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RanmoDataAppMVC.Models;
using RanmoDB;

namespace RanmoDataAppMVC.Controllers
{
    public class ProductionRejectsController : Controller
    {
        private RanSanDBEntities db = new RanSanDBEntities();

        // GET: ProductionRejects
        public ActionResult Index(int? productionId)
        {
            var prodId = (productionId == null) ? 0 : (int)productionId;

            ViewBag.ProductionId = prodId;

            var data = db.ProductionRejects.Where(q => q.ProductionId == prodId).ToList();
            var convertedData = ConvertDBtoModelList(data);
            return View(convertedData);
        }


        private List<Models.ProductionReject> ConvertDBtoModelList(List<RanmoDB.ProductionReject> db_prList)
        {
            List<Models.ProductionReject> mod_prList = new List<Models.ProductionReject>();
            foreach (var db_pr in db_prList)
            {
                Models.ProductionReject mod_pr = ConvertDBtoModel(db_pr);
                mod_prList.Add(mod_pr);
            }

            return mod_prList;
        }

        private Models.ProductionReject ConvertDBtoModel(RanmoDB.ProductionReject db_pr)
        {
            Models.ProductionReject mod_pr = new Models.ProductionReject();
            mod_pr.Id = db_pr.Id;
            mod_pr.ProductionId = db_pr.ProductionId;
            mod_pr.RejectReasonId = db_pr.RejectReasonId;
            mod_pr.RejectReasonName = db.RejectReasons.Where(q => q.Id == db_pr.RejectReasonId).Select(q => q.RejectReasonName).FirstOrDefault();
            mod_pr.NumberOfRejects = db_pr.NumberOfRejects;

            return mod_pr;
        }


        // GET: ProductionRejects/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ProductionReject productionReject = db.ProductionRejects.Find(id);
        //    if (productionReject == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(productionReject);
        //}

        // GET: ProductionRejects/Create
        public ActionResult Create(int? productionId)
        {
            SelectList rejectReasons = new SelectList(db.RejectReasons.ToList(), "Id", "RejectReasonName");
            ViewBag.RejectReasonList = rejectReasons;

            return View();
        }

        // POST: ProductionRejects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // .
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ProductionId,RejectReasonId,RejectReasonName,NumberOfRejects")] Models.ProductionReject productionReject)
        {
            var productionReject_DB = ConvertModeltoDB(productionReject);

            //if (ModelState.IsValid)
            //{
                db.ProductionRejects.Add(productionReject_DB);
                db.SaveChanges();
            //}

            var prodId = productionReject.ProductionId;

            ViewBag.ProductionId = prodId;

            var data = db.ProductionRejects.Where(q => q.ProductionId == prodId).ToList();
            var convertedData = ConvertDBtoModelList(data);
            return View("Index", convertedData);

        }


        //private List<Models.ProductionReject> ConvertModelListtoDB(List<Models.ProductionReject> mod_prList)
        //{
        //    List<RanmoDB.ProductionReject> db_prList = new List<RanmoDB.ProductionReject>();
        //    foreach (var mod_pr in mod_prList)
        //    {
        //        RanmoDB.ProductionReject db_pr = ConvertModeltoDB(mod_pr);
        //        db_prList.Add(db_pr);
        //    }

        //    return mod_prList;
        //}

        private RanmoDB.ProductionReject ConvertModeltoDB(Models.ProductionReject mod_pr)
        {
            RanmoDB.ProductionReject db_pr = new RanmoDB.ProductionReject();
            db_pr.Id = mod_pr.Id;
            db_pr.ProductionId = mod_pr.ProductionId;
            db_pr.RejectReasonId = mod_pr.RejectReasonId;
            db_pr.NumberOfRejects = mod_pr.NumberOfRejects;

            return db_pr;
        }

        // GET: ProductionRejects/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var productionReject = db.ProductionRejects.Find(id);
            if (productionReject == null)
            {
                return HttpNotFound();
            }
            var data = ConvertDBtoModel(productionReject);

            SelectList rejectReasons = new SelectList(db.RejectReasons.ToList(), "Id", "RejectReasonName");
            ViewBag.RejectReasonList = rejectReasons;

            return View(data);
        }

        // POST: ProductionRejects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // .
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ProductionId,RejectReasonId,RejectReasonName,NumberOfRejects")] Models.ProductionReject productionReject)
        {
            var productionReject_ForDB = ConvertModeltoDB(productionReject);

            if (ModelState.IsValid)
            {
                db.Entry(productionReject_ForDB).State = EntityState.Modified;
                db.SaveChanges();
                //return RedirectToAction("Index");
            }

            var prodId = productionReject.ProductionId;
            ViewBag.ProductionId = prodId;

            var data = db.ProductionRejects.Where(q => q.ProductionId == prodId).ToList();
            var convertedData = ConvertDBtoModelList(data);
            return View("Index", convertedData);
        }

        // GET: ProductionRejects/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            var productionReject = db.ProductionRejects.Find(id);
            if (productionReject == null)
            {
                return HttpNotFound();
            }
            var data = ConvertDBtoModel(productionReject);

            return View(data);
        }

        // POST: ProductionRejects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RanmoDB.ProductionReject productionReject = db.ProductionRejects.Find(id);
            var prodId = productionReject.ProductionId;

            db.ProductionRejects.Remove(productionReject);
            db.SaveChanges();

            ViewBag.ProductionId = prodId;

            var data = db.ProductionRejects.Where(q => q.ProductionId == prodId).ToList();
            var convertedData = ConvertDBtoModelList(data);
            return View("Index", convertedData);
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
