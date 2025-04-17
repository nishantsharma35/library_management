$(document).ready(function () {
    console.log("ready"); // just to check

    $("#AddBorrowForm").validate({
        //rules: {
        //    FirstName: {
        //        required: true,
        //        lettersOnly: true, // Custom method for letters only
        //        maxlength: 20
        //    },
        //    LastName: {
        //        lettersOnly: true // Custom method for letters only
        //    },
        //    Username: {
        //        required: true
        //    },
        //    Email: {
        //        required: true,
        //        email: true
        //    },
        //    Phone: {
        //        required: true,
        //        digits: true,
        //        minlength: 6,
        //        maxlength: 15
        //    },
        //    PasswordHash: {
        //        minlength: 6,
        //        required: function () {
        //            return $("#UserId").val() === ""; // Require password only if AdminId is empty (new admin)
        //        }
        //    }
        //},
        //messages: {
        //    FirstName: {
        //        required: "Please enter your first name."
        //    },
        //    Email: {
        //        required: "Please enter your email.",
        //        email: "Please enter a valid email address."
        //    },
        //    Phone: {
        //        required: "Please enter your phone number.",
        //        digits: "Please enter only numbers.",
        //        minlength: "Phone number must be at least 6 digits.",
        //        maxlength: "Phone number cannot exceed 15 digits."
        //    },
        //    PasswordHash: {
        //        minlength: "Password must have atleast 6 digits",
        //        required: "Password is required"
        //    },
        //    Username: {
        //        required: "Please enter admin Username"
        //    }
        //},

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);

            
            const btnRegister = $("#btnborrow");
            const btnLoader = $("#btnLoader");
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");

                // AJAX submission
                $.ajax({
                    url: '/BorrowMaster/AddBorrow',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.success) {
                            // Open OTP modal
                            $("#otpModal").modal("show");

                            // Optional: store borrowId or MemberId if needed for VerifyOTP
                            $("#otpModal").data("borrow-id", result.borrowId); // only if you're sending this from backend
                        } else {
                            showToast(result.message, "error");
                        }
                    },

                    complete: function () {
                        // Re-enable button and hide loader
                        btnRegister.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function () {
                        alert('An error occurred while registering the user.');
                    }
                });
        }
    });


    $("#otpForm").submit(function (e) {
        e.preventDefault();

        const otp = $("#otpInput").val();
        const borrowId = $("#otpModal").data("borrow-id"); // optional, if needed

        $.ajax({
            url: "/BorrowMaster/VerifyOTP",
            method: "POST",
            data: {
                otp: otp,
                borrowId: borrowId
            }, // adjust based on action method
            success: function (res) {
                if (res.success) {
                    showToast("OTP verified successfully!", "success");
                    window.location.href = "/BorrowMaster/BorrowList";
                } else {
                    $("#otpError").removeClass("d-none").text(res.message || "Invalid OTP. Try again.");
                }
            },
            error: function () {
                $("#otpError").removeClass("d-none").text("Something went wrong. Try again.");
            }
        });
    });



});