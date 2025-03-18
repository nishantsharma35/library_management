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
                alert(result.message);

                if (result.success) {
                    window.location.href = "/AdminMaster/PendingMemberships";
                }
            },
            error: function () {
                alert('An error occurred while processing the request.');
            }
        });
    });
});
