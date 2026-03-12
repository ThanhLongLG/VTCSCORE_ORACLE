
using BaoCaoDACS.Models;

namespace BaoCaoDACS.Reponsitory
{
    public interface IGiaiDaureponsitory
    {
        Task<IEnumerable<V_Tournament_All>> GetAllAsync(string? searchValue);
        Task<IEnumerable<Tournament>> GetAllAsync();
        Task<Tournament> GetByIdAsync(int tournamentId);
        Task AddAsync(Tournament tournament);
        Task DeleteAsync(int tournamentId);
        Task UpdateAsync(Tournament tournament);
        Task SaveChangeAsync();
        Task<int> GetTotalCountAsync();
    }
        
}
