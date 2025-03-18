document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".approve-btn").forEach(button => {
        button.addEventListener("click", function () {
            var borrowId = this.dataset.id;
            console.log("Clicked Approve ID:", borrowId);  // Debugging ke liye
            fetch("/BorrowMaster/ApproveBorrow", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ borrowId: parseInt(borrowId) }) // Ensure number format
            })
                .then(response => response.json())
                .then(data => {
                    alert(data.message);
                    location.reload();
                })
                .catch(error => alert("Error: " + error));
        });
    });

    document.querySelectorAll(".reject-btn").forEach(button => {
        button.addEventListener("click", function () {
            var borrowId = this.dataset.id;
            console.log("Clicked Reject ID:", borrowId);  // Debugging ke liye
            fetch("/BorrowMaster/RejectBorrow", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ borrowId: parseInt(borrowId) }) // Ensure number format
            })
                .then(response => response.json())
                .then(data => {
                    alert(data.message);
                    location.reload();
                })
                .catch(error => alert("Error: " + error));
        });
    });
});
