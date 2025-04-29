$(document).ready(function () {


    $("#googleDetailsForm").validate({
        rules: {
            Username: {
                required: true,
            },
            Email: {
                required: true,
            }
        },
        messages: {
            Username: {
                required: "Please enter your Username."
            },
            Password: {
                required: "Please enter your Password.",
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);
            const btnRegister = $("#btnSave");
            const btnLoader = $("#btnLoader");
            // AJAX submission
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");
            setTimeout(function () {

                $.ajax({
                    url: '/library/GoogleDetails',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        setTimeout(() => {
                        if (result.success) {
                            window.location.href = '/library/libraryRegistration'
                        } else {
                            showToast(result.message,"success")
                        }
                    }, 2000);
                    },
                    complete: function () {
                        // Re-enable button and hide loader
                        btnRegister.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function () {
                        showToast("Unknown error occurred")
                    }
                });
            }, 2000);
        }
    });

});