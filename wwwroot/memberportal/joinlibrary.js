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
                alert(result.message); // Show success/failure message
                if (result.success) {
                    window.location.href = "/Dashboard"; // Redirect if needed
                }
            },
            error: function (xhr) {
                console.log(xhr.responseText);
                alert("An error occurred while processing the request.");
            }
        });
    });
});
