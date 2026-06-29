# PLAN WEB THƯƠNG MẠI ĐIỆN TỬ ĐA NGÀNH HÀNG 

**Stack chốt:** BE ASP.NET API (.NET 10) — đã có base | FE Next.js (Node 24.13.1) — chưa có base
**Base BE hiện có (từ Solution Explorer):** Repository + UnitOfWork + GraphQL + DTOs + Models + Controllers → kiến trúc layered đã ổn, build tiếp lên trên là được.

> **v3 — đã chốt định hướng:** Schema (Mục 1) đã merge fix. Kiến trúc FE, API, guest checkout đã chốt (xem Mục 7). Ký hiệu schema: ✚ = thêm · ✎ = sửa/bỏ · 🔒 = mã hóa.

---

## 0. Quyết định lớn nhất phải chốt trước: MÔ HÌNH CATALOG ĐA NGÀNH

Đây là điểm sống còn khi bán **nhiều loại hàng khác hẳn nhau** (vợt, giày, sách) trong cùng 1 web. Sai chỗ này là phải đập đi xây lại.

**Đừng** làm mỗi sản phẩm 1 bảng (bảng vợt, bảng giày, bảng sách). **Cũng đừng** nhét tất cả thuộc tính vào 1 bảng `Product` phẳng.

**Khuyến nghị — mô hình lai (hybrid), thực dụng:**

```
Category  →  Product  →  ProductVariant (SKU)
                 │              │
                 └── Attribute (định nghĩa theo Category)
```

- **Product** = mặt hàng người dùng nhìn thấy (1 cái vợt Yonex Astrox 100ZZ, 1 đôi giày Lining, 1 cuốn sách).
- **ProductVariant (SKU)** = thứ thực sự bán & trừ kho. Mỗi tổ hợp option là 1 SKU riêng có **giá + tồn kho** riêng.
  - Giày → variant theo *Size (42, 43)* + *Màu*
  - Vợt → variant theo *Độ cứng/U (3U, 4U)* + *Grip (G4, G5)*
  - Sách → thường 1 variant (hoặc theo *Bìa cứng / Bìa mềm*)
- **Attribute / AttributeValue** = hệ thuộc tính linh hoạt, gán theo Category. Nhờ vậy thêm ngành hàng mới (ví dụ "Quần áo") **không cần đổi schema**, chỉ thêm dữ liệu.

→ Một schema này gánh được cả 3 ngành + mọi ngành tương lai. Đây là cách Shopify/Magento làm.

---

## 1. SCHEMA DB (bản đã sửa từ DB hiện tại của bạn)

### 🔴 4 thay đổi quan trọng nhất (đọc trước)
1. **Bỏ `Price`/`Stock` khỏi `Products`** — nguồn sự thật về giá & tồn **chỉ ở `ProductVariants`** (tránh mâu thuẫn). Mọi SP bán qua variant (sách = 1 variant mặc định).
2. **Bỏ `ProductVariants.VariantName` (string)** → thay bằng hệ **Attribute** có cấu trúc, để **filter/facet theo ngành** ("giày size 42", "vợt 3U").
3. **`Orders` phải SNAPSHOT địa chỉ giao** (không chỉ giữ `AddressId`) — sửa/xóa địa chỉ sau không làm hỏng đơn cũ.
4. **`Payments.TransactionId` UNIQUE + `RawPayload`** — chống cộng tiền 2 lần khi webhook bắn lặp.

---

### 1. Authentication & Users
- **Users** (Id, Email `[UNIQUE]`, PasswordHash, FullName, Phone, AvatarUrl, RoleId, **EmailConfirmed ✚**, Is2FAEnabled, TwoFASecret 🔒, Provider, ProviderId `[UNIQUE(Provider,ProviderId)]`, **FailedLoginCount ✚**, **LockoutEnd ✚**, LastLoginAt, CreatedAt, UpdatedAt, IsActive, DeletedAt) — ✎ **bỏ cột `Address`** (trùng bảng Addresses)
- **Roles** (Id, Name, Description)
- **RefreshTokens** (Id, UserId, Token, ExpiresAt, CreatedAt, Revoked)
- **EmailOtps** (Id, Email, OtpCode, **Purpose ✚** (register/login/reset), **AttemptCount ✚**, ExpiresAt, Used, CreatedAt)

