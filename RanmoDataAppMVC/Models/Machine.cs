using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RanmoDataAppMVC.Models
{
    public class Machine
    {
        public int Id { get; set; }
        public string MachineName { get; set; }

    }
}

/*
 @model RanmoDataAppMVC.Models.Employee

@{
    ViewBag.Title = "AddOrEdit";
}

<div class="form-body">
    @using (Html.BeginForm())
    {
        @Html.HiddenFor(model => model.Id)
    }
    <div class="form-group">
        @Html.LabelFor(model => model.EmpNo)
        @Html.EditorFor(model => model.EmpNo)
        @Html.ValidationMessageFor(model => model.EmpNo)
    </div>
    <div class="form-group">
        @Html.LabelFor(model => model.EmpName)
        @Html.EditorFor(model => model.EmpName)
        @Html.ValidationMessageFor(model => model.EmpName)
    </div>

</div>

*/