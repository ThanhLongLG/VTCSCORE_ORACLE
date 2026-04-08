using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaoCaoDACS.Models
{
    [Keyless]
    [Table("V_MATCH_ALL")] 
    public class V_Match_All
    {
        public string MatchId { get; set; }
        public string Vongdau { get; set; }
        public string SanDau { get; set; }
        public string Hangcan { get; set; }
        public string Trongtai { get; set; }
        public int? trangthai { get; set; }
        
        [Column("MATCHDATE")] // Bắt buộc viết hoa
        public DateTime MatchDate { get; set; }

        public int? TournamentID { get; set; }

        [Column("TOURNAMENTNAME")] // THÊM DÒNG NÀY ĐỂ FIX LỖI
        public string TournamentName { get; set; }

        public int? LoaiHinhThiDauId { get; set; }

        [Column("LOAIHINHNAME")] // THÊM DÒNG NÀY ĐỂ FIX LỖI
        public string LoaiHinhName { get; set; }

        // Mẹo: Nếu cột MonVo trong Oracle cũng bị báo lỗi tương tự sau khi chạy, 
        // hãy thêm luôn [Column("MONVO")] lên trên thuộc tính MonVo nhé.
        public string MonVo { get; set; } 
    }
}