> Khuyến nghị: dùng **ASP.NET Core Identity** thì các cột 2FA / external login / lockout này có sẵn, đỡ tự quản.

### 2. Address Management
- **Addresses** (Id, UserId, FullName, Phone, Province, District, Ward, AddressLine, IsDefault, CreatedAt, UpdatedAt, DeletedAt) — ✅ giữ nguyên, ổn.

### 3. Product Management
- **Brands** (Id, Name, **Slug ✚**, Description, CreatedAt, UpdatedAt, DeletedAt)
- **Categories** (Id, Name, **Slug ✚**, Description, ParentId, **SortOrder ✚**, **IsActive ✚**, CreatedAt, UpdatedAt, DeletedAt)
- **Products** (Id, Name, Slug `[UNIQUE]`, Description, CategoryId, BrandId, **DisplayPrice ✎** (giá "từ…" hiển thị, denormalized — **KHÔNG trừ kho**), ViewCount, SoldCount, Thumbnail, CreatedAt, UpdatedAt, IsActive, DeletedAt) — 🔴 ✎ **bỏ `Price` cũ & `Stock`**
- **ProductImages** (Id, ProductId, ImageUrl, IsMain, SortOrder, CreatedAt) — ✅ giữ
- **ProductVariants** (Id, ProductId, **Sku ✚** `[UNIQUE]`, Price, **CompareAtPrice ✚**, Stock, **ImageUrl ✚**, **Weight ✚**, **IsActive ✚**, CreatedAt, UpdatedAt, DeletedAt) — 🔴 ✎ **bỏ `VariantName`**

**✚ Hệ Attribute (mới — để filter đa ngành):**
- **Attributes** (Id, Name, Code, Type (select/number/text), IsVariant) — ví dụ: Size, Màu, Độ cứng(U), Grip, Định dạng sách
- **AttributeValues** (Id, AttributeId, Value) — 42, 43, Đỏ, 3U, 4U, "Bìa cứng"
- **CategoryAttributes** (Id, CategoryId, AttributeId, IsRequired) — ngành nào dùng thuộc tính nào
- **VariantAttributeValues** (Id, VariantId, AttributeId, AttributeValueId) — tổ hợp option của 1 SKU
- **ProductSpecs ✚** *(tùy chọn)* (Id, ProductId, SpecKey, SpecValue) — thông số không-biến-thể: ISBN, Tác giả, Chất liệu cán…

> 💡 Nếu **giai đoạn đầu chưa cần filter mạnh**, có thể tạm dùng bảng nhẹ **`ProductVariantOptions`** (Id, VariantId, OptionName, OptionValue) thay cho 4 bảng Attribute — nhưng đổi sang Attribute sau sẽ phải migrate dữ liệu. Đa ngành như bạn → nên làm Attribute luôn.

### 4. Cart System
- **Carts** (Id, UserId `nullable`, **SessionId ✚** `nullable` (cho khách chưa login — đã chốt cho guest checkout), CreatedAt, UpdatedAt)
- **CartItems** (Id, CartId, ProductId, VariantId, Quantity, Price (snapshot lúc thêm), CreatedAt, UpdatedAt)

### 5. Wishlist
- **Wishlists** (Id, UserId, CreatedAt) · **WishlistItems** (Id, WishlistId, ProductId, CreatedAt) — ✅ giữ

### 6. Order System
- **Orders** (Id, OrderCode `[UNIQUE]`, UserId, ~~AddressId~~ → **snapshot giao hàng ✚**: ShipRecipient, ShipPhone, ShipProvince, ShipDistrict, ShipWard, ShipAddressLine, TotalAmount (subtotal), ShippingFee, DiscountAmount, FinalAmount, Status, **PaymentStatus ✚** (tách khỏi Status đơn), PaymentMethod, Note, CreatedAt, UpdatedAt, DeletedAt)
  - 🔴 ✎ Có thể giữ `AddressId` để tham chiếu, **nhưng bắt buộc copy snapshot** các cột Ship* ở trên. (Muốn gọn thì tách bảng riêng **OrderShippingInfo**.)
