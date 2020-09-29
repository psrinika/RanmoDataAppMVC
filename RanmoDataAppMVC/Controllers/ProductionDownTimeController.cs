
// public class ProductionDownTimeController : Controller
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
    public class ProductionDownTimeController : Controller
    {
        private RanSanDBEntities db = new RanSanDBEntities();

        // GET: ProductionDownTime
        public ActionResult Index(int? productionId)
        {
            var prodId = (productionId == null) ? 0 : (int)productionId;

            ViewBag.ProductionId = prodId;

            var data = db.ProductionDownTimes.Where(q => q.ProductionId == prodId).ToList();
            var convertedData = ConvertDBtoModelList(data);
            return View(convertedData);
        }

        private List<Models.ProductionDownTime> ConvertDBtoModelList(List<RanmoDB.ProductionDownTime> db_pdtList)
        {
            List<Models.ProductionDownTime> mod_pdtList = new List<Models.ProductionDownTime>();
            foreach (var db_pdt in db_pdtList)
            {
                Models.ProductionDownTime mod_pdt = ConvertDBtoModel(db_pdt);
                mod_pdtList.Add(mod_pdt);
            }

            return mod_pdtList;
        }

        private Models.ProductionDownTime ConvertDBtoModel(RanmoDB.ProductionDownTime db_pdt)
        {
            Models.ProductionDownTime mod_pdt = new Models.ProductionDownTime();
            mod_pdt.Id = db_pdt.Id;
            mod_pdt.ProductionId = db_pdt.ProductionId;
            mod_pdt.DownTimeReasonId = db_pdt.DownTimeReasonId;
            mod_pdt.DownTimeReasonName = db.DownTimeReasons.Where(q => q.Id == db_pdt.DownTimeReasonId).Select(q => q.DownTimeReasonName).FirstOrDefault();
            mod_pdt.DownTimeMinutes = db_pdt.DownTimeMinutes;

            return mod_pdt;
        }

        // GET: ProductionDownTime/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ProductionDownTime productionDownTime = db.ProductionDownTimes.Find(id);
        //    if (productionDownTime == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(productionDownTime);
        //}

        // GET: ProductionDownTime/Create
        public ActionResult Create(int? productionId)
        {
            SelectList DownTimeReasons = new SelectList(db.DownTimeReasons.ToList(), "Id", "DownTimeReasonName");
            ViewBag.DownTimeReasonList = DownTimeReasons;

            return View();
        }

        // POST: ProductionDownTime/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // .
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ProductionId,DownTimeReasonId,DownTimeReasonName,DownTimeMinutes")] Models.ProductionDownTime productionDownTime)
        {
            var productionDownTime_DB = ConvertModeltoDB(productionDownTime);


            db.ProductionDownTimes.Add(productionDownTime_DB);
            db.SaveChanges();


            var prodId = productionDownTime.ProductionId;

            ViewBag.ProductionId = prodId;

            var data = db.ProductionDownTimes.Where(q => q.ProductionId == prodId).ToList();
            var convertedData = ConvertDBtoModelList(data);
            return View("Index", convertedData);

        }

        private RanmoDB.ProductionDownTime ConvertModeltoDB(Models.ProductionDownTime mod_pdt)
        {
            RanmoDB.ProductionDownTime db_pdt = new RanmoDB.ProductionDownTime();
            db_pdt.Id = mod_pdt.Id;
            db_pdt.ProductionId = mod_pdt.ProductionId;
            db_pdt.DownTimeReasonId = mod_pdt.DownTimeReasonId;
            db_pdt.DownTimeMinutes = mod_pdt.DownTimeMinutes;

            return db_pdt;
        }

        // GET: ProductionDownTime/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var productionDownTime = db.ProductionDownTimes.Find(id);
            if (productionDownTime == null)
            {
                return HttpNotFound();
            }
            var data = ConvertDBtoModel(productionDownTime);

            SelectList DownTimeReasons = new SelectList(db.DownTimeReasons.ToList(), "Id", "DownTimeReasonName");
            ViewBag.DownTimeReasonList = DownTimeReasons;

            return View(data);
        }

        // POST: ProductionDownTime/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // .
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ProductionId,DownTimeReasonId,DownTimeReasonName,DownTimeMinutes")] Models.ProductionDownTime productionDownTime)
        {
            var productionDownTime_ForDB = ConvertModeltoDB(productionDownTime);

            if (ModelState.IsValid)
            {
                db.Entry(productionDownTime_ForDB).State = EntityState.Modified;
                db.SaveChanges();
                //return RedirectToAction("Index");
            }

            var prodId = productionDownTime.ProductionId;
            ViewBag.ProductionId = prodId;

            var data = db.ProductionDownTimes.Where(q => q.ProductionId == prodId).ToList();
            var convertedData = ConvertDBtoModelList(data);
            return View("Index", convertedData);
        }

        // GET: ProductionDownTime/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            var productionDownTime = db.ProductionDownTimes.Find(id);
            if (productionDownTime == null)
            {
                return HttpNotFound();
            }
            var data = ConvertDBtoModel(productionDownTime);

            return View(data);
        }

        // POST: ProductionDownTime/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RanmoDB.ProductionDownTime productionDownTime = db.ProductionDownTimes.Find(id);
            var prodId = productionDownTime.ProductionId;

            db.ProductionDownTimes.Remove(productionDownTime);
            db.SaveChanges();

            ViewBag.ProductionId = prodId;

            var data = db.ProductionDownTimes.Where(q => q.ProductionId == prodId).ToList();
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
