//Add Multiple RejReasons.
$("#addToRjRsnList").click(function (e) {
    e.preventDefault();

    if ($.trim($("#rejectedReason").val()) == "" || $.trim($("#quantity").val()) == "") return;
    var rejectedReasonName = $("#rejectedReason  option:selected").text();
    var rejectedReasonId = $("#rejectedReason").val(),
        Id = $("#Id").val(),
        quantity = $("#quantity").val(),
        detailsTableRejBody = $("#detailsTableRej tbody");

    var productItem = '<tr><td style="display:none;">' + Id + '</td><td style="display:none;">' + rejectedReasonId + '</td><td>' + rejectedReasonName + '</td><td>' + quantity + '</td><td><a data-itemId="0" href="#" class="deleteItem">Remove</a></td></tr>';
    detailsTableRejBody.append(productItem);
    clearItem();
});

//After Add A New RejReasons In The List, Clear Clean The Form For Add More RejReasons.
function clearItem() {
    $("#rejectedReasonId").val('');
    $("#quantity").val('');
}

// After Add A New RejReasons In The List, If You Want, You Can Remove It.
$(document).on('click', 'a.deleteItem', function (e) {
    e.preventDefault();
    var $self = $(this);
    if ($(this).attr('data-itemId') == "0") {
        $(this).parents('tr').css("background-color", "#ff6347").fadeOut(800, function () {
            $(this).remove();
        });
    }
});




//Add Multiple DtReasons.
$("#addToDtRsnList").click(function (e) {
    e.preventDefault();


    if ($.trim($("#downTimeReason").val()) == "" || $.trim($("#dtMinutes").val()) == "") return;
    var downTimeReasonName = $("#downTimeReason  option:selected").text();

    var downTimeReasonId = $("#downTimeReason").val(),
        Id = $("#Id").val(),
        dtMinutes = $("#dtMinutes").val(),
        detailsTableDTBody = $("#detailsTableDT tbody");


    var productItem = '<tr><td style="display:none;">' + Id + '</td><td style="display:none;">' + downTimeReasonId + '</td><td>' + downTimeReasonName + '</td><td>' + dtMinutes + '</td><td><a data-itemId="0" href="#" class="deleteItem">Remove</a></td></tr>';
    detailsTableDTBody.append(productItem);
    clearItem();
});

//After Add A New DtReasons In The List, Clear Clean The Form For Add More DtReasons.
function clearItem() {
    $("#downTimeReasonId").val('');
    $("#dtMinutes").val('');
}

// After Add A New DtReasons In The List, If You Want, You Can Remove It.
$(document).on('click', 'a.deleteItem', function (e) {
    e.preventDefault();
    var $self = $(this);
    if ($(this).attr('data-itemId') == "0") {
        $(this).parents('tr').css("background-color", "#ff6347").fadeOut(800, function () {
            $(this).remove();
        });
    }
});
