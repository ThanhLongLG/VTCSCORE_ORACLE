# BaoCaoDACS - Nền tảng Dự đoán và Quản lý Giải đấu Thể thao

## Giới thiệu

**BaoCaoDACS** là một ứng dụng web xây dựng trên nền tảng ASP.NET Core 8 nhằm mục đích quản lý các giải đấu thể thao, dự đoán kết quả trận đấu sử dụng Machine Learning, và tích hợp hệ thống thanh toán trực tuyến. Ứng dụng được thiết kế cho các huấn luyện viên, vận động viên và người quản lý giải đấu.

## 🎯 Tính năng chính

- **Quản lý giải đấu**: Tạo, chỉnh sửa, xóa và theo dõi các giải đấu thể thao
- **Quản lý trận đấu**: Lên lịch, cập nhật kết quả, theo dõi điểm số và xếp hạng
- **Dự đoán trận đấu**: Sử dụng mô hình Machine Learning (ML.NET) để dự đoán kết quả trận đấu
- **Hệ thống xếp hạng**: Tính toán, cập nhật và hiển thị bảng xếp hạng người chơi
- **Thanh toán trực tuyến**: Tích hợp Momo và VnPay để xử lý thanh toán
- **Quản lý người dùng**: Hệ thống xác thực, phân quyền và quản lý tài khoản người dùng
- **Kiểm tra video**: Module để kiểm tra và xác thực kết quả từ video
- **Cho điểm**: Giao diện cho người裁 để nhập điểm số trận đấu

## 🏗️ Kiến trúc hệ thống

### Cấu trúc thư mục

```
BaoCaoDACS/
├── Controllers/              # Lớp điều khiển (MVC Controllers)
│   ├── HomeController.cs
│   ├── PredictController.cs  # Dự đoán trận đấu
│   ├── PaymentController.cs  # Xử lý thanh toán
│   ├── ChamDiemController.cs # Cho điểm
│   └── VideoCheckController.cs
├── Models/                   # Các lớp mô hình dữ liệu
│   ├── AppDbContext.cs       # Database context
│   ├── MatchTrainingSample.cs
│   ├── MatchPredictionOutput.cs
│   ├── Tournament.cs
│   ├── Match.cs
│   ├── Participant.cs
│   ├── TournamentRanking.cs
│   └── DTO/                  # Data Transfer Objects
├── Reponsitory/              # Lớp repository (Data Access)
│   ├── IGiaiDaureponsitory.cs
│   ├── IKetquareponsitory.cs
│   ├── ITranDaureponsitory.cs
│   ├── Services/             # Business logic services
│   │   ├── EFMatchPredictionService.cs
│   │   ├── EFRankingService.cs
│   │   └── MomoService.cs
│   └── ...
├── Views/                    # Giao diện (Razor Views)
├── wwwroot/                  # Tài nguyên tĩnh (CSS, JS, hình ảnh)
│   └── Models/              # Thư mục lưu ML model
├── Areas/
│   ├── Admin/               # Khu vực quản trị
│   └── Identity/            # Khu vực xác thực người dùng
├── Libraries/               # Thư viện hỗ trợ
│   └── VnPayLibrary.cs
└── appsettings.json         # Cấu hình ứng dụng

BaoCaoDACS.MLTrain/          # Dự án huấn luyện mô hình ML
├── Program.cs
└── AppDbContext.cs
```

## 📋 Công nghệ sử dụng

| Công nghệ | Phiên bản | Mục đích |
|-----------|----------|---------|
| .NET | 8.0 | Framework chính |
| ASP.NET Core | 8.0 | Web framework |
| Entity Framework Core | 8.0.11 | ORM |
| Oracle Database | - | Cơ sở dữ liệu |
| ML.NET | 5.0.0 | Machine Learning |
| Microsoft.AspNetCore.Identity | 8.0.11 | Xác thực và phân quyền |
| RestSharp | 112.1.0 | HTTP client |

## 🚀 Hướng dẫn cửng cấp

### Điều kiện tiên quyết

- **.NET SDK 8.0** hoặc phiên bản cao hơn
- **Visual Studio 2022** hoặc **Visual Studio Code**
- **Oracle Database** (hoặc kết nối tới server Oracle)
- **Git** để clone repository

### Cài đặt và chạy

#### 1. Clone repository
```bash
git clone https://github.com/your-repo/BaoCaoDACS.git
cd BaoCaoDACS
```

#### 2. Khôi phục các gói NuGet
```bash
dotnet restore
```

#### 3. Cấu hình kết nối cơ sở dữ liệu

Chỉnh sửa file `appsettings.json` trong thư mục `BaoCaoDACS/` để cấu hình kết nối Oracle:

