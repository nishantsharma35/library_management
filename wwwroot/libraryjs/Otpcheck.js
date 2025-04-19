$(document).ready(function () {

    const timerElement = $("#timer");
    const resendSection = $("#resend-section");
    const resendButton = $("#resend-btn");
    const duration = 60;
    let timeRemaining = duration;
    let interval;


    // Update the timer every second
    //function startTimer() {
    //    clearInterval(interval); // Clear any existing timer
    //    timeRemaining = duration; // Reset the timer duration

    //    const interval = setInterval(() => {
    //        if (timeRemaining <= 0) {
    //            clearInterval(interval);
    //            timerElement.text("You can resend the OTP.");
    //            resendSection.show(); // Show the resend button when timer expires
    //        } else {
    //            timeRemaining--;
    //            const minutes = Math.floor(timeRemaining / 60).toString().padStart(2, "0");
    //            const seconds = (timeRemaining % 60).toString().padStart(2, "0");
    //            timerElement.text(`Time remaining: ${minutes}:${seconds}`);
    //        }
    //    }, 1000);
    //    resendButton.prop("disabled", true); // Disable the button while the timer is active
    //}

    //startTimer();

    // Resend OTP functionality
    resendButton.click(function (e) {
        e.preventDefault();
        $.ajax({
            url: "/library/ResendOTP", // Adjust the route to your server's endpoint
            method: "GET",
            success: function (data) {
                if (data.success) {
                    alert("OTP has been resent to your email.");
                    timeRemaining = duration; // Reset the timer
                    resendSection.hide(); // Hide the resend button
                    timerElement.text("Time remaining: 02:00"); // Reset the timer text
                    setInterval(interval); // Restart the interval
                } else {
                    alert("Failed to resend OTP. Please try again.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error resending OTP:", error);
                alert("An error occurred. Please try again.");
            }
        });
    });


    //$("#OtpCheck").submit(function (event) {
    //    event.preventDefault()
    //    const formData = {
    //        Email: $('#Email').val(),
    //        Otp: $("#Otp").val()
    //    }

    //    console.log(formData.Email)
    //    console.log(formData.Otp)
    //    // AJAX submission
    //    $.ajax({
    //        url: '/library/Otpcheck',
    //        type: 'POST',
    //        data: formData,
    //        success: function (result) {
    //            console.log(result); // Check the JSON structure in the console
    //            alert(result.message); // This should only print the message

    //            if (result.success) {
    //                alert("done for the day");
    //                if (result.id == 2) {
    //                    window.location.href = '/library/libraryRegistration';
    //                } else {
    //                    window.location.href = '/Home/Index';
    //                }
    //            }
    //        },

    //        error: function () {
    //            alert('An error occurred while registering the user.');
    //        }
    //    });
    //});

    // Handle focus movement for OTP input fields


    $("#OtpCheck").submit(function (event) {
        event.preventDefault();

        $("#submit-btn").prop("disabled", true); // disable button
        $("#loader").show(); // show loader

        const formData = {
            Email: $('#Email').val(),
            Otp: $("#Otp").val()
        };

        $.ajax({
            url: '/library/Otpcheck',
            type: 'POST',
            data: formData,
            success: function (result) {
                console.log(result);
                alert(result.message);

                if (result.success) {
                    if (result.id == 2) {
                        window.location.href = '/library/libraryRegistration';
                    } else {
                        window.location.href = '/Dashboard';
                    }
                }
            },
            error: function () {
                alert('An error occurred while registering the user.');
            },
            complete: function () {
                $("#submit-btn").prop("disabled", false); // enable again
                $("#loader").hide(); // hide loader
            }
        });
    });




    $(".otp-input").on("input", function () {
        const $this = $(this);
        const value = $this.val();

        if (value.length === 1) {
            $this.next(".otp-input").focus(); // Move to the next input
        }
    });

    $(".otp-input").on("keydown", function (e) {
        const $this = $(this);

        if (e.key === "Backspace" && !$this.val()) {
            $this.prev(".otp-input").focus(); // Move to the previous input
        }
    });

});







//$(document).ready(function () {

//    const timerElement = $("#timer");
//    const resendSection = $("#resend-section");
//    const resendButton = $("#resend-btn");
//    const duration = 60;
//    let timeRemaining = duration;
//    let interval;

//    function startTimer() {
//        clearInterval(interval); // Clear any existing timer
//        timeRemaining = duration; // Reset timer
//        resendSection.hide(); // Hide resend button
//        resendButton.prop("disabled", true); // Disable resend button

//        interval = setInterval(() => {
//            if (timeRemaining <= 0) {
//                clearInterval(interval);
//                timerElement.text("You can resend the OTP.");
//                resendSection.show(); // Show resend button when timer ends
//                resendButton.prop("disabled", false); // Enable resend
//            } else {
//                timeRemaining--;
//                const minutes = Math.floor(timeRemaining / 60).toString().padStart(2, "0");
//                const seconds = (timeRemaining % 60).toString().padStart(2, "0");
//                timerElement.text(`Time remaining: ${minutes}:${seconds}`);
//            }
//        }, 1000);
//    }

//    // Start timer on page load
//    startTimer();

//    // Resend OTP functionality
//    resendButton.click(function (e) {
//        e.preventDefault();
//        $.ajax({
//            url: "/library/ResendOTP", // Adjust your endpoint
//            method: "GET",
//            success: function (data) {
//                if (data.success) {
//                    alert("OTP has been resent to your email.");
//                    startTimer(); // Restart the timer properly
//                } else {
//                    alert("Failed to resend OTP. Please try again.");
//                }
//            },
//            error: function (xhr, status, error) {
//                console.error("Error resending OTP:", error);
//                alert("An error occurred. Please try again.");
//            }
//        });
//    });

//    // Submit form
//    $("#OtpCheck").submit(function (event) {
//        event.preventDefault();

//        $("#submit-btn").prop("disabled", true);
//        $("#loader").show();

//        const formData = {
//            Email: $('#Email').val(),
//            Otp: $("#Otp").val()
//        };

//        $.ajax({
//            url: '/library/Otpcheck',
//            type: 'POST',
//            data: formData,
//            success: function (result) {
//                console.log(result);
//                alert(result.message);

//                if (result.success) {
//                    if (result.id == 2) {
//                        window.location.href = '/library/libraryRegistration';
//                    } else {
//                        window.location.href = '/Dashboard';
//                    }
//                }
//            },
//            error: function () {
//                alert('An error occurred while verifying the OTP.');
//            },
//            complete: function () {
//                $("#submit-btn").prop("disabled", false);
//                $("#loader").hide();
//            }
//        });
//    });

//    // Auto-move between OTP inputs
//    $(".otp-input").on("input", function () {
//        const $this = $(this);
//        if ($this.val().length === 1) {
//            $this.next(".otp-input").focus();
//        }
//    });

//    $(".otp-input").on("keydown", function (e) {
//        if (e.key === "Backspace" && !$(this).val()) {
//            $(this).prev(".otp-input").focus();
//        }
//    });
//});


