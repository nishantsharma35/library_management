$('.approve-btn').on('submit', function (e) {
    e.preventDefault();

    var formData = new FormData(this);

    $.ajax({
        url: '/BorrowMaster/ApproveBorrow',   // Controller ka sahi URL
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');
                $('.approve-btn')[0].reset(); // Form reset
                // Optionally: Redirect to BookList after few seconds
                setTimeout(function () {
                    window.location.href = '/BorrowMaster/BorrowList';
                }, 3000);
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

    document.querySelectorAll(".reject-btn").forEach(button => {
        button.addEventListener("click", function () {
            var borrowId = this.dataset.id;
            console.log("Clicked Reject ID:", borrowId);  // Debugging ke liye
            fetch("/BorrowMaster/RejectBorrow", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ borrowId: parseInt(borrowId) }) // Ensure number format
            })
                .then(response => response.json())
                .then(data => {
                    alert(data.message);
                    location.reload();
                })
                .catch(error => alert("Error: " + error));
        });
    });
});


$('#pendingreturn').on('submit', function (e) {
    e.preventDefault();

    var formData = new FormData(this);

    $.ajax({
        url: '/BorrowMaster/ApproveReturn',   // Controller ka sahi URL
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');
                $('#pendingreturn')[0].reset(); // Form reset
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