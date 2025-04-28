$(document).ready(function () {
    $(".btnReturnBook").click(function () {
        const borrowId = $(this).data("borrow-id");

        Swal.fire({
            title: 'Are you sure?',
            text: "You want to return this book?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, Return it!'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/BorrowMaster/ReturnBook',
                    type: 'POST',
                    data: { borrowId: borrowId },
                    success: function (res) {
                        if (res.success) {
                            showToast(res.message, 'success');
                            setTimeout(() => {
                                window.location.reload(); // List refresh
                            }, 1500);
                        } else {
                            showToast(res.message || 'Something went wrong.', 'error');
                        }
                    },
                    error: function () {
                        showToast('Server error. Please try again.', 'error');
                    }
                });
            }
        });
    });

    $('#payfine').on('submit', function (e) {
        e.preventDefault();

        var formData = new FormData(this);

        $.ajax({
            url: '/Fine/PayFine',   // Controller ka sahi URL
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.success) {
                    showToast(response.message, 'success');
                    $('#payfine')[0].reset(); // Form reset
                    // Optionally: Redirect to BookList after few seconds
                    setTimeout(function () {
                        window.location.href = '/BorrowMaster/BorrowList';
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

    function showToast(message, icon = 'success') {
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: icon,
            title: message,
            showConfirmButton: false,
            timer: 3000,
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
