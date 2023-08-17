function CreateNewUser() {
    let form = $("#createNewUser");
    let data = form.serialize();

    $.ajax({
        url: "/Dashboard/CreateUser",
        type: "POST",
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: data,
        success: function (result) {
            if (result.success) {
                form.trigger("reset");

                $("#SelectedUser").empty();
                $.each(result.model.users, function (index, item) {
                    $("#SelectedUser").append($('<option>', {value: item.value, text: item.text}));
                });

                $("#generatedPasswordInfo input").val(result.model.newUserPassword);
                $("#generatedPasswordInfo").removeClass("visually-hidden");
            } else {
                $("#generatedPasswordInfo").addClass("visually-hidden");
            }
        },
        error: function (xhr, status, error) {
            $("#generatedPasswordInfo").addClass("visually-hidden");
        }
    });
}

function ResetUserPassword() {
    let form = $("#userManagement");
    let data = form.serialize();

    $.ajax({
        url: "/Dashboard/ResetPasswordOfUser",
        type: "POST",
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: data,
        success: function (result) {
            if (result.success) {
                $("#resetPasswordInfo input").val(result.model.resetUserPwNewPassword);
                $("#resetPasswordInfo").removeClass("visually-hidden");
            } else {
                $("#resetPasswordInfo").addClass("visually-hidden");
            }
        },
        error: function (result) {
            $("#resetPasswordInfo").addClass("visually-hidden");
        }
    });
}

function DeleteUser() {
    let form = $("#userManagement");
    let data = form.serialize();

    let dialog = $("#deleteConfirmation");
    dialog.modal("hide");

    $.ajax({
        url: "/Dashboard/DeleteUser",
        type: "POST",
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: data,
        success: function (result) {
            if (result.success) {
                form.trigger("reset");

                $("#SelectedUser").empty();
                $.each(result.model.users, function (index, item) {
                    $("#SelectedUser").append($('<option>', {value: item.value, text: item.text}));
                });

                $("#resetPasswordInfo").addClass("visually-hidden");
                $("#generatedPasswordInfo").addClass("visually-hidden");
            } else {
                $("#resetPasswordInfo").addClass("visually-hidden");
                $("#generatedPasswordInfo").addClass("visually-hidden");
            }
        },
        error: function (result) {
            $("#resetPasswordInfo").addClass("visually-hidden");
        }
    });
}