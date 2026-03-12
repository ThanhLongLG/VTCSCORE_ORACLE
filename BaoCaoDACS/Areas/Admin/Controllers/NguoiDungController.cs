using System.Drawing.Printing;
using BAO_CAO.Models;
using BaoCaoDACS.Areas.Admin.Models;
using BaoCaoDACS.Models;
using BaoCaoDACS.Reponsitory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BAO_CAO.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class NguoiDungController : Controller
    {

        private readonly INguoidungreponsitory _nguoidungreponsitory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<NguoiDungController> _logger;
        private readonly AppDbContext _context;
        private readonly IRankingService _rankingService;
        public NguoiDungController(
            INguoidungreponsitory nguoidungreponsitory, 
            ILogger<NguoiDungController> logger,
            UserManager<ApplicationUser> userManager,
            AppDbContext context,
            IRankingService rankingService)
        {
            _userManager = userManager;
            _nguoidungreponsitory= nguoidungreponsitory;
                 _logger = logger;
            _context = context;
            _rankingService= rankingService;
        }

        public async Task<IActionResult> Index(string? searchValue)
        {
            var participants = await _nguoidungreponsitory.GetAllAsync(searchValue);
            ViewBag.searchValue = searchValue;  
            return View(participants);
        }

        public IActionResult Add()
        {
            // Tạo danh sách giải đấu cho dropdown
            ViewBag.Tournaments = _context.Tournaments
                .Select(t => new SelectListItem
                {
                    Value = t.TournamentID.ToString(), // Convert int to string for dropdown
                    Text = t.Name
                })
                .ToList();

            // Thêm tùy chọn "Không tham gia giải đấu"
            ViewBag.Tournaments.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "-- Không tham gia giải đấu --"
            });

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Participant participant)
        {
            try
            {
                _logger.LogInformation($"Đang thêm người dùng: ID={participant.ParticipantID}, Tên={participant.FullName}");

                if (!ModelState.IsValid)
                {
                    // Lấy lại danh sách giải đấu khi form không hợp lệ
                    PrepareToumamentDropdown(participant.TournamentID);
                    return View(participant);
                }

                // Kiểm tra ID đã tồn tại
                if (await _nguoidungreponsitory.GetByIdAsync(participant.ParticipantID) != null)
                {
                    ModelState.AddModelError("ParticipantID", "Mã người dùng này đã tồn tại");
                    PrepareToumamentDropdown(participant.TournamentID);
                    return View(participant);
                }

    

                // TournamentID đã là nullable int, không cần xử lý thêm

                await _nguoidungreponsitory.AddAsync(participant);
                TempData["SuccessMessage"] = "Đã thêm người dùng thành công!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm người dùng");
                ModelState.AddModelError("", "Có lỗi xảy ra. Vui lòng thử lại.");
                PrepareToumamentDropdown(participant.TournamentID);
                return View(participant);
            }
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
            var khachHang = await _nguoidungreponsitory.GetByIdAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }
            return View(khachHang);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var khachHang = await _nguoidungreponsitory.GetByIdAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }
            return View(khachHang);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {  
            var khachHang = await _nguoidungreponsitory.GetByIdAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }
            try
            {
              
                var user = await _userManager.FindByIdAsync(khachHang.UserId);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View("Delete", khachHang);
                    }
                }

                // Xóa người dùng trong CSDL của bạn
                await _nguoidungreponsitory.DeleteAsync(id);


                var tournaments = await _context.match
                .Include(m => m.Socre)  // Load Socre
                  .ThenInclude(s => s.participant)  
                .Where(m => m.Socre.Any(s => s.participant != null && s.participant.UserId == khachHang.UserId))  
                .Select(m => m.TournamentID) 
                .Distinct()  
                .ToListAsync();  

                foreach (var tid in tournaments.Where(t => t.HasValue))  // Duyệt các tid không null
                {
                    await _rankingService.RebuildTournamentAsync(tid.Value);  // Rebuild ranking
                }
                // Thông báo và chuyển hướng
                TempData["SuccessMessage"] = "Đã xóa người dùng thành công";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                ModelState.AddModelError("", "Không thể xóa người dùng: " + ex.Message);
                return View("Delete", khachHang);
            }
        }

        public async Task<IActionResult> Update(string id)
        {
            var participant = await _nguoidungreponsitory.GetByIdAsync(id);
            if (participant == null)
            {
                return NotFound();
            }

            var tournaments = await _context.Tournaments.ToListAsync();
            ViewBag.Tournaments = new SelectList(tournaments, "TournamentID", "Name", participant.TournamentID);

            return View(participant);
        }
        [HttpPost]
        public async Task<IActionResult> Update(string id, Participant khachHang)
        {
            if (id != khachHang.ParticipantID.ToString())
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tìm khách hàng cũ để cập nhật
                    var existingCustomer = await _nguoidungreponsitory.GetByIdAsync(id);
                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật tất cả các thuộc tính
                    existingCustomer.FullName = khachHang.FullName;
                    existingCustomer.Club = khachHang.Club;
                    existingCustomer.sdt = khachHang.sdt;
                    existingCustomer.email = khachHang.email;
                    existingCustomer.ChieuCao = khachHang.ChieuCao;
                    existingCustomer.CanNang = khachHang.CanNang;
                    existingCustomer.tuoi = khachHang.tuoi;
                    existingCustomer.Diachi = khachHang.Diachi;
                    existingCustomer.TournamentID = khachHang.TournamentID;
                    // Lưu thay đổi vào cơ sở dữ liệu
                    await _nguoidungreponsitory.UpdateAsync(existingCustomer);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật khách hàng: " + ex.Message);
                }
            }
            var tournaments = await _context.Tournaments.ToListAsync();
            ViewBag.Tournaments = new SelectList(tournaments, "TournamentID", "Name", khachHang.TournamentID);
            return View(khachHang);
        }
    }   
}