- **OrderItems** (Id, OrderId, ProductId, VariantId, ProductName, **VariantName ✚**, **Sku ✚**, ImageUrl, Quantity, Price, DiscountAmount, FinalPrice) — ✅ snapshot tốt, chỉ thêm thông tin variant
- **OrderStatusHistories** (Id, OrderId, Status, **Note ✚**, **ChangedBy ✚** (NV nào đổi), CreatedAt)

> **Order status:** Pending → Confirmed → Packing → Shipping → Delivered → Completed | Cancelled | Refunded
> **Payment status:** Unpaid → Pending → Paid → Failed → Refunded

### 7. Payment
- **Payments** (Id, OrderId, Method, TransactionId `[UNIQUE]` 🔴, Amount, Status, **RawPayload ✚** (lưu callback cổng để đối soát), **PaidAt ✚**, CreatedAt, **UpdatedAt ✚**)

### 8. Coupon / Discount
- **Coupons** (Id, Code `[UNIQUE]`, **DiscountType ✚** (percent/fixed), **DiscountValue ✚**, ~~DiscountPercent~~ ✎ gộp vào DiscountType+Value, MaxDiscountAmount, MinOrderAmount, UsageLimit, UsedCount, UserUsageLimit, **StartsAt ✚**, ExpiredAt, CreatedAt, IsActive, DeletedAt) — ✎ thêm giảm **số tiền cố định** + lịch bắt đầu
- **CouponUsages** (Id, CouponId, UserId, OrderId, DiscountAmount, CreatedAt) — ✅ giữ

### 9. Chat Realtime
- **Chats** (Id, User1Id, User2Id, CreatedAt)
- **Messages** (Id, ChatId, SenderId, Content, **IsRead ✚**, **ReadAt ✚**, CreatedAt)

### 10. Notifications
- **Notifications** (Id, UserId, **Type ✚**, Title, Content, **Link/RefId ✚** (deep-link tới đơn), IsRead, CreatedAt)

### 11. Product Reviews
- **Reviews** (Id, UserId, ProductId, **OrderId ✚** (verified purchase), Rating, Comment, **Status ✚** (duyệt/ẩn), CreatedAt, UpdatedAt, DeletedAt)

### ⚪ Quy ước chung (áp cho cả DB)
- **Tiền = `decimal(18,2)`**, không dùng float.
- **Unique index:** Users.Email · (Provider,ProviderId) · Products.Slug · ProductVariants.Sku · Orders.OrderCode · Coupons.Code · Payments.TransactionId.
- **`TwoFASecret` mã hóa at-rest** 🔒.
- **Soft-delete (`DeletedAt`)** chốt policy đồng nhất; riêng `Orders` **không xóa cứng**.
- `RoleId` đơn (mỗi user 1 role) — ổn cho 4 vai trò; chỉ đổi sang bảng `UserRoles` nếu sau này 1 người kiêm nhiều vai.

---

## 2. BACKEND — xây gì trên base có sẵn

Bạn đã có Repository + UoW + GraphQL. Bổ sung:

1. **ASP.NET Core Identity** — đừng tự code auth. Identity lo sẵn: user/role, hash password, **email confirm (OTP), Google external login, 2FA authenticator** → đúng y hệt yêu cầu trong doc, tiết kiệm hàng tuần code.
2. **JWT + Refresh token** — cấp token sau login, FE giữ trong httpOnly cookie.
3. **Phân tầng API — ✅ ĐÃ CHỐT: REST-ghi + GraphQL-đọc:**
   - **GraphQL** → **đọc** catalog cho storefront (query lồng product → variant → attribute → review rất gọn, FE chọn field tránh over-fetch). Tận dụng đúng phần GraphQL bạn đã scaffold.
   - **REST Controllers** → **xương sống / mọi lệnh ghi & callback ngoài**: auth, checkout, **payment IPN/webhook** (cổng bắn HTTP POST, GraphQL không hợp), upload ảnh, admin CRUD, SignalR negotiate.
   - *(Nếu team chưa quen GraphQL và muốn ra MVP nhanh → làm REST hết trước, thêm GraphQL cho catalog sau. Đừng để GraphQL chặn tiến độ.)*
