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
    public class ShiftTimesController : Controller
    {
        private RanSanDBEntities db = new RanSanDBEntities();

        // GET: ShiftTimes
        public ActionResult Index()
        {
            return View(db.ShiftTimes.ToList());
        }

        // GET: ShiftTimes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShiftTime shiftTime = db.ShiftTimes.Find(id);
            if (shiftTime == null)
            {
                return HttpNotFound();
            }
            return View(shiftTime);
        }

        // GET: ShiftTimes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ShiftTimes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // .
        [HttpPost]
       // [ValidateAntiForgeryToken] 
        public ActionResult Create([Bind(Include = "Id,FromToTime")] ShiftTime shiftTime)
        {
            if (ModelState.IsValid)
            {
                db.ShiftTimes.Add(shiftTime);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(Json(shiftTime));
        }

        // GET: ShiftTimes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShiftTime shiftTime = db.ShiftTimes.Find(id);
            if (shiftTime == null)
            {
                return HttpNotFound();
            }
            return View(shiftTime);
        }

        // POST: ShiftTimes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // .
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FromToTime")] ShiftTime shiftTime)
        {
            if (ModelState.IsValid)
            {
                db.Entry(shiftTime).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(shiftTime);
        }

        // GET: ShiftTimes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShiftTime shiftTime = db.ShiftTimes.Find(id);
            if (shiftTime == null)
            {
                return HttpNotFound();
            }
            return View(shiftTime);
        }

        // POST: ShiftTimes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ShiftTime shiftTime = db.ShiftTimes.Find(id);
            db.ShiftTimes.Remove(shiftTime);
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
