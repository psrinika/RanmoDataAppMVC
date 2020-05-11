using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RanmoDB;
using RanmoDataAppMVC.Models;


namespace RanmoDataAppMVC.Controllers
{
    public class ProdTestController : Controller
    {
        private RanSanDBEntities db = new RanSanDBEntities();

        // GET: ProductionDatas
        public ActionResult Index()
        {
            int numberOfRecs = 10;

            ViewBag.startDate = new DateTime(2020, 1, 1).ToShortDateString();
            ViewBag.endDate = DateTime.Today.ToShortDateString();
            List<Models.Production> prodRecList = ConvertProdRecListFromDB(db.ProductionDatas);
            var data = prodRecList.OrderByDescending(q => q.DateAdded).OrderBy(q => q.FromTime).Take(numberOfRecs).ToList();

            var downTimeReason = db.DownTimeReasons.ToList();
            ViewBag.DownTimeReasonList = new SelectList(downTimeReason);

            return View(data);
        }

        private Models.Production ConvertProdRecFromDB(ProductionData pd)
        {
            Production pr = new Production();
            pr.Id = pd.Id;
            pr.WorkStartDate = pd.WorkStartDate;
            pr.ShiftDN = pd.ShiftDN;
            pr.MachineId = pd.MachineId;
            pr.MachineName = db.Machines.Where(q => q.Id == pd.MachineId).Select(q => q.MachineName).FirstOrDefault();
            pr.ItemId = pd.ItemId;
            pr.ItemName = db.Items.Where(q => q.Id == pd.ItemId).Select(q => q.ItemFor + " - " + q.ItemName).FirstOrDefault();
            pr.ItemObj = ConvertItemFromDB(db.Items.Where(q => q.Id == pd.ItemId).FirstOrDefault());

            pr.EmployeeId = pd.EmployeeId;
            pr.EmployeeName = db.Employees.Where(q => q.Id == pd.EmployeeId).Select(q => q.EmpNo + " - " + q.EmpName).FirstOrDefault();
            pr.FromTime = pd.FromTime;
            pr.MachineCounterFrom = pd.MachineCounterFrom;
            pr.FromTime = pd.FromTime;
            pr.MachineCounterFrom = pd.MachineCounterFrom;
            pr.ToTime = pd.ToTime;
            pr.MachineCounterTo = pd.MachineCounterTo;
            pr.TotalDamaged = pd.TotalDamaged;
            pr.DownTimeMinutes = pd.DownTimeMinutes;
            pr.DownTimeReason = pd.DownTimeReason;
//            pr.DamagesBreakdownAndNotes = getDamagesBreakdownAndNotes(pd.Id, pd.Notes);
            pr.DateAdded = pd.Timestamp;
            //pr.ProductionRejectList = GetProductionRejectList(pd.Id);
            return pr;
        }

        private List<Models.Production> ConvertProdRecListFromDB(DbSet<ProductionData> pdl)
        {
            List<Models.Production> prodRecList = new List<Production>();
            foreach (var item in pdl)
            {
                Production pr = ConvertProdRecFromDB(item);
                prodRecList.Add(pr);
            }
            return prodRecList;
        }

        private Models.Item ConvertItemFromDB(RanmoDB.Item item)
        {
            Models.Item rtnItem = new Models.Item();
            rtnItem.Id = item.Id;
            rtnItem.ItemFor = item.ItemFor;
            rtnItem.ItemName = item.ItemName;
            rtnItem.IsActive = item.IsActive;
            return rtnItem;
        }


        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductionData productionData = db.ProductionDatas.Find(id);
            if (productionData == null)
            {
                return HttpNotFound();
            }

            SelectList itemsFor = new SelectList(db.ForItems.ToList(), "ItemFor", "ItemFor");
            ViewBag.ItemForList = itemsFor;

            SelectList machines = new SelectList(db.Machines.ToList(), "Id", "MachineName");
            ViewBag.MachineNames = machines;

            var shiftList = ShiftTimeConvertList(db.ShiftTimes).Select(q => new { q.Id, FromTime = q.FromToTime }).ToList(); // .ToString(@"hh\:mm") 

            SelectList fromToTime = new SelectList(shiftList, "FromTime", "FromTime");
            ViewBag.FromToTimeList = fromToTime;

