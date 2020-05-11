using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RanmoDataAppMVC.Models
{
    public partial class ProductionReject
    {
        public int Id { get; set; }
        public int ProductionId { get; set; }
        public int RejectReasonId { get; set; }
        public string RejectReasonName { get; set; }

        [Range(1, 1000)]
        public int NumberOfRejects { get; set; }
    }

}

