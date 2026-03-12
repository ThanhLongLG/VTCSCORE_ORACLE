using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;

namespace BaoCaoDACS.Controllers
{
    public class VideoCheckController : Controller
    {
        private readonly ILogger<VideoCheckController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public VideoCheckController(ILogger<VideoCheckController> logger, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadAndCheck(IFormFile videoFile)
        {
            try
            {
                if (videoFile == null || videoFile.Length == 0)
                    return Json(new { success = false, message = "Vui lòng chọn file video" });

                // 1. Lưu file Input
                var uploadsPath = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(videoFile.FileName)}";
                var inputFilePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(inputFilePath, FileMode.Create))
                {
                    await videoFile.CopyToAsync(stream);
                }

                // 2. Tạo đường dẫn Output
                var outputFileName = $"output_{Guid.NewGuid()}.mp4";
                var outputFilePath = Path.Combine(uploadsPath, outputFileName);

                // 3. Gọi Python xử lý
                var result = RunPythonInference(inputFilePath, outputFilePath);

                if (!result.success)
                {
                    // Giữ lại file input để debug nếu cần, hoặc xóa đi
                    // System.IO.File.Delete(inputFilePath);
                    return Json(new { success = false, message = result.message });
                }

                // 4. Trả kết quả về Frontend
                return Json(new
                {
                    success = true,
                    message = "Xử lý thành công!",
                    outputVideo = $"/uploads/{outputFileName}",
                    originalVideo = $"/uploads/{fileName}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi Server: {ex.Message}" });
            }
        }

        private (bool success, string message) RunPythonInference(string inputPath, string outputPath)
        {
            try
            {
                // Đường dẫn thư mục 'project'
                var projectPath = Path.Combine(_hostEnvironment.ContentRootPath, "project");
                var scriptPath = Path.Combine(projectPath, "inference_video.py");

                // --- CẤU HÌNH QUAN TRỌNG: TRỎ VÀO PYTHON TRONG VENV ---
                // Sửa lại đường dẫn này cho chính xác với máy bạn
                string pythonExePath = @"D:\HOC\BAO_CAO_DACS\BaoCaoCN - Copy\BaoCaoDACS\project\venv\Scripts\python.exe";

                if (!System.IO.File.Exists(pythonExePath))
                    return (false, "Không tìm thấy môi trường Python (venv). Kiểm tra lại đường dẫn trong Controller.");

                var processInfo = new ProcessStartInfo
                {
                    FileName = pythonExePath,
                    Arguments = $"\"{scriptPath}\" \"{inputPath}\" \"{outputPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = projectPath // Quan trọng để Python load được file model
                };

                using (var process = Process.Start(processInfo))
                {
                    // Chờ tối đa 10 phút
                    process.WaitForExit(600000);

                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    _logger.LogInformation($"Python Log: {output}");

                    if (process.ExitCode != 0)
                    {
                        _logger.LogError($"Python Error: {error}");
                        return (false, $"Lỗi từ Python AI: {error}");
                    }

                    return (true, "OK");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi gọi Process: {ex.Message}");
            }
        }
    }
}