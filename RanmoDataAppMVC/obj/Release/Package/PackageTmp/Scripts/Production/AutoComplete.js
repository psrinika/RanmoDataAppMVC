



$(document).ready(function () {
    $("#ItemName").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: '/RanmoDataAppMVC/Production/GetItems/")',
                datatype: "json",
                data:
                {
                    term: request.term,
                    itemFor: $("#ItemFor").val(),
                    activeStatus: $("#activeStatus").val()
                },
                success: function (data) {
                    response($.map(data, function (val, item) {
                        return {
                            label: val.ItemName,
                            value: val.ItemName,
                            ItemId: val.ItemId
                        }
                    }))
                }
            })
        },
        select: function (event, ui) {
            $("#ItemId").val(ui.item.ItemId);
        }
    });
});


$(document).ready(function () {
    $("#EmployeeName").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: '/RanmoDataAppMVC/Production/GetEmployees/")', //'@Url.Action("GetEmployees", "Production")',
                datatype: "json",
                data:
                {
                    term: request.term
                },
                success: function (data) {
                    response($.map(data, function (val, item) {
                        return {
                            label: val.EmployeeName,
                            value: val.EmployeeName,
                            EmployeeId: val.EmployeeId
                        }
                    }))
                }
            })
        },
        select: function (event, ui) {
            $("#EmployeeId").val(ui.item.EmployeeId);
        }
    });
});