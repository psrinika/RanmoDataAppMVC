//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RanmoDB
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductionData
    {
        public int Id { get; set; }
        public Nullable<System.DateTime> Timestamp { get; set; }
        public Nullable<System.DateTime> WorkStartDate { get; set; }
        public string ShiftDN { get; set; }
        public Nullable<int> MachineId { get; set; }
        public Nullable<int> ItemId { get; set; }
        public Nullable<double> EmployeeId { get; set; }
        public Nullable<System.TimeSpan> FromTime { get; set; }
        public Nullable<int> MachineCounterFrom { get; set; }
        public Nullable<System.TimeSpan> ToTime { get; set; }
        public Nullable<int> MachineCounterTo { get; set; }
        public Nullable<int> TotalDamaged { get; set; }
        public Nullable<int> DownTimeMinutes { get; set; }
        public string DownTimeReason { get; set; }
        public string Notes { get; set; }
    }
}
