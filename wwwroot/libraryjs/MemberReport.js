document.getElementById("exportReportBtn").addEventListener("click", function () {
    fetch('/ReportsMaster/ExportMemberReport', {
        method: 'GET'
    })
        .then(response => response.blob())
        .then(blob => {
            const link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = "Member_Report.xlsx";
            link.click();
        })
        .catch(error => console.error("Export Error:", error));
});