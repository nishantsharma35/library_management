$(document).ready(function () {

    const username = "nishant_35"; // Replace with your Geonames username
    const stateGeonameId = 1269750;

    // Fetch States
    fetch(`http://api.geonames.org/childrenJSON?geonameId=${stateGeonameId}&username=${username}`)
        .then(response => response.json())
        .then(data => {
            const states = data.geonames;
            let stateOptions = `<option value="">Select State</option>`;
            states.forEach(state => {
                stateOptions += `<option value="${state.geonameId}">${state.name}</option>`;
            });
            $("#State").html(stateOptions);
        })
        .catch(error => {
            console.error("Error fetching states:", error);
        });

    // Fetch Cities when a state is selected
    $("#State").on("change", function () {
        const selectedStateGeonameId = $(this).val();
        if (selectedStateGeonameId) {
            fetch(`http://api.geonames.org/childrenJSON?geonameId=${selectedStateGeonameId}&username=${username}`)
                .then(response => response.json())
                .then(data => {
                    const cities = data.geonames;
                    let cityOptions = `<option value="">Select City</option>`;
                    cities.forEach(city => {
                        cityOptions += `<option value="${city.geonameId}">${city.name}</option>`;
                    });
                    $("#City").html(cityOptions);
                })
                .catch(error => {
                    console.error("Error fetching cities:", error);
                });
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
                alert(result.message);
                if (result.success) {
                    window.location.href = '/Dashboard';
                }
            },
            error: function () {
                alert('An error occurred while registering the user');
            }
        });
    });

});