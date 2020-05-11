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


    public class zzzProductionWithRejItemsController : Controller
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
            Models.Production pr = new Models.Production();
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
            pr.Notes = pd.Notes;
            pr.DamagesBreakdownAndNotes = getDamagesBreakdownAndNotes(pd);
            pr.DateAdded = pd.Timestamp;

            return pr;
        }

        private List<Models.Production> ConvertProdRecListFromDB(DbSet<ProductionData> pdl)
        {
            List<Models.Production> prodRecList = new List<Models.Production>();
            foreach (var item in pdl)
            {
                Models.Production pr = ConvertProdRecFromDB(item);
                prodRecList.Add(pr);
            }
            return prodRecList;
        }


        private List<Models.Production> ConvertProdRecListFromDB_Old()
        {
            List<Models.Production> prodRecList = new List<Models.Production>();
            foreach (var item in db.ProductionDatas)
            {
                Models.Production pr = new Models.Production();
                pr.Id = item.Id;
                pr.WorkStartDate = item.WorkStartDate;
                pr.ShiftDN = item.ShiftDN;
                pr.MachineId = item.MachineId;
                pr.MachineName = db.Machines.Where(q => q.Id == item.MachineId).Select(q => q.MachineName).FirstOrDefault();
                pr.ItemId = item.ItemId;
                pr.ItemName = db.Items.Where(q => q.Id == item.ItemId).Select(q => q.ItemFor + " - " + q.ItemName).FirstOrDefault();
                pr.ItemObj = ConvertItemFromDB(db.Items.Where(q => q.Id == item.ItemId).FirstOrDefault());

                pr.EmployeeId = item.EmployeeId;
                pr.EmployeeName = db.Employees.Where(q => q.Id == item.EmployeeId).Select(q => q.EmpNo + " - " + q.EmpName).FirstOrDefault();
                pr.FromTime = item.FromTime;
                pr.MachineCounterFrom = item.MachineCounterFrom;
                pr.FromTime = item.FromTime;
                pr.MachineCounterFrom = item.MachineCounterFrom;
                pr.ToTime = item.ToTime;
                pr.MachineCounterTo = item.MachineCounterTo;
                pr.TotalDamaged = item.TotalDamaged;
                pr.DownTimeMinutes = item.DownTimeMinutes;
                pr.DownTimeReason = item.DownTimeReason;
                pr.Notes = item.Notes;
                pr.DamagesBreakdownAndNotes = getDamagesBreakdownAndNotes(item);
                pr.DateAdded = item.Timestamp;

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


        [HttpPost]
        public ActionResult FilteredData(DateTime? startDate, DateTime? endDate, string shift, string machineNameContains,
                string itemNameContains, string empNameContains, int? noOfRecs)
        {

            DateTime nullStartDate = new DateTime(2020, 1, 1);
            DateTime nullEndDate = DateTime.Today;

            DateTime startDateShift = (startDate == null) ? nullStartDate : (DateTime)startDate;
            DateTime endDateShift = (endDate == null) ? nullEndDate : (DateTime)endDate;

            ViewBag.startDate = startDateShift.ToShortDateString();
            ViewBag.endDate = endDateShift.ToShortDateString();

            string shiftDN = shift;
            string machineName = machineNameContains.ToLower().Trim();
            string itemName = itemNameContains.ToLower().Trim();
            string empName = empNameContains.ToLower().Trim();
            if (noOfRecs == null || noOfRecs <= 0 || noOfRecs > 1000)
            {
                noOfRecs = 10;
            }
            int numberOfRecs = (int)noOfRecs;


            List<Models.Production> prodRecList = ConvertProdRecListFromDB(db.ProductionDatas);

            var dataDateShiftFiltered = prodRecList.Where(q => q.WorkStartDate >= startDateShift && q.WorkStartDate <= endDateShift);
            var dataShiftFiltered = dataDateShiftFiltered.Where(q => q.ShiftDN == shiftDN);
            var dataMachineNameFiltered = dataShiftFiltered.Where(q => q.MachineName.ToLower().Contains(machineName));
            var dataItemNameFiltered = dataMachineNameFiltered.Where(q => q.ItemName.ToLower().Contains(itemName));
            var dataEmpNameFiltered = dataItemNameFiltered.Where(q => q.EmployeeName.ToLower().Contains(empName));

            var data = dataEmpNameFiltered.OrderByDescending(q => q.DateAdded).OrderBy(q => q.FromTime).Take(numberOfRecs).ToList();

            return View("Index", data);
        }

        private string getDamagesBreakdownAndNotes(ProductionData pd)
        {
            var nl = System.Environment.NewLine;
            var tbl_head = @"<table border='1' cellspacing='0' cellpadding='0' style='width:100%'>";
            var tbl_footer = @"</table>";


            string rtnHTMLTbl = "<tr><td>Total damaged : </td><td>" + pd.TotalDamaged + "</td></tr>" + nl;
            rtnHTMLTbl += "<tr><td>Down Time Reason : </td><td>" + pd.DownTimeReason + "</td></tr>" + nl;
            rtnHTMLTbl += "<tr><td>Down Time Minutes : </td><td>" + pd.DownTimeMinutes + "</td></tr>" + nl;
            rtnHTMLTbl += "<tr><td>Notes : </td><td>" + pd.Notes + "</td></tr>" + nl;
            rtnHTMLTbl = tbl_head + nl + rtnHTMLTbl + nl + tbl_footer;
            return rtnHTMLTbl;
        }


        // GET: ProductionDatas/Details/5
        public ActionResult Details(int? id)
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
            return View(productionData);
        }

        // GET: ProductionDatas/Create
        public ActionResult Create()
        {
            var downTimeReason = db.DownTimeReasons.ToList();
            ViewBag.DownTimeReasonList = new SelectList(downTimeReason);

            return View();
        }

        // POST: ProductionDatas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Timestamp,WorkStartDate,ShiftDN,MachineId,ItemId,EmployeeId,FromTime,MachineCounterFrom,ToTime,MachineCounterTo,TotalDamaged,DownTimeMinutes,DownTimeReason,Notes")] ProductionData productionData)
        {
            var downTimeReason = db.DownTimeReasons.ToList();
            ViewBag.DownTimeReasonList = new SelectList(downTimeReason);

            if (ModelState.IsValid)
            {
                db.ProductionDatas.Add(productionData);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(productionData);
        }

        // GET: ProductionDatas/Edit/5
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

            //var machines = db.Machines.ToList();
            //ViewBag.Machines = new SelectList(machines);

            SelectList machines = new SelectList(db.Machines.ToList(), "Id", "MachineName");
            ViewBag.MachineNames = machines;

            var a = ShiftTimeConvertList(db.ShiftTimes).Select(q => new { q.Id, FromTime = q.FromToTime }).ToList();

            var b = db.ShiftTimes.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.FromToTime.ToString()
            }).ToList();

            //var c = b.Select(q => new { q.Value, Text = q.Text.ToString("{0:hh\\:mm}") });

            SelectList fromToTime = new SelectList(a, "FromTime", "FromTime");
            ViewBag.FromToTimeList = fromToTime;

            SelectList fromToTimeB = new SelectList(a, "FromTime", "FromTime");
            ViewBag.FromToTimeListB = fromToTimeB;



            var data = ConvertProdRecFromDB(productionData);

            return View(data);
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

        public JsonResult GetItems(string term = "")
        {
            var objCustomerlist = db.Items
                            .Where(c => c.ItemName.ToUpper()
                            .Contains(term.ToUpper()))
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
            if (ModelState.IsValid)
            {
                db.Entry(productionData).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productionData);
        }

        // GET: ProductionDatas/Delete/5
        public ActionResult Delete(int? id)
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
            return View(productionData);
        }

        // POST: ProductionDatas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductionData productionData = db.ProductionDatas.Find(id);
            db.ProductionDatas.Remove(productionData);
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