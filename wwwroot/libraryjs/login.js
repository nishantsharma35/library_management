$(document).ready(function () {

    $("#forgotPassword").validate({
        rules: {
            Email: {
                required: true,
                email: true
            }
        },
        messages: {
            Email: {
                required: "Please enter your Email.",
                email: "Not a valid email"
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);

            // AJAX submission
            $.ajax({
                url: '/library/ForgotPassword',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    alert(result.message);
                },
                error: function () {
                    alert('An error occurred while sending the email.');
                }
            });
        }
    });


    $("#ResetPassword").validate({
        rules: {
            Password: {
                required: true,
                minlength: 8
            },
            ConfirmPassword: {
                required: true,
                minlength: 8,
                equalTo: "#Password"
            },
        },
        messages: {
            Password: {
                required: "Please enter a password.",
                minlength: "Password must be at least 8 characters."
            },
            ConfirmPassword: {
                required: "Please enter confirm password",
                minlength: "Confirm password must be least 8 characters",
                equalTo: "Password doesn't match"
            },
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);
            //var formData = {
            //    PasswordHash: $('#PasswordHash').val(),
            //}
            // AJAX submission
            $.ajax({
                url: '/library/ResetPasswordAction',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        window.location.href = '/library/login';
                    }
                },
                error: function () {
                    alert('An error occurred while registering the user.');
                }
            });
        }
    });

    $('#showPasswordCheckbox').on('change', function () {
        const passwordField = document.getElementById("Password");
        if (this.checked) {
            passwordField.type = "text";
        } else {
            passwordField.type = "password";
            }
    });



    $("#loginForm").validate({
        rules: {
            EmailOrName: {
                required: true,
            },
            Password: {
                required: true,
            }
        },
        messages: {
            EmailOrName: {
                required: "Please enter your Credentials."
            },
            Password: {
                required: "Please enter your Password.",
            }
        },


        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);
            const btnRegister = $("#btnSubmit");
            const btnLoader = $("#btnLoader");
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");
            // AJAX submission
            $.ajax({
                url: '/library/Login',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        if (result.res != "Dashboard") {
                            window.location.href = "/library/" + result.res;
                        }
                        else {
                            window.location.href = '/Dashboard';
                        }
                    }
                },
                complete: function () {
                    // Re-enable button and hide loader
                    btnRegister.prop("disabled", false);
                    btnLoader.addClass("d-none");
                },
                error: function () {
                    alert('An error occurred while login.');
                }
            });
        }
    });
});

