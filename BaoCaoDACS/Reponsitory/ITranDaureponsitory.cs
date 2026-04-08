
using BaoCaoDACS.Models;

namespace BaoCaoDACS.Reponsitory
{
    public interface ITranDaureponsitory
    {
   
        Task<IEnumerable<V_Match_All>> GetAllAsync(string? searchValue);
        Task<IEnumerable<Match>> GetAllAsync();
        Task<Match> GetByIdAsync(string MatchId);
        Task AddAsync(Match match);
        Task DeleteAsync(string MatchId);
        Task UpdateAsync(Match khachHang);
        Task<int> GetTotalCountAsync();
    }
}
