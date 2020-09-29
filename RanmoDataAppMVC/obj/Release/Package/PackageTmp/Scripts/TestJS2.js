
function AutoComplete_P() {

    alert('sssssss');
    $(document).ready(function () {
        $("#CustomerName").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '@Url.Action("GetCustomers", "Invoices")',
                    datatype: "json",
                    data: {
                        term: request.term
                    },
                    success: function (data) {
                        response($.map(data, function (val, item) {
                            return {
                                label: val.CustomerName,
                                value: val.CustomerName,
                                CustomerId: val.CustomerId
                            }
                        }))
                    }
                })
            },
            select: function (event, ui) {
                $("#CustomerId").val(ui.item.CustomerId);
            }
        });
    });
}