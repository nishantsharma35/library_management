$(document).ready(function () {

    $("#forgotPassword").validate({
        rules: {
            Email: {
                required: true,
                email: true
            }
        },
        messages: {
            Email: {
                required: "Please enter your Email.",
                email: "Not a valid email"
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault();
            const formData = new FormData(form);

            // Manually add the Email value (if required)
            formData.append("Email", $("#Email").val());

            // AJAX submission
            $.ajax({
                url: '/library/ForgotPassword',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    if (result.message) {
                        // Show success toast with message from backend
                        showToast(result.message, 'success');
                    } else {
                        // Show error toast if no message is returned
                        showToast('Unknown response from server.', 'error');
                    }
                },
                error: function () {
                    // Show error toast if AJAX fails
                    showToast('An error occurred while sending the email.', 'error');
                }
            });
        }
    });

    function showToast(message, icon = 'success') {
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: icon,
            title: message,
            showConfirmButton: false,
            timer: 2000,
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