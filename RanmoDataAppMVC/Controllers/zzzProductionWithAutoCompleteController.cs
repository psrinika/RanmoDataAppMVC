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
    public class zzzProductionWithAutoCompleteController : Controller
    {
        private RanSanDBEntities db = new RanSanDBEntities();
        // GET: ProductionWithAutoComplete
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

        private List<Models.Production> ConvertProdRecListFromDB(System.Data.Entity.DbSet<ProductionData> pdl)
        {
            List<Models.Production> prodRecList = new List<Models.Production>();
            foreach (var item in pdl)
            {
                Models.Production pr = ConvertProdRecFromDB(item);
                prodRecList.Add(pr);
            }
            return prodRecList;
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
            pr.DamagesBreakdownAndNotes = "";
            pr.DateAdded = pd.Timestamp;

            return pr;
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

        public JsonResult GetItems(string term = "")
        {
            var objCustomerlist = db.Items
                            .Where(c => c.ItemName.ToUpper()
                            .Contains(term.ToUpper()))
                            .Select(c => new { ItemName = c.ItemFor + " - " + c.ItemName, c.Id })
                            .Distinct().ToList();
            return Json(objCustomerlist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Test(int Id)
        {

            var items = db.Items.Where(c => c.Id == Id).FirstOrDefault();
            return View("Index", items);
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

            //var machines = db.Machines.ToList();
            //ViewBag.Machines = new SelectList(machines);

            SelectList machines = new SelectList(db.Machines.ToList(), "Id", "MachineName");
            ViewBag.MachineNames = machines;

            var data = ConvertProdRecFromDB(productionData);


            return View(data);
        }
    }
}