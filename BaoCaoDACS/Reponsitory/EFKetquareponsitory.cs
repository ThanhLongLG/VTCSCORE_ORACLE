using System.Drawing.Printing;
using BaoCaoDACS.Models;

using Microsoft.EntityFrameworkCore;

namespace BaoCaoDACS.Reponsitory
{
    public class EFKetquareponsitory : IKetquareponsitory
    {

        private readonly AppDbContext _context;

        public EFKetquareponsitory(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Socre>> GetAllAsync(string? searchValue)
        {
            var query = _context.socre
               .Include(s => s.participant)
                    .ThenInclude(p => p.Tournament)
               .Include(s => s.match)
                    .ThenInclude(s => s.LoaiHinhThiDau )
               .AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(s =>
                    s.participant.FullName.Contains(searchValue)
                );
            }
            return await query.ToListAsync();
        }
        public async Task<IEnumerable<Socre>> GetAllAsync()
        {
            return await _context.socre.ToListAsync();
        }

     
        public async Task AddAsync(Socre socre)
        {
            _context.socre.Add(socre);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int socreid)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"BEGIN SP_DELETE_SCORE({socreid}); END;");
        }


        public async Task<Socre> GetByIdAsync(int socreid)
        {
            return await _context.socre
                .Include(s => s.participant)
                    .ThenInclude(p => p.Tournament)
                .Include(s => s.match) 
                .FirstOrDefaultAsync(s => s.ScoreId == socreid);
        }

        public async Task UpdateAsync(Socre socre1)
        {
            _context.socre.Update(socre1);
            await _context.SaveChangesAsync();
        }
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.socre.CountAsync();
        }
    }
}
