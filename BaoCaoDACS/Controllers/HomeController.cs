using System.Diagnostics;
using BAO_CAO.Models;
using BaoCaoDACS.Models;
using BaoCaoDACS.Models.NewFolder;
using BaoCaoDACS.Models.VnPay;
using BaoCaoDACS.Reponsitory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace BaoCaoDACS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INguoidungreponsitory _nguoidungreponsitory;
        private readonly IRankingService rankingService;
        private readonly IMatchPredictionService _predictService;
        private readonly IGiaiDaureponsitory _giaidau;
        private static DateTime? _lastTournamentStatusUpdateDate;
        private static readonly object _updateLock = new object();

        public HomeController(
            INguoidungreponsitory nguoidungreponsitory,
             UserManager<ApplicationUser> UserManager,
            ILogger<HomeController> logger,
            AppDbContext context,
            IRankingService rankingService,
            IMatchPredictionService predictService,
            IGiaiDaureponsitory giaidau)
        {
            _nguoidungreponsitory = nguoidungreponsitory;
            _userManager = UserManager;
            _logger = logger;
            _context = context;
            this.rankingService = rankingService;
            _predictService = predictService;
            _giaidau = giaidau;
        }

        public async Task<IActionResult> Index()
        {
            await EnsureTournamentStatusUpdatedTodayAsync();
            return View();
        }
        public IActionResult ThanhToansucces()
        {
            // Lấy các tham số từ query string do VNPAY trả về
            var responseCode = Request.Query["vnp_ResponseCode"].ToString(); ;
            var transactionStatus = Request.Query["vnp_TransactionStatus"];
            var amount = Request.Query["vnp_Amount"];
            var orderInfo = Request.Query["vnp_OrderInfo"];
            var bankCode = Request.Query["vnp_BankCode"];
            var payDate = Request.Query["vnp_PayDate"];
            var txnRef = Request.Query["vnp_TxnRef"];

            // Kiểm tra giao dịch thành công
            if (responseCode == "00" && transactionStatus == "00")
            {
                ViewBag.Message = "Thanh toán thành công!";
                ViewBag.Amount = amount.ToString();
                ViewBag.OrderInfo = orderInfo.ToString();
                ViewBag.BankCode = bankCode.ToString();
                ViewBag.PayDate = payDate.ToString();
                ViewBag.TxnRef = txnRef.ToString();
            }
            else if (!string.IsNullOrEmpty(responseCode))
            {
                ViewBag.Message = "Thanh toán thất bại hoặc bị hủy!";
            }
            else
            {
                ViewBag.Message = null; 
            }

            return View();
        }

        public async Task<IActionResult> ThanhToan()
        {
            var userId = _userManager.GetUserId(User);
            var participant = await _context.Participants
               .FirstOrDefaultAsync(p => p.UserId == userId);
            if (participant == null)
            {
                return View("ThanhToan", null);
            }
            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.TournamentID == participant.TournamentID);

            if (tournament == null)
            {
                return View("ThanhToan", null);
            }


            var paymentViewModel = new MomoInfoModel
            {
                FullName = participant?.FullName ?? "Người tham dự",
                Amount = tournament.Phithamgia,
                OrderInfo = $"Thanh toán phí giải đấu {tournament.Name}"
            };
            var vnpay = new PaymentInformationModel
            {
                Name = participant?.FullName ?? "Người tham dự",
                Amount = tournament.Phithamgia,
                OrderDescription = $"Thanh toán phí giải đấu {tournament.Name}",
                OrderType="other"

            };
            var vm = new PaymentMultiViewModel
            {
                MomoInfo = paymentViewModel,
                VnpayInfo = vnpay
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        
        public IActionResult ChamDiem()
        {
            return View();
        }

        public async Task<IActionResult> LichThiDau()
        {
            var matches = await _context.V_Match_Schedules
             .OrderBy(m => m.Date) // <-- Trả lệnh sắp xếp về đúng vị trí của nó!
             .ToListAsync();

            var result = new List<MatchScheduleVM>();

            var predictionsData = await _context.V_Match_Predictions.ToListAsync();

            foreach (var pData in predictionsData)
            {
                // Bỏ qua nếu trận đấu chưa có võ sĩ nào được xếp
                if (pData.FighterA_Age == 0 && pData.FighterB_Age == 0) continue;

                // Oracle đã chuẩn bị sẵn Cân nặng, Chiều cao, Tuổi, Elo cho bạn rồi!
                var input = new MatchTrainingSample
                {
                    FighterA_Weight = pData.FighterA_Weight,
                    FighterA_Height = pData.FighterA_Height,
                    FighterA_Age = pData.FighterA_Age,

                    FighterB_Weight = pData.FighterB_Weight,
                    FighterB_Height = pData.FighterB_Height,
                    FighterB_Age = pData.FighterB_Age,

                    FighterA_Rating = pData.FighterA_Rating,
                    FighterB_Rating = pData.FighterB_Rating,

                    // Tính độ lệch ngay tại đây
                    RatingDiff = pData.FighterA_Rating - pData.FighterB_Rating,
                    DiffWeight = pData.FighterA_Weight - pData.FighterB_Weight,
                    DiffHeight = pData.FighterA_Height - pData.FighterB_Height,
                    DiffAge = pData.FighterA_Age - pData.FighterB_Age,

                    LoaiHinhThiDauId = pData.LoaiHinhThiDauId ?? 0,
                    HangCan = pData.Hangcan,
                    VongDau = pData.Vongdau
                };
                // Gọi AI dự đoán
                var winRateA = _predictService.PredictWinRate(input);

                result.Add(new MatchScheduleVM
                {
                    MatchId = pData.MatchId,
                    FighterAWinPercent = winRateA
                });
            }

            // ✅ map MatchId -> winRate
            ViewData["WinRateMap"] = result.ToDictionary(x => x.MatchId, x => x.FighterAWinPercent);

            // ✅ GIỮ view cũ: @model List<Match>
            return View(matches);
        }




        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            DateTime now;
            try
            {
                now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"));
            }
            catch
            {
                now = DateTime.UtcNow.AddHours(7);
            }

            // 1️⃣ Participant đang tham gia
            var activeParticipant = await _context.Participants
                .Include(p => p.Tournament)
                .Where(p => p.UserId == user.Id &&
                            p.Tournament != null &&
                            p.Tournament.StartDate <= now &&
                            (p.Tournament.EndDate == null || p.Tournament.EndDate >= now))
                .FirstOrDefaultAsync();

            if (activeParticipant != null)
                return View(activeParticipant);

            // 2️⃣ Participant của giải sắp tới (StartDate > now)
            var nextParticipant = await _context.Participants
                .Include(p => p.Tournament)
                .Where(p => p.UserId == user.Id &&
                            p.Tournament != null &&
                            p.Tournament.StartDate > now)
                .OrderBy(p => p.Tournament.StartDate)
                .FirstOrDefaultAsync();

            if (nextParticipant != null)
                return View(nextParticipant);

            // 3️⃣ Không tham gia giải nào
            ViewBag.Message = "Bạn chưa tham gia giải đấu nào.";
            return View(null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(Participant model)
        {
            ModelState.Remove("ParticipantID");
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError($"Model Validation Error: {error.ErrorMessage}");
                    }
                }
                return View(model);
            }


            try
            {
                // Lấy thông tin người dùng hiện tại
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                // Tìm participant trong database
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);


                _logger.LogInformation($"FullName: {model.FullName}");
              
                if (participant == null)
                {
                    // Nếu chưa tồn tại thì tạo mới
                    participant = new Participant { UserId = user.Id };
                    _context.Participants.Add(participant);
                }

                // Cập nhật thông tin
                participant.Club = model.Club;
                participant.FullName = model.FullName;
                participant.email = model.email;
                participant.sdt = model.sdt;
                participant.tuoi = model.tuoi;
                participant.Diachi = model.Diachi;
                participant.ChieuCao = model.ChieuCao;
                participant.CanNang = model.CanNang;

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                // Thông báo thành công
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
      
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View("Profile", model);
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetNumberOfParticipants(int id)
        {
            // Lấy số lượng người tham gia
            var tournament = await _context.Tournaments
                .Include(t => t.participant)
                .FirstOrDefaultAsync(t => t.TournamentID == id);

            if (tournament == null)
            {
                return NotFound();
            }

            var numberOfParticipants = tournament.participant.Count;
            return Json(new { count = numberOfParticipants });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostParticipant([FromBody] Participant participant)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized(new { Message = "Vui lòng đăng nhập để thực hiện hành động này." });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(kv => kv.Value.Errors.Count > 0)
                        .ToDictionary(kv => kv.Key, kv => kv.Value.Errors.Select(e => e.ErrorMessage).ToArray());
                    return BadRequest(new { Message = "Dữ liệu không hợp lệ", Errors = errors });
                }

                // Lấy tournament đích (tournament mà user muốn đăng ký)
                if (participant.TournamentID == null)
                    return BadRequest(new { Message = "Vui lòng chọn giải đấu muốn đăng ký." });

                var tournamentTarget = await _context.Tournaments
                    .FirstOrDefaultAsync(t => t.TournamentID == participant.TournamentID);

                if (tournamentTarget == null)
                    return BadRequest(new { Message = "Giải đấu không tồn tại." });

                DateTime now;
                try
                {
                    now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"));
                }
                catch
                {
                    now = DateTime.UtcNow.AddHours(7);
                }

                // Kiểm tra: user đã đăng ký chính xác giải này chưa?
                var alreadyRegisteredSameTournament = await _context.Participants
                    .AnyAsync(p => p.UserId == user.Id && p.TournamentID == participant.TournamentID);
                if (alreadyRegisteredSameTournament)
                    return BadRequest(new { Message = "Bạn đã đăng ký tham gia giải này." });

                // Tìm participant của user mà đang thuộc giải 'đang hoạt động' (active)
                // Active tournament = EndDate == null || EndDate >= now
                var activeParticipant = await _context.Participants
                    .Include(p => p.Tournament)
                    .Where(p => p.UserId == user.Id && p.Tournament != null
                                && (p.Tournament.EndDate == null || p.Tournament.EndDate >= now))
                    .FirstOrDefaultAsync();

                if (activeParticipant != null)
                {
                    // Nếu đã có participant trong 1 giải đang hoạt động, chỉ cho đăng ký
                    // khi giải mục tiêu bắt đầu sau khi giải hiện tại kết thúc.
                    var currentTournament = activeParticipant.Tournament;

                    // Nếu giải hiện tại không có EndDate => coi như vẫn đang mở, không cho đăng ký.
                    if (currentTournament?.EndDate == null)
                    {
                        return BadRequest(new
                        {
                            Message = "Bạn đang tham gia một giải đang diễn ra. Vui lòng đợi giải đó kết thúc trước khi đăng ký giải khác."
                        });
                    }

                    // Nếu giải mục tiêu không có StartDate => không thể quyết định, từ chối
                    // (hoặc bạn có thể cho phép tuỳ nghiệp vụ)
                    if (tournamentTarget.StartDate == null)
                    {
                        return BadRequest(new { Message = "Giải đăng ký chưa có ngày bắt đầu. Không thể đăng ký tại thời điểm này." });
                    }

                    // Cho phép đăng ký nếu StartDate của target > EndDate của current
                    if (tournamentTarget.StartDate <= currentTournament.EndDate)
                    {
                        return BadRequest(new
                        {
                            Message = "Bạn đang tham gia một giải đấu khác và chưa kết thúc. Chỉ có thể đăng ký giải có thời gian bắt đầu sau khi giải hiện tại kết thúc."
                        });
                    }
                }
                string nextId = "";
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "SELECT GET_NEXT_PARTICIPANT_ID FROM DUAL";
                    await _context.Database.OpenConnectionAsync();
                    var kq = await command.ExecuteScalarAsync();
                    nextId = kq.ToString();
                    await _context.Database.CloseConnectionAsync();
                }

                var newParticipant = new Participant
                {
                    ParticipantID = nextId,
                    FullName = participant.FullName ?? "Chưa tên",
                    Club = string.IsNullOrWhiteSpace(participant.Club) ? "Không có" : participant.Club,
                    sdt = participant.sdt,
                    email = participant.email,
                    CanNang = participant.CanNang,
                    ChieuCao = participant.ChieuCao,
                    tuoi = participant.tuoi,
                    Diachi = participant.Diachi,
                    TournamentID = participant.TournamentID,
                    UserId = user.Id
                };

                _context.Participants.Add(newParticipant);
                await _context.SaveChangesAsync();

                await rankingService.GetOrCreateAsync(user.Id, newParticipant.TournamentID.Value);
                // Map sang DTO để trả ra
                var dto = new ParticipantDto
                {
                    ParticipantID = newParticipant.ParticipantID,
                    FullName = newParticipant.FullName,
                    Club = newParticipant.Club,
                    sdt = newParticipant.sdt,
                    email = newParticipant.email,
                    CanNang = newParticipant.CanNang,
                    ChieuCao = newParticipant.ChieuCao,
                    tuoi = newParticipant.tuoi,
                    Diachi = newParticipant.Diachi,
                    TournamentID = newParticipant.TournamentID
                };

                return CreatedAtAction(
                    nameof(GetParticipant),
                    new { id = newParticipant.ParticipantID },
                    dto
                );
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error when creating participant");
                var sqlMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                return StatusCode(500, new { Message = "Database update error", Detail = sqlMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                return StatusCode(500, new { Message = "Unexpected error", Detail = ex.Message });
            }
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<Participant>> GetParticipant(string id)
        {
            var participant = await _context.Participants.FindAsync(id);
            return participant ?? (ActionResult<Participant>)NotFound();
        }

        [HttpGet]
        public async  Task<IActionResult> GetTournaments()
        {
            await EnsureTournamentStatusUpdatedTodayAsync();


            DateTime now;
            now = DateTime.UtcNow.AddHours(7);
            DateTime cutoff = now.AddMonths(-1);
            var tournaments = _context.Tournaments
            .AsNoTracking()
            .Where(t => t.EndDate == null || t.EndDate >= cutoff)
            .Select(t => new
            {
            TournamentID = t.TournamentID,
            Name = t.Name,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            Location = t.Location,
            HinhThucThiDau = t.HinhThucThiDau,
            DoiTuongThamGia = t.DoiTuongThamGia,
            QuyMoiaiDa = t.QuyMoiaiDa,
            BanToChuc = t.BanToChuc,
            Status = t.Status,
            ImageUrl = t.ImageUrl ?? "/images/anime3.png",
            })
            .ToList();
            return Json(tournaments);
        }
        [HttpGet]
        public async Task<IActionResult> KiemtraGiaDau(int tournamentId)
        {
            int isOverlap = 0;
            var user = await _userManager.GetUserAsync(User);
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT CHECK_TOURNAMENT_OVERLAP(:userId, :tourId) FROM DUAL";

                var p1 = command.CreateParameter();
                p1.ParameterName = "userId";
                p1.Value = user.Id;

                var p2 = command.CreateParameter();
                p2.ParameterName = "tourId";
                p2.Value = tournamentId;

                command.Parameters.Add(p1);
                command.Parameters.Add(p2);

                await _context.Database.OpenConnectionAsync();

                var result = await command.ExecuteScalarAsync();
                isOverlap = Convert.ToInt32(result);

                await _context.Database.CloseConnectionAsync();
            }
            if (isOverlap == 1)
            {
                return Json(new { canRegister = false, message = "Bạn đã đăng ký một giải đấu khác trùng thời gian!" });
            }
            else
            {
                return Json(new { canRegister = true, message = "Bạn có thể đăng ký giải đấu này." });
            }
        }

            [HttpGet]
            public async Task<IActionResult> GetMatches()
            {
                try
                {
                    var matches = await _context.match
                        .Include(m => m.LoaiHinhThiDau)
                        .Include(m => m.Tournament)
                        .Include(m => m.Socre)
                        .Where(m => m.trangthai != 1)
                        .AsNoTracking()
                        .Select(m => new
                        {
                            m.MatchId,
                            m.Vongdau,
                            m.SanDau,
                            m.Hangcan,
                            m.Trongtai,
                            m.trangthai,
                            Date = m.Date.ToString("HH:mm - dd/MM/yyyy"),
                            LoaiHinhThiDau = m.LoaiHinhThiDau != null ? m.LoaiHinhThiDau.Name : "",
                            TournamentName = m.Tournament != null ? m.Tournament.Name : "",
                            Participants = m.Socre.Select(s => new {
                                s.ParticipantId,
                                s.Diem,
                                s.Kq,
                                s.KietQua
                            }).ToList()
                        })
                        .ToListAsync();

                    return Ok(matches);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Error = "Lỗi hệ thống",
                        Detail = ex.Message
                    });
                }
            }

        [HttpGet]
        public async Task<IActionResult> TopScores()
        {
            var topScores = await _context.V_Top4_Scores.ToListAsync();

            return Json(topScores);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task EnsureTournamentStatusUpdatedTodayAsync()
        {
            var today = DateTime.UtcNow.AddHours(7).Date;

            if (_lastTournamentStatusUpdateDate == today)
                return;

            bool shouldRun = false;

            lock (_updateLock)
            {
                if (_lastTournamentStatusUpdateDate != today)
                {
                    _lastTournamentStatusUpdateDate = today;
                    shouldRun = true;
                }
            }

            if (!shouldRun)
                return;

            try
            {
                await _giaidau.UpdateTournamentStatusAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái giải đấu hàng ngày.");

                lock (_updateLock)
                {
                    _lastTournamentStatusUpdateDate = null;
                }
            }
        }
        
    }
}
