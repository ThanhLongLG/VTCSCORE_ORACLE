using System.Drawing.Printing;
using BaoCaoDACS.Models;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
namespace BaoCaoDACS.Reponsitory
{
    public class EFGiaDaureponsitory : IGiaiDaureponsitory
    {

        private readonly AppDbContext _context;

        public EFGiaDaureponsitory(AppDbContext context)
        {
            _context = context;
        }
        //public async Task<IEnumerable<Tournament>> GetAllAsync(string? searchValue)
        //{
        //    var query = _context.Tournaments
        //        .Include(t => t.LoaiHinhThiDau)
        //        .AsQueryable();
        //    if (!string.IsNullOrEmpty(searchValue))
        //    {
        //        query = query.Where(c => c.Name.Contains(searchValue));
        //    }
        //    return await query.ToListAsync();
        //}
        public async Task<IEnumerable<V_Tournament_All>> GetAllAsync(string? searchValue)
        {
            // Truy vấn trực tiếp từ View, dữ liệu đã được JOIN sẵn từ Oracle
            var query = _context.V_Tournament_All.AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                // Oracle phân biệt hoa thường, nên dùng ToUpper để tìm kiếm chính xác
                query = query.Where(c => c.Name.ToUpper().Contains(searchValue.ToUpper()));
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Tournament>> GetAllAsync()
        {
            return await _context.Tournaments.ToListAsync();
        }

         public async Task UpdateTournamentStatusAsync()
        {
            await _context.Database.ExecuteSqlRawAsync(
                "BEGIN NHOM4.PRC_UPDATE_TOURNAMENT_STATUS(:p_id); END;",
                new OracleParameter("p_id", DBNull.Value)
            );
        }
       
        public async Task AddAsync(Tournament tournament)
        {
            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int tournamentId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($"BEGIN DELETE_TOURNAMENT({tournamentId}); END;");
        }

        public async Task<Tournament> GetByIdAsync(int tournamentId)
        {
            return await _context.Tournaments
                .Include(t => t.participant)
                .FirstOrDefaultAsync(t => t.TournamentID == tournamentId);
        }

        public async Task UpdateAsync(Tournament tournament)
        {
            _context.Tournaments.Update(tournament);
            await _context.SaveChangesAsync();
        }
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Tournaments.CountAsync();
        }
        public async Task SaveChangeAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

}
