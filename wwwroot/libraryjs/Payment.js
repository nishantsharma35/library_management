$(document).ready(function () {
    console.log("Transaction.js loaded");

    let razorpayTimeout = null;

    $(document).on("submit", ".FinePaymentForm", function (event) {
        event.preventDefault();
        console.log("FinePaymentForm submit triggered");

        const form = $(this); // get the form that was submitted
        const fineId = form.find(".razorFineId").val();
        const fineamount = form.find(".razorFineAmount").val();
        const transactionType = form.find(".TransactionType").val();

        console.log("fineId:", fineId);
        console.log("fineamount:", fineamount);
        console.log("transactionType:", transactionType);


        $.ajax({
            url: '/Payment/InitiatePayment',
            type: 'POST',
            contentType: "application/json",
            data: JSON.stringify({
                FineId: fineId,
                FineAmount: fineamount
            }),
            success: function (res) {
                console.log(res + "res");
                if (res.success) {
                    var options = {
                        "key": "rzp_test_TuMnDefva6xmie",
                        "amount": res.fineAmount, // ✅ Correct amount
                        "currency": "INR",
                        "description": "Fine Payment",
                        "order_id": res.orderId,   // ✅ Correct ID
                        "handler": function (response) {
                            clearTimeout(razorpayTimeout);
                            SaveTransaction(fineId, response.razorpay_payment_id, response.razorpay_order_id, response.razorpay_signature, fineamount);
                        },
                        "theme": { "color": "#3399cc" },
                        "modal": {
                            ondismiss: function () {
                                console.log("Razorpay payment popup closed by user.");
                                clearTimeout(razorpayTimeout);
                                resetButton();
                            }
                        }
                    };

                    var rzp = new Razorpay(options);

                    razorpayTimeout = setTimeout(function () {
                        console.log("Payment timeout after 3 minutes.");
                        resetButton();
                        rzp.close();
                        showToast("Payment session expired. Please try again.", "error");
                    }, 180000);

                    rzp.open();
                } else {
                    showToast(res.message, "error");
                    resetButton();
                }
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                showToast("Error initiating payment: " + xhr.responseText, "error");
                resetButton();
            }
        });
    });

    function SaveTransaction(fineId, razorpayPaymentId, razorpayOrderId, razorpaySignature, fineAmount) {
        let TransactionData = {
            FineId: fineId,
            ReferenceNo: razorpayPaymentId,
            TransactionType: $(".TransactionType").val(),
            Amount: fineAmount, // safe
            RazorpayOrderId: razorpayOrderId,
            RazorpaySignature: razorpaySignature
        };

        console.log(TransactionData + "data");
        $.ajax({
            url: '/MylibraryMaster/PayFine',
            type: 'POST',
            contentType: "application/json",
            data: JSON.stringify(TransactionData),
            success: function (result) {
                if (result.success) {
                    window.location.href = "/MylibraryMaster/BorrowedBooks";
                } else {
                    showToast(result.message, "error");
                }
            },
            complete: function () {
                resetButton();
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                showToast("Error saving transaction: " + xhr.responseText, "error");
            }
        });
    }

    function resetButton() {
        $("#btnSubmitPayment").prop("disabled", false);
        $("#btnLoader").addClass("d-none");
    }
});
