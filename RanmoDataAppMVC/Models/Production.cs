using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RanmoDataAppMVC.Models
{
    public class Production
    {
        public int Id { get; set; }
        //public Nullable<System.DateTime> Timestamp { get; set; }

        //[DisplayFormat(DataFormatString = "{0:d}")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> WorkStartDate { get; set; }
        public string ShiftDN { get; set; }
        public Nullable<int> MachineId { get; set; }
        public Nullable<int> ItemId { get; set; }
        public Nullable<double> EmployeeId { get; set; }

        public string MachineName { get; set; }
        public string ItemName { get; set; }
        public string EmployeeName { get; set; }


        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public Nullable<System.TimeSpan> FromTime { get; set; }

        [Range(0, 1000000)]
        public Nullable<int> MachineCounterFrom { get; set; }

        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public Nullable<System.TimeSpan> ToTime { get; set; }

        [Range(0, 1000000)]
        public Nullable<int> MachineCounterTo { get; set; }
        public Nullable<int> TotalDamaged { get; set; }
        public Nullable<int> DownTimeMinutes { get; set; }
        public string DownTimeReason { get; set; }
        public string Notes { get; set; }

        public string DamagesBreakdownAndNotes { get; set; }
        public Nullable<System.DateTime> DateAdded { get; set; }

        public Item ItemObj { get; set; }

        public List<ProductionReject> ProductionRejectList { get; set; }

    }
}