# RPMS Test Case Suite — Assumptions & Gaps

## Nguồn phân tích
- Source code RPMS (.NET 8 WinForms / BLL / DAL)
- `Docs/TongQuanDuAn_RPMS.docx`
- `Database/RPMS_Full.sql` + `DatabaseSchemaUpdater`
- Hành vi UI đã implement (Login, Contract Draft/Active, Meter tháng trước, Assignment theo Manager ID/Username…)

## Giả định
1. **RegisterForm** tạo user Active; role mặc định tùy UI (cần xác nhận Role mặc định khi đăng ký).
2. **BackupForm / BackupService**: Program.cs đăng ký nhưng file có thể thiếu trên disk — TC Backup ghi nhận “pass nếu có / fail rõ nếu thiếu”.
3. **Pay invoice notify Landlord**: có thể chưa implement notify chủ khi thanh toán — TC ghi “nếu implement”.
4. **Appointment past date**: validation phía UI có thể chưa chặn — TC exploratory/boundary.
5. **Email format validation**: có thể chỉ Unique, chưa regex — TC ghi nhận hành vi thực tế.
6. **Session timeout**: app desktop không có JWT timeout server; TC Logout thủ công thay cho timeout.
7. **XSS**: WinForms không execute HTML; TC security ghi nhận lưu plain text.
8. **Pagination**: nhiều list không phân trang server — TC Performance/Usability thay Pagination.
9. **Import**: không có import hàng loạt user/room — ngoài scope, không tạo TC Import trừ media upload.
10. **Direct URL bypass**: không có web URL; “Bypass UI” = không có menu + không resolve form trái role từ shell.

## Mâu thuẫn / rủi ro tài liệu
- README còn nói Admin “Phân công Manager” trong khi code đã chuyển sang **LandlordAssignmentForm**.
- CHECK Appointment: script SQL có `Completed`; EF config có thể khác — runtime DB là nguồn đúng.
- `IBackupService` đăng ký trong DI trong khi file implement có thể thiếu → build/runtime risk.

## Cách dùng file Excel
- Sheet `99_Summary`: tổng quan số lượng.
- Sheet `98_Coverage_Matrix`: bao phủ module × role.
- Sheet `00_All_TestCases`: full list + AutoFilter.
- Các sheet theo Module: thực thi theo đội.
- Cột **Actual Result / Status** để trống cho QA điền (Pass/Fail/Blocked).
- Priority: P0 blocker/smoke; P1 core; P2 nice-to-have.

## Khuyến nghị thực thi
1. Chạy hết **Smoke Suite** + **Regression Hotspots** trước mỗi build.
2. Chạy full P0 theo role trên môi trường SQLEXPRESS seed.
3. Automation Candidate=Yes ưu tiên API/service-level (xUnit + DB test), UI WinForms tự động hóa sau.
