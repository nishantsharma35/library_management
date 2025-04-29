$(document).ready(function () {
    $(".joinLibrary").on("click", function (event) {
        event.preventDefault(); // ✅ Prevent default form submission

        var libraryId = $(this).data("id"); // Get the Library ID from button data attribute
       // alert("Button Clicked! Sending request for Library ID: " + libraryId);

        $.ajax({
            url: "/Membership/JoinLibrary",
            type: "POST",
            data: { LibraryId: libraryId },
            success: function (result) {
                showToast(result.message, "success")
                if (result.success) {
                    setTimeout(function () {
                        window.location.href = "/Dashboard"; // Redirect if needed
                    }, 2000);
                }
            },
            error: function (xhr) {
                console.log(xhr.responseText);
                showToast("An error occurred while processing the request.", "danger");
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
