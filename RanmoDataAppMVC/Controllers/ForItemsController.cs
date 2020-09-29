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
    public class ForItemsController : Controller
    {
        private RanSanDBEntities db = new RanSanDBEntities();

        // GET: ForItems
        public ActionResult Index()
        {
            return View(db.ForItems.ToList());
        }

        // GET: ForItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ForItem forItem = db.ForItems.Find(id);
            if (forItem == null)
            {
                return HttpNotFound();
            }
            return View(forItem);
        }

        // GET: ForItems/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ForItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // .
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ItemFor")] ForItem forItem)
        {
            if (ModelState.IsValid)
            {
                db.ForItems.Add(forItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(forItem);
        }

        // GET: ForItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ForItem forItem = db.ForItems.Find(id);
            if (forItem == null)
            {
                return HttpNotFound();
            }
            return View(forItem);
        }

        // POST: ForItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // .
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ItemFor")] ForItem forItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(forItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(forItem);
        }

        // GET: ForItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ForItem forItem = db.ForItems.Find(id);
            if (forItem == null)
            {
                return HttpNotFound();
            }
            return View(forItem);
        }

        // POST: ForItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ForItem forItem = db.ForItems.Find(id);
            db.ForItems.Remove(forItem);
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
