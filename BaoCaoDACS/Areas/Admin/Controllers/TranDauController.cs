using System.Drawing.Printing;
using BaoCaoDACS.Areas.Admin.Models;
using BaoCaoDACS.Models;
using BaoCaoDACS.Reponsitory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BAO_CAO.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class TranDauController : Controller
    {

        private readonly INguoidungreponsitory _nguoidungreponsitory;
        private readonly ITranDaureponsitory _tranDaureponsitory;
        private readonly IKetquareponsitory _ketquareponsitory;
        private readonly ILoaiHinhreponsitory _loaihreponsitory;
        private readonly ILogger<NguoiDungController> _logger;
        private readonly AppDbContext _context;
        private readonly IRankingService _rankingService;
        public TranDauController(
            INguoidungreponsitory nguoidungreponsitory, 
            ILogger<NguoiDungController> logger,
            AppDbContext context,
            ITranDaureponsitory tranDaureponsitory,
            IKetquareponsitory ketquareponsitory,
            ILoaiHinhreponsitory loaihreponsitory,
            IRankingService rankingService
            )
           
        {
            _tranDaureponsitory = tranDaureponsitory;
            _ketquareponsitory = ketquareponsitory;
            _loaihreponsitory = loaihreponsitory;
            _nguoidungreponsitory= nguoidungreponsitory;
                 _logger = logger;
            _context = context;
            _rankingService= rankingService ;
        }

        public async Task<IActionResult> Index(string? searchValue)
        {
            var Match = await _tranDaureponsitory.GetAllAsync(searchValue);
           
            ViewBag.searchValue = searchValue;  
            return View(Match);
        }

    public async Task<IActionResult> Add()
    {
                var loaiHinhThiDauList = (await _loaihreponsitory.GetAllAsync())
                   .Select(l => new SelectListItem
                   {
                       Value = l.LoaiHinhThiDauId.ToString(),
                       Text = $"{l.Name} - {l.MonVo}"
                   })
                   .ToList();
                ViewBag.LoaiHinhThiDauList = loaiHinhThiDauList;


                // Tạo danh sách giải đấu cho dropdown
                ViewBag.Tournaments = _context.Tournaments
                    .Select(t => new SelectListItem
                    {
                        Value = t.TournamentID.ToString(), // Convert int to string for dropdown
                        Text = t.Name
                    })
                    .ToList();

              
                return View();
    }
     

        [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Add(Match match)
            {
                try
                {
                    _logger.LogInformation($"Đang thêm tran dau: ID={match.MatchId},{match.LoaiHinhThiDauId}");
                    var loaiHinhThiDau = await _context.loaiHinhThiDau.FindAsync(match.LoaiHinhThiDauId);
                    if (loaiHinhThiDau == null)
                    {
                        ModelState.AddModelError("LoaiHinhThiDauId", "Loại hình thi đấu không tồn tại");
                    }
                    if (match.LoaiHinhThiDauId == 0) 
                    {
                        ModelState.AddModelError("LoaiHinhThiDauId", "Chưa chọn loại hình thi đấu");
                    }
                if (!ModelState.IsValid)
                    {
                        // Lấy lại danh sách giải đấu khi form không hợp lệ
                        PrepareToumamentDropdown(match.TournamentID);
                        return View(match);
                    }

                    // Kiểm tra ID đã tồn tại
                    if (await _tranDaureponsitory.GetByIdAsync(match.MatchId) != null)
                    {
                        ModelState.AddModelError("matchID", "Mã trận đấu này đã tồn tại");
                        PrepareToumamentDropdown(match.TournamentID);
                        return View(match);
                    }

                    ViewBag.LoaiHinhThiDauList = await GetLoaiHinhThiDauSelectList();
                    await _tranDaureponsitory.AddAsync(match);
                    TempData["SuccessMessage"] = "Đã thêm trận đấu thành công!";
                    return RedirectToAction("Add", "KetQua", new { area = "Admin", matchId = match.MatchId });
            }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi thêm rận đấu");
                    ModelState.AddModelError("", "Có lỗi xảy ra. Vui lòng thử lại.");
                    PrepareToumamentDropdown(match.TournamentID);
                    ViewBag.LoaiHinhThiDauList = await GetLoaiHinhThiDauSelectList(match.LoaiHinhThiDauId);
                    return View(match);
                }
            }
            private async Task<IEnumerable<SelectListItem>> GetLoaiHinhThiDauSelectList(int selectedId = 0)
            {
                return (await _loaihreponsitory.GetAllAsync())
                    .Select(l => new SelectListItem
                    {
                        Value = l.LoaiHinhThiDauId.ToString(),
                        Text = $"{l.Name} - {l.MonVo}",
                        Selected = l.LoaiHinhThiDauId == selectedId
                    })
                    .ToList();
            }

            // Phương thức helper để chuẩn bị dropdown giải đấu, sử dụng int? thay vì string
            private void PrepareToumamentDropdown(int? selectedTournamentId = null)
            {
                var tournaments = _context.Tournaments
                    .Select(t => new SelectListItem
                    {
                        Value = t.TournamentID.ToString(),
                        Text = t.Name,
                        Selected = t.TournamentID == selectedTournamentId
                    })
                    .ToList();

                // Thêm tùy chọn "Không tham gia giải đấu"
                tournaments.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Không tham gia giải đấu --",
                    Selected = selectedTournamentId == null
                });

                ViewBag.Tournaments = tournaments;
            }
        public async Task<IActionResult> Display(string id)
        {
            var match = await _tranDaureponsitory.GetByIdAsync(id);
            if (match == null)
            {
                return NotFound();
            }
            return View(match);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var match = await _tranDaureponsitory.GetByIdAsync(id);
            if (match == null)
            {
                return NotFound();
            }
            return View(match); // Hiển thị trang xác nhận xóa
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _tranDaureponsitory.DeleteAsync(id);
            return RedirectToAction(nameof(Index)); // Hoặc trang khác sau khi xóa
        }





        public async Task<IActionResult> Update(string id)
        {
            var match = await _tranDaureponsitory.GetByIdAsync(id);
            if (match == null)
            {
                return NotFound();
            }
            var loaiHinhThiDauList = (await _loaihreponsitory.GetAllAsync())
                .Select(l => new SelectListItem
                {
                    Value = l.LoaiHinhThiDauId.ToString(),
                    Text = $"{l.Name} - {l.MonVo}"
                })
                .ToList();
            ViewBag.LoaiHinhThiDauList = loaiHinhThiDauList;

            var tournaments = await _context.Tournaments.ToListAsync();
            ViewBag.Tournaments = new SelectList(tournaments, "TournamentID", "Name", match.TournamentID);

            return View(match);
        }
        [HttpPost]
        public async Task<IActionResult> Update(string id, Match match)
        {
            if (id != match.MatchId.ToString())
            {
                return NotFound();
            }
           
            if (ModelState.IsValid)
            {
                try
                {
                    // Tìm khách hàng cũ để cập nhật

                    var existingCustomer = await _tranDaureponsitory.GetByIdAsync(id);
                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật tất cả các thuộc tính
                    
                    existingCustomer.Hangcan = match.Hangcan;
                    existingCustomer.SanDau = match.SanDau;
                    existingCustomer.Date = match.Date;
                    existingCustomer.Vongdau = match.Vongdau;
                    existingCustomer.Trongtai = match.Trongtai;
                    existingCustomer.trangthai = match.trangthai;
                    existingCustomer.TournamentID = match.TournamentID;
                    existingCustomer.LoaiHinhThiDauId = match.LoaiHinhThiDauId;

                    var loaiHinhThiDau = await _loaihreponsitory.GetByIdAsync(match.LoaiHinhThiDauId);
                    if (loaiHinhThiDau == null)
                    {
                        ModelState.AddModelError("LoaiHinhThiDauId", "Loại hình thi đấu không tồn tại");
                        var loaiHinhThiDauListItems = (await _loaihreponsitory.GetAllAsync())
                            .Select(l => new SelectListItem
                            {
                                Value = l.LoaiHinhThiDauId.ToString(),
                                Text = $"{l.Name} - {l.MonVo}",
                                Selected = l.LoaiHinhThiDauId == match.LoaiHinhThiDauId
                            }).ToList();
                        ViewBag.LoaiHinhThiDauList = loaiHinhThiDauListItems;
                        return View(match);
                    }
                    // Lưu thay đổi vào cơ sở dữ liệu
                    await _tranDaureponsitory.UpdateAsync(existingCustomer);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật khách hàng: " + ex.Message);
                }
            }
            var tournaments = await _context.Tournaments.ToListAsync();
            ViewBag.Tournaments = new SelectList(tournaments, "TournamentID", "Name", match.TournamentID);
            return View(match);
        }
    }   
}
