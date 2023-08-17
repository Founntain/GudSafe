
$(function () {
    $("#login").submit(function (e){
        e.preventDefault();
        Login();
    });
});

function Login() {
    let form = $("#login");
    let data = form.serialize();

    let reqUrl = "/Home/Login";

    if (window.location.search) {
        reqUrl += window.location.search;
    }

    $.ajax({
        url: reqUrl,
        type: "POST",
        data: data,
        success: function (result) {
            if (result.success) {
                window.location.href = result.redirectUrl;
            }
        },
        error: function (xhr, status, error) {
            alert("Error: " + error);
        }
    });
}