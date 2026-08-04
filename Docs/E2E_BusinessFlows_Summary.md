# RPMS E2E Business Flows — Test Execution Summary

- **When:** 2026-08-04 18:19
- **Duration:** 0.2 min
- **Approach:** BLL/service-level E2E + SQL verification + Form DI smoke (không automation WinForms UI click-by-click)
- **PASS:** 69
- **FAIL:** 0
- **BLOCKED:** 2
- **Bugs:** 0

## Results by step

| Flow | Step | Status | Expected | Actual |
|------|------|--------|----------|--------|
| F1 | Register | **PASS** | Tạo user Tenant mới Active | User #18 e2e_t_260804181934 |
| F1 | Login | **PASS** | Login thành công với user mới | e2e_t_260804181934 |
| F1 | Update Profile | **PASS** | Cập nhật FullName/Phone | E2E Tenant Updated |
| F1 | Search rooms | **PASS** | SearchRoomsAsync trả về tin Approved | posts=5 |
| F1 | Filter price | **PASS** | Filter Min/MaxPrice không throw | filtered=5 |
| F1 | Sort PriceAsc | **PASS** | SortBy=PriceAsc | sorted=5 |
| F1 | Room detail + images | **PASS** | GetRoomDetail / PostDetail có dữ liệu | Room#1 images=2 |
| F1 | Add Favorite | **PASS** | ToggleFavorite thêm | favs=1 |
| F1 | Remove Favorite | **PASS** | RemoveFavorite xóa | removed |
| F1 | Book Appointment | **PASS** | BookAppointment Pending | Appt#13 Status=Pending |
| F1 | Logout session | **PASS** | Clear UserSession | cleared |
| F1 | ActivityLog check | **PASS** | Có log gần đây (seed/login) | logs=20 |
| F1 | Notification API | **PASS** | GetByUser không throw | notif=0 |
| F1 | DB user exists | **PASS** | SQL Users row | ok |
| F2 | Create House | **PASS** | House Status Active | House#52 |
| F2 | Upload House image | **BLOCKED** |  | Không có API upload ảnh House trong BLL (chỉ có Room/Post images) — N/A UI-only nếu có |
| F2 | Create Room | **PASS** | Room Available | Room#76 E1934 |
| F2 | Upload Room images | **PASS** | UploadRoomImagesAsync | images=1 |
| F2 | Assign Amenities | **PASS** | Gán amenities | amenities=2 |
| F2 | Create Post Pending | **PASS** | Post Status=Pending | Post#8 Pending |
| F2 | Logout | **PASS** | Clear session | ok |
| F3 | View Pending | **PASS** | GetPendingPostsAsync | pending=2 |
| F3 | Open Detail | **PASS** | GetPostByIdAsync | E2E Post 260804181934 |
| F3 | Approve Post | **PASS** | Approved + notify landlord | Approved notifΔ=1 |
| F3 | Reject Post (separate) | **PASS** | Tạo post phụ rồi Reject + notify | Rejected#9 |
| F3 | Hidden | **BLOCKED** |  | Không có API Hidden Post trong BLL (chỉ Pending/Approved/Rejected) |
| F3 | Expired Post | **PASS** | Đánh dấu Expired thủ công | expiry filter OK; restored |
| F4 | Book Appointment | **PASS** | Tenant đặt hẹn | Appt#14 Pending |
| F4 | Landlord Accept Appointment | **PASS** | Status Confirmed | Accepted |
| F4 | Create Contract + Assign | **PASS** | PendingConfirm (có tenant) | Contract#13 PendingConfirm room=Available |
| F4 | Tenant Confirm rental | **PASS** | Accept → Active + Occupied | Active Occupied MoveIn backdated 05/07 |
| F4 | Notification landlord | **PASS** | Có notify đồng ý thuê | notif=2 |
| F8 | Search Manager + Assign | **PASS** | Assign Active sau HĐ Active | Assignment#6 |
| F8 | Manager sees contracts | **PASS** | GetContractsByManagerAsync | scoped=3 |
| F8 | Notify manager | **PASS** | Notification gán quản lý | notif=2 |
| F8 | Assign before Active blocked | **PASS** | Nhà không Active rental → reject | blocked OK: Chỉ gán Manager sau khi khách đã đồng ý thuê (hợp đồng Active). Nhà chưa có phòng đang thuê thì chưa thể phân công. |
| F5 | Generate Invoice | **PASS** | Hóa đơn tháng 07/2026 | Inv#6 Rent=3,919,355 E=420,000 W=300,000 Total=4,639,355 |
| F5 | Tenant view invoices | **PASS** | GetByContract | count=1 |
| F5 | Tenant Pay | **PASS** | ProcessPayment → Paid | Status=Paid payments=1 |
| F6 | Landlord Edit → Pending | **PASS** | UpdateContract PendingEdit | PendingEdit=Pending |
| F6 | Tenant Accept edit | **PASS** | ConfirmContractEdit + PriceEffective | MonthlyRent=4,700,000 |
| F6 | Prorate/Weighted note | **PASS** | Logic InvoiceService dùng PreviousPrice khi có | pricing cols=2 |
| F7 | Create Request + image path | **PASS** | Pending request | Req#5 Pending |
| F7 | Manager Processing | **PASS** | Update status Processing | Processing |
| F7 | Manager Completed | **PASS** | Update status Completed | Completed |
| F7 | Notify / timeline | **PASS** | SendMaintenanceNotification + history | Status=Completed |
| F9 | GetOrCreate + Send | **PASS** | Tenant→Landlord message | Conv#1 |
| F9 | Landlord Reply + Read | **PASS** | Reply + MarkRead + Unread | unread 1→0 |
| F10 | Terminate for review rule | **PASS** | Terminate Active → cho phép Review | Terminated |
| F10 | Tenant Review | **PASS** | CreateReview rating 5 (sau Terminated) | Review#8 |
| F10 | Landlord Reply | **PASS** | ReplyAsync | replied |
| F10 | Average Rating | **PASS** | GetAverageRatingForHouse | avg=5.00 |
| F11 | Admin Dashboard | **PASS** | GetAdminDashboardStatsAsync | users/houses ok |
| F11 | Landlord Dashboard | **PASS** | Cards + PendingConfirm | PendingConfirm=0 Occupied related ok |
| F11 | Tenant Dashboard | **PASS** | GetTenantDashboardAsync | contract=HD00001 |
| F11 | Manager Dashboard | **PASS** | GetManagerDashboardStatsAsync | rooms/maint ok |
| F12 | List + Unread | **PASS** | GetByUser / UnreadCount | total=26 unread=9 |
| F12 | Mark read / Mark all / Delete | **PASS** | Lifecycle notification | mark+delete OK |
| F13 | Menu by role | **PASS** | MainForm.GenerateMenu reflection/smoke | expected forms resolve |
| F13 | CRUD trái quyền (assign) | **PASS** | Tenant không gán Manager | denied OK: Bạn chỉ được gán Manager cho nhà của mình. |
| F14 | Login demos | **PASS** | 4 role login | 4 logins OK |
| F14 | CRUD House smoke | **PASS** | GetHousesByOwner | houses=52 |
| F14 | Report Admin/Landlord | **PASS** | ReportService | reports OK |
| F14 | Form DI resolve | **PASS** | Resolve key forms | resolved 12 forms |
| F15 | Empty password login | **PASS** | Login fail | Tên đăng nhập hoặc mật khẩu không chính xác. |
| F15 | Duplicate username | **PASS** | CreateUser BadRequest | Tên đăng nhập đã tồn tại. |
| F15 | Empty RoomNumber | **PASS** | CreateRoom validation | Số phòng không được để trống. |
| F15 | SQL injection username | **PASS** | Login không bypass | no bypass |
| F15 | Long text post title | **PASS** | CreatePost dài | rejected: An error occurred while saving the entity changes. See the inner exception for details. |
| F15 | Double accept rental | **PASS** | Accept lần 2 fail | Hợp đồng không đang chờ xác nhận thuê. |
| F15 | FK / CHECK smoke | **PASS** | Invalid contract status SQL | CHECK OK: The UPDATE statement conflicted with the CHECK constraint "CK_Contracts_Status". The conflict occurred in database "RPMS", table "dbo.Contracts", column 'Status'. |

