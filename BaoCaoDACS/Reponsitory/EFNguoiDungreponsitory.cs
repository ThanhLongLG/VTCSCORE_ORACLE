using System.Drawing.Printing;
using BaoCaoDACS.Models;
using BaoCaoDACS.Models;
using Microsoft.EntityFrameworkCore;

namespace BaoCaoDACS.Reponsitory
{
    public class EFNguoiDungreponsitory : INguoidungreponsitory
    {

        private readonly AppDbContext _context;

        public EFNguoiDungreponsitory(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Participant>> GetAllAsync(string? searchValue)
        {
            var query = _context.Participants.AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(c => c.FullName.Contains(searchValue));
            }

            return await query.ToListAsync();
        }
        public async Task<IEnumerable<Participant>> GetAllAsync()
        {
            return await _context.Participants.ToListAsync();
        }

        public async Task<Participant?> GetByUserIdAsync(string userId)
        {
            return await _context.Participants.FirstOrDefaultAsync(kh => kh.ParticipantID == userId);
        }
        public async Task AddAsync(Participant participant)
        {
            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(string Makh)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"BEGIN DELETE_PARTICIPANT({Makh}); END;");
        }

        public async Task<Participant> GetByIdAsync(string Makh)
        {
            return await _context.Participants.FindAsync(Makh);
        }

        public async Task UpdateAsync(Participant participant)
        {
            _context.Participants.Update(participant );
            await _context.SaveChangesAsync();
        }
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Participants.CountAsync();
        }
    }
}
