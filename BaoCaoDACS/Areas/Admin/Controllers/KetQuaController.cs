using System.Drawing.Printing;
using System.Text.Json;
using BaoCaoDACS.Areas.Admin.Models;
using BaoCaoDACS.Models;
using BaoCaoDACS.Reponsitory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Formats.Asn1.AsnWriter;

namespace BAO_CAO.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class KetQuaController : Controller
    {
        private readonly INguoidungreponsitory _nguoidungreponsitory;
        private readonly ITranDaureponsitory _tranDaureponsitory;
        private readonly IKetquareponsitory _ketquareponsitory;
        private readonly ILoaiHinhreponsitory _loaihreponsitory;
        private readonly ILogger<KetQuaController> _logger;
        private readonly AppDbContext _context;
        private readonly IRankingService _rankingService;
        public KetQuaController(
            INguoidungreponsitory nguoidungreponsitory,
            ILogger<KetQuaController> logger,
            AppDbContext context,
            ITranDaureponsitory tranDaureponsitory,
            IKetquareponsitory ketquareponsitory,
            ILoaiHinhreponsitory loaihreponsitory,
            IRankingService rankingService)
        {
            _tranDaureponsitory = tranDaureponsitory;
            _ketquareponsitory = ketquareponsitory;
            _loaihreponsitory = loaihreponsitory;
            _nguoidungreponsitory = nguoidungreponsitory;
            _logger = logger;
            _context = context;
            _rankingService = rankingService;
        }

        // Danh sách kết quả
        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                // Sử dụng phương thức GetAllAsync từ repository
                var ketQuas = await _ketquareponsitory.GetAllAsync(searchString);

                // Tổng số bản ghi
                ViewBag.TotalCount = await _ketquareponsitory.GetTotalCountAsync();

                // Truyền search value để giữ lại giá trị tìm kiếm
                ViewBag.SearchValue = searchString;

                return View(ketQuas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách kết quả");
                return View("Error");
            }
        }

       

        // Phương thức hỗ trợ chuẩn bị dropdown
        private async Task PrepareDropdownLists(Socre score)
        {
            // Chuẩn bị danh sách Participant
            ViewBag.Participants = (await _context.Participants.ToListAsync())
                .Select(p => new SelectListItem
                {
                    Value = p.ParticipantID,
                    Text = p.FullName,
                    Selected = p.ParticipantID == score.ParticipantId
                })
                .ToList();

            // Chuẩn bị danh sách Match
            ViewBag.Matches = (await _context.match.ToListAsync())
                .Select(m => new SelectListItem
                {
                    Value = m.MatchId,
                    Text = $"Trận {m.MatchId}",
                    Selected = m.MatchId == score.MatchId
                })
                .ToList();
        }

        private async Task<IEnumerable<SelectListItem>> GetTournamentSelectList(int? selectedId = null)
        {
            return (await _context.Tournaments.ToListAsync())
                .Select(t => new SelectListItem
                {
                    Value = t.TournamentID.ToString(),
                    Text = t.Name,
                    Selected = t.TournamentID == selectedId
                })
                .ToList();
        }
        private async Task<IEnumerable<SelectListItem>> GetParticipantByTournament(int? tournamentId)
        {
            if (tournamentId == null)
                return new List<SelectListItem>();

            return await _context.Participants
                .Where(p => p.TournamentID == tournamentId)
                .Select(p => new SelectListItem
                {
                    Value = p.ParticipantID,
                    Text = p.FullName
                })
                .ToListAsync();
        }


       
        private async Task<IEnumerable<SelectListItem>> GetMatchSelectListByTournament(int? tournamentId)
        {
            if (tournamentId == null)
                return new List<SelectListItem>();

            return await _context.match
                .Where(m => m.TournamentID == tournamentId)
                .Select(m => new SelectListItem
                {
                    Value = m.MatchId,
                    Text = $"Trận {m.MatchId} - {m.Date} - {m.LoaiHinhThiDau.Name}"
                })
                .ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetParticipantSelectList(string selectedId = "")
        {
            return (await _context.Participants.ToListAsync())
                .Select(p => new SelectListItem
                {
                    Value = p.ParticipantID,
                    Text = p.FullName,
                    Selected = p.ParticipantID == selectedId
                })
                .ToList();
        }

        private async Task<IEnumerable<SelectListItem>> GetMatchSelectList(string selectedId = "")
        {
  
            return (await _context.match
                .Include(m => m.LoaiHinhThiDau)
                .ToListAsync())
                   .Select(m => new SelectListItem
                   {
                       Value = m.MatchId,
                       Text = $"Trận {m.MatchId}-{m.Vongdau}-{m.Date}--{m.LoaiHinhThiDau?.Name}", 
                       Selected = m.MatchId == selectedId
                   })
                   .ToList();
        }

        // Chi tiết kết quả
        public async Task<IActionResult> Display(int id)
        {
            var ketQua = await _ketquareponsitory.GetByIdAsync(id);

            if (ketQua == null)
            {
                return NotFound();
            }

            return View(ketQua);
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                // Lấy thông tin điểm số hiện tại
                var score = await _ketquareponsitory.GetByIdAsync(id);
                if (score == null)
                {
                    return NotFound();
                }

                // Chuẩn bị dropdown
                await PrepareDropdownListsForScore(score);

                return View(score);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang cập nhật điểm số");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Socre score)
        {
          
            // Kiểm tra ID
            if (id != score.ScoreId)
            {
                return NotFound();
            }


            _logger.LogInformation($"Received values: ParticipantId={score.ParticipantId}, MatchId={score.MatchId}");

            if (!ModelState.IsValid)
            {
                // Log chi tiết lỗi
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) });
                _logger.LogWarning($"Validation errors: {JsonSerializer.Serialize(errors)}");
            }


            try
            {
                // Lấy đối tượng hiện tại
                var existingScore = await _ketquareponsitory.GetByIdAsync(id);
                if (existingScore == null)
                {
                    return NotFound();
                }

                // Cập nhật từng thuộc tính
                existingScore.KietQua = score.KietQua;
                existingScore.Danhgia = score.Danhgia;
                existingScore.Diem = score.Diem;
                existingScore.Kq = score.Kq;
                existingScore.ParticipantId = score.ParticipantId;
                existingScore.MatchId = score.MatchId;


                // Lưu thay đổi
                await _ketquareponsitory.UpdateAsync(existingScore);

                // Thông báo thành công
                TempData["SuccessMessage"] = "Cập nhật điểm số thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết
                _logger.LogError(ex, "Chi tiết lỗi cập nhật: {ErrorMessage}", ex.Message);

                // Thêm lỗi vào ModelState
                ModelState.AddModelError("", "Có lỗi xảy ra. Vui lòng thử lại.");
                await PrepareDropdownListsForScore(score);
                return View(score);
            }
        }


        private async Task PrepareDropdownListsForScore(Socre score)
        {
            // Lấy danh sách Participants
            ViewBag.Participants = (await _nguoidungreponsitory.GetAllAsync())
                .Select(p => new SelectListItem
                {
                    Value = p.ParticipantID,
                    Text = p.FullName,
                    Selected = p.ParticipantID == score.ParticipantId
                })
                .ToList();

            // Lấy danh sách Matches
            ViewBag.Matches = (await _tranDaureponsitory.GetAllAsync())
                .Select(m => new SelectListItem
                {
                    Value = m.MatchId,
                    Text = $"Trận {m.Vongdau}-{m.Date}-{m.LoaiHinhThiDau?.Name}",
                    Selected = m.MatchId == score.MatchId
                })
                .ToList();
        }

        // Xóa kết quả
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var ketQua = await _ketquareponsitory.GetByIdAsync(id);

            if (ketQua == null)
            {
                return NotFound();
            }

            return View(ketQua);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var score = await _ketquareponsitory.GetByIdAsync(id);
                var match = await _context.match.FirstOrDefaultAsync(m => m.MatchId == score.MatchId);


                await _ketquareponsitory.DeleteAsync(id);
                //  reset Elo cho giải
                if (match?.TournamentID != null)
                {
                    await _rankingService.RebuildTournamentAsync(match.TournamentID.Value);
                }


                TempData["SuccessMessage"] = "Xóa kết quả thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa kết quả");
                TempData["ErrorMessage"] = "Không thể xóa kết quả này.";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task PrepareDropdowns(Socre? selectedKetQua = null)
        {
            // Danh sách participant
            ViewBag.Participants = await _context.Participants
                .Select(p => new SelectListItem
                {
                    Value = p.ParticipantID.ToString(),
                    Text = p.FullName,
                    Selected = selectedKetQua != null && p.ParticipantID == selectedKetQua.ParticipantId
                })
                .ToListAsync();

            // Danh sách match
            ViewBag.Matches = await _context.match
                .Select(m => new SelectListItem
                {
                    Value = m.MatchId.ToString(),
                    Text = $"Trận {m.MatchId} - {m.Date:dd/MM/yyyy}",
                    Selected = selectedKetQua != null && m.MatchId == selectedKetQua.MatchId
                })
                .ToListAsync();
        }
        private async Task PrepareDropdowns(string selectedParticipantId = "", string selectedMatchId = "")
        {
            // Danh sách Participant
            ViewBag.Participants = await _context.Participants
                .Select(p => new SelectListItem
                {
                    Value = p.ParticipantID,
                    Text = $"{p.FullName} - {p.tuoi}",
                    Selected = p.ParticipantID == selectedParticipantId
                })
                .ToListAsync();

            // Danh sách Match
            ViewBag.Matches = await _context.match
                .Select(m => new SelectListItem
                {
                    Value = m.MatchId,
                    Text = $"Trận {m.MatchId} - {m.Date:dd/MM/yyyy}",
                    Selected = m.MatchId == selectedMatchId
                })
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> SuggestSecondParticipant(string matchId, string firstParticipantId, int take = 10)
        {
            if (string.IsNullOrWhiteSpace(matchId) || string.IsNullOrWhiteSpace(firstParticipantId))
                return BadRequest(new { message = "Thiếu matchId hoặc firstParticipantId" });

            var match = await _context.match.AsNoTracking()
                .FirstOrDefaultAsync(m => m.MatchId == matchId);

            if (match == null || match.TournamentID == null)
                return NotFound(new { message = "Không tìm thấy trận hoặc trận không thuộc giải đấu" });

            var first = await _context.Participants.AsNoTracking()
                .FirstOrDefaultAsync(p => p.ParticipantID == firstParticipantId);

            if (first?.UserId == null)
                return NotFound(new { message = "Không tìm thấy User của VĐV #1" });

            // gợi ý theo Elo + cân nặng + chiều cao + tuổi (bạn đã implement)
            var suggestions = await _rankingService.SuggestOpponentsAsync(first.UserId, match.TournamentID.Value, take);

            // Loại bỏ trường hợp đã được thêm vào match (để không trùng)
            var existParticipantIds = await _context.socre
                .Where(s => s.MatchId == matchId)
                .Select(s => s.ParticipantId)
                .ToListAsync();

            suggestions = suggestions
                .Where(x => !existParticipantIds.Contains(x.ParticipantId))
                .ToList();

            return Ok(suggestions);
        }

        [HttpGet]
        public async Task<IActionResult> Add(int? tournamentId = null, string matchId = null)
        {
            var model = new Socre();

            if (!string.IsNullOrEmpty(matchId))
            {
                var match = await _context.match.FirstOrDefaultAsync(m => m.MatchId == matchId);
                if (match == null)
                {
                    ModelState.AddModelError("", "Trận đấu không tồn tại");
                    return View(model);
                }

                tournamentId = match.TournamentID;
                model.MatchId = matchId;
            }

            ViewBag.Tournaments = await GetTournamentSelectList(tournamentId);
            ViewBag.Matches = await GetMatchSelectListByTournament(tournamentId);
            ViewBag.Participants = await GetParticipantByTournament(tournamentId);

            await PrepareDropdownsForAdd(model);

            // PHẦN GỢI Ý VĐV THỨ 2 – BẠN ĐÃ ĐẶT ĐÚNG CHỖ
            if (!string.IsNullOrEmpty(matchId))
            {
                var existing = await _context.socre.AsNoTracking()
                    .Where(s => s.MatchId == matchId)
                    .OrderBy(s => s.ScoreId)
                    .Select(s => s.ParticipantId)
                    .ToListAsync();

                if (existing.Count == 1)
                {
                    ViewBag.FirstParticipantId = existing[0];
                    ViewBag.MatchId = matchId;
                }
            }

            return View(model);
        }


        private async Task PrepareDropdownsForAdd(Socre score)
        {
            int? tournamentId = null;

            
            if (!string.IsNullOrEmpty(score.MatchId))
            {
                var match = await _context.match
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.MatchId == score.MatchId);

                tournamentId = match?.TournamentID;
            }

            ViewBag.Tournaments = await GetTournamentSelectList(tournamentId);
            ViewBag.Matches = await GetMatchSelectListByTournament(tournamentId);
            ViewBag.Participants = await GetParticipantByTournament(tournamentId);
        }
        [HttpGet]
        public async Task<IActionResult> GetMatchesByTournament(int tournamentId)
        {
            var matches = await _context.match
                .Where(m => m.TournamentID == tournamentId)
                .Include(m => m.LoaiHinhThiDau)
                .ToListAsync();

            var options = matches.Select(m => new SelectListItem
            {
                Value = m.MatchId,
                Text = $"Trận {m.MatchId} - {m.Date:dd/MM/yyyy} - {m.LoaiHinhThiDau?.Name}"
            }).ToList();

            // Trả về HTML <option> để dùng với .load()
            return PartialView("_Options", options);
        }

        [HttpGet]
        public async Task<IActionResult> GetParticipantsByTournament(int tournamentId)
        {
            var participants = await _context.Participants
                .Where(p => p.TournamentID == tournamentId)
                .ToListAsync();

            var options = participants.Select(p => new SelectListItem
            {
                Value = p.ParticipantID,
                Text = p.FullName
            }).ToList();

            return PartialView("_Options", options);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Socre score, string matchId = null)
        {
            try
            {
               
                if (!string.IsNullOrEmpty(matchId))
                {
                    score.MatchId = matchId;
                }

                ModelState.Remove("participant");
                ModelState.Remove("match");

                var existingScore = await _context.socre
                    .FirstOrDefaultAsync(s =>
                        s.ParticipantId == score.ParticipantId &&
                        s.MatchId == score.MatchId);

                if (existingScore != null)
                {
                    ModelState.AddModelError("", "Vận động viên này đã có điểm số trong trận đấu này.");
                    await PrepareDropdownsForAdd(score);
                    ViewBag.MatchId = matchId;
                    return View(score);
                }

                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.ParticipantID == score.ParticipantId);

                if (participant == null)
                {
                    ModelState.AddModelError("ParticipantId", "Vận động viên không tồn tại.");
                    await PrepareDropdownsForAdd(score);
                    ViewBag.MatchId = matchId;  
                    return View(score);
                }

                var match = await _context.match
                    .Include(m => m.LoaiHinhThiDau)
                    .FirstOrDefaultAsync(m => m.MatchId == score.MatchId);

                if (match == null)
                {
                    ModelState.AddModelError("MatchId", "Trận đấu không tồn tại.");
                    await PrepareDropdownsForAdd(score);
                    ViewBag.MatchId = matchId;
                    return View(score);
                }

                if (string.IsNullOrEmpty(score.ParticipantId))
                {
                    ModelState.AddModelError("ParticipantId", "Vui lòng chọn vận động viên");
                }

                if (string.IsNullOrEmpty(score.MatchId))
                {
                    ModelState.AddModelError("MatchId", "Vui lòng chọn trận đấu");
                }

                if (!ModelState.IsValid)
                {
                    await PrepareDropdownsForAdd(score);
                    ViewBag.MatchId = matchId;
                    return View(score);
                }

                // Thêm điểm số
                await _ketquareponsitory.AddAsync(score);


                bool isDoiKhang = match?.LoaiHinhThiDau?.Name == "Đối Kháng";
                int soLuongScore = await _context.socre.CountAsync(s => s.MatchId == score.MatchId);
                _logger.LogInformation($"Loại hình: {isDoiKhang}, Số kết quả hiện tại: {soLuongScore}");
                if (isDoiKhang && soLuongScore < 2)
                {
                    TempData["InfoMessage"] = "Hãy thêm kết quả cho vận động viên tiếp theo!";
                    return RedirectToAction("Add", new { matchId = matchId });
                }
                else
                {
                    TempData["SuccessMessage"] = "Đã thêm điểm số thành công!";
                    // Quay lại trang chi tiết trận đấu hoặc danh sách kết quả của trận đấu đó
                    return RedirectToAction("Display", "TranDau", new { area = "Admin", id = matchId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm điểm số");
                await PrepareDropdownsForAdd(score);
                ViewBag.MatchId = matchId;
                ModelState.AddModelError("", "Có lỗi xảy ra. Vui lòng thử lại.");
                return View(score);
            }
        }
    }
}
