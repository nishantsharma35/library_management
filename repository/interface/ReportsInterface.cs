using library_management.DTO;

namespace library_management.repository.internalinterface
{
    public interface ReportsInterface
    {
        SuperAdminReportsDto GetSuperAdminReport(string libraryName, string bookTitle, int? minStock, int? maxStock, decimal? minFine, decimal? maxFine);

        LibraryAdminReportDto GetLibraryAdminReport(int libraryId);

        MemberReportDto GetMemberReport(int memberId);
    }
}
