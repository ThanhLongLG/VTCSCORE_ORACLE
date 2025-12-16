using System.Diagnostics;
using BaoCaoDACS.Models;
using BaoCaoDACS.Reponsitory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Formats.Asn1.AsnWriter;

namespace BaoCaoDACS.Controllers
{
    [Authorize]
    public class ChamDiemController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly IRankingService _rankingService;

        public ChamDiemController(
            ILogger<HomeController> logger,
            AppDbContext context,
            IRankingService rankingService)
        {
            _logger = logger;
            _context = context;
            _rankingService = rankingService;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GetMatch(string matchId)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(matchId))
                {
                    return BadRequest(new { Message = "Thiếu tham số MatchId" });
                }

                var match = _context.match
                    .Where(m => m.MatchId == matchId)
                    .Select(t => new
                    {
                        MatchId = t.MatchId,
                        Vongdau = t.Vongdau,
                        Date = t.Date.ToString("dd/MM/yyyy HH:mm"),
                        SanDau = t.SanDau,
                        Trongtai = t.Trongtai,
                        Hangcan = t.Hangcan,
                        LoaiHinhThiDau = t.LoaiHinhThiDau,
                        Tournament = t.TournamentID,
                        Scores = t.Socre.Select(s => new
                        {
                            s.ParticipantId,
                            s.Diem,
                            s.Kq,
                            s.KietQua,
                            s.Danhgia,
                            ParticipantName = s.participant.FullName // nếu bạn có cột "Hoten"
                        }).ToList()

                    })
                    .FirstOrDefault();

                if (match == null)
                {
                    return NotFound(new { Message = "Không tìm thấy trận đấu" });
                }

