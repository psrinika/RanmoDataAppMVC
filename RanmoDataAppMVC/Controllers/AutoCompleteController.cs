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
    public class AutoCompleteController : Controller
    {
        private RanSanDBEntities db = new RanSanDBEntities();

        // GET: AutoComplete
        public ActionResult Index()
        {
            return View();
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

        public ActionResult Test(int Id )
        {

            var items = db.Items.Where(c => c.Id == Id).FirstOrDefault();
            return View("Index", items);
        }

    }
}