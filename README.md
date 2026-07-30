# ECommerceApi

Backend API cho web thương mại điện tử đa ngành hàng. ASP.NET Core (.NET 10) + EF Core + SQL Server.

## Stack
- **.NET 10** (ASP.NET Core Web API)
- **EF Core 10** + SQL Server (Docker)
- **HotChocolate 16** (GraphQL — đọc catalog, chưa wiring)
- FluentValidation · AutoMapper · BCrypt.Net · JWT

## Yêu cầu
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- (tuỳ chọn) Visual Studio 2022 17.14+ hoặc VS Code

## Cài đặt & chạy

### 1. Chạy SQL Server bằng Docker Compose
Tạo file `.env` ở thư mục gốc (copy từ `.env.example`), đặt mật khẩu SA:
```
SA_PASSWORD=<mật khẩu mạnh: ≥8 ký tự, đủ hoa/thường/số/đặc biệt>
```
Rồi bật container (chạy ở thư mục gốc — chỗ có `docker-compose.yml`):
```powershell
docker compose up -d
```
> Container `ecom-sql` map cổng host **14330** → 1433 (tránh đụng SQL Server native nếu máy đã có instance chiếm 1433). `restart: unless-stopped` nên tự bật lại khi mở máy.
> Lệnh hằng ngày: `docker compose up -d` (bật) · `docker compose down` (tắt, giữ data) · `docker compose logs -f sqlserver` (xem log).

### 2. Cấu hình connection string (user-secrets)
Repo **không chứa** connection string (tránh commit secret). Tự set vào user-secrets — **chỉ cần thay `<YOUR_PASSWORD>`** cho khớp mật khẩu ở bước 1:
```powershell
cd ECommerceApi
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,14330;Database=ECommerceDb;User Id=sa;Password=<YOUR_PASSWORD>;TrustServerCertificate=True"
```
Kiểm tra: `dotnet user-secrets list`

### 3. Tạo database (chạy migration)
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update
```
> ⚠️ Bắt buộc set `ASPNETCORE_ENVIRONMENT=Development` — nếu không `dotnet ef` chạy môi trường Production, không đọc được user-secrets → connection string null.

### 4. Chạy API
```powershell
dotnet run
```
OpenAPI (dev): `http://localhost:5232/openapi/v1.json`

## Cấu trúc
```
ECommerceApi/
├── Models/                 # Entity (BaseEntity + domain)
├── Data/
│   ├── AppDbContext.cs
│   └── Configurations/     # IEntityTypeConfiguration cho từng entity
├── Repositories/           # Generic Repository<T>
├── UnitOfWork/             # UnitOfWork + generic repository accessor
├── Common/                 # ApiResponse, GlobalExceptionHandler, Pagination
├── Controllers/            # ApiControllerBase + controllers
└── Migrations/             # EF migrations
```

## Ghi chú
- Tiền dùng `decimal(18,2)`. Soft-delete qua `DeletedAt` + query filter.
- `CreatedAt`/`UpdatedAt` tự set khi SaveChanges.
- Thêm ngành hàng mới = thêm dữ liệu (Category + Attribute), **không đổi schema**.