                return Json(match);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi API GetMatch");
                return StatusCode(500, new { Error = "Lỗi máy chủ" });
            }
        }
        public async Task<IActionResult> GetMatchParticipants(string matchId)
        {
            try
            {
                // Lấy danh sách điểm số theo matchId
                var scores = await _context.socre
                    .Where(s => s.MatchId == matchId)
                    .Include(s => s.participant)
                    .Include(s => s.match)
                    .ThenInclude(m => m.Tournament)
                    .ToListAsync();

                // Kiểm tra nếu không có điểm số
                if (scores == null || !scores.Any())
                {
                    return NotFound(new { message = "Không tìm thấy thông tin trận đấu" });
                }

                // Lấy thông tin người tham gia
                var participants = scores
                    .Select(s => new
                    {
                        ParticipantId = s.participant.ParticipantID,
                        Matchid = s.match.MatchId,
                        trangthai=s.match.trangthai,
                        HoTen = s.participant.FullName,
                        Clb = s.participant.Club,
                        Tuoi = s.participant.tuoi,
                        CanNang = s.participant.CanNang,
                        ChieuCao = s.participant.ChieuCao,
                        Giadau = s.match.Tournament?.Name,
                        SoTranThang = CalculateWins(s.participant.ParticipantID),
                        SoTranThua = CalculateLosses(s.participant.ParticipantID)
                    })
                    .ToList();

                switch (participants.Count)
                {
                    case 0:
                        return BadRequest(new { message = "Không có thông tin người tham gia" });

                    case 1:
                        return Ok(new
                        {
                            VanDongVien1 = participants[0]
                            
                        });

                    case 2:
                        return Ok(new
                        {
                            VanDongVien1 = participants[0],
                            VanDongVien2 = participants[1]
                        });

                    default:
                        // Nếu có nhiều hơn 2 VĐV, lấy 2 VĐV đầu tiên
                        return Ok(new
                        {
                            VanDongVien1 = participants[0],
                            VanDongVien2 = participants[1]
                        });
                }
            }
            catch (Exception ex)
            {
            
                _logger.LogError(ex, "Lỗi khi lấy thông tin người tham gia");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi xử lý yêu cầu" });
            }
        }



        // Phương thức hỗ trợ tính số trận thắng 
        private int CalculateWins(string participantId)
        {
           
            return _context.socre
                .Count(s => s.ParticipantId == participantId && s.Kq == 1); // Giả sử Kq = 1 là thắng
        }

        // Phương thức hỗ trợ tính số trận thua 
        private int CalculateLosses(string participantId)
        {
           
            return _context.socre
                .Count(s => s.ParticipantId == participantId && s.Kq == 0); // Giả sử Kq = 0 là thua
        }

        [HttpPost]
        public async Task<IActionResult> SubmitMatchResult([FromBody] Scoreupdate scoreData)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(scoreData.MatchId) ||
                    string.IsNullOrEmpty(scoreData.BlueParticipantId) ||
                    string.IsNullOrEmpty(scoreData.RedParticipantId))
                {
                    return BadRequest(new { message = "Thông tin trận đấu không đầy đủ" });
                }

                // Chuyển đổi điểm số
                float blueScoreValue = float.TryParse(scoreData.BlueScore, out float blueScore)
                    ? blueScore
                    : 0;
                float redScoreValue = float.TryParse(scoreData.RedScore, out float redScore)
                    ? redScore
                    : 0;

                // Xác định kết quả
                string kietQua1 = DetermineResultText(blueScoreValue, redScoreValue);
                byte kq1 = DetermineResultByte(blueScoreValue, redScoreValue);
                string kietQua2 = DetermineResultText(redScoreValue, blueScoreValue);
                byte kq2 = DetermineResultByte(redScoreValue, blueScoreValue);

                // Lưu kết quả cho Participant Xanh
                var socre = await _context.socre
                   .FirstOrDefaultAsync(p => p.ParticipantId == scoreData.BlueParticipantId);




                if (socre == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin Kết quả" });
                }
                else {



                    socre.Diem = blueScoreValue;
                    socre.Kq = kq1;
                    socre.KietQua = kietQua1;
                    socre.Danhgia = scoreData.BlueCautions.ToString();

                    await _context.SaveChangesAsync();

                }

                // Lưu kết quả cho Participant Đỏ
                var socre2 = await _context.socre
                  .FirstOrDefaultAsync(p => p.ParticipantId == scoreData.RedParticipantId);




                if (socre2 == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin Kết quả" });
                }
                else
                {
                    socre2.Diem = redScoreValue;
                    socre2.Kq = kq2;
                    socre2.KietQua = kietQua2;
                    socre2.Danhgia = scoreData.BlueCautions.ToString();

                    await _context.SaveChangesAsync();

                }
                var match = await _context.match
                  .FirstOrDefaultAsync(p => p.MatchId == scoreData.MatchId);
                if (match == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin Trận đấu" });
                }
                else
                {
                    match.trangthai = 1;
                    await _context.SaveChangesAsync();

                }

                // Log kết quả
                _logger.LogInformation($"Đã lưu kết quả trận đấu {scoreData.MatchId}. " +
                    $"Xanh: {blueScoreValue}, Đỏ: {redScoreValue}");

                // Cập nhật Elo sau khi chấm điểm
                await _rankingService.UpdateAfterMatchAsync(scoreData.MatchId);

                return Ok(new
                {
                    message = "Đã lưu kết quả thành công",
       
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu kết quả trận đấu");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lưu kết quả", error = ex.Message });
            }
        }

        // Các phương thức hỗ trợ
        private string DetermineResultText(float blueScore, float redScore)
        {
            if (blueScore > redScore) return "Thắng";
            else return "Thua";
            
        }

        private byte DetermineResultByte(float blueScore, float redScore)
        {
            if (blueScore > redScore) return 1; // Xanh thắng
            return 0; // Hòa
        }



        [HttpPost]
        public async Task<IActionResult> SubmitPerformanceScore([FromBody] PerformanceScoreDto performanceData)
        {
            try
            {
                // Tìm bản ghi socre tương ứng
                var socre = await _context.socre
                    .FirstOrDefaultAsync(s =>
                        s.ParticipantId == performanceData.ParticipantId &&
                        s.MatchId == performanceData.MatchId);



                if (socre == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin vận động viên" });
                }
                if (performanceData.loaitructiep != null)
                {
                    
                    socre.Diem = 0;
                }
                else
                {
                    // Lưu tổng điểm vào Diem
                    socre.Diem = performanceData.FinalScore;
                }

                // Lưu đánh giá vào Danhgia
                socre.Danhgia = performanceData.Danhgia;

               

                // Để trống KQ
                socre.Kq = null;

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                var match = await _context.match
                  .FirstOrDefaultAsync(p => p.MatchId == performanceData.MatchId);
                if (match == null)
                {
                    return NotFound(new { message = "Không tìm thấy thông tin Trận đấu" });
                }
                else
                {
                    match.trangthai = 1;
                    await _context.SaveChangesAsync();

                }
                return Ok(new
                {
                    message = "Chấm điểm biểu diễn thành công",
                    scoreId = socre.ScoreId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi chấm điểm", error = ex.Message });
            }
        }

    }
    public class PerformanceScoreDto
    {
        public float FinalScore { get; set; }
        public string Danhgia { get; set; }
        public string ParticipantId { get; set; }
        public string MatchId { get; set; }
        public string? loaitructiep { get; set; }
    }
    public class Scoreupdate
    {
        public string MatchId { get; set; }
        public string BlueParticipantId { get; set; }
        public string RedParticipantId { get; set; }
        public string BlueScore { get; set; }
        public string RedScore { get; set; }
        public int BlueCautions { get; set; }
        public int RedCautions { get; set; }
        public string Result { get; set; }
    }
}
