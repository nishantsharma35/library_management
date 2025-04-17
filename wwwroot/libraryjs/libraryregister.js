$(document).ready(function () {

    const username = "nishant_35"; // Replace with your Geonames username
    const stateGeonameId = 1269750;

    // Fetch States
    fetch(`/library/states`)
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
            fetch(`cities/${selectedStateGeonameId}`)
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


    // ✅ Proper Form Submission Handling
    $("#libraryRegistration").submit(function (event) {
        event.preventDefault();
        console.log("here-1");

        const formdata =  new FormData(this); // ✅ Fix: Use `this`

        $.ajax({
            url: '/library/libraryRegistration',
            type: 'POST',
            processData: false,
            contentType: false,
            data: formdata,
            success: function (result) {
                showToast(result.message, "success");
                if (result.success) {
                    window.location.href = '/Dashboard';
                }
            },
            error: function () {
                showToast('An error occurred while registering the user');
            }
        });
    });

});