using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.EntityFrameworkCore;

namespace library_management.repository.classes
{
    public class MembershipClass : MembershipInterface
    {
        private readonly dbConnect _connect;

        public MembershipClass(dbConnect connect)
        {
            _connect = connect;
        }

        public async Task<bool> AddMembership(Membership membership)
        {
            await _connect.Memberships.AddAsync(membership);
            return await _connect.SaveChangesAsync()>0;
        }

        public async Task<List<Membership>> GetAllMemberships(bool includePending = false)

        {
            var query = _connect.Memberships.AsQueryable();  // AsQueryable() ensures query is not null

            // Include Member and Library if needed
            query = query.Include(m => m.Member)
                         .Include(m => m.Library);

            if (!includePending) // ✅ Agar includePending false hai to sirf approved memberships lo
            {
                query = query.Where(m => m.IsActive != null && m.IsActive == true); // ✅ Null check + IsActive true hona chahiye
            }

            return await query.ToListAsync();

            // return await _connect.Memberships.Include(m =>m.Member).Include(m=>m.Library).ToListAsync();
        }

        public async Task<Membership?> GetMembershipById(int id)
        {
            return await _connect.Memberships.FirstOrDefaultAsync(m=>m.MembershipId == id);
        }

        public async Task<List<Membership>> GetMembershipsByMemberIdAsync(int memberId)
        {
            return await _connect.Memberships
                .Where(m => m.MemberId == memberId)
                .ToListAsync();
        }

    }
}
