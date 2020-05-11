using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RanmoDataAppMVC.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string EmpNo { get; set; }
        public string EmpName { get; set; }
        public Nullable<System.DateTime> DateAdded { get; set; }
    }
}