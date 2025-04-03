document.addEventListener("DOMContentLoaded", function () {
    const libraryFilter = document.getElementById("libraryFilter");
    const memberFilter = document.getElementById("memberFilter");

    function filterTables() {
        let selectedLibrary = libraryFilter.value.toLowerCase();
        let selectedMember = memberFilter.value.toLowerCase();

        // Filter Library Fine Collection Table
        document.querySelectorAll("#libraryFineTable tbody tr").forEach(row => {
            let library = row.getAttribute("data-library").toLowerCase();
            row.style.display = (selectedLibrary === "" || library === selectedLibrary) ? "" : "none";
        });

        // Filter Member Fine Breakdown Table
        document.querySelectorAll("#memberFineTable tbody tr").forEach(row => {
            let library = row.getAttribute("data-library").toLowerCase();
            let member = row.getAttribute("data-member").toLowerCase();

            if ((selectedLibrary === "" || library === selectedLibrary) &&
                (selectedMember === "" || member === selectedMember)) {
                row.style.display = "";
            } else {
                row.style.display = "none";
            }
        });
    }

    libraryFilter.addEventListener("change", filterTables);
    memberFilter.addEventListener("change", filterTables);

    // 📤 Export Function for Excel
    function exportTableToExcel(tableId, filename) {
        let table = document.getElementById(tableId);
        let wb = XLSX.utils.book_new();
        let ws = XLSX.utils.table_to_sheet(table);

        XLSX.utils.book_append_sheet(wb, ws, "Sheet1");
        XLSX.writeFile(wb, filename + ".xlsx");
    }

    // Assign export function to buttons
    window.exportTableToExcel = exportTableToExcel;
});
