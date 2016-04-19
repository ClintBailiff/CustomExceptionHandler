

/************************************************************
Click event handler for the Call API on the API Error page
*************************************************************/
$('#ApiError').click(function () {
    $.ajax({
        url: '/API/ApiError',
        type: 'GET',
        cache: false,
        success: function (html) {
            showMessage(html);
        },
        error: function (error) {
            showError(error.responseText);
        }
    });
});


/***************************************************
Shows an error message in a bootstrap modal div.
****************************************************/
function showError(errorMessage) {
    $("#ModalContent").load(window.location.origin + "/partials/modalerror.html", function () {
        $("#ModalBodyContent").html(errorMessage);
        $('#MainModal').modal('show');
    });
}


/*****************************************************
Shows a friendly message in a bootstrap modal div.
******************************************************/
function showMessage(message) {
    $("#ModalContent").load(window.location.origin + "/partials/modalmessage.html", function () {
        $("#ModalBodyContent").html(message);
        $('#MainModal').modal('show');
    });
}