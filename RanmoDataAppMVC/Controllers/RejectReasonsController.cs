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
    public class RejectReasonsController : Controller
    {
        private RanSanDBEntities db = new RanSanDBEntities();

        // GET: RejectReasons
        public ActionResult Index()
        {
            return View(db.RejectReasons.ToList());
        }

        // GET: RejectReasons/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RejectReason rejectReason = db.RejectReasons.Find(id);
            if (rejectReason == null)
            {
                return HttpNotFound();
            }
            return View(rejectReason);
        }

        // GET: RejectReasons/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: RejectReasons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // .
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,RejectReasonName")] RejectReason rejectReason)
        {
            if (ModelState.IsValid)
            {
                db.RejectReasons.Add(rejectReason);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(rejectReason);
        }

        // GET: RejectReasons/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RejectReason rejectReason = db.RejectReasons.Find(id);
            if (rejectReason == null)
            {
                return HttpNotFound();
            }
            return View(rejectReason);
        }

        // POST: RejectReasons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // .
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,RejectReasonName")] RejectReason rejectReason)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rejectReason).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(rejectReason);
        }

        // GET: RejectReasons/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RejectReason rejectReason = db.RejectReasons.Find(id);
            if (rejectReason == null)
            {
                return HttpNotFound();
            }
            return View(rejectReason);
        }

        // POST: RejectReasons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RejectReason rejectReason = db.RejectReasons.Find(id);
            db.RejectReasons.Remove(rejectReason);
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
