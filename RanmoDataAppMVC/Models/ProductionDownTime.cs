using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RanmoDataAppMVC.Models
{
    public class ProductionDownTime
    {
        public int Id { get; set; }
        public int ProductionId { get; set; }
        public int DownTimeReasonId { get; set; }
        public string DownTimeReasonName { get; set; }

        [Range(5, 1000)]
        public int DownTimeMinutes { get; set; }
    }
}