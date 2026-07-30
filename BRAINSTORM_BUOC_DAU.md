# BRAINSTORM — Bước đầu tiên (Phase 0 / Bước A)

> Kết quả phiên brainstorm dựa trên `PLAN_ECOMMERCE_MULTI_NGANH.md`. Mục tiêu: xác định first step cụ thể, không implement.

## Vấn đề
Plan đã chốt định hướng (schema, catalog đa ngành, FE monorepo, REST-ghi/GraphQL-đọc, guest checkout). Câu hỏi: **gõ cái gì TRƯỚC?**

## Sự thật về "base đã có"
Base BE thực chất là **vỏ rỗng**: `AppDbContext` trống (DbSet comment hết), `Repository<T>`/`UnitOfWork` generic chưa gắn entity, Models/DTOs/Services/GraphQL đều là thư mục rỗng. Packages đã đủ (EF Core 10, SqlServer, HotChocolate 15, JWT, AutoMapper, FluentValidation, BCrypt). → Bước A gần như từ con số 0, nhưng sạch.

## Quyết định đã chốt trong phiên
1. **Auth = Custom + BCrypt** (KHÔNG dùng ASP.NET Core Identity).
   - Lý do: schema Mục 1.1 vốn đã thiết kế custom-auth (RefreshTokens, EmailOtps, FailedLoginCount, LockoutEnd, TwoFASecret); BCrypt đã cài; Phase 1 chỉ cần login cơ bản. Identity sẽ làm nhiều cột/bảng thừa + đổi nền tảng (`IdentityDbContext`).
   - Hệ quả: `Users.PasswordHash` = BCrypt. `RefreshTokens` rotate + cờ `Revoked`. `TwoFASecret`/2FA để Phase 2 (cột nullable, chưa code).
2. **Migration đầu tiên = Lát 1: catalog + auth tối thiểu** (KHÔNG gõ cả 11 nhóm bảng 1 lần).
   - EF migration cộng dồn được → cắt lát, mỗi lát chạy được rồi bồi tiếp.

## Phạm vi Lát 1 (migration `InitialCatalog`)
- Auth tối thiểu: `Roles`, `Users`, `RefreshTokens`
- Catalog: `Brands`, `Categories`, `Products`, `ProductImages`, `ProductVariants`
- Hệ Attribute đa ngành (sống còn — Mục 0): `Attributes`, `AttributeValues`, `CategoryAttributes`, `VariantAttributeValues`
- (`EmailOtps`, `ProductSpecs` có thể kéo vào Lát 1 hoặc để lát sau — tùy.)

## Thứ tự thực thi
1. Chốt connection string + DB rỗng (LocalDB/SQL Server).
2. Viết entity Lát 1 vào `Models/`. Tiền = `decimal(18,2)`. Unique index qua Fluent API: `Users.Email`, `Products.Slug`, `ProductVariants.Sku`.
3. Khai báo `DbSet<>` trong `AppDbContext`.
4. `dotnet ef migrations add InitialCatalog` → `dotnet ef database update` → **DB chạy được**.
5. Seed 2–3 ngành (vợt / giày / sách) để CHỨNG MINH mô hình Attribute gánh nổi đa ngành trước khi xây API.

## Rủi ro / lưu ý
- **Bỏ `Price`/`Stock` khỏi `Products`** — nguồn sự thật giá+tồn chỉ ở `ProductVariants` (Mục 1 🔴). Products chỉ giữ `DisplayPrice` denormalized, KHÔNG trừ kho.
- Mô hình Attribute (4 bảng) nặng tay hơn `ProductVariantOptions`, nhưng đa ngành → làm luôn để khỏi migrate dữ liệu sau.
- .NET 10 mới → đã verify EF Core 10 + HotChocolate 15 có trong csproj, ổn.
- Lát 1 chưa có Cart/Order → chưa đặt được đơn; đó là Lát 2, đúng lộ trình.

## VIỆC SỐ 1 (ngày đầu) — dựng "đường ray" trước khi gõ entity
> Không gõ entity ngay. Chứng minh pipeline EF chạy được trên DbContext rỗng, để tách lỗi môi trường khỏi lỗi schema.

DB host đã chốt: **Docker SQL Server** (mcr.microsoft.com/mssql/server:2022-latest).

3 bẫy Docker phải biết:
1. Mật khẩu SA ≥8 ký tự đủ hoa/thường/số/đặc biệt — yếu thì container Exited im lặng.
2. Connection string PHẢI có `TrustServerCertificate=True` (EF Core 10 mặc định Encrypt=True, cert self-signed sẽ bị từ chối).
3. Mount volume để không mất data khi `docker rm`.

Conn (Development):
`Server=localhost,1433;Database=ECommerceDb;User Id=sa;Password=<Strong#Pass>;TrustServerCertificate=True`

Trình tự:
1. `docker run` container SQL Server, port 1433, MSSQL_SA_PASSWORD mạnh, volume.
2. `docker ps` xác nhận container Up (không Exited).
3. Dán conn string vào appsettings.Development.json.
4. Đăng ký AppDbContext + UseSqlServer trong Program.cs (hiện CHƯA có).
5. dotnet build xanh → `dotnet tool install -g dotnet-ef` → thêm entity nháp `Ping` → `migrations add Init` → `database update`.
6. Kiểm chứng DB + __EFMigrationsHistory + Ping → pipeline thông → xóa Ping → gõ Lát 1.

## ✅ TIẾN ĐỘ ĐÃ LÀM (cập nhật 2026-06-29)
- Môi trường: .NET 10 SDK + Docker SQL Server (container `ecom-sql`, pass `Ecom@2026Dev`, DB `ECommerceDb`).
- `Program.cs`: AddDbContext + UseSqlServer (key conn = `DefaultConnection` trong appsettings.Development.json).
- **Lát 1 = 12 entity** đã viết + migrate (custom auth + BCrypt, IEntityTypeConfiguration, BaseEntity soft-delete, cascade an toàn). Class `ProductAttribute` (tránh đụng System.Attribute), bảng vẫn tên `Attributes`.
- Migration: `InitialCatalog`, `SeedRoles` (4 vai Admin/Manager/Staff/Customer).
- Dọn nền: xóa WeatherForecast; nâng HotChocolate 16.3.0 (hết NU1904).
- ⚠️ Lưu ý vận hành: chạy `dotnet ef` phải set `$env:ASPNETCORE_ENVIRONMENT="Development"` (không thì đọc Production, không thấy conn string). Container hay Exited khi máy sleep → `docker start ecom-sql`.
- 4 warning EF 10622 (query filter ↔ child không filter) = vô hại, cố ý.

## Bước kế tiếp sau Lát 1
Lát 2:Notification  `Addresses`, `Carts/CartItems`, `Orders/OrderItems/OrderStatusHistories` → đủ để vào Phase 1 (checkout COD, đặt đơn thật). Sau đó mới Payment/Coupon/Review/Chat/(Lát 3+).