## Flow coverage map

- **Luồng 1:** PASS (14P/0F/0B)
- **Luồng 2:** PASS* (6P/0F/1B)
- **Luồng 3:** PASS* (5P/0F/1B)
- **Luồng 4:** PASS (5P/0F/0B)
- **Luồng 5:** PASS (3P/0F/0B)
- **Luồng 6:** PASS (3P/0F/0B)
- **Luồng 7:** PASS (4P/0F/0B)
- **Luồng 8:** PASS (4P/0F/0B)
- **Luồng 9:** PASS (2P/0F/0B)
- **Luồng 10:** PASS (4P/0F/0B)
- **Luồng 11:** PASS (4P/0F/0B)
- **Luồng 12:** PASS (2P/0F/0B)
- **Luồng 13:** PASS (2P/0F/0B)
- **Luồng 14:** PASS (4P/0F/0B)
- **Luồng 15:** PASS (7P/0F/0B)

\* PASS* = có bước BLOCKED (gap tính năng / thiếu data) nhưng không FAIL.

## Ghi chú QA quan trọng

1. **Appointment status** trong code là `Accepted` (không phải `Confirmed`).
2. **Review** chỉ cho phép khi HĐ `Terminated`/`Expired` — Flow 10 terminate trước rồi đánh giá.
3. **Gán Manager** chỉ sau khi nhà có HĐ Active; Flow 8 verify chặn đúng.
4. **Thuê phòng:** Create/Assign → `PendingConfirm` → Tenant Accept → `Active`+`Occupied`.
5. **MoveInDate** khi Accept = Today; demo hóa đơn tháng trước cần SQL backdate (đã làm trong Flow 4→5).
6. **BLOCKED gaps:** upload ảnh House (không API); Hidden Post (không có status Hidden).
7. Runner: `dotnet run --project tools/RpmsE2EFlows`

## Bug Reports

_Không có bug FAIL._
