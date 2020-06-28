//After Click Save Button Pass All Data View To Controller For Save Database
function saveAll(data) {
    return $.ajax({
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        type: 'POST',
        url: "/RanmoDataAppMVC/Production/SaveAll",
        data: data,
        success: function (result) {
            alert(result);
            location.reload();
        },
        error: function () {
           // alert("Error!")
        }
    });
}

//Collect Multiple RejReasons List For Pass To Controller
$("#saveAll").click(function (e) {
    e.preventDefault();

    var rejectReasonArr = [];
    rejectReasonArr.length = 0;

    $.each($("#detailsTableRej tbody tr"), function () {
        var Id = 0;
        var productionId = $(this).find('td:eq(0)').html();
        var rejectedReasonId = $(this).find('td:eq(1)').html();
        var rejectedReasonName = $(this).find('td:eq(2)').html();
        var quantity = $(this).find('td:eq(3)').html();

        rejectReasonArr.push({
            Id: Id,
            ProductionId: productionId,
            RejectReasonId: rejectedReasonId,
            RejectReasonName: rejectedReasonName,
            NumberOfRejects: quantity
        });
    });

    var downTimeReasonArr = [];
    downTimeReasonArr.length = 0;

    $.each($("#detailsTableDT tbody tr"), function () {
        var Id = 0;
        var productionId = $(this).find('td:eq(0)').html();
        var downTimeReasonId = $(this).find('td:eq(1)').html();
        var downTimeReasonName = $(this).find('td:eq(2)').html();
        var dtMinutes = $(this).find('td:eq(3)').html();

        downTimeReasonArr.push({
            Id: Id,
            ProductionId: productionId,
            DownTimeReasonId: downTimeReasonId,
            DownTimeReasonName: downTimeReasonName,
            DownTimeMinutes: dtMinutes
        });
    });

    var data = JSON.stringify({
        Id: $("#Id").val(),
        WorkStartDate: $("#WorkStartDate").val(),
        ShiftDN: $("#ShiftDN").val(),
        MachineId: $("#MachineId").val(),
        // ItemName: $("#ItemName").val(),
        ItemId: $("#ItemId").val(),
        EmployeeId: $("#EmployeeId").val(),
        FromTime: $("#FromTime").val(),
        MachineCounterFrom: $("#MachineCounterFrom").val(),
        ToTime: $("#ToTime").val(),
        MachineCounterTo: $("#MachineCounterTo").val(),
        Notes: $("#Notes").val(),

        productionRejData: rejectReasonArr,
        productionDTData: downTimeReasonArr

    });

    $.when(saveAll(data)).then(function (response) {
        console.log(response);
    }).fail(function (err) {
        console.log(err);
    });
});
