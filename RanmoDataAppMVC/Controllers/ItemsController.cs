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
    public class ItemsController : Controller
    {
        private RanSanDBEntities db = new RanSanDBEntities();

        // GET: Items
        public ActionResult Index(int? noOfRecs, string itemForId, string itemNameContains, int? activeStatus)
        {
            string itemFor =  db.ForItems.Where( q => q.Id.ToString() == itemForId).Select (q=>q.ItemFor).FirstOrDefault();
            string itemName = itemNameContains;
            int isActive = (activeStatus == null) ? -1 : (int)activeStatus;
            if (noOfRecs == null || noOfRecs == 0 || noOfRecs > 1000)
            {
                noOfRecs = 10;
            }

            int numberOfRecs = (int)noOfRecs;

            var dataItemNoFiltered = db.Items.Where(q => q.ItemFor.ToLower().Contains(itemFor.ToLower()) || itemFor == null || itemFor.Trim() == string.Empty);
            var dataItemNameFiltered = dataItemNoFiltered.Where(q => q.ItemName.ToLower().Contains(itemName.ToLower()) || itemName == "Any" || itemName == null || itemName.Trim() == string.Empty);
            var dataisActiveFiltered = dataItemNameFiltered.Where(q => q.IsActive == (isActive == 1) || isActive == -1);
            var data = dataisActiveFiltered.Take(numberOfRecs).ToList();

           // var itemsFor = db.ForItems.ToList();
           // ViewBag.ItemForList = new SelectList(itemsFor);

            SelectList itemsFor = new SelectList(db.ForItems.ToList(), "Id", "ItemFor");
            ViewBag.ItemForList = itemsFor;


            return View(data);
        }

        // GET: Items/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // GET: Items/Create
        public ActionResult Create()
        {
            //var itemsFor = db.ForItems.ToList();
            //ViewBag.ItemForList = new SelectList(itemsFor);

            SelectList itemsFor = new SelectList(db.ForItems.ToList(), "ItemFor", "ItemFor");
            ViewBag.ItemForList = itemsFor;

            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ItemFor,ItemName,FullShotWeight,WithoutRunnerWeight,CycleTime,PerHourTarget,IsActive")] Item item)
        {
            if (ModelState.IsValid)
            {
                db.Items.Add(item);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(item);
        }

        // GET: Items/Edit/5
        public ActionResult Edit(int? id)
        {
            //var itemsFor = db.ForItems.ToList();
            //ViewBag.ItemForList = new SelectList(itemsFor);

            SelectList itemsFor = new SelectList(db.ForItems.ToList(), "ItemFor", "ItemFor");
            ViewBag.ItemForList = itemsFor;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ItemFor,ItemName,FullShotWeight,WithoutRunnerWeight,CycleTime,PerHourTarget,IsActive")] Item item)
        {
            if (ModelState.IsValid)
            {
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        // GET: Items/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Item item = db.Items.Find(id);
            db.Items.Remove(item);
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
