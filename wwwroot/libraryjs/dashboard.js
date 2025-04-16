let chartInstance = null;
function populateYearDropdown(startYear = 2020) {
    const currentYear = new Date().getFullYear();
    const yearSelect = document.getElementById('yearSelect');

    // Clear existing options to prevent duplicates
    yearSelect.innerHTML = '';

    for (let year = currentYear; year >= startYear; year--) {
        const option = document.createElement('option');
        option.value = year;
        option.text = year;
        yearSelect.appendChild(option);
    }

    yearSelect.value = currentYear; // Set default value
    loadGraphByYear(); // ✅ Move this here
    loadBorrowReturnChart(currentYear);
    loadMemberBorrowReturnChart(currentYear);

}

function loadGraphByYear() {
    const selectedYear = document.getElementById("yearSelect").value;

    fetch(`/Dashboard/GetMonthlyUserStats?year=${selectedYear}`)
        .then(res => res.json())
        .then(data => {
            const ctx = document.getElementById("dashboardChart").getContext("2d");

            if (chartInstance) {
                chartInstance.destroy();
            }

            chartInstance = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: data.labels, // Month names
                    datasets: [
                        {
                            label: "Admins",
                            data: data.adminCounts,
                            backgroundColor: "#4e73df"
                        },
                        {
                            label: "Members",
                            data: data.memberCounts,
                            backgroundColor: "#1cc88a"
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        tooltip: {
                            callbacks: {
                                label: function (tooltipItem) {
                                    return `${tooltipItem.dataset.label}: ${tooltipItem.raw}`;
                                }
                            }
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            title: {
                                display: true,
                                text: 'User Count'
                            }
                        }
                    }
                }
            });
        });
}

document.addEventListener('DOMContentLoaded', function () {
    populateYearDropdown(); // This will also call loadGraphByYear() at the end
});



function loadSuperadminFineChart() {
    $.ajax({
        url: '/Dashboard/GetFinePaymentStatussuperadmin', // Make sure controller and action are correct
        type: 'GET',
        success: function (data) {
            const ctx = document.getElementById("myPieChart").getContext('2d');
            if (window.myPieChart) {
                window.myPieChart.destroy();
            }
            window.myPieChart = new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: data.labels,
                    datasets: [{
                        data: data.values,
                        backgroundColor: ['#4e73df', '#1cc88a'],
                        hoverBackgroundColor: ['#17a673', '#be2617'],
                        hoverBorderColor: "rgba(234, 236, 244, 1)",
                    }],
                },
                options: {
                    maintainAspectRatio: false,
                    tooltips: {
                        backgroundColor: "rgb(255,255,255)",
                        bodyFontColor: "#858796",
                        borderColor: '#dddfeb',
                        borderWidth: 1,
                        xPadding: 15,
                        yPadding: 15,
                        displayColors: false,
                        caretPadding: 10,
                    },
                    legend: {
                        display: true,
                    },
                    cutoutPercentage: 60,
                },
            });
        },
        error: function (err) {
            console.error("Error loading Fine Chart:", err);
        }
    });
}
$(document).ready(function () {
    loadSuperadminFineChart(); // Call the function when page is ready
});






