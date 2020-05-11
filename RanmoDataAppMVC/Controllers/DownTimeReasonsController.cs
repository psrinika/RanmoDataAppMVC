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
    public class DownTimeReasonsController : Controller
    {
        private RanSanDBEntities db = new RanSanDBEntities();

        // GET: DownTimeReasons
        public ActionResult Index()
        {
            return View(db.DownTimeReasons.ToList());
        }

        // GET: DownTimeReasons/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DownTimeReason downTimeReason = db.DownTimeReasons.Find(id);
            if (downTimeReason == null)
            {
                return HttpNotFound();
            }
            return View(downTimeReason);
        }

        // GET: DownTimeReasons/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DownTimeReasons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DownTimeReasonName")] DownTimeReason downTimeReason)
        {
            if (ModelState.IsValid)
            {
                db.DownTimeReasons.Add(downTimeReason);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(downTimeReason);
        }

        // GET: DownTimeReasons/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DownTimeReason downTimeReason = db.DownTimeReasons.Find(id);
            if (downTimeReason == null)
            {
                return HttpNotFound();
            }
            return View(downTimeReason);
        }

        // POST: DownTimeReasons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,DownTimeReasonName")] DownTimeReason downTimeReason)
        {
            if (ModelState.IsValid)
            {
                db.Entry(downTimeReason).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(downTimeReason);
        }

        // GET: DownTimeReasons/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DownTimeReason downTimeReason = db.DownTimeReasons.Find(id);
            if (downTimeReason == null)
            {
                return HttpNotFound();
            }
            return View(downTimeReason);
        }

        // POST: DownTimeReasons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DownTimeReason downTimeReason = db.DownTimeReasons.Find(id);
            db.DownTimeReasons.Remove(downTimeReason);
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
