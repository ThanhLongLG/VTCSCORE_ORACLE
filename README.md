# BaoCaoDACS

Ung dung web ASP.NET Core MVC phuc vu quan ly giai dau, quan ly tran dau, cham diem, xep hang, du doan ket qua bang ML.NET va tich hop thanh toan truc tuyen.

## Tong quan

Repository hien co 2 project chinh:

- `BaoCaoDACS`: ung dung web ASP.NET Core 8
- `BaoCaoDACS.MLTrain`: console app dung de train va xuat model du doan

Stack chinh:

- .NET 8 / ASP.NET Core MVC
- Entity Framework Core 8
- Oracle Database
- ASP.NET Core Identity
- ML.NET
- RestSharp

## Tinh nang chinh

- Quan ly giai dau, loai hinh thi dau va tran dau
- Quan ly ket qua, cham diem va bang xep hang
- Dang nhap, xac thuc va phan quyen bang Identity
- Du doan ket qua tran dau bang model ML.NET
- Tich hop thanh toan Momo va VNPAY
- Khu vuc quan tri trong `Areas/Admin`

## Cau truc du an

```text
BaoCaoCN-banketnoioracle/
|-- BaoCaoDACS/                  # Web app ASP.NET Core MVC
|   |-- Areas/Admin/             # Man hinh quan tri
|   |-- Controllers/             # Home, Predict, Payment, ChamDiem, VideoCheck
|   |-- Libraries/               # Thu vien ho tro, vi du VnPayLibrary
|   |-- Models/                  # Entity, DTO, DbContext, model du doan
|   |-- Reponsitory/             # Repository va service xu ly nghiep vu
|   |-- Views/                   # Razor views
|   |-- wwwroot/                 # Static files va model ML sau khi train
|   |-- appsettings.json
|   `-- Program.cs
|-- BaoCaoDACS.MLTrain/          # Chuong trinh train model ML
|   |-- Program.cs
|   `-- appsettings.json
|-- publish/                     # Ban publish san co
`-- BaoCaoDACS.sln
```

## Yeu cau moi truong

- .NET SDK 8.0
- Oracle Database co service name hop le
- Visual Studio 2022 hoac VS Code

## Cau hinh

### 1. Chuoi ket noi Oracle

Cap nhat `ConnectionStrings:QLTAPVO` trong [BaoCaoDACS/appsettings.json](D:/HOC/BAO_CAO_DACS/BaoCaoCN-banketnoioracle/BaoCaoDACS/appsettings.json) va [BaoCaoDACS.MLTrain/appsettings.json](D:/HOC/BAO_CAO_DACS/BaoCaoCN-banketnoioracle/BaoCaoDACS.MLTrain/appsettings.json).

Vi du:

```json
{
  "ConnectionStrings": {
    "QLTAPVO": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=FREEPDB1)));User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  }
}
```

### 2. Cau hinh Momo va VNPAY

Ung dung doc cau hinh tu:

- `MomoAPI`
- `Vnpay`
- `TimeZoneId`

Neu ban khong dung thanh toan, van nen giu cau truc cau hinh de tranh loi bind option.

### 3. Model ML

Web app load model khi khoi dong tai duong dan:

`BaoCaoDACS/wwwroot/Models/match_predictor.zip`

Neu file nay chua ton tai, ung dung se nem `FileNotFoundException` ngay luc startup. Hay train model truoc, hoac dat san file model vao dung thu muc.

## Cai dat va chay

### 1. Restore package

```bash
dotnet restore BaoCaoDACS.sln
```

### 2. Train model ML

Lenh nay se:

- doc du lieu tran dau da hoan thanh tu Oracle
- train model bang ML.NET
- sinh file `match_predictor.zip`
- copy file vao `BaoCaoDACS/wwwroot/Models/`

```bash
dotnet run --project BaoCaoDACS.MLTrain
```

### 3. Chay web app

```bash
dotnet run --project BaoCaoDACS
```

Sau khi chay, truy cap URL duoc in ra trong terminal, thong thuong la `https://localhost:<port>`.

## Tai khoan mac dinh

Trong `Program.cs`, ung dung seed san:

- Email: `admin@example.com`
- Password: `Admin@123`
- Role: `Admin`

Tai khoan nay chi duoc tao neu chua ton tai trong database.

## Luong khoi tao thuc te

De chay du an tren may moi, thu tu nen la:

1. Cai Oracle va tao schema/database phu hop.
2. Chinh `appsettings.json` cho ca web app va project train ML.
3. Restore package.
4. Dam bao database da co schema va du lieu can thiet.
5. Train model bang `BaoCaoDACS.MLTrain`.
6. Chay `BaoCaoDACS`.

## Cac thanh phan dang chu y

### Web app `BaoCaoDACS`

- MVC + Razor Views
- Identity + Entity Framework Core + Oracle
- Controllers chinh:
  - `HomeController`
  - `PredictController`
  - `PaymentController`
  - `ChamDiemController`
  - `VideoCheckController`
- Admin area:
  - `GiaiDauController`
  - `KetQuaController`
  - `LoaiHinhThiDauController`
  - `NguoiDungController`
  - `ThongKeController`
  - `TranDauController`
  - `TrangQLController`

### Project `BaoCaoDACS.MLTrain`

- Lay du lieu tran dau da co ket qua
- Tao training set doi xung thang/thua
- One-hot encode mot so bien danh muc
- Train binary classification bang `FastTree`
- Danh gia voi `Accuracy`, `AUC`, `F1Score`

## Luu y van hanh

- Project dang de gia tri cau hinh nhay cam trong `appsettings.json`. Nen doi sang bien moi truong, Secret Manager hoac file cau hinh rieng theo moi truong.
- Web app phu thuoc truc tiep vao file model ML. Neu muon deploy on dinh hon, nen co buoc kiem tra file model hoac fallback an toan khi model chua san sang.
- Repository co san thu muc `publish/`, nhung ban van nen build lai tu source de tranh sai lech cau hinh moi truong.

## Lenh huu ich

```bash
dotnet build BaoCaoDACS.sln
dotnet run --project BaoCaoDACS.MLTrain
dotnet run --project BaoCaoDACS
```

## Huong phat trien tiep

- Tach secrets khoi source code
- Bo sung migration/huong dan tao schema ro rang
- Bo sung script seed du lieu mau
- Viet them test cho service ranking, prediction va payment

## Giay phep

Chua thay file `LICENSE` trong repository. Neu du an can phat hanh cong khai, nen bo sung license phu hop.
