$(document).ready(function () {
    $("#librarianForm").submit(function (event) {
        event.preventDefault();

        var formData = new FormData(this);
        const btnRegister = $("#btnSubmit");
        const btnLoader = $("#btnLoader");

        // Disable button and show loader
        btnRegister.prop("disabled", true);
        btnLoader.removeClass("d-none");

        setTimeout(function () {
            $.ajax({
                url: "/library/AddLibrarian",
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    showToast(result.message, "success");
                    if (result.success) {
                        setTimeout(() => {
                            window.location.href = '/Dashboard';
                        }, 1500);
                    }
                },
                complete: function () {
                    // Re-enable button and hide loader
                    btnRegister.prop("disabled", false);
                    btnLoader.addClass("d-none");
                },
                error: function () {
                    showToast("Something went wrong!");
                }
            });
        }, 2000); // Delay for loader effect
    });
});
