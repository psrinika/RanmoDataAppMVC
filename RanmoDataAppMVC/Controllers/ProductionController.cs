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


/*
todo list
keep data on postback (refresh) or not in a check box 
Clean - "Day Shift" to "Day" ...
Fill Data of To field and the derived Column of no of items prod
Another Grid ... report to chk whether the # of prod units r acceptable ...
Finish Create / Edit ...

Another View / .... to record, Damaged, ...(multiple)

    Filter by From - To Fileds ( using just From Field range)

Need different sorting .... techniques (better using jquery datatable for that)

----------------
Add to Panel

Encrypt pswd
Add to GITHUB

*/

namespace RanmoDataAppMVC.Controllers
{
    public class ProductionController : Controller
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
            //pr.Notes = pd.Notes;
            pr.DamagesBreakdownAndNotes = getDamagesBreakdownAndNotes(pd.Id, pd.Notes);
            pr.DateAdded = pd.Timestamp;
            //pr.ProductionRejectList = GetProductionRejectList(pd.Id);
            return pr;
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


        private List<Models.ProductionDownTime> GetProductionDownTimeList(int id)
        {
            List<Models.ProductionDownTime> rtnList = new List<Models.ProductionDownTime>();
            var prList = from pr in db.ProductionDownTimes
                         join rr in db.DownTimeReasons
                         on pr.DownTimeReasonId equals rr.Id
                         where pr.ProductionId == id
                         select new { pr.Id, pr.ProductionId, pr.DownTimeReasonId, rr.DownTimeReasonName, pr.DownTimeMinutes };

            foreach (var item in prList)
            {
                Models.ProductionDownTime prItem = new Models.ProductionDownTime();
                prItem.Id = item.Id;
                prItem.ProductionId = item.ProductionId;
                prItem.DownTimeReasonId = item.DownTimeReasonId;
                prItem.DownTimeReasonName = item.DownTimeReasonName;
                prItem.DownTimeMinutes = item.DownTimeMinutes;
                rtnList.Add(prItem);
            }
            return rtnList;
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

        private string getDamagesBreakdownAndNotes(int productionId, string notes)
        {
            string rtnHTMLTbl = string.Empty;

            var rejData = GetProductionRejectList(productionId);
            var downTimeData = GetProductionDownTimeList(productionId);

            if (rejData.Count == 0 && downTimeData.Count == 0 && (notes == null || notes.Trim().Length == 0))
            {
                return rtnHTMLTbl;
            }

            var nl = System.Environment.NewLine;
            var tbl_head = @"<table border='1' cellspacing='0' cellpadding='0' style='width:100%'>";
            var tbl_footer = @"</table>";
            
            if (rejData.Count > 0)
            {
                rtnHTMLTbl += "<tr><td colspan='2'><span  style='font - weight:bold'>Rejected Items</span></td></tr>" + nl;

                foreach (var item in rejData)
                {
                    rtnHTMLTbl += "<tr><td>Reason</td><td>Count</td></tr>" + nl;
                    rtnHTMLTbl += "<tr><td>"+ item.RejectReasonName + "</td><td>" + item.NumberOfRejects + "</td></tr>" + nl;
                }
                rtnHTMLTbl += "<tr><td colspan='2'><span  style='font - weight:bold'>-------------</span></td></tr>" + nl;
            }

            if (downTimeData.Count > 0)
            {
                rtnHTMLTbl += "<tr><td colspan='2'><span  style='font - weight:bold'>Down Time</span></td></tr>" + nl;

                foreach (var item in downTimeData)
                {
                    rtnHTMLTbl += "<tr><td>Reason</td><td>Miutes</td></tr>" + nl;
                    rtnHTMLTbl += "<tr><td>" + item.DownTimeReasonName + "</td><td>" + item.DownTimeMinutes + "</td></tr>" + nl;
                }
                rtnHTMLTbl += "<tr><td colspan='2'><span  style='font - weight:bold'>-------------</span></td></tr>" + nl;
            }
            if (notes == null || notes.Trim().Length > 0)
            {
                rtnHTMLTbl += "<tr><td  colspan='2'>Notes</td></tr>" + nl;
                rtnHTMLTbl += "<tr><td  colspan='2'>"+ notes +"</td></tr>" + nl;
            }

            rtnHTMLTbl += tbl_head + nl + rtnHTMLTbl + nl + tbl_footer;
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
            SelectList itemsFor = new SelectList(db.ForItems.ToList(), "ItemFor", "ItemFor");
            ViewBag.ItemForList = itemsFor;

            SelectList machines = new SelectList(db.Machines.ToList(), "Id", "MachineName");
            ViewBag.MachineNames = machines;

            var shiftList = ShiftTimeConvertList(db.ShiftTimes).Select(q => new { q.Id, FromTime = q.FromToTime  }).ToList(); //.ToString(@"hh\:mm")

            SelectList fromToTime = new SelectList(shiftList, "FromTime", "FromTime");
            ViewBag.FromToTimeList = fromToTime;


            return View();
        }

        // POST: ProductionDatas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Timestamp,WorkStartDate,ShiftDN,MachineId,ItemId,EmployeeId,FromTime,MachineCounterFrom,ToTime,MachineCounterTo,TotalDamaged,DownTimeMinutes,DownTimeReason,Notes")] ProductionData productionData)
        {
            productionData.Timestamp = DateTime.Now;

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

            SelectList itemsFor = new SelectList(db.ForItems.ToList(), "ItemFor", "ItemFor");
            ViewBag.ItemForList = itemsFor;

            SelectList machines = new SelectList(db.Machines.ToList(), "Id", "MachineName");
            ViewBag.MachineNames = machines;

            var shiftList = ShiftTimeConvertList(db.ShiftTimes).Select(q => new { q.Id, FromTime = q.FromToTime}).ToList(); // .ToString(@"hh\:mm") 

            SelectList fromToTime = new SelectList(shiftList, "FromTime", "FromTime");
            ViewBag.FromToTimeList = fromToTime;

            SelectList rejectReasons = new SelectList(db.RejectReasons.ToList(), "Id", "RejectReasonName");
            ViewBag.RejectReasonList = rejectReasons;

            SelectList downTimeReason = new SelectList(db.DownTimeReasons.ToList(), "Id", "DownTimeReasonName");
            ViewBag.DownTimeReasonList = downTimeReason;

            var prodId = (id == null) ? 0 : (int)id;
            ViewBag.ProductionRejectList = GetProductionRejectList(prodId);
            ViewBag.ProductionDownTimeList = GetProductionDownTimeList(prodId);

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

        [HttpPost]
        public ActionResult SaveAll(Production productionData, List<Models.ProductionReject> productionRejData, List<Models.ProductionDownTime> productionDTData) // [Bind(Include = "data")] 
        {
            var productionDataDB = ConvertProdRecToDB(productionData);
            var productionRejDataDB = ConvertProdRejRecListToDB(productionRejData);


            if (ModelState.IsValid)
            {
                db.Entry(productionDataDB).State = EntityState.Modified;
                db.SaveChanges();

                foreach (var item in productionRejDataDB)
                {
                    var pr = db.ProductionRejects.Where(q => q.ProductionId == item.ProductionId && q.RejectReasonId == item.RejectReasonId && q.NumberOfRejects == item.NumberOfRejects);
                    if (pr != null && pr.Count() > 0)
                    {
                        foreach (var prItem in pr)
                        {
                            db.ProductionRejects.Remove(prItem);
                        }
                    }
                }
                db.SaveChanges();

                foreach (var item in productionRejDataDB)
                {
                    var pr = db.ProductionRejects.Where(q => q.ProductionId == item.ProductionId && q.RejectReasonId == item.RejectReasonId && q.NumberOfRejects == item.NumberOfRejects);
                    if (pr == null || pr.Count() == 0)
                    {
                        db.ProductionRejects.Add(item);
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productionData);
        }


        private ProductionData ConvertProdRecToDB(Production pr)
        {
            ProductionData pd = new ProductionData();

            pd.Id = pr.Id;
            pd.WorkStartDate = pr.WorkStartDate;
            pd.ShiftDN = pr.ShiftDN;
            pd.MachineId = pr.MachineId;
            //pr.MachineName = db.Machines.Where(q => q.Id == pd.MachineId).Select(q => q.MachineName).FirstOrDefault();
            pd.ItemId = pr.ItemId;
            //pr.ItemName = db.Items.Where(q => q.Id == pd.ItemId).Select(q => q.ItemFor + " - " + q.ItemName).FirstOrDefault();
            //pr.ItemObj = ConvertItemFromDB(db.Items.Where(q => q.Id == pd.ItemId).FirstOrDefault());

            pd.EmployeeId = pr.EmployeeId;
            //pr.EmployeeName = db.Employees.Where(q => q.Id == pd.EmployeeId).Select(q => q.EmpNo + " - " + q.EmpName).FirstOrDefault();
            pd.FromTime = pr.FromTime;
            pd.MachineCounterFrom = pr.MachineCounterFrom;
            pd.FromTime = pr.FromTime;
            pd.MachineCounterFrom = pr.MachineCounterFrom;
            pd.ToTime = pr.ToTime;
            pd.MachineCounterTo = pr.MachineCounterTo;
            pd.TotalDamaged = pr.TotalDamaged;
            pd.DownTimeMinutes = pr.DownTimeMinutes;
            pd.DownTimeReason = pr.DownTimeReason;
            //pr.Notes = pd.Notes;
            //pr.DamagesBreakdownAndNotes = getDamagesBreakdownAndNotes(pd.Id, pd.Notes);
            pd.Timestamp = DateTime.Now;
            //pr.ProductionRejectList = GetProductionRejectList(pd.Id);
            return pd;
        }

        private List<ProductionData> ConvertProdRecListToDB(DbSet<Production> pdl)
        {
            List<ProductionData> prodRecList = new List<ProductionData>();
            foreach (var item in pdl)
            {
                ProductionData pr = ConvertProdRecToDB(item);
                prodRecList.Add(pr);
            }
            return prodRecList;
        }


        private RanmoDB.ProductionReject ConvertProdRejRecToDB(Models.ProductionReject pr)
        {
            RanmoDB.ProductionReject pd = new RanmoDB.ProductionReject();

            pd.Id = pr.Id;
            pd.ProductionId = pr.ProductionId;
            pd.RejectReasonId = pr.RejectReasonId;
            pd.NumberOfRejects = pr.NumberOfRejects;

            return pd;
        }

        private List<RanmoDB.ProductionReject> ConvertProdRejRecListToDB(List<Models.ProductionReject> pdl)
        {
            List<RanmoDB.ProductionReject> prodRecRejList = new List<RanmoDB.ProductionReject>();
            foreach (var item in pdl)
            {
                RanmoDB.ProductionReject pr = ConvertProdRejRecToDB(item);
                prodRecRejList.Add(pr);
            }
            return prodRecRejList;
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
