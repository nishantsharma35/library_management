﻿using library_management.Models;

namespace library_management.repository.internalinterface
{
    public interface libraryInterface
    {
        Task<object> AddmemberAsync(Member member); 
        Task<object> AddLibraryAsync(Models.Library library,int id); 
        Task<int?> GetUserIdByEmail(string email);

        Task<bool> GetAdmindetails(int id);
        
        Task<bool> ismembernameexitsAsync(string Membername);
        Task<bool> isemailexitsAsync(string Email);

        Task<bool> OtpVerification(string Otp);
        Task<object> updateStatus(string Email);

        Task<Member> GetUserDataByEmail(string email);

        Task<string> fetchEmail(string cred);

        Task<bool> IsVerified(string cred);

        Task<List<Member>> GetAllMembersAsync();

        Task<List<Member>> GetLibraryMembersAsync(int libraryId);

        Task<int?> GetLibraryIdByMemberAsync(int memberId);

        Task<bool> IsMemberInLibraryAsync(int memberId, int libraryId);

        Task<object> AddLibrarianAsync(Member librarian, int adminLibraryId);



        Task<Member> GetUserData(int id);
        Task<object> UpdateUserDetailsAsync(Member user);

        //Task<List<string>> getallstate();

        //Task<List<string>> getcities(string state1);
    }
}
