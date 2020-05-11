using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RanmoDataAppMVC.Models
{
    public partial class ShiftTime
    {
        public int Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:HH\\:mm}", ApplyFormatInEditMode = true)]
        public System.TimeSpan FromToTime { get; set; }
    }
}