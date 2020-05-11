using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using RanmoDataAppMVC.Models;

namespace RanmoDataAppMVC.Controllers
{
    public class Machine_WebAPIController : Controller
    {

        // GET: Machine
        public ActionResult Index()
        {
            IEnumerable<Machine> mcList;
            HttpResponseMessage response = GlobalVariables.WebApiClient.GetAsync("Machines").Result;
            mcList = response.Content.ReadAsAsync<IEnumerable<Machine>>().Result;
            return View(mcList);
        }


        // GET: Machine/AddOrEdit
        public ActionResult AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View(new Machine());
            }
            else
            {
                HttpResponseMessage response = GlobalVariables.WebApiClient.GetAsync("Machines/" + id.ToString()).Result;
                return View(response.Content.ReadAsAsync<Machine>().Result);
            }

        }

        // POST: Machine/Create
        [HttpPost]
        public ActionResult AddOrEdit(Machine mcObj)
        {
            try
            {
                if (mcObj.Id == 0)
                {
                    HttpResponseMessage response = GlobalVariables.WebApiClient.PostAsJsonAsync("Machines", mcObj).Result;
                    return RedirectToAction("Index");
                }
                else
                {
                    HttpResponseMessage response = GlobalVariables.WebApiClient.PutAsJsonAsync("Machines/" + mcObj.Id.ToString(), mcObj).Result;
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: Machine/Delete/5
        public ActionResult Delete(int id)
        {
            HttpResponseMessage response = GlobalVariables.WebApiClient.DeleteAsync("Machines/" + id.ToString()).Result;
            return RedirectToAction("Index");
        }

    }
}
