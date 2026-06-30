# BRAINSTORM — Nâng cấp FE hay sang Phase 2? (sau khi Phase 1 chạy)

> Phiên brainstorm 2026-06-30. Mục tiêu dự án đã chốt: **sản phẩm thật / launch**. Không implement — chỉ chốt hướng + thứ tự.

## Vấn đề
Phase 1 (luồng mua COD end-to-end) đã chạy trên web thật, nhưng FE còn thô. Hỏi: hoàn thiện FE tới mức nào trước khi sang Phase 2 (thanh toán online), hay song song?

## Sự thật về hiện trạng FE (`ecommerce-fe`)
- ~486 dòng, 8 page. Chỉ có `next + react + tailwind` — **không** component lib, design system, react-query, form lib.
- **Trang chủ vẫn là boilerplate create-next-app** ("edit page.tsx" + logo Next/Vercel) → trang chủ chưa tồn tại.
- Token để `localStorage` (rủi ro XSS) — lệch plan "httpOnly cookie".
- 1 app đơn (chưa phải monorepo storefront/admin như plan).
→ Đây là **proof-of-flow**, đúng & chạy được, chưa phải sản phẩm.

## Phân biệt (đang bị gộp làm một)
- (A) **Nền FE**: layout/nav/home + design system + component → mọi màn sau tái dùng.
- (B) **Đánh bóng pixel**: animation/ảnh/SEO/ISR → thuộc Phase 4.
- (C) **Phase 2**: thanh toán → đẻ thêm màn FE mới (chọn cổng, QR/redirect, return, order-success).

## Hai sự thật phũ cho mục tiêu "launch"
1. **Vật cản launch lớn nhất là ADMIN đã hoãn, không phải FE đẹp.** Khách đặt COD → không có chỗ xem/xác nhận/đổi trạng thái đơn, sửa tồn, CRUD sản phẩm → không vận hành được. COD-only vẫn launch được ở VN; thiếu admin vận hành thì không.
2. **Không ai nhập tiền trên web có logo Next.js ở homepage.** Online payment đòi storefront trông thật → nền FE là *điều kiện tiên quyết* của Phase 2, không phải trang trí.

## Quyết định: thứ tự mở khóa launch (tuần tự, solo dev — KHÔNG song song mù)
| # | Việc | Vì sao trước | Mức |
|---|------|------|-----|
| 1 | **Nền FE mỏng** — shadcn/ui + layout/nav + home thật + componentize 8 page | Thuần FE, không chờ BE; tái dùng mọi màn; gỡ boilerplate → tạo niềm tin | Vài ngày. "Gọn, nhất quán, đáng tin", KHÔNG pixel-perfect |
| 2 | **Admin tối thiểu** — list đơn + đổi trạng thái + CRUD sản phẩm/tồn cơ bản | Không có = không vận hành = không launch | Vừa đủ chạy; UI nội bộ xấu cũng được |
| 3 | **Thanh toán online** — SePay trước, khung `IPaymentProvider` (Strategy) | Sau khi storefront trông thật | Webhook idempotent + verify chữ ký, chắc 1 lần |
| 4 | **Email thông báo đơn + auth cứng** — httpOnly cookie thay localStorage, OTP | Bồi UX + bảo mật tiền | Theo plan Phase 2 |
| — | ~~Animation/SEO/ISR/tách monorepo~~ | Phase 4 / chưa cần (admin mới 1 app) | **Hoãn** |

## Bước tiếp theo ngay
**Bước 1 — Nền FE mỏng.** Cài shadcn/ui → layout + header/nav + footer → trang chủ thật (thay boilerplate) → componentize 8 page hiện có dùng chung Button/Card/Input/Badge.

## Nợ kỹ thuật ghi sổ (xử lý khi tới phase tương ứng)
- Token `localStorage` → chuyển httpOnly cookie ở Bước 4 (auth).
- 1 app → tách monorepo storefront/admin chỉ khi admin lớn lên (YAGNI, chưa cần).
