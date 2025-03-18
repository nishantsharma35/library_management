$(document).ready(function () {
    console.log(typeof $.fn.validate); // Should print "function"

    document.getElementById('ProfileFile').addEventListener('change', function (event) {
        const file = event.target.files[0];
        const previewImage = document.getElementById('imagePreview');
        const previewText = document.getElementById('imagePreviewText');

        if (file) {
            const reader = new FileReader(); // Create a FileReader instance

            // When the file is loaded, set the image source to display it
            reader.onload = function (e) {
                previewImage.src = e.target.result;
                previewImage.style.display = 'block';
                previewText.style.display = 'none';
            };

            // Read the selected file as a data URL
            reader.readAsDataURL(file);
        } else {
            // Reset the preview if no file is selected
            previewImage.src = '#';
            previewImage.style.display = 'none';
            previewText.style.display = 'block';
        }
    });


    const username = "nishant_35"; // Replace with your Geonames username
    const stateGeonameId = 1269750; // ID for India

    // Fetch States
    fetch(`http://api.geonames.org/childrenJSON?geonameId=${stateGeonameId}&username=${username}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            const states = data.geonames;
            let stateOptions = `<option value="">Select State</option>`;
            states.forEach(state => {
                // Store name as the value and geonameId in data-id
                stateOptions += `<option value="${state.name}" data-id="${state.geonameId}">${state.name}</option>`;
            });
            $("#State").html(stateOptions);
        })
        .catch(error => {
            console.error("Error fetching states:", error);
        });

    // Fetch Cities when a state is selected
    $("#State").on("change", function () {
        const selectedStateGeonameId = $(this).find("option:selected").data("id"); // Get geonameId from data-id

        if (selectedStateGeonameId) {
            fetch(`http://api.geonames.org/childrenJSON?geonameId=${selectedStateGeonameId}&username=${username}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`HTTP error! status: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    const cities = data.geonames;
                    let cityOptions = `<option value="">Select City</option>`;
                    cities.forEach(city => {
                        // Store city name as the value and optionally include geonameId in data-id
                        cityOptions += `<option value="${city.name}" data-id="${city.geonameId}">${city.name}</option>`;
                    });
                    $("#City").html(cityOptions);
                })
                .catch(error => {
                    console.error("Error fetching cities:", error);
                });
        } else {
            $("#City").html(`<option value="">Select City</option>`); // Reset city dropdown
        }
    });



    $("#libraryRegistration").validate({
        //rules: {
        //    FirstName: {
        //        required: true,
        //        lettersOnly: true, // Custom method for letters only
        //        maxlength: 20
        //    },
        //    LastName: {
        //        lettersOnly: true // Custom method for letters only
        //    },
        //    Username: {
        //        required: true
        //    },
        //    Email: {
        //        required: true,
        //        email: true
        //    },
        //    Phone: {
        //        required: true,
        //        digits: true,
        //        minlength: 6,
        //        maxlength: 15
        //    },
        //    PasswordHash: {
        //        minlength: 6,
        //        required: function () {
        //            return $("#UserId").val() === ""; // Require password only if AdminId is empty (new admin)
        //        }
        //    }
        //},
        //messages: {
        //    FirstName: {
        //        required: "Please enter your first name."
        //    },
        //    Email: {
        //        required: "Please enter your email.",
        //        email: "Please enter a valid email address."
        //    },
        //    Phone: {
        //        required: "Please enter your phone number.",
        //        digits: "Please enter only numbers.",
        //        minlength: "Phone number must be at least 6 digits.",
        //        maxlength: "Phone number cannot exceed 15 digits."
        //    },
        //    PasswordHash: {
        //        minlength: "Password must have atleast 6 digits",
        //        required: "Password is required"
        //    },
        //    Username: {
        //        required: "Please enter admin Username"
        //    }
        //},

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);

            const btnRegister = $("#btnSubmit");
            const btnLoader = $("#btnLoader");
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");
            setTimeout(function () {

                // AJAX submission
                $.ajax({
                    url: '/AdminMaster/AddAdmin',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        alert(result.message);
                        if (result.success) {
                            window.location.href = '/AdminMaster/AdminList';
                        }
                    },
                    complete: function () {
                        // Re-enable button and hide loader
                        btnRegister.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function () {
                        alert('An error occurred while registering the user.');
                    }
                });
            }, 2000);
        }
    });


});
