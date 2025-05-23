﻿$('#addBookForm').on('submit', function (e) {
    e.preventDefault();

    var formData = new FormData(this);

    $.ajax({
        url: '/BookMaster/AddBook',   // Controller ka sahi URL
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');
                $('#addBookForm')[0].reset(); // Form reset
                // Optionally: Redirect to BookList after few seconds
                setTimeout(function () {
                    window.location.href = '/BookMaster/BookList';
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
