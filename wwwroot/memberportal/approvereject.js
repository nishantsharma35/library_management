$(document).ready(function () {
    $(".approveMembership, .rejectMembership").click(function () {
        const membershipId = $(this).attr("data-id");
        
        const actionType = $(this).hasClass("approveMembership") ? "ApproveMembership" : "RejectMembership";

        $.ajax({
            url: "/AdminMaster/" + actionType,
            type: 'POST',
            data: { id: membershipId },
            dataType: "json",
            success: function (result) {
                showToast(result.message, "success")

                if (result.success) {
                    setTimeout(function () {
                        window.location.href = "/AdminMaster/PendingMemberships";
                    }, 2000);
                }
            },
            error: function () {
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
