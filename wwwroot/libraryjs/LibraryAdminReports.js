document.addEventListener("DOMContentLoaded", function () {
    // ✅ Export Library Report from Backend
    function exportLibraryAdminReport() {
        fetch('/ReportsMaster/ExportLibraryAdminReport', {
            method: 'GET'
        })
            .then(response => response.blob())
            .then(blob => {
                const link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = "LibraryAdminReports.xlsx";
                link.click();
            })
            .catch(error => console.error("Export Error:", error));
    }

    // ✅ Buttons Ke Event Listeners
    document.getElementById("exportLibraryReportBtn").addEventListener("click", exportLibraryAdminReport);
});
