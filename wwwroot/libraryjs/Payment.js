$(document).ready(function () {

    console.log("Transaction.js loaded");

    $(".FinePaymentForm").each(function () {
        $(this).validate({
            submitHandler: function (form, event) {
                event.preventDefault();

                let $form = $(form); // Reference to the current form
                let fineAmount = $form.find("#payAmount").val();
                let fineId = $form.find("#fineId").val();

                $.ajax({
                    url: '/Payment/InitiatePayment',
                    type: 'POST',
                    contentType: "application/json",
                    data: JSON.stringify({
                        FineId: fineId,
                        FineAmount: fineAmount
                    }),
                    success: function (res) {
                        console.log(res)
                        if (res.success) {
                            var options = {
                                "key": "rzp_test_TuMnDefva6xmie",
                                "amount": res.amount,
                                "currency": "INR",
                                "description": "Order Payment",
                                "fine_id": res.fineId,
                                "handler": function (response) {
                                    SaveTransaction(fineId, response.razorpay_payment_id, response.razorpay_fine_id, response.razorpay_signature, fineAmount);
                                },
                                "theme": {
                                    "color": "#3399cc"
                                }
                            };
                            var rzp = new Razorpay(options);
                            rzp.open();
                        } else {
                            showToast(res.message, "error");
                        }
                    },
                    error: function (xhr) {
                        console.error(xhr.responseText);
                        showToast("Error initiating payment: " + xhr.responseText, "error");
                    }
                });
            }
        });
    });

    function SaveTransaction(fineId, razorpayPaymentId, razorpayfineId, razorpaySignature, fineAmount) {
        let TransactionData = {
            FineId: fineId,
            reference: razorpayPaymentId,
            PaymentMode: "Razorpay", // hardcoded or get from dropdown
            FineAmount: fineAmount,
            RazorpayFineId: razorpayfineId,
            RazorpaySignature: razorpaySignature
        };

        $.ajax({
            url: '/MyLibraryMaster/PayFineMember',
            type: 'POST',
            contentType: "application/json",
            data: JSON.stringify(TransactionData),
            success: function (result) {
                if (result.success) {
                    window.location.href = "/";
                } else {
                    showToast(result.message, "error");
                }
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                showToast("Error saving transaction: " + xhr.responseText, "error");
            }
        });
    }

});