```json
{
  "ConnectionStrings": {
    "QLTAPVO": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=FREEPDB1)));User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  },
  "MomoAPI": {
    "MomoApiUrl": "https://test-payment.momo.vn/v2/gateway/api/create",
    "SecretKey": "YOUR_SECRET_KEY",
    "AccessKey": "YOUR_ACCESS_KEY",
    "ReturnUrl": "https://localhost:7143/Home/ThanhToan",
    "NotifyUrl": "https://localhost:7143/Checkout/MomoNotify",
    "PartnerCode": "MOMO",
    "RequestType": "captureWallet"
  },
  "Vnpay": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "Command": "pay",
    "CurrCode": "VND",
    "Version": "2.1.0",
    "Locale": "vn",
    "PaymentBackReturnUrl": "https://localhost:7143/Home/ThanhToansucces"
  },
  "TimeZoneId": "SE Asia Standard Time"
}
```

#### 4. Cập nhật cơ sở dữ liệu

```bash
cd BaoCaoDACS
dotnet ef database update
```

#### 5. Chạy ứng dụng

```bash
dotnet run
```

Ứng dụng sẽ chạy tại `https://localhost:7143`

### Huấn luyện mô hình ML

Để huấn luyện lại mô hình dự đoán trận đấu:

```bash
cd BaoCaoDACS.MLTrain
dotnet run
```

Mô hình được lưu tại `BaoCaoDACS/wwwroot/Models/match_predictor.zip`

## ⚙️ Cấu hình

### Cấu hình cơ sở dữ liệu

Chỉnh sửa `appsettings.json`:

```json
"ConnectionStrings": {
  "QLTAPVO": "YOUR_ORACLE_CONNECTION_STRING"
}
```

### Cấu hình thanh toán Momo

Cục bộ ứng dụng trong `Program.cs`:

```csharp
builder.Services.Configure<MomoOptionModel>(
  builder.Configuration.GetSection("MomoAPI"));
```

### Cấu hình thanh toán VnPay

Được cấu hình thông qua file `appsettings.json` và sử dụng trong `VnPayLibrary.cs`

### Cấu hình ML.NET Model

Mô hình ML được tải tại startup trong `Program.cs`:

```csharp
var modelPath = Path.Combine(
  builder.Environment.WebRootPath, 
  "Models", 
  "match_predictor.zip");
```

Đảm bảo file mô hình tồn tại tại đường dẫn trên.

## 📚 Hướng dẫn sử dụng chính

### Tạo giải đấu

1. Đăng nhập vào ứng dụng
2. Truy cập Admin > Quản lý giải đấu
3. Nhấn "Tạo giải đấu mới"
4. Điền thông tin giải đấu (tên, loại hình, thời gian, v.v.)
5. Thêm các vận động viên tham gia

### Dự đoán kết quả trận đấu

1. Truy cập "Dự đoán trận đấu" từ menu chính
2. Chọn giải đấu và trận đấu cần dự đoán
3. Hệ thống sẽ hiển thị dự đoán kết quả dựa trên mô hình ML
4. Xem xác suất chiến thắng của từng đối thủ

### Quản lý thanh toán

1. Người dùng chọn phương thức thanh toán (Momo hoặc VnPay)
2. Được chuyển hướng đến cổng thanh toán tương ứng
3. Sau khi xác nhận, trả về ứng dụng với kết quả thanh toán

### Xem bảng xếp hạng

1. Truy cập "Bảng xếp hạng" từ giải đấu cụ thể
2. Xem chỉ số xếp hạng của các vận động viên
3. Dữ liệu được cập nhật sau mỗi trận đấu

## 🔒 Xác thực và phân quyền

Ứng dụng sử dụng **ASP.NET Core Identity**.Các vai trò có thể bao gồm:
- **Admin**: Quản lý toàn bộ hệ thống
- **Organisator**: Quản lý giải đấu
- **Referee**: Cho điểm trận đấu
- **Participant**: Vận động viên tham gia
- **User**: Người dùng thông thường

## 🐛 Xử lý lỗi và ghi nhật ký

Ứng dụng ghi nhật ký các sự kiện quan trọng. Kiểm tra file nhật ký hoặc console output để chẩn đoán sự cố.

## 📞 Hỗ trợ và liên hệ

Để được hỗ trợ:
- Kiểm tra các issue trên GitHub
- Liên hệ với nhóm phát triển
- Báo cáo lỗi chi tiết kèm screenshot hoặc error log

## 📄 Giấy phép

Dự án này được phát hành dưới giấy phép [Chỉ định giấy phép của bạn]. Chi tiết xem file `LICENSE`.

## 👥 Đóng góp

Chúng tôi hoan nghênh các đóng góp! Để đóng góp:

1. Fork repository
2. Tạo branch feature (`git checkout -b feature/AmazingFeature`)
3. Commit thay đổi (`git commit -m 'Add some AmazingFeature'`)
4. Push lên branch (`git push origin feature/AmazingFeature`)
5. Mở Pull Request

## 📝 Changelog

### v1.0.0
- Phiên bản ban đầu
- Quản lý giải đấu cơ bản
- Dự đoán trận đấu với ML.NET
- Tích hợp thanh toán Momo và VnPay
- Hệ thống xếp hạng

---

**Phát triển bởi**: Nhóm phát triển BaoCaoDACS  
**Lần cập nhật cuối**: April 2026