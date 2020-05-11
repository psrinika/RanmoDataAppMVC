using RanmoDataAppMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RanmoDataAppMVC.Controllers
{
    public class RejectedItemsController : Controller
    {
        // GET: RejectedItems
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveOrder(string d) //ProductionReject[]
        {
            return View();
        }



    }
}