4. **FluentValidation** (validate input) + **AutoMapper** (đã có DTOs).
5. **Background job** (Hangfire hoặc HostedService): gửi email, tự hủy đơn quá hạn chưa thanh toán.
6. **Redis:** cache catalog (TTL), giỏ hàng khách vãng lai, rate-limit, backplane cho SignalR khi scale nhiều instance.
7. **Payment sandbox:** mỗi cổng (VNPay/Momo/ZaloPay) có *Return URL* (user quay về) + *IPN/Webhook* (server xác nhận). Bắt buộc: **verify chữ ký** + **xử lý idempotent** (1 txn_ref chỉ cộng tiền 1 lần dù webhook bắn nhiều lần).
8. **Email:** Gmail SMTP ok cho volume thấp (có giới hạn gửi/ngày); volume cao thì đổi sang provider transactional sau.

---

## 3. FRONTEND — ✅ ĐÃ CHỐT: TÁCH 2 APP TRONG MONOREPO

### Cấu trúc
```
monorepo/ (Turborepo + pnpm workspace)
├── apps/
│   ├── storefront   → Next.js App Router (SSR/SEO) — cho khách
│   └── admin        → Next.js HOẶC Vite+React SPA (nhẹ, không cần SEO) — cho quản trị
├── packages/
│   ├── types        → DTO / type / API SDK client dùng chung
│   └── ui            → component dùng chung
```

**Vì sao tách** (không nhồi chung 1 app): storefront cần SSR/SEO + bundle nhẹ cho khách; admin là mớ table/form/chart nặng, nhồi chung làm chậm trang bán hàng; admin còn khác **auth surface** (giấu sau IP allowlist/VPN), khác **nhịp deploy**, và team đông thì 2 nhóm làm song song không đụng nhau. Monorepo cho phép vừa tách app vừa **share type + UI**.

### Stack & cách làm
- **TypeScript + Tailwind + shadcn/ui.**
- **Data fetching:**
  - Trang catalog/sản phẩm → **Server Component** (SSR/ISR) để SEO.
  - Tương tác client (giỏ, filter, checkout) → **React Query/SWR**.
  - GraphQL client: **urql** hoặc Apollo (cho phần đọc catalog).
- **Auth FE:** Auth.js (NextAuth) — Credentials provider gọi BE + Google provider; token cất httpOnly cookie; luồng 2FA do BE xử lý.
- **Realtime:** SignalR client cho widget chat + chuông thông báo.

### Danh sách trang tối thiểu
**apps/storefront:** Trang chủ · Danh mục/Listing (filter theo *size/brand/giá/độ cứng vợt...* — chính là Attribute facet) · Chi tiết SP (chọn variant → đổi giá/tồn) · Giỏ hàng · Checkout (địa chỉ → vận chuyển → thanh toán) · Theo dõi đơn · Tài khoản (đơn, địa chỉ, wishlist) · Đăng nhập/ký.
**apps/admin:** Dashboard · CRUD Product/Variant · Quản lý đơn (đổi trạng thái) · Khách hàng · Tồn kho · Coupon · Inbox chat · Phân quyền nhân viên.

### SEO (storefront — bắt buộc)
ISR cho trang sản phẩm/danh mục · sitemap.xml · structured data schema.org/Product · ảnh dùng `next/image`.

---

## 4. ÁNH XẠ YÊU CẦU TRONG DOC → CÁCH LÀM

| Yêu cầu trong doc | Triển khai |
|---|---|
| Login Admin/Quản trị/NV/Customer | Identity Roles + policy-based authorization |
| OTP email / Login Google / 2FA | Identity built-in (email token, Google OAuth, Authenticator) |
| Mã hóa password Bcrypt | Identity mặc định dùng PBKDF2; muốn Bcrypt thì cắm `IPasswordHasher` custom |
| Chat realtime + Notification | SignalR Hub + bảng Notification + Redis backplane |
| Gmail services | SMTP gửi mail xác nhận đơn / OTP / cập nhật trạng thái |
| COD / Banking / Momo / ZaloPay / VNPay | Strategy pattern: 1 `IPaymentProvider`, mỗi cổng 1 implementation |
| Redis cache | Distributed cache catalog + cart khách + rate limit |