function loadBorrowReturnChart(year) {
    $.ajax({
        url: '/Dashboard/GetMonthlyBorrowReturnStats',
        type: 'GET',
        data: { year: year },
        success: function (data) {
            const ctx = document.getElementById('borrowReturnChart').getContext('2d');

            if (window.myChart) {
                window.myChart.destroy();
            }

            window.myChart = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: data.labels, // ["Jan", "Feb", ...]
                    datasets: [
                        {
                            label: 'Books Borrowed',
                            backgroundColor: 'rgba(78, 115, 223, 0.8)',
                            borderColor: 'rgba(78, 115, 223, 1)',
                            borderWidth: 1,
                            data: data.borrowCounts,
                            barPercentage: 0.45,
                            categoryPercentage: 0.5
                        },
                        {
                            label: 'Books Returned',
                            backgroundColor: 'rgba(28, 200, 138, 0.8)',
                            borderColor: 'rgba(28, 200, 138, 1)',
                            borderWidth: 1,
                            data: data.returnCounts,
                            barPercentage: 0.45,
                            categoryPercentage: 0.5
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'top',
                        },
                        title: {
                            display: true,
                            text: `Borrow vs Return Stats (${year})`
                        }
                    },
                    scales: {
                        x: {
                            stacked: false
                        },
                        y: {
                            beginAtZero: true,
                            ticks: {
                                precision: 0 // For integer values only
                            }
                        }
                    }
                }
            });
        }
    });
}
function loadFineChart() {
    $.ajax({
        url: '/Dashboard/GetFinePaymentStatus', // Make sure controller and action are correct
        type: 'GET',
        success: function (data) {
            const ctx = document.getElementById("fineChart").getContext('2d');

            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: data.labels,
                    datasets: [{
                        data: data.values,
                        backgroundColor: ['#4e73df', '#1cc88a'],
                        hoverBackgroundColor: ['#17a673', '#be2617'],
                        hoverBorderColor: "rgba(234, 236, 244, 1)",
                    }],
                },
                options: {
                    maintainAspectRatio: false,
                    tooltips: {
                        backgroundColor: "rgb(255,255,255)",
                        bodyFontColor: "#858796",
                        borderColor: '#dddfeb',
                        borderWidth: 1,
                        xPadding: 15,
                        yPadding: 15,
                        displayColors: false,
                        caretPadding: 10,
                    },
                    legend: {
                        display: true,
                    },
                    cutoutPercentage: 60,
                },
            });
        },
        error: function (err) {
            console.error("Error loading Fine Chart:", err);
        }
    });
}
$(document).ready(function () {
    loadFineChart(); // Call the function when page is ready
});

function loadMemberBorrowReturnChart(year) {
    $.ajax({
        url: '/Dashboard/GetMonthlyMemberStats',
        type: 'GET',
        data: { year: year },
        success: function (data) {
            const ctx = document.getElementById('borrowMemberReturnChart').getContext('2d');

            if (window.myChart) {
                window.myChart.destroy();
            }

            window.myChart = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: data.labels, // ["Jan", "Feb", ...]
                    datasets: [
                        {
                            label: 'Books Borrowed',
                            backgroundColor: 'rgba(78, 115, 223, 0.8)',
                            borderColor: 'rgba(78, 115, 223, 1)',
                            borderWidth: 1,
                            data: data.borrowCounts,
                            barPercentage: 0.45,
                            categoryPercentage: 0.5
                        },
                        {
                            label: 'Books Returned',
                            backgroundColor: 'rgba(28, 200, 138, 0.8)',
                            borderColor: 'rgba(28, 200, 138, 1)',
                            borderWidth: 1,
                            data: data.returnCounts,
                            barPercentage: 0.45,
                            categoryPercentage: 0.5
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'top',
                        },
                        title: {
                            display: true,
                            text: `Borrow vs Return Stats (${year})`
                        }
                    },
                    scales: {
                        x: {
                            stacked: false
                        },
                        y: {
                            beginAtZero: true,
                            ticks: {
                                precision: 0 // For integer values only
                            }
                        }
                    }
                }
            });
        }
    });
}

function loadMemberFineChart() {
    $.ajax({
        url: '/Dashboard/GetMemberFineStatus', // Make sure controller and action are correct
        type: 'GET',
        success: function (data) {
            const ctx = document.getElementById("fineMemberChart").getContext('2d');

            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: data.labels,
                    datasets: [{
                        data: data.values,
                        backgroundColor: ['#4e73df', '#1cc88a'],
                        hoverBackgroundColor: ['#17a673', '#be2617'],
                        hoverBorderColor: "rgba(234, 236, 244, 1)",
                    }],
                },
                options: {
                    maintainAspectRatio: false,
                    tooltips: {
                        backgroundColor: "rgb(255,255,255)",
                        bodyFontColor: "#858796",
                        borderColor: '#dddfeb',
                        borderWidth: 1,
                        xPadding: 15,
                        yPadding: 15,
                        displayColors: false,
                        caretPadding: 10,
                    },
                    legend: {
                        display: true,
                    },
                    cutoutPercentage: 60,
                },
            });
        },
        error: function (err) {
            console.error("Error loading Fine Chart:", err);
        }
    });
}
$(document).ready(function () {
    if (document.getElementById("fineMemberChart")) {
        loadMemberFineChart();
    } else {
        console.warn("fineMemberChart canvas not found on page");
    }
});
