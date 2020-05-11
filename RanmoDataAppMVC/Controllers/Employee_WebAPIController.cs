using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using RanmoDataAppMVC.Models;

namespace RanmoDataAppMVC.Controllers
{
    public class Employee_WebAPIController : Controller
    {
        // GET: Employee
        public ActionResult Index()
        {
            IEnumerable<Employee> empList;
            HttpResponseMessage response = GlobalVariables.WebApiClient.GetAsync("Employees").Result;
            empList = response.Content.ReadAsAsync<IEnumerable<Employee>>().Result;
            return View(empList);
        }

        // GET: Employee/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Employee/AddOrEdit
        public ActionResult AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View(new Employee());
            }
            else
            {
                HttpResponseMessage response = GlobalVariables.WebApiClient.GetAsync("Employees/" + id.ToString()).Result;
                return View(response.Content.ReadAsAsync<Employee>().Result);
            }

        }

        // POST: Employee/Create
        [HttpPost]
        public ActionResult AddOrEdit(Employee empObj)
        {
            try
            {
                if (empObj.Id == 0)
                {
                    HttpResponseMessage response = GlobalVariables.WebApiClient.PostAsJsonAsync("Employees", empObj).Result;
                    return RedirectToAction("Index");
                }
                else
                {
                    HttpResponseMessage response = GlobalVariables.WebApiClient.PutAsJsonAsync("Employees/" + empObj.Id.ToString(), empObj).Result;
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                return View();
            }
        }


        // GET: Employee/Delete/5
        public ActionResult Delete(int id)
        {
            HttpResponseMessage response = GlobalVariables.WebApiClient.DeleteAsync("Employees/" + id.ToString()).Result;
            return RedirectToAction("Index");
        }


    }
}