---

## 5. LỘ TRÌNH THEO PHASE

> Nguyên tắc: ưu tiên **đặt được 1 đơn hàng thật** sớm nhất, rồi mới bồi tính năng.

**Phase 0 — Nền (chốt trước khi code FE):**
☐ Chốt schema DB + viết migration · ☐ Chốt hợp đồng API (REST/GraphQL) · ☐ Dựng skeleton monorepo Next.js + design system + vỏ auth.

**Phase 1 — MVP Storefront (mục tiêu: đặt được đơn COD):**
☐ Catalog (Category/Product/Variant) · ☐ Trang chi tiết chọn variant · ☐ Giỏ hàng (guest + user) · ☐ Checkout COD · ☐ Tạo đơn + lịch sử đơn · ☐ Admin CRUD sản phẩm cơ bản.

**Phase 2 — Thanh toán & Tài khoản:**
☐ VNPay/Momo/ZaloPay sandbox · ☐ OTP email + Google login + 2FA · ☐ Email thông báo đơn · ☐ Coupon.

**Phase 3 — Tương tác & Vận hành:**
☐ Review (verified purchase) + Wishlist · ☐ SignalR chat + notification · ☐ Redis cache · ☐ Filter/facet + search · ☐ Admin dashboard đầy đủ (đơn, tồn kho, phân quyền NV).

**Phase 4 — Hoàn thiện & Scale:**
☐ Performance (ISR/CDN/tối ưu ảnh) · ☐ Analytics + SEO · ☐ Observability + rate limit + security hardening · ☐ Tích hợp vận chuyển (GHN/GHTK).

---

## 6. RỦI RO / LƯU Ý

- **Snapshot giá & tên SP vào OrderItem** lúc đặt — không join về Product/Variant (giá đổi sau sẽ làm sai đơn cũ).
- **Webhook thanh toán phải idempotent + verify chữ ký** — đây là chỗ hay bị mất tiền/double-count nhất.
- **Trừ kho ở tầng SKU (ProductVariants)** và xử lý race condition (2 người mua đôi giày size 42 cuối cùng) — dùng transaction + optimistic concurrency.
- **Merge giỏ guest → giỏ user khi login** (vì có guest checkout) — gộp CartItems theo VariantId, cộng dồn số lượng.
- **.NET 10 còn rất mới** — kiểm tra package thư viện (Hangfire, payment SDK...) đã hỗ trợ chưa, kẻo kẹt version.

---

## 7. QUYẾT ĐỊNH ĐÃ CHỐT + BƯỚC TIẾP THEO

### ✅ Đã chốt
1. **Schema** — bản sửa ở Mục 1.
2. **Catalog đa ngành** — Product → Variant (SKU) + hệ Attribute (Mục 0).
3. **FE** — tách **2 app trong monorepo** (storefront SSR + admin riêng), share type/UI (Mục 3).
4. **API** — **REST-ghi (xương sống) + GraphQL-đọc (storefront catalog)** (Mục 2.3).
5. **Guest checkout** — cho mua **không cần login**, giữ `Carts.SessionId`, **merge giỏ guest → user** khi đăng nhập.

### ☐ Bước tiếp theo (làm từ từ)
- **A. BE trước:** Entity classes C# + DbContext + **EF Core migration .NET 10** theo schema Mục 1 → có DB chạy được.
- **B. FE skeleton:** dựng monorepo (apps/storefront + apps/admin + packages) + vỏ auth + design system.
- → Xong A+B thì vào **Phase 1** (Mục 5): catalog → chi tiết variant → giỏ → checkout COD → đặt được đơn thật.

> Gợi ý thứ tự: làm **A trước** (có schema + API contract) rồi **B**, để FE có cái thật mà gọi thay vì mock.