            var prodId = (id == null) ? 0 : (int)id;
            ViewBag.ProductionRejectList = GetProductionRejectList(prodId);


            var data = ConvertProdRecFromDB(productionData);

            return View(data);
        }

        private List<Models.ProductionReject> GetProductionRejectList(int id)
        {
            List<Models.ProductionReject> rtnList = new List<Models.ProductionReject>();
            var prList = from pr in db.ProductionRejects
                         join rr in db.RejectReasons
                         on pr.RejectReasonId equals rr.Id
                         where pr.ProductionId == id
                         select new { pr.Id, pr.ProductionId, pr.RejectReasonId, rr.RejectReasonName, pr.NumberOfRejects };

            foreach (var item in prList)
            {
                Models.ProductionReject prItem = new Models.ProductionReject();
                prItem.Id = item.Id;
                prItem.ProductionId = item.ProductionId;
                prItem.RejectReasonId = item.RejectReasonId;
                prItem.RejectReasonName = item.RejectReasonName;
                prItem.NumberOfRejects = item.NumberOfRejects;
                rtnList.Add(prItem);
            }
            return rtnList;
        }


        private List<Models.ShiftTime> ShiftTimeConvertList(DbSet<RanmoDB.ShiftTime> shiftTimeToConvert)
        {
            List<Models.ShiftTime> rtnST = new List<Models.ShiftTime>();
            foreach (var item in shiftTimeToConvert)
            {
                rtnST.Add(ShiftTimeConvert(item));
            }

            return rtnST;
        }

        private Models.ShiftTime ShiftTimeConvert(RanmoDB.ShiftTime shiftTimeToConvert)
        {
            Models.ShiftTime rtnST = new Models.ShiftTime();
            rtnST.Id = shiftTimeToConvert.Id;
            rtnST.FromToTime = shiftTimeToConvert.FromToTime;
            //           rtnST.FromToTime =  (new DateTime() + shiftTimeToConvert.FromToTime).ToString("HH\\:mm");

            return rtnST;
        }

        public JsonResult GetItems(string term = "", string itemFor = "", int activeStatus = -1)
        {
            int isActive = activeStatus;

            var objCustomerlist = db.Items
                            .Where(c => c.ItemName.ToUpper().Contains(term.ToUpper()) &&
                                (itemFor == "" || itemFor == "Any" || c.ItemFor == itemFor) &&
                                (c.IsActive == (activeStatus == 1) || activeStatus == -1))
                            .Select(c => new { ItemName = c.ItemFor + " - " + c.ItemName, ItemId = c.Id })
                            .Distinct().ToList();
            return Json(objCustomerlist, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEmployees(string term = "")
        {
            var objCustomerlist = db.Employees
                            .Where(c => c.EmpName.ToUpper()
                            .Contains(term.ToUpper()))
                            .Select(c => new { EmployeeName = c.EmpNo + " - " + c.EmpName, EmployeeId = c.Id })
                            .Distinct().ToList();
            return Json(objCustomerlist, JsonRequestBehavior.AllowGet);
        }

        // POST: ProductionDatas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Timestamp,WorkStartDate,ShiftDN,MachineId,ItemId,EmployeeId,FromTime,MachineCounterFrom,ToTime,MachineCounterTo,TotalDamaged,DownTimeMinutes,DownTimeReason,Notes")] ProductionData productionData)
        {
            productionData.Timestamp = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(productionData).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productionData);
        }


        private RanmoDB.ProductionReject ConvertProductionRejectFromDB(Models.ProductionReject  pr)
        {
            RanmoDB.ProductionReject rtnItem = new RanmoDB.ProductionReject();
            rtnItem.Id = pr.Id;
            rtnItem.ProductionId = pr.ProductionId;
            rtnItem.RejectReasonId = pr.RejectReasonId;
            rtnItem.NumberOfRejects = pr.NumberOfRejects;
            return rtnItem;
        }

        [HttpPost]
        public ActionResult ProdRejSave([Bind(Include = "Id,ProductionId,RejectReasonId,NumberOfRejects")] Models.ProductionReject prodRejRsn)
        {

            var prodRejRsnDB = ConvertProductionRejectFromDB(prodRejRsn);

            if (ModelState.IsValid)
            {
                db.ProductionRejects.Add(prodRejRsnDB);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(Json(prodRejRsn));
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
