using System.Drawing.Printing;
using BaoCaoDACS.Models;

using Microsoft.EntityFrameworkCore;

namespace BaoCaoDACS.Reponsitory
{
    public class EFTrandaureponsitory : ITranDaureponsitory
    {

        private readonly AppDbContext _context;

        public EFTrandaureponsitory(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<V_Match_All>> GetAllAsync(string? searchValue)
        {
            var query = _context.V_Match_Alls.AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(c => c.Vongdau.ToUpper().Contains(searchValue.ToUpper()));
            }

            // Không cần Include() gì nữa, dữ liệu tên giải và loại hình đã có sẵn!
            return await query.ToListAsync();
        }
        public async Task<IEnumerable<Match>> GetAllAsync()
        {
            return await _context.match
                 .Include(m => m.LoaiHinhThiDau)
                 .ToListAsync();
        }

     
        public async Task AddAsync(Match match)
        {
            _context.match.Add(match);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(string matchid)
        {
            var match1 = await _context.match.FindAsync(matchid);
            _context.match.Remove(match1);
            await _context.SaveChangesAsync();
        }

        public async Task<Match> GetByIdAsync(string matchid)
        {
            return await _context.match
           .Include(m => m.Tournament)
           .Include(m => m.LoaiHinhThiDau)
           .Include(m => m.Socre)
                .ThenInclude(m => m.participant)
           .FirstOrDefaultAsync(m => m.MatchId == matchid);
        }

        public async Task UpdateAsync(Match match)
        {
            _context.match.Update(match);
            await _context.SaveChangesAsync();
        }
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.match.CountAsync();
        }
    }
}
