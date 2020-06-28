/// <reference path="jquery-1.10.2.js" />
//http://localhost/RanmoDataAppMVC/Production/Edit/@Url.Action(% 22GetItems % 22,% 20 % 22Production % 22)?term=sa&itemFor=&activeStatus=-1


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
