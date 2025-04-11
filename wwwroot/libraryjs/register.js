$(document).ready(function () {

    // JavaScript to preview image
    document.getElementById('profilefile').addEventListener('change', function (e) {
        var file = e.target.files[0];
        if (file) {
            var reader = new FileReader();
            reader.onload = function (event) {
                var imagePreview = document.getElementById('imagePreview');
                imagePreview.src = event.target.result;
                imagePreview.style.display = 'block';
            };
            reader.readAsDataURL(file);
        }
    });

        const phoneInputField = document.querySelector("#Phoneno");
        phoneInput = window.intlTelInput(phoneInputField, {
            initialCountry: "IN", // Set initial country (auto or a specific code like "us")
            geoIpLookup: function (callback) {
                fetch('https://ipapi.co/json', { mode: 'no-cors' })
                    .then((response) => response.json())
                    .then((data) => callback(data.country_code))
                    .catch(() => callback("us"));
            },
            utilsScript: "https://cdnjs.cloudflare.com/ajax/libs/intl-tel-input/17.0.8/js/utils.js"
        });



    //$('#showPasswordCheckbox').on('change', function () {
    //    const passwordField = document.getElementById("Password");
    //    const confirmPasswordField = document.getElementById("Confirmpassword");
    //    if (this.checked) {
    //        passwordField.type = "text";
    //        confirmPasswordField.type = "text";
    //    } else {
    //        passwordField.type = "password";
    //        confirmPasswordField.type = "password";
    //    }
    //});


    document.querySelectorAll('.toggle-password').forEach(toggle => {
        toggle.addEventListener('click', () => {
            // Locate the password input within the same parent (.input-wrap)
            const passwordInput = toggle.closest('.input-wrap').querySelector('.pass-input');

            // Toggle input type between "password" and "text"
            if (passwordInput.type === "password") {
                passwordInput.type = "text";
                toggle.classList.remove("fa-lock");
                toggle.classList.add("fa-lock-open");
            } else {
                passwordInput.type = "password";
                toggle.classList.remove("fa-lock-open");
                toggle.classList.add("fa-lock");
            }
        });
    });


    const username = "nishant_35"; // Replace with your Geonames username
    const stateGeonameId = 1269750; // ID for India

    // Fetch States
    fetch(`states`)
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



    $('#Register').validate({
        rules: {
            Name: {
                required: true,
                minlength: 3,
                maxlength:20
            },
            Email: {
                required: true,
                email: true
            },
            Gender: {
                required: true,
            },
            Phoneno: {
                required: true,
                minlength:6,
                maxlength:16
            },
            address: {
                required: true,
                minlength: 7,
                maxlength: 50
            },
            State: {
                required: true
            },
            City: {
                required: true
            },
            Joiningdate: {
                required: true
            },
            Password: {
                required: true,
                minlength: 7,
                maxlength: 15
            },
            Confirmpassword: {
                required: true,
                minlength: 7,
                maxlength: 15,
                equalTo:"#passwordField"
            },
            RoleId: {
                required: true
            },
        },
        message: {
            Name    : {
                required: "please enter your name",
                minlength: "you have to add atleast 3 character",
                maxlength: "you have reached max lenght"
            },
            Email: {
                required: "please enter your email",
                email :"please enter validate email address"
            },
            Gender: {
                required:"please enter your gender"
            },
            Phoneno: {
                required: "please enter your phone no",
                minlength: "minimum length of phone is 7 number",
                maxlength: "you have reached your maximum length"
            },
            address: {
                required: "please enter your address",
                minlength: "minimum length of address is 7 character",
                maxlength: "you have reached your maximum length"
            },
            State: {
                required:"please enter your state"
            },
            City: {
                required:"please enter your city"
            },
            Password: {
                required: "please enter your password",
                minlength: "minimum length of password is 7",
                maxlength:"maximum length of password is 15"
            },
            Confirmpassword: {
                required: "please enter your password",
                minlength: "minimum length of password is 7",
                maxlength: "maximum length of password is 15",
                equalTo: "Passwords do not match" // Error message when passwords don't match

            },
            RoleId: {
                required: "Please enter your role"
            }

        },


        submitHandler: function (form, event) {
            event.preventDefault()
            console.log("here-1")
            const formdata = new FormData(form);

            const formattedPhoneNumber = phoneInput.getNumber(intlTelInputUtils.numberFormat.E164); // E.164 format
            formdata.set("Phoneno", formattedPhoneNumber); 



            $.ajax({
                url: '/library/Register',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formdata,
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        window.location.href = '/library/Otpcheck';
                    }
                },
                error: function () {
                    alert('an occured while registering the user');
                }
            });
        }
    });

});
