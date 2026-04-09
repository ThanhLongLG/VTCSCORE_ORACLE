using System.Drawing.Printing;
using BAO_CAO.Areas.Admin.Controllers;
using BaoCaoDACS.Areas.Admin.Models;
using BaoCaoDACS.Models;
using BaoCaoDACS.Reponsitory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BaoCaoDACS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class GiaiDauController : Controller
    {

        private readonly INguoidungreponsitory _nguoidungreponsitory;
        private readonly IGiaiDaureponsitory _igaiDaureponsitory;
        private readonly ILogger<NguoiDungController> _logger;
        private readonly AppDbContext _context;
        private readonly ILoaiHinhreponsitory _IloaiHinhreponsitory;
        public GiaiDauController(
            INguoidungreponsitory nguoidungreponsitory,
            ILogger<NguoiDungController> logger,
            IGiaiDaureponsitory giaiDaureponsitory,
            AppDbContext context,
            ILoaiHinhreponsitory iloaiHinhreponsitory)
        {
            _igaiDaureponsitory = giaiDaureponsitory;
            _nguoidungreponsitory = nguoidungreponsitory;
            _logger = logger;
            _context = context;
            _IloaiHinhreponsitory = iloaiHinhreponsitory;
        }

        public async Task<IActionResult> Index(string? searchValue)
        {
            var participantCounts = _context.Participants
               .Where(p => p.TournamentID != null)
               .GroupBy(p => p.TournamentID)
               .ToDictionary(g => g.Key, g => g.Count());

            // Lưu vào ViewBag để sử dụng trong view
            ViewBag.ParticipantCounts = participantCounts;

            var tournaments = await _igaiDaureponsitory.GetAllAsync(searchValue);
            ViewBag.searchValue = searchValue;
            return View(tournaments);
        }
        public async Task<IActionResult> Add()
        {
            var loaiHinhThiDauList = (await _IloaiHinhreponsitory.GetAllAsync())
                .Select(l => new SelectListItem
                {
                    Value = l.LoaiHinhThiDauId.ToString(),
                    Text = $"{l.Name} - {l.MonVo}"
                })
                .ToList();
            ViewBag.LoaiHinhThiDauList = loaiHinhThiDauList;
            return View(new Tournament
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                Status = "Upcoming"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Name,StartDate,EndDate,Location,DoiTuongThamGia,Status,QuyMoiaiDa,HinhThucThiDau,BanToChuc,LoaiHinhThiDauId,Phithamgia")] Tournament tournament, IFormFile mainImageFile, List<IFormFile> additionalImageFiles)
        {
            if (ModelState.IsValid)
            {
            
                try
                {
                   
                    var loaiHinhThiDau = await _context.loaiHinhThiDau.FindAsync(tournament.LoaiHinhThiDauId);
                    if (loaiHinhThiDau == null)
                    {
                        ModelState.AddModelError("LoaiHinhThiDauId", "Loại hình thi đấu không tồn tại");
                    }
                    else
                    {
                        if (mainImageFile != null)
                        {
                            tournament.ImageUrl = await SaveImage(mainImageFile);
                        }
                        if (additionalImageFiles != null)
                        {
                            tournament.ImageUrls = new List<string>();
                            foreach (var file in additionalImageFiles)
                            {
                                tournament.ImageUrls.Add(await SaveImage(file));
                            }
                        }
                        await _context.AddAsync(tournament);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"Đã thêm giải đấu {tournament.Name} thành công!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }
            ViewBag.LoaiHinhThiDauList = await GetLoaiHinhThiDauSelectList(tournament.LoaiHinhThiDauId); // Thêm await ở đây
            return View(tournament);
        }
        private async Task<string> SaveImage(IFormFile image)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo đường dẫn file
            var filePath = Path.Combine(uploadsFolder, image.FileName);

            // Lưu file vào thư mục
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // Trả về đường dẫn tương đối để hiển thị ảnh
            return "/images/" + image.FileName;
        }
        private async Task<IEnumerable<SelectListItem>> GetLoaiHinhThiDauSelectList(int selectedId = 0)
        {
            return (await _IloaiHinhreponsitory.GetAllAsync())
                .Select(l => new SelectListItem
                {
                    Value = l.LoaiHinhThiDauId.ToString(),
                    Text = $"{l.Name} - {l.MonVo}",
                    Selected = l.LoaiHinhThiDauId == selectedId
                })
                .ToList();
        }
        public async Task<IActionResult> Display(int id)
        {
            var giadau = await _igaiDaureponsitory.GetByIdAsync(id);
            if (giadau == null)
            {
                return NotFound();
            }
            return View(giadau);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var giadau = await _igaiDaureponsitory.GetByIdAsync(id);
            if (giadau == null)
            {
                return NotFound();
            }
            return View(giadau);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfired(int id)
        {
            var tournament = await _igaiDaureponsitory.GetByIdAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }
            try
            {
                await _igaiDaureponsitory.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Không thể xóa vì còn dữ liệu liên quan.");
                ViewBag.ErrorMessage = "Không thể xóa vì còn dữ liệu liên quan.";
                return View(tournament); 
            }
        }



        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var giadau = await _igaiDaureponsitory.GetByIdAsync(id);
            if (giadau == null)
            {
                return NotFound();
            }
            var loaiHinhThiDauList = await _IloaiHinhreponsitory.GetAllAsync();
            if (loaiHinhThiDauList != null && loaiHinhThiDauList.Any())
            {
                var selectListItems = loaiHinhThiDauList.Select(x => new SelectListItem 
                { 
                    Value = x.LoaiHinhThiDauId.ToString(), 
                    Text = $"{x.Name} - {x.MonVo}",
                    Selected = x.LoaiHinhThiDauId == giadau.LoaiHinhThiDauId
                }).ToList();
                ViewBag.LoaiHinhThiDauList = selectListItems;
            }
            else
            {
                ViewBag.LoaiHinhThiDauList = new List<SelectListItem>();
            }
            return View(giadau);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [Bind("TournamentID,Name,StartDate,EndDate,Location,HinhThucThiDau,DoiTuongThamGia,QuyMoiaiDa,BanToChuc,Status,LoaiHinhThiDauId,ImageUrl,ImageUrls,Phithamgia")]  Tournament tournament, IFormFile? imageFile)
        {
            if (id != tournament.TournamentID)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                var loaiHinhThiDauList = (await _IloaiHinhreponsitory.GetAllAsync())
                    .Select(l => new SelectListItem
                    {
                        Value = l.LoaiHinhThiDauId.ToString(),
                        Text = $"{l.Name} - {l.MonVo}",
                        Selected = l.LoaiHinhThiDauId == tournament.LoaiHinhThiDauId
                    }).ToList();
                ViewBag.LoaiHinhThiDauList = loaiHinhThiDauList;
                return View(tournament);
            }
            try
            {
                var existingTournament = await _igaiDaureponsitory.GetByIdAsync(id);
                if (existingTournament == null)
                {
                    return NotFound();
                }
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Tạo thư mục lưu trữ nếu chưa có
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Tạo tên file duy nhất
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadPath, uniqueFileName);

                    // Lưu file ảnh vào server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Xóa ảnh cũ (nếu có)
                    if (!string.IsNullOrEmpty(existingTournament.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(uploadPath, Path.GetFileName(existingTournament.ImageUrl));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Cập nhật đường dẫn ảnh mới
                    existingTournament.ImageUrl = "/images/" + uniqueFileName;
                }
                // Cập nhật các thuộc tính
                existingTournament.Name = tournament.Name;
                existingTournament.StartDate = tournament.StartDate;
                existingTournament.EndDate = tournament.EndDate;
                existingTournament.Location = tournament.Location;
                existingTournament.HinhThucThiDau = tournament.HinhThucThiDau;
                existingTournament.DoiTuongThamGia = tournament.DoiTuongThamGia;
                existingTournament.QuyMoiaiDa = tournament.QuyMoiaiDa;
                existingTournament.BanToChuc = tournament.BanToChuc;
                existingTournament.Status = tournament.Status;
                existingTournament.Phithamgia = tournament.Phithamgia;
                existingTournament.LoaiHinhThiDauId = tournament.LoaiHinhThiDauId;

                _context.Entry(existingTournament).State = EntityState.Modified; 

                var loaiHinhThiDau = await _IloaiHinhreponsitory.GetByIdAsync(tournament.LoaiHinhThiDauId);
                if (loaiHinhThiDau == null)
                {
                    ModelState.AddModelError("LoaiHinhThiDauId", "Loại hình thi đấu không tồn tại");
                    var loaiHinhThiDauListItems = (await _IloaiHinhreponsitory.GetAllAsync())
                        .Select(l => new SelectListItem
                        {
                            Value = l.LoaiHinhThiDauId.ToString(),
                            Text = $"{l.Name} - {l.MonVo}",
                            Selected = l.LoaiHinhThiDauId == tournament.LoaiHinhThiDauId
                        }).ToList();
                    ViewBag.LoaiHinhThiDauList = loaiHinhThiDauListItems;
                    return View(tournament);
                }

                await _igaiDaureponsitory.UpdateAsync(existingTournament);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
                ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + ex.Message);

                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật giải đấu: " + ex.Message);
                var loaiHinhThiDauListItemsCatch = (await _IloaiHinhreponsitory.GetAllAsync())
                    .Select(l => new SelectListItem
                    {
                        Value = l.LoaiHinhThiDauId.ToString(),
                        Text = $"{l.Name} - {l.MonVo}",
                        Selected = l.LoaiHinhThiDauId == tournament.LoaiHinhThiDauId
                    }).ToList();
                ViewBag.LoaiHinhThiDauList = loaiHinhThiDauListItemsCatch;
                return View(tournament);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTournamentStatus()
        {
            try
            {
                await _igaiDaureponsitory.UpdateTournamentStatusAsync();
                TempData["SuccessMessage"] = "Cập nhật trạng thái giải đấu thành công.";
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                TempData["ErrorMessage"] = "Cập nhật trạng thái thất bại: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
