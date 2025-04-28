$('#pendingreturn').on('submit', function (e) {
    e.preventDefault(); // Prevent form default submission

    var formData = new FormData(this);

    $.ajax({
        url: '/BorrowMaster/ApproveReturn',   // Correct URL for the action method
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');
                $('#pendingreturn')[0].reset(); // Reset the form after submission
                setTimeout(function () {
                    window.location.href = '/BorrowMaster/BorrowList'; // Redirect after success
                }, 2000);
            } else {
                showToast(response.message, 'error');
                // Optional: Show detailed validation errors
                if (response.errors) {
                    console.log(response.errors);
                }
            }
        },
        error: function () {
            showToast('Server error occurred!', 'error');
        }
    });
});
