$(document).ready(function () {
    const toggleIcon = document.getElementById("passwordToggleIcon");
    const passwordInput = document.getElementById("passwordField");

    toggleIcon.addEventListener("click", function () {
        if (passwordInput.type === "password") {
            passwordInput.type = "text";
            toggleIcon.classList.remove("fa-lock");
            toggleIcon.classList.add("fa-lock-open");
        } else {
            passwordInput.type = "password";
            toggleIcon.classList.remove("fa-lock-open");
            toggleIcon.classList.add("fa-lock");
        }
    });


    //$("#forgotPassword").validate({
    //    rules: {
    //        Email: {
    //            required: true,
    //            email: true
    //        }
    //    },
    //    messages: {
    //        Email: {
    //            required: "Please enter your Email.",
    //            email: "Not a valid email"
    //        }
    //    },

    //    submitHandler: function (form, event) {
    //        event.preventDefault();
    //        const formData = new FormData(form);

    //        // Manually add the Email value (if required)
    //        formData.append("Email", $("#Email").val());

    //        // AJAX submission
    //        $.ajax({
    //            url: '/library/ForgotPassword',
    //            type: 'POST',
    //            processData: false,
    //            contentType: false,
    //            data: formData,
    //            success: function (result) {
    //                if (result.message) {
    //                    // Show success toast with message from backend
    //                    showToast(result.message, 'success');
    //                } else {
    //                    // Show error toast if no message is returned
    //                    showToast('Unknown response from server.', 'error');
    //                }
    //            },
    //            error: function () {
    //                // Show error toast if AJAX fails
    //                showToast('An error occurred while sending the email.', 'error');
    //            }
    //        });
    //    }
    //});


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
                    showToast(result.message, 'success');
                    if (result.success) {
                        window.location.href = '/library/login';
                    }
                },
                error: function () {
                    showToast('An error occurred while registering the user.');
                }
            });
        }
    });

    //$('#showPasswordCheckbox').on('change', function () {
    //    const passwordField = document.getElementById("Password");
    //    if (this.checked) {
    //        passwordField.type = "text";
    //    } else {
    //        passwordField.type = "password";
    //        }
    //});
    


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
                    if (result.success) {
                        // Show success toast
                        showToast(result.message, 'success');

                        if (result.res != "Dashboard") {
                            window.location.href = "/library/" + result.res;
                        } else {
                            window.location.href = '/Dashboard';
                        }
                    } else {
                        // Show error toast
                        showToast(result.message, 'error');
                    }
                },
                complete: function () {
                    // Re-enable button and hide loader
                    btnRegister.prop("disabled", false);
                    btnLoader.addClass("d-none");
                },
                error: function () {
                    // Show error toast
                    showToast('An error occurred while login.', 'error');
                }
            });
        }
    });

    function showToast(message, icon = 'success') {
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: icon,
            title: message,
            showConfirmButton: false,
            timer: 2000,
            timerProgressBar: true,
            customClass: {
                popup: 'custom-toast-popup',
                title: 'custom-toast-title'
            },
            iconColor: icon === 'success' ? '#28a745' :
                icon === 'error' ? '#dc3545' :
                    icon === 'warning' ? '#ffc107' : '#17a2b8',
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer);
                toast.addEventListener('mouseleave', Swal.resumeTimer);
            }
        });
    }

});

