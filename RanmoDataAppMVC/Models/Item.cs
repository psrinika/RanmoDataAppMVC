using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RanmoDataAppMVC.Models
{
    //public class Item
    //{
    //    public int Id { get; set; }
    //    public string ItemName { get; set; }
    //    public Nullable<decimal> FullShotWeight { get; set; }
    //    public Nullable<decimal> WithoutRunnerWeight { get; set; }
    //    public Nullable<decimal> CycleTime { get; set; }
    //    public Nullable<decimal> PerHourTarget { get; set; }

        //  test 

    //}

    public partial class Item
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public Nullable<decimal> FullShotWeight { get; set; }
        public Nullable<decimal> WithoutRunnerWeight { get; set; }
        public Nullable<decimal> CycleTime { get; set; }
        public Nullable<decimal> PerHourTarget { get; set; }
        public bool IsActive { get; set; }
        public string ItemFor { get; set; }


        private string itemForAndName;
        public string ItemForAndName
        {
            get
            {
                if (ItemFor != null && ItemName != null)
                {
                    return ItemFor + " - " + ItemName;
                }
                return itemForAndName;
            }
            set { itemForAndName = value; }
        }
    }
}