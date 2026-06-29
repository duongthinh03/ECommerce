# ECommerceApi

Backend API cho web thương mại điện tử đa ngành hàng. ASP.NET Core (.NET 10) + EF Core + SQL Server.

> Định hướng & schema: xem `PLAN_ECOMMERCE_MULTI_NGANH.md`. Nhật ký dựng nền: `BRAINSTORM_BUOC_DAU.md`.

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

### 1. Chạy SQL Server bằng Docker
```powershell
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Ecom@2026Dev" -p 1433:1433 --name ecom-sql -v ecom-sqldata:/var/opt/mssql -d mcr.microsoft.com/mssql/server:2022-latest
```
> Mật khẩu SA phải đủ mạnh (≥8 ký tự, đủ hoa/thường/số/đặc biệt) nếu không container sẽ tắt.
> Container đã có sẵn lần sau chỉ cần: `docker start ecom-sql`.

### 2. Cấu hình connection string (user-secrets)
Repo **không chứa** connection string (tránh commit secret). Tự set vào user-secrets — **chỉ cần thay `<YOUR_PASSWORD>`** cho khớp mật khẩu ở bước 1:
```powershell
cd ECommerceApi
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ECommerceDb;User Id=sa;Password=<YOUR_PASSWORD>;TrustServerCertificate=True"
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
