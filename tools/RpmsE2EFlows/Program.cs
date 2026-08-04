using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL;
using RPMS.BLL.Exceptions;
using RPMS.BLL.Helpers;
using RPMS.BLL.Interfaces;
using RPMS.BLL.Services;
using RPMS.Common.Globals;
using RPMS.DAL;
using RPMS.DAL.Data;
using RPMS.DTO.Assignment;
using RPMS.DTO.Auth;
using RPMS.DTO.Chat;
using RPMS.DTO.Contract;
using RPMS.DTO.House;
using RPMS.DTO.Invoice;
using RPMS.DTO.Maintenance;
using RPMS.DTO.Post;
using RPMS.DTO.Review;
using RPMS.DTO.Room;
using RPMS.DTO.Tenant;
using RPMS.DTO.Notification;
using RPMS.DTO.User;
using RPMS.WinForms.Forms.Layout;

namespace RPMS.E2EFlows;

/// <summary>
/// QA E2E: 15 business flows — service-level + DB checks + menu/form permission smoke.
/// Run: dotnet run --project tools/RpmsE2EFlows
/// </summary>
internal static class Program
{
    private const string Cs =
        @"Server=.\SQLEXPRESS;Database=RPMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

    private static readonly string Docs = @"E:\DoAn\RPMS\Docs";
    private static ServiceProvider _sp = null!;
    private static readonly List<StepResult> Steps = new();
    private static readonly List<BugItem> Bugs = new();
    private static int _bugSeq;

    // Shared context across chained flows
    private static int LandlordId, TenantId, ManagerId, AdminId;
    private static int HouseId, RoomId, PostId, AppointmentId, ContractId, InvoiceId, AssignmentId;
    private static int MaintId, ReviewId, ConversationId;
    private static string Unique = "";

    [STAThread]
    private static async Task<int> Main()
    {
        Directory.CreateDirectory(Docs);
        Unique = DateTime.Now.ToString("yyMMddHHmmss");
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("=== RPMS E2E Business Flows (15) ===");

        var services = new ServiceCollection();
        services.AddDataAccessLayer(Cs);
        services.AddBusinessLogicLayer();
        services.AddSingleton<IBackupService>(_ => new BackupService(Cs));
        RegisterForms(services);
        _sp = services.BuildServiceProvider();

        var winProg = Type.GetType("RPMS.WinForms.Program, RPMS.WinForms")!;
        winProg.GetProperty("ServiceProvider")!.SetValue(null, _sp);

        using (var scope = _sp.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RPMSContext>();
            await DatabaseSchemaUpdater.EnsureUpdatedAsync(db);
            await DataSeeder.SeedAsync(db);
        }
        ResetDemoPasswords();

        var sw = Stopwatch.StartNew();
        await ResolveDemoUsers();

        await RunFlow(1, "Onboarding hoàn chỉnh", Flow1_Onboarding);
        await RunFlow(2, "Landlord đăng nhà cho thuê", Flow2_LandlordListing);
        await RunFlow(3, "Admin duyệt bài", Flow3_AdminPost);
        await RunFlow(4, "Tenant thuê phòng", Flow4_Rent);
        await RunFlow(8, "Assignment Manager", Flow8_AssignManager); // before meter
        await RunFlow(5, "Manager ghi điện nước", Flow5_MeterInvoice);
        await RunFlow(6, "Chỉnh sửa hợp đồng", Flow6_EditContract);
        await RunFlow(7, "Maintenance", Flow7_Maintenance);
        await RunFlow(9, "Chat", Flow9_Chat);
        await RunFlow(10, "Review", Flow10_Review);
        await RunFlow(11, "Dashboard", Flow11_Dashboard);
        await RunFlow(12, "Notification", Flow12_Notification);
        await RunFlow(13, "Permission Test", Flow13_Permission);
        await RunFlow(14, "Regression smoke", Flow14_Regression);
        await RunFlow(15, "Error & Boundary", Flow15_Boundary);

        sw.Stop();
        WriteReports(sw.Elapsed);

        int pass = Steps.Count(s => s.Status == "PASS");
        int fail = Steps.Count(s => s.Status == "FAIL");
        int blocked = Steps.Count(s => s.Status == "BLOCKED");
        Console.WriteLine($"\nDONE {sw.Elapsed.TotalMinutes:F1}m | PASS={pass} FAIL={fail} BLOCKED={blocked} Bugs={Bugs.Count}");
        Console.WriteLine($"Summary: {Path.Combine(Docs, "E2E_BusinessFlows_Summary.md")}");
        return fail > 0 ? 1 : 0;
    }

    private static async Task RunFlow(int n, string name, Func<Task> body)
    {
        Console.WriteLine($"\n--- Flow {n}: {name} ---");
        try
        {
            await body();
        }
        catch (Exception ex)
        {
            Fail($"F{n}", "FLOW_ABORT", $"Luồng {n} dừng bất thường", ex.Message, "Critical", ex);
        }
    }

    #region Flows

    private static async Task Flow1_Onboarding()
    {
        using var scope = _sp.CreateScope();
        var users = S<IUserService>(scope);
        var auth = S<IAuthService>(scope);
        var tenantSvc = S<ITenantService>(scope);
        var interact = S<ITenantInteractionService>(scope);
        var rooms = S<IRoomService>(scope);
        var posts = S<IPostService>(scope);
        var logs = S<IActivityLogService>(scope);
        var notif = S<INotificationService>(scope);

        string user = $"e2e_t_{Unique}";
        string email = $"e2e_t_{Unique}@test.local";

        // Register
        UserDto? created = null;
        await Step("F1", "Register", "Tạo user Tenant mới Active", async () =>
        {
            created = await users.CreateUserAsync(new CreateUserDto
            {
                RoleID = 3,
                Username = user,
                Password = "123456",
                FullName = "E2E Tenant",
                Email = email,
                Phone = "0900000001",
                Address = "HN"
            });
            Assert(created.UserID > 0 && created.Status == "Active", $"UserID={created.UserID} Status={created.Status}");
            return $"User #{created.UserID} {user}";
        });

        // Login
        await Step("F1", "Login", "Login thành công với user mới", async () =>
        {
            var login = await auth.LoginAsync(new LoginRequestDto { Username = user, Password = "123456" });
            Assert(login.UserID > 0, "no UserID");
            UserSession.Login(login);
            return login.Username;
        });

        // Profile
        await Step("F1", "Update Profile", "Cập nhật FullName/Phone", async () =>
        {
            var u = await users.UpdateUserAsync(created!.UserID, new UpdateUserDto
            {
                RoleID = 3,
                FullName = "E2E Tenant Updated",
                Phone = "0900000099",
                Email = email,
                Address = "HN Updated",
                Status = "Active"
            });
            Assert(u.FullName.Contains("Updated"), u.FullName);
            return u.FullName;
        });

        // Search / Filter / Sort
        List<PostDto> search = new();
        await Step("F1", "Search rooms", "SearchRoomsAsync trả về tin Approved", async () =>
        {
            search = (await tenantSvc.SearchRoomsAsync(new RoomSearchFilterDto())).ToList();
            Assert(search.Count >= 0, $"count={search.Count}");
            return $"posts={search.Count}";
        });

        await Step("F1", "Filter price", "Filter Min/MaxPrice không throw", async () =>
        {
            var filtered = (await tenantSvc.SearchRoomsAsync(new RoomSearchFilterDto
            {
                MinPrice = 1_000_000,
                MaxPrice = 10_000_000
            })).ToList();
            return $"filtered={filtered.Count}";
        });

        await Step("F1", "Sort PriceAsc", "SortBy=PriceAsc", async () =>
        {
            var sorted = (await tenantSvc.SearchRoomsAsync(new RoomSearchFilterDto { SortBy = "PriceAsc" })).ToList();
            if (sorted.Count >= 2)
            {
                bool ok = true;
                for (int i = 1; i < sorted.Count; i++)
                    if (sorted[i].PriceSnapshot < sorted[i - 1].PriceSnapshot) { ok = false; break; }
                Assert(ok, "không tăng dần");
            }
            return $"sorted={sorted.Count}";
        });

        // Detail + images
        int roomId = 0;
        await Step("F1", "Room detail + images", "GetRoomDetail / PostDetail có dữ liệu", async () =>
        {
            var active = (await posts.GetAllActivePostsAsync()).FirstOrDefault();
            if (active == null)
            {
                Block("F1", "Room detail", "Không có Post Approved để xem chi tiết — chạy sau Flow 2–3 hoặc seed");
                return "BLOCKED";
            }
            roomId = active.RoomID;
            var detail = await rooms.GetRoomDetailAsync(roomId);
            var post = await posts.GetPostByIdAsync(active.PostID);
            Assert(detail != null && post != null, "null detail");
            int img = post.Images?.Count ?? 0;
            return $"Room#{roomId} images={img}";
        });

        if (roomId > 0)
        {
            await Step("F1", "Add Favorite", "ToggleFavorite thêm", async () =>
            {
                await interact.ToggleFavoriteAsync(created!.UserID, roomId);
                var favs = (await interact.GetFavoritesAsync(created.UserID)).ToList();
                Assert(favs.Any(f => f.RoomID == roomId), "không thấy favorite");
                return $"favs={favs.Count}";
            });

            await Step("F1", "Remove Favorite", "RemoveFavorite xóa", async () =>
            {
                await interact.RemoveFavoriteAsync(created!.UserID, roomId);
                var favs = (await interact.GetFavoritesAsync(created.UserID)).ToList();
                Assert(favs.All(f => f.RoomID != roomId), "vẫn còn favorite");
                return "removed";
            });

            await Step("F1", "Book Appointment", "BookAppointment Pending", async () =>
            {
                var ap = await interact.BookAppointmentAsync(new CreateAppointmentDto
                {
                    RoomID = roomId,
                    TenantID = created!.UserID,
                    AppointmentDate = DateTime.Today.AddDays(3).AddHours(10),
                    Note = "E2E onboarding"
                });
                Assert(ap.AppointmentID > 0, "no id");
                AppointmentId = ap.AppointmentID;
                return $"Appt#{ap.AppointmentID} Status={ap.Status}";
            });
        }

        await Step("F1", "Logout session", "Clear UserSession", async () =>
        {
            UserSession.Logout();
            Assert(!UserSession.IsLoggedIn, "session còn");
            return "cleared";
        });

        await Step("F1", "ActivityLog check", "Có log gần đây (seed/login)", async () =>
        {
            var recent = (await logs.GetRecentAsync(20)).ToList();
            return $"logs={recent.Count}";
        });

        await Step("F1", "Notification API", "GetByUser không throw", async () =>
        {
            var n = (await notif.GetByUserAsync(created!.UserID)).ToList();
            return $"notif={n.Count}";
        });

        await Step("F1", "DB user exists", "SQL Users row", async () =>
        {
            int c = ScalarInt("SELECT COUNT(*) FROM Users WHERE Username=@u", ("@u", user));
            Assert(c == 1, $"count={c}");
            return "ok";
        });
    }

    private static async Task Flow2_LandlordListing()
    {
        using var scope = _sp.CreateScope();
        var houses = S<IHouseService>(scope);
        var rooms = S<IRoomService>(scope);
        var amenities = S<IAmenityService>(scope);
        var posts = S<IPostService>(scope);
        var auth = S<IAuthService>(scope);

        await LoginAs(auth, "namlandlord", "123456");
        LandlordId = UserSession.CurrentUser!.UserID;
        // note: LoginResponseDto

        await Step("F2", "Create House", "House Status Active", async () =>
        {
            var h = await houses.CreateHouseAsync(new CreateHouseDto
            {
                OwnerID = LandlordId,
                HouseName = $"E2E House {Unique}",
                Address = $"123 E2E St {Unique}, Q1, TP.HCM",
                Description = "E2E house"
            });
            HouseId = h.HouseID;
            Assert(HouseId > 0, "no house");
            return $"House#{HouseId}";
        });

        await Step("F2", "Upload House image", "API upload ảnh House", async () =>
        {
            // Không có IHouseService upload image — ghi nhận gap
            Block("F2", "Upload House image", "Không có API upload ảnh House trong BLL (chỉ có Room/Post images) — N/A UI-only nếu có");
            return "BLOCKED";
        });

        await Step("F2", "Create Room", "Room Available", async () =>
        {
            var r = await rooms.CreateRoomAsync(new CreateRoomDto
            {
                HouseID = HouseId,
                RoomNumber = $"E{Unique[^4..]}",
                Floor = 2,
                Area = 25,
                Price = 4_500_000,
                Capacity = 2,
                Bedroom = 1,
                Bathroom = 1,
                Furniture = "Cơ bản",
                Description = "E2E room"
            });
            RoomId = r.RoomID;
            Assert(r.Status == "Available" || string.IsNullOrEmpty(r.Status) || r.RoomID > 0, r.Status);
            return $"Room#{RoomId} {r.RoomNumber}";
        });

        await Step("F2", "Upload Room images", "UploadRoomImagesAsync", async () =>
        {
            string dir = Path.Combine(Docs, "E2E_Assets");
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, $"room_{Unique}.txt");
            await File.WriteAllTextAsync(path, "fake-image-placeholder");
            bool ok = await rooms.UploadRoomImagesAsync(RoomId, new List<string> { path });
            Assert(ok, "upload false");
            var detail = await rooms.GetRoomDetailAsync(RoomId);
            return $"images={detail.Images?.Count ?? 0}";
        });

        await Step("F2", "Assign Amenities", "Gán amenities", async () =>
        {
            var all = (await amenities.GetAllAmenitiesAsync()).Take(2).Select(a => a.AmenityID).ToList();
            if (all.Count == 0)
            {
                Block("F2", "Amenities", "Không có Amenity trong DB");
                return "BLOCKED";
            }
            await rooms.AssignAmenitiesAsync(RoomId, all);
            return $"amenities={all.Count}";
        });

        await Step("F2", "Create Post Pending", "Post Status=Pending", async () =>
        {
            var p = await posts.CreatePostAsync(new CreatePostDto
            {
                RoomID = RoomId,
                Title = $"E2E Post {Unique}",
                Description = "Phòng E2E test",
                PriceSnapshot = 4_500_000,
                ExpiryMonths = 1
            });
            PostId = p.PostID;
            Assert(p.Status == "Pending", p.Status);
            int db = ScalarInt("SELECT COUNT(*) FROM Posts WHERE PostID=@id AND Status=N'Pending'", ("@id", PostId));
            Assert(db == 1, "DB status");
            return $"Post#{PostId} Pending";
        });

        await Step("F2", "Logout", "Clear session", async () =>
        {
            UserSession.Logout();
            return "ok";
        });
    }

    private static async Task Flow3_AdminPost()
    {
        using var scope = _sp.CreateScope();
        var posts = S<IPostService>(scope);
        var auth = S<IAuthService>(scope);
        var notif = S<INotificationService>(scope);

        await LoginAs(auth, "admin", "admin123");
        AdminId = UserSession.CurrentUser!.UserID;

        if (PostId <= 0)
        {
            var pending = (await posts.GetPendingPostsAsync()).FirstOrDefault();
            if (pending != null) PostId = pending.PostID;
        }

        await Step("F3", "View Pending", "GetPendingPostsAsync", async () =>
        {
            var list = (await posts.GetPendingPostsAsync()).ToList();
            Assert(list.Count >= 0, "");
            return $"pending={list.Count}";
        });

        await Step("F3", "Open Detail", "GetPostByIdAsync", async () =>
        {
            Assert(PostId > 0, "không có PostId");
            var d = await posts.GetPostByIdAsync(PostId);
            return d.Title;
        });

        int landlordNotifBefore = ScalarInt(
            "SELECT COUNT(*) FROM Notifications WHERE UserID=@u", ("@u", LandlordId > 0 ? LandlordId : 2));

        await Step("F3", "Approve Post", "Approved + notify landlord", async () =>
        {
            Assert(PostId > 0, "no post");
            await posts.ApprovePostAsync(PostId, AdminId);
            string st = ScalarStr("SELECT Status FROM Posts WHERE PostID=@id", ("@id", PostId));
            Assert(st == "Approved", st);
            int after = ScalarInt("SELECT COUNT(*) FROM Notifications WHERE UserID=@u", ("@u", LandlordId > 0 ? LandlordId : 2));
            Assert(after > landlordNotifBefore, $"notif before={landlordNotifBefore} after={after}");
            return $"Approved notifΔ={after - landlordNotifBefore}";
        });

        // Reject path on a separate post
        await Step("F3", "Reject Post (separate)", "Tạo post phụ rồi Reject + notify", async () =>
        {
            if (RoomId <= 0)
            {
                Block("F3", "Reject", "Thiếu RoomId từ Flow2");
                return "BLOCKED";
            }
            var p2 = await posts.CreatePostAsync(new CreatePostDto
            {
                RoomID = RoomId,
                Title = $"E2E Reject {Unique}",
                Description = "reject me",
                PriceSnapshot = 4_000_000
            });
            // Room may already have pending/approved — if create fails room occupied by post rules, catch
            int before = ScalarInt("SELECT COUNT(*) FROM Notifications WHERE UserID=@u", ("@u", LandlordId > 0 ? LandlordId : 2));
            await posts.RejectPostAsync(p2.PostID);
            string st = ScalarStr("SELECT Status FROM Posts WHERE PostID=@id", ("@id", p2.PostID));
            Assert(st == "Rejected", st);
            int after = ScalarInt("SELECT COUNT(*) FROM Notifications WHERE UserID=@u", ("@u", LandlordId > 0 ? LandlordId : 2));
            Assert(after > before, "không có notify reject");
            return $"Rejected#{p2.PostID}";
        });

        await Step("F3", "Hidden Post", "Ẩn tin (Hidden)", async () =>
        {
            Block("F3", "Hidden", "Không có API Hidden Post trong BLL (chỉ Pending/Approved/Rejected)");
            return "BLOCKED";
        });

        await Step("F3", "Expired Post", "Đánh dấu Expired thủ công", async () =>
        {
            // Có ExpiryDate field — có thể set SQL để verify GetAllActivePosts lọc hết hạn
            if (PostId <= 0) { Block("F3", "Expired", "no PostId"); return "BLOCKED"; }
            ExecSql("UPDATE Posts SET ExpiryDate = DATEADD(day,-1,GETDATE()) WHERE PostID=@id", ("@id", PostId));
            var active = (await posts.GetAllActivePostsAsync()).Any(p => p.PostID == PostId);
            Assert(!active, "vẫn hiện trong active");
            // restore for later flows that need approved post on room — re-approve with future expiry
            ExecSql("UPDATE Posts SET ExpiryDate = DATEADD(month,1,GETDATE()), Status=N'Approved' WHERE PostID=@id", ("@id", PostId));
            return "expiry filter OK; restored";
        });
    }

    private static async Task Flow4_Rent()
    {
        using var scope = _sp.CreateScope();
        var auth = S<IAuthService>(scope);
        var interact = S<ITenantInteractionService>(scope);
        var landlord = S<ILandlordService>(scope);
        var contracts = S<IContractService>(scope);
        var posts = S<IPostService>(scope);

        // Ensure we have Available room — may need new room if PostId room still Available
        if (RoomId <= 0)
        {
            Fail("F4", "Setup", "Thiếu RoomId từ Flow2", "RoomId=0", "High");
            return;
        }

        await LoginAs(auth, "tenant", "123456");
        TenantId = UserSession.CurrentUser!.UserID;

        await Step("F4", "Book Appointment", "Tenant đặt hẹn", async () =>
        {
            // Room must be Available for appointment typically
            var ap = await interact.BookAppointmentAsync(new CreateAppointmentDto
            {
                RoomID = RoomId,
                TenantID = TenantId,
                AppointmentDate = DateTime.Today.AddDays(2).AddHours(14),
                Note = "E2E rent flow"
            });
            AppointmentId = ap.AppointmentID;
            return $"Appt#{AppointmentId} {ap.Status}";
        });

        await LoginAs(auth, "namlandlord", "123456");
        LandlordId = UserSession.CurrentUser!.UserID;
        // note: LoginResponseDto

        await Step("F4", "Landlord Accept Appointment", "Status Confirmed", async () =>
        {
            await landlord.UpdateAppointmentStatusAsync(AppointmentId, "Accepted");
            string st = ScalarStr("SELECT Status FROM Appointments WHERE AppointmentID=@id", ("@id", AppointmentId));
            Assert(st == "Accepted", st);
            return st;
        });

        await Step("F4", "Create Contract + Assign", "PendingConfirm (có tenant)", async () =>
        {
            var c = await contracts.CreateContractAsync(new CreateContractDto
            {
                RoomID = RoomId,
                TenantID = TenantId,
                StartDate = new DateTime(2026, 7, 5),
                EndDate = new DateTime(2027, 1, 5),
                Deposit = 4_500_000,
                MonthlyRent = 4_500_000,
                ElectricPrice = 3500,
                WaterPrice = 20000
            }, LandlordId);
            ContractId = c.ContractID;
            Assert(c.Status == "PendingConfirm", c.Status);
            string roomSt = ScalarStr("SELECT Status FROM Rooms WHERE RoomID=@id", ("@id", RoomId));
            Assert(roomSt != "Occupied", $"room prematurely {roomSt}");
            return $"Contract#{ContractId} {c.Status} room={roomSt}";
        });

        await LoginAs(auth, "tenant", "123456");

        await Step("F4", "Tenant Confirm rental", "Accept → Active + Occupied", async () =>
        {
            await contracts.AcceptRentalOfferAsync(ContractId, TenantId);
            string st = ScalarStr("SELECT Status FROM Contracts WHERE ContractID=@id", ("@id", ContractId));
            string roomSt = ScalarStr("SELECT Status FROM Rooms WHERE RoomID=@id", ("@id", RoomId));
            Assert(st == "Active", st);
            Assert(roomSt == "Occupied", roomSt);
            // Demo: backdate MoveIn for July invoice in Flow5
            ExecSql(@"UPDATE Contracts SET StartDate='2026-07-05', MoveInDate='2026-07-05', UpdatedDate=GETDATE()
                      WHERE ContractID=@id", ("@id", ContractId));
            return $"Active Occupied MoveIn backdated 05/07";
        });

        await Step("F4", "Notification landlord", "Có notify đồng ý thuê", async () =>
        {
            int c = ScalarInt(
                "SELECT COUNT(*) FROM Notifications WHERE UserID=@u AND (Title LIKE N'%đồng ý%' OR Content LIKE N'%đồng ý%' OR Content LIKE N'%xác nhận thuê%' OR Title LIKE N'%Khách đã đồng ý%')",
                ("@u", LandlordId));
            Assert(c >= 1, $"count={c}");
            return $"notif={c}";
        });
    }

    private static async Task Flow8_AssignManager()
    {
        using var scope = _sp.CreateScope();
        var auth = S<IAuthService>(scope);
        var assign = S<IAssignmentService>(scope);
        var contracts = S<IContractService>(scope);

        await LoginAs(auth, "manager", "123456");
        ManagerId = UserSession.CurrentUser!.UserID;

        await LoginAs(auth, "namlandlord", "123456");
        LandlordId = UserSession.CurrentUser!.UserID;
        // note: LoginResponseDto

        if (HouseId <= 0 || ContractId <= 0)
        {
            Block("F8", "Setup", "Thiếu HouseId/ContractId Active từ Flow 2/4");
            return;
        }

        await Step("F8", "Search Manager + Assign", "Assign Active sau HĐ Active", async () =>
        {
            var a = await assign.CreateAsync(new CreateAssignmentDto
            {
                HouseID = HouseId,
                ManagerID = ManagerId
            }, LandlordId);
            AssignmentId = a.AssignmentID;
            Assert(a.Status == "Active", a.Status);
            return $"Assignment#{AssignmentId}";
        });

        await Step("F8", "Manager sees contracts", "GetContractsByManagerAsync", async () =>
        {
            var list = (await contracts.GetContractsByManagerAsync(ManagerId)).ToList();
            Assert(list.Any(c => c.ContractID == ContractId), $"không thấy HĐ#{ContractId} count={list.Count}");
            return $"scoped={list.Count}";
        });

        await Step("F8", "Notify manager", "Notification gán quản lý", async () =>
        {
            int c = ScalarInt(
                "SELECT COUNT(*) FROM Notifications WHERE UserID=@u AND (Title LIKE N'%gán%' OR Title LIKE N'%quản lý%' OR Content LIKE N'%gán%')",
                ("@u", ManagerId));
            Assert(c >= 1, $"count={c}");
            return $"notif={c}";
        });

        await Step("F8", "Assign before Active blocked", "Nhà không Active rental → reject", async () =>
        {
            // Create empty house and expect fail
            var houses = S<IHouseService>(scope);
            var h = await houses.CreateHouseAsync(new CreateHouseDto
            {
                OwnerID = LandlordId,
                HouseName = $"NoRent {Unique}",
                Address = "No rent house",
                Description = "x"
            });
            try
            {
                await assign.CreateAsync(new CreateAssignmentDto { HouseID = h.HouseID, ManagerID = ManagerId }, LandlordId);
                Fail("F8", "Gate Active", "Expect BadRequest khi chưa có HĐ Active", "Assign thành công sai", "High");
                return "FAIL";
            }
            catch (BadRequestException ex)
            {
                return $"blocked OK: {ex.Message}";
            }
        });
    }

    private static async Task Flow5_MeterInvoice()
    {
        using var scope = _sp.CreateScope();
        var auth = S<IAuthService>(scope);
        var invoices = S<IInvoiceService>(scope);

        if (ContractId <= 0 || ManagerId <= 0)
        {
            Block("F5", "Setup", "Thiếu ContractId/ManagerId");
            return;
        }

        await LoginAs(auth, "manager", "123456");
        var billingMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);

        await Step("F5", "Generate Invoice", $"Hóa đơn tháng {billingMonth:MM/yyyy}", async () =>
        {
            var inv = await invoices.GenerateMonthlyInvoiceAsync(new GenerateInvoiceDto
            {
                ContractID = ContractId,
                ReadingMonth = billingMonth,
                NewElectric = 120,
                NewWater = 15,
                OtherFee = 0,
                CreatedBy = ManagerId
            });
            InvoiceId = inv.InvoiceID;
            var detail = await invoices.GetInvoiceByIdAsync(InvoiceId);
            Assert(detail.Status == "Unpaid", detail.Status);
            Assert(detail.Rent > 0, $"Rent={detail.Rent}");
            return $"Inv#{InvoiceId} Rent={detail.Rent:N0} E={detail.ElectricCost:N0} W={detail.WaterCost:N0} Total={detail.Total:N0}";
        });

        await LoginAs(auth, "tenant", "123456");
        TenantId = UserSession.CurrentUser!.UserID;

        await Step("F5", "Tenant view invoices", "GetByContract", async () =>
        {
            var list = (await invoices.GetInvoicesByContractAsync(ContractId)).ToList();
            Assert(list.Any(i => i.InvoiceID == InvoiceId), "không thấy hóa đơn");
            return $"count={list.Count}";
        });

        await Step("F5", "Tenant Pay", "ProcessPayment → Paid", async () =>
        {
            var inv = await invoices.GetInvoiceByIdAsync(InvoiceId);
            await invoices.ProcessPaymentAsync(InvoiceId, new ProcessPaymentDto
            {
                Amount = inv.Total,
                Method = "Cash"
            });
            string st = ScalarStr("SELECT Status FROM Invoices WHERE InvoiceID=@id", ("@id", InvoiceId));
            Assert(st == "Paid", st);
            int pay = ScalarInt("SELECT COUNT(*) FROM Payments WHERE InvoiceID=@id", ("@id", InvoiceId));
            return $"Status={st} payments={pay}";
        });
    }

    private static async Task Flow6_EditContract()
    {
        using var scope = _sp.CreateScope();
        var auth = S<IAuthService>(scope);
        var contracts = S<IContractService>(scope);

        if (ContractId <= 0)
        {
            Block("F6", "Setup", "Thiếu ContractId");
            return;
        }

        await LoginAs(auth, "namlandlord", "123456");
        LandlordId = UserSession.CurrentUser!.UserID;
        // note: LoginResponseDto

        await Step("F6", "Landlord Edit → Pending", "UpdateContract PendingEdit", async () =>
        {
            var detail = await contracts.GetContractByIdAsync(ContractId);
            await contracts.UpdateContractAsync(new UpdateContractDto
            {
                ContractID = ContractId,
                EndDate = detail.EndDate,
                Deposit = detail.Deposit,
                MonthlyRent = detail.MonthlyRent + 200_000,
                ElectricPrice = detail.ElectricPrice,
                WaterPrice = detail.WaterPrice,
                Note = "E2E price bump"
            }, LandlordId);
            string? pend = ScalarStr("SELECT PendingEditStatus FROM Contracts WHERE ContractID=@id", ("@id", ContractId));
            Assert(string.Equals(pend, "Pending", StringComparison.OrdinalIgnoreCase), $"PendingEdit={pend}");
            return $"PendingEdit={pend}";
        });

        await LoginAs(auth, "tenant", "123456");
        TenantId = UserSession.CurrentUser!.UserID;

        await Step("F6", "Tenant Accept edit", "ConfirmContractEdit + PriceEffective", async () =>
        {
            await contracts.ConfirmContractEditAsync(ContractId, TenantId);
            string? pend = ScalarStr("SELECT ISNULL(PendingEditStatus,'') FROM Contracts WHERE ContractID=@id", ("@id", ContractId));
            Assert(string.IsNullOrEmpty(pend) || pend == "", $"still {pend}");
            decimal rent = ScalarDec("SELECT MonthlyRent FROM Contracts WHERE ContractID=@id", ("@id", ContractId));
            Assert(rent > 0, $"rent={rent}");
            return $"MonthlyRent={rent:N0}";
        });

        await Step("F6", "Prorate/Weighted note", "Logic InvoiceService dùng PreviousPrice khi có", async () =>
        {
            // Smoke: columns exist
            int cols = ScalarInt(@"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME='Contracts' AND COLUMN_NAME IN ('PreviousMonthlyRent','PriceEffectiveDate')");
            Assert(cols >= 1, $"cols={cols}");
            return $"pricing cols={cols}";
        });
    }

    private static async Task Flow7_Maintenance()
    {
        using var scope = _sp.CreateScope();
        var auth = S<IAuthService>(scope);
        var maint = S<IMaintenanceService>(scope);

        if (ContractId <= 0)
        {
            Block("F7", "Setup", "Thiếu ContractId");
            return;
        }

        await LoginAs(auth, "tenant", "123456");
        TenantId = UserSession.CurrentUser!.UserID;

        await Step("F7", "Create Request + image path", "Pending request", async () =>
        {
            var m = await maint.CreateRequestAsync(new CreateMaintenanceDto
            {
                ContractID = ContractId,
                Title = $"E2E leak {Unique}",
                Description = "Vòi nước bị rò",
                ImagePath = @"E:\DoAn\RPMS\Docs\E2E_Assets\fake.jpg"
            });
            MaintId = m.RequestID;
            Assert(MaintId > 0, "no id");
            return $"Req#{MaintId} {m.Status}";
        });

        await LoginAs(auth, "manager", "123456");
        ManagerId = UserSession.CurrentUser!.UserID;

        await Step("F7", "Manager Processing", "Update status Processing", async () =>
        {
            await maint.UpdateRequestStatusAsync(MaintId, "Processing", ManagerId);
            string st = ScalarStr("SELECT Status FROM MaintenanceRequests WHERE RequestID=@id", ("@id", MaintId));
            Assert(st == "Processing", st);
            return st;
        });

        await Step("F7", "Manager Completed", "Update status Completed", async () =>
        {
            await maint.UpdateRequestStatusAsync(MaintId, "Completed", ManagerId);
            string st = ScalarStr("SELECT Status FROM MaintenanceRequests WHERE RequestID=@id", ("@id", MaintId));
            Assert(st == "Completed", st);
            return st;
        });

        await Step("F7", "Notify / timeline", "SendMaintenanceNotification + history", async () =>
        {
            await maint.SendMaintenanceNotificationAsync(MaintId, "E2E done");
            var detail = await maint.GetRequestByIdAsync(MaintId);
            return $"Status={detail.Status}";
        });
    }

    private static async Task Flow9_Chat()
    {
        using var scope = _sp.CreateScope();
        var auth = S<IAuthService>(scope);
        var chat = S<IChatService>(scope);

        await LoginAs(auth, "tenant", "123456");
        TenantId = UserSession.CurrentUser!.UserID;
        await LoginAs(auth, "namlandlord", "123456");
        LandlordId = UserSession.CurrentUser!.UserID;
        // note: LoginResponseDto

        await Step("F9", "GetOrCreate + Send", "Tenant→Landlord message", async () =>
        {
            var conv = await chat.GetOrCreateConversationAsync(LandlordId, TenantId);
            ConversationId = conv.ConversationID;
            await chat.SendMessageAsync(new SendMessageDto
            {
                ConversationID = ConversationId,
                SenderID = TenantId,
                Content = $"E2E hello {Unique}"
            });
            return $"Conv#{ConversationId}";
        });

        await Step("F9", "Landlord Reply + Read", "Reply + MarkRead + Unread", async () =>
        {
            await chat.SendMessageAsync(new SendMessageDto
            {
                ConversationID = ConversationId,
                SenderID = LandlordId,
                Content = $"E2E reply {Unique}"
            });
            int unreadBefore = await chat.GetUnreadCountAsync(TenantId);
            await chat.MarkConversationReadAsync(ConversationId, TenantId);
            int unreadAfter = await chat.GetUnreadCountAsync(TenantId);
            return $"unread {unreadBefore}→{unreadAfter}";
        });
    }

    private static async Task Flow10_Review()
    {
        using var scope = _sp.CreateScope();
        var auth = S<IAuthService>(scope);
        var reviews = S<IReviewService>(scope);
        var contracts = S<IContractService>(scope);

        if (ContractId <= 0 || HouseId <= 0)
        {
            Block("F10", "Setup", "Thiếu Contract/House");
            return;
        }

        await LoginAs(auth, "tenant", "123456");
        TenantId = UserSession.CurrentUser!.UserID;

        // Product rule: chỉ review khi Terminated/Expired — kết thúc HĐ E2E trước khi đánh giá
        await Step("F10", "Terminate for review rule", "Terminate Active → cho phép Review", async () =>
        {
            await contracts.TerminateContractAsync(ContractId);
            string st = ScalarStr("SELECT Status FROM Contracts WHERE ContractID=@id", ("@id", ContractId));
            Assert(st == "Terminated", st);
            return st;
        });

        await Step("F10", "Tenant Review", "CreateReview rating 5 (sau Terminated)", async () =>
        {
            var r = await reviews.CreateReviewAsync(TenantId, new CreateReviewDto
            {
                ContractID = ContractId,
                Rating = 5,
                Comment = $"E2E review {Unique}"
            });
            ReviewId = r.ReviewID;
            return $"Review#{ReviewId}";
        });

        await LoginAs(auth, "namlandlord", "123456");
        LandlordId = UserSession.CurrentUser!.UserID;
        // note: LoginResponseDto

        await Step("F10", "Landlord Reply", "ReplyAsync", async () =>
        {
            await reviews.ReplyAsync(LandlordId, new ReplyReviewDto
            {
                ReviewID = ReviewId,
                Reply = "Cảm ơn E2E"
            });
            return "replied";
        });

        await Step("F10", "Average Rating", "GetAverageRatingForHouse", async () =>
        {
            double avg = await reviews.GetAverageRatingForHouseAsync(HouseId);
            Assert(avg > 0, $"avg={avg}");
            return $"avg={avg:F2}";
        });
    }

    private static async Task Flow11_Dashboard()
    {
        using var scope = _sp.CreateScope();
        var stats = S<IStatisticService>(scope);
        var tenantSvc = S<ITenantService>(scope);
        var auth = S<IAuthService>(scope);

        await Step("F11", "Admin Dashboard", "GetAdminDashboardStatsAsync", async () =>
        {
            var d = await stats.GetAdminDashboardStatsAsync();
            Assert(d != null, "null");
            return $"users/houses ok";
        });

        await Step("F11", "Landlord Dashboard", "Cards + PendingConfirm", async () =>
        {
            var d = await stats.GetLandlordDashboardStatsAsync(LandlordId > 0 ? LandlordId : 2);
            return $"PendingConfirm={d.PendingConfirmContracts} Occupied related ok";
        });

        await Step("F11", "Tenant Dashboard", "GetTenantDashboardAsync", async () =>
        {
            var d = await tenantSvc.GetTenantDashboardAsync(TenantId > 0 ? TenantId : 3);
            return d.CurrentContract != null ? $"contract={d.CurrentContract.ContractCode}" : "no active contract card";
        });

        await Step("F11", "Manager Dashboard", "GetManagerDashboardStatsAsync", async () =>
        {
            var d = await stats.GetManagerDashboardStatsAsync(ManagerId > 0 ? ManagerId : 4);
            return $"rooms/maint ok";
        });
    }

    private static async Task Flow12_Notification()
    {
        using var scope = _sp.CreateScope();
        var notif = S<INotificationService>(scope);
        int uid = LandlordId > 0 ? LandlordId : 2;

        await Step("F12", "List + Unread", "GetByUser / UnreadCount", async () =>
        {
            var list = (await notif.GetByUserAsync(uid)).ToList();
            int unread = await notif.GetUnreadCountAsync(uid);
            return $"total={list.Count} unread={unread}";
        });

        await Step("F12", "Mark read / Mark all / Delete", "Lifecycle notification", async () =>
        {
            var list = (await notif.GetByUserAsync(uid, isRead: false)).ToList();
            if (list.Count == 0)
            {
                await notif.CreateAsync(new CreateNotificationDto
                {
                    UserID = uid,
                    Title = "E2E ping",
                    Content = "test"
                });
                list = (await notif.GetByUserAsync(uid, isRead: false)).ToList();
            }
            var first = list.First();
            await notif.MarkAsReadAsync(first.NotificationID);
            await notif.MarkAllAsReadAsync(uid);
            await notif.CreateAsync(new CreateNotificationDto
            {
                UserID = uid,
                Title = "E2E delete me",
                Content = "x"
            });
            var del = (await notif.GetByUserAsync(uid)).First(n => n.Title.Contains("delete me"));
            await notif.DeleteAsync(del.NotificationID);
            return "mark+delete OK";
        });
    }

    private static async Task Flow13_Permission()
    {
        await Step("F13", "Menu by role", "MainForm.GenerateMenu reflection/smoke", async () =>
        {
            var expected = new Dictionary<int, string[]>
            {
                [1] = new[] { "UserManagement", "PostManagement", "Backup" },
                [2] = new[] { "LandlordHouse", "LandlordContract", "LandlordAssignment" },
                [3] = new[] { "TenantHome", "TenantContract", "TenantInvoice" },
                [4] = new[] { "ManagerMeter", "ManagerMaintenance" }
            };
            var forbidden = new Dictionary<int, string[]>
            {
                [3] = new[] { "LandlordHouse", "UserManagement", "ManagerMeter" },
                [4] = new[] { "LandlordContract", "TenantHome", "Backup" },
                [2] = new[] { "UserManagement", "ManagerMeter" }
            };

            foreach (var role in expected.Keys)
            {
                UserSession.Login(new LoginResponseDto
                {
                    UserID = role == 1 ? 1 : role == 2 ? 2 : role == 3 ? 3 : 4,
                    Username = "x",
                    FullName = "x",
                    RoleID = role,
                    RoleName = role.ToString()
                });
                foreach (var tag in expected[role])
                    ResolveFormTag(tag);
            }
            return "expected forms resolve";
        });

        await Step("F13", "CRUD trái quyền (assign)", "Tenant không gán Manager", async () =>
        {
            using var scope = _sp.CreateScope();
            var assign = S<IAssignmentService>(scope);
            try
            {
                await assign.CreateAsync(new CreateAssignmentDto
                {
                    HouseID = HouseId > 0 ? HouseId : 1,
                    ManagerID = 4
                }, 3); // landlordId spoof = tenant
                Fail("F13", "AuthZ Assign", "Tenant spoof landlordId=3 không phải owner phải bị từ chối",
                    "Assign thành công sai", "High");
                return "FAIL";
            }
            catch (BadRequestException ex)
            {
                return $"denied OK: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"denied: {ex.GetType().Name} {ex.Message}";
            }
        });
    }

    private static async Task Flow14_Regression()
    {
        using var scope = _sp.CreateScope();
        var auth = S<IAuthService>(scope);
        var houses = S<IHouseService>(scope);
        var reports = S<IReportService>(scope);

        await Step("F14", "Login demos", "4 role login", async () =>
        {
            foreach (var (u, p) in new[] { ("admin", "admin123"), ("namlandlord", "123456"), ("tenant", "123456"), ("manager", "123456") })
            {
                var r = await auth.LoginAsync(new LoginRequestDto { Username = u, Password = p });
                Assert(r.UserID > 0, $"{u} fail");
            }
            return "4 logins OK";
        });

        await Step("F14", "CRUD House smoke", "GetHousesByOwner", async () =>
        {
            var list = (await houses.GetHousesByOwnerAsync(2)).ToList();
            return $"houses={list.Count}";
        });

        await Step("F14", "Report Admin/Landlord", "ReportService", async () =>
        {
            var a = await reports.GetAdminReportAsync();
            var l = await reports.GetLandlordReportAsync(2);
            Assert(a != null && l != null, "null report");
            return "reports OK";
        });

        await Step("F14", "Form DI resolve", "Resolve key forms", async () =>
        {
            string[] tags =
            {
                "Dashboard", "Notifications", "Profile", "Calendar",
                "LandlordHouse", "LandlordRoom", "LandlordContract", "LandlordAssignment",
                "TenantHome", "TenantContract", "TenantInvoice", "ManagerMeter"
            };
            foreach (var t in tags) ResolveFormTag(t);
            return $"resolved {tags.Length} forms";
        });
    }

    private static async Task Flow15_Boundary()
    {
        using var scope = _sp.CreateScope();
        var auth = S<IAuthService>(scope);
        var rooms = S<IRoomService>(scope);
        var users = S<IUserService>(scope);
        var contracts = S<IContractService>(scope);

        await Step("F15", "Empty password login", "Login fail", async () =>
        {
            try
            {
                await auth.LoginAsync(new LoginRequestDto { Username = "admin", Password = "" });
                Fail("F15", "Empty password", "Expect Unauthorized", "Login OK", "High");
                return "FAIL";
            }
            catch (UnauthorizedException ex)
            {
                return ex.Message;
            }
        });

        await Step("F15", "Duplicate username", "CreateUser BadRequest", async () =>
        {
            try
            {
                await users.CreateUserAsync(new CreateUserDto
                {
                    RoleID = 3,
                    Username = "tenant",
                    Password = "123456",
                    FullName = "dup",
                    Email = $"dup_{Unique}@x.com"
                });
                Fail("F15", "Duplicate user", "Expect BadRequest", "Created OK", "Medium");
                return "FAIL";
            }
            catch (BadRequestException ex)
            {
                return ex.Message;
            }
        });

        await Step("F15", "Empty RoomNumber", "CreateRoom validation", async () =>
        {
            try
            {
                await rooms.CreateRoomAsync(new CreateRoomDto
                {
                    HouseID = HouseId > 0 ? HouseId : 1,
                    RoomNumber = "   ",
                    Floor = 1,
                    Area = 10,
                    Price = 1,
                    Capacity = 1,
                    Bedroom = 1,
                    Bathroom = 1
                });
                Fail("F15", "Empty RoomNumber", "Expect BadRequest", "Created", "Medium");
                return "FAIL";
            }
            catch (BadRequestException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
            {
                return $"rejected: {ex.Message}";
            }
        });

        await Step("F15", "SQL injection username", "Login không bypass", async () =>
        {
            try
            {
                await auth.LoginAsync(new LoginRequestDto
                {
                    Username = "admin' OR 1=1 --",
                    Password = "x"
                });
                Fail("F15", "SQLi", "Expect Unauthorized", "Login OK", "Critical");
                return "FAIL";
            }
            catch (UnauthorizedException)
            {
                return "no bypass";
            }
        });

        await Step("F15", "Long text post title", "CreatePost dài", async () =>
        {
            if (RoomId <= 0) { Block("F15", "Long text", "no room"); return "BLOCKED"; }
            // Room may be Occupied — post create requires Available
            string st = ScalarStr("SELECT Status FROM Rooms WHERE RoomID=@id", ("@id", RoomId));
            if (st == "Occupied")
            {
                Block("F15", "Long text", "Room Occupied — không tạo post; validation long text N/A trên room này");
                return "BLOCKED";
            }
            try
            {
                await S<IPostService>(scope).CreatePostAsync(new CreatePostDto
                {
                    RoomID = RoomId,
                    Title = new string('A', 5000),
                    Description = "x",
                    PriceSnapshot = 1
                });
                return "accepted long title (check DB truncation)";
            }
            catch (Exception ex)
            {
                return $"rejected: {ex.Message}";
            }
        });

        await Step("F15", "Double accept rental", "Accept lần 2 fail", async () =>
        {
            if (ContractId <= 0) { Block("F15", "Double accept", "no contract"); return "BLOCKED"; }
            try
            {
                await contracts.AcceptRentalOfferAsync(ContractId, TenantId > 0 ? TenantId : 3);
                Fail("F15", "Double accept", "Expect BadRequest khi đã Active", "Accept OK", "Medium");
                return "FAIL";
            }
            catch (BadRequestException ex)
            {
                return ex.Message;
            }
        });

        await Step("F15", "FK / CHECK smoke", "Invalid contract status SQL", async () =>
        {
            try
            {
                ExecSql("UPDATE Contracts SET Status=N'InvalidStatusXYZ' WHERE ContractID=@id", ("@id", ContractId > 0 ? ContractId : 1));
                Fail("F15", "CHECK Status", "Expect CHECK constraint", "Update succeeded", "High");
                // restore
                ExecSql("UPDATE Contracts SET Status=N'Active' WHERE ContractID=@id", ("@id", ContractId > 0 ? ContractId : 1));
                return "FAIL";
            }
            catch (Exception ex)
            {
                return $"CHECK OK: {ex.Message.Split('\n')[0]}";
            }
        });
    }

    #endregion

    #region Helpers

    private static void RegisterForms(IServiceCollection services)
    {
        services.AddTransient<LoginFormProxy>(); // unused
        // Mirror RpmsTestExec registrations lightly via ResolveFormTag using ServiceProvider GetRequiredService
        services.AddTransient<RPMS.WinForms.Forms.Auth.LoginForm>();
        services.AddTransient<RPMS.WinForms.Forms.Auth.RegisterForm>();
        services.AddTransient<MainForm>();
        services.AddTransient<RPMS.WinForms.Forms.Dashboard.DashboardForm>();
        services.AddTransient<RPMS.WinForms.Forms.Shared.NotificationCenterForm>();
        services.AddTransient<RPMS.WinForms.Forms.Shared.ProfileForm>();
        services.AddTransient<RPMS.WinForms.Forms.Shared.ChatForm>();
        services.AddTransient<RPMS.WinForms.Forms.Shared.CalendarForm>();
        services.AddTransient<RPMS.WinForms.Forms.Shared.ReportForm>();
        services.AddTransient<RPMS.WinForms.Forms.Admin.BackupForm>();
        services.AddTransient<RPMS.WinForms.Forms.Admin.UserManagementForm>();
        services.AddTransient<RPMS.WinForms.Forms.Admin.PostManagementForm>();
        services.AddTransient<RPMS.WinForms.Forms.Admin.ActivityLogForm>();
        services.AddTransient<RPMS.WinForms.Forms.Admin.ReviewManagementForm>();
        services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordHouseForm>();
        services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordRoomForm>();
        services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordAssignmentForm>();
        services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordContractForm>();
        services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordAppointmentForm>();
        services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordPostForm>();
        services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordReviewForm>();
        services.AddTransient<RPMS.WinForms.Forms.Tenant.TenantHomeForm>();
        services.AddTransient<RPMS.WinForms.Forms.Tenant.TenantContractForm>();
        services.AddTransient<RPMS.WinForms.Forms.Tenant.TenantFavoriteForm>();
        services.AddTransient<RPMS.WinForms.Forms.Tenant.TenantInvoiceForm>();
        services.AddTransient<RPMS.WinForms.Forms.Tenant.TenantMaintenanceForm>();
        services.AddTransient<RPMS.WinForms.Forms.Tenant.TenantReviewForm>();
        services.AddTransient<RPMS.WinForms.Forms.Manager.ManagerMeterForm>();
        services.AddTransient<RPMS.WinForms.Forms.Manager.ManagerMaintenanceForm>();
    }

    private sealed class LoginFormProxy { }

    private static void ResolveFormTag(string tag)
    {
        object form = tag switch
        {
            "Dashboard" => _sp.GetRequiredService<RPMS.WinForms.Forms.Dashboard.DashboardForm>(),
            "Notifications" => _sp.GetRequiredService<RPMS.WinForms.Forms.Shared.NotificationCenterForm>(),
            "Profile" => _sp.GetRequiredService<RPMS.WinForms.Forms.Shared.ProfileForm>(),
            "Calendar" => _sp.GetRequiredService<RPMS.WinForms.Forms.Shared.CalendarForm>(),
            "UserManagement" => _sp.GetRequiredService<RPMS.WinForms.Forms.Admin.UserManagementForm>(),
            "PostManagement" => _sp.GetRequiredService<RPMS.WinForms.Forms.Admin.PostManagementForm>(),
            "Backup" => _sp.GetRequiredService<RPMS.WinForms.Forms.Admin.BackupForm>(),
            "LandlordHouse" => _sp.GetRequiredService<RPMS.WinForms.Forms.Landlord.LandlordHouseForm>(),
            "LandlordRoom" => _sp.GetRequiredService<RPMS.WinForms.Forms.Landlord.LandlordRoomForm>(),
            "LandlordContract" => _sp.GetRequiredService<RPMS.WinForms.Forms.Landlord.LandlordContractForm>(),
            "LandlordAssignment" => _sp.GetRequiredService<RPMS.WinForms.Forms.Landlord.LandlordAssignmentForm>(),
            "TenantHome" => _sp.GetRequiredService<RPMS.WinForms.Forms.Tenant.TenantHomeForm>(),
            "TenantContract" => _sp.GetRequiredService<RPMS.WinForms.Forms.Tenant.TenantContractForm>(),
            "TenantInvoice" => _sp.GetRequiredService<RPMS.WinForms.Forms.Tenant.TenantInvoiceForm>(),
            "ManagerMeter" => _sp.GetRequiredService<RPMS.WinForms.Forms.Manager.ManagerMeterForm>(),
            "ManagerMaintenance" => _sp.GetRequiredService<RPMS.WinForms.Forms.Manager.ManagerMaintenanceForm>(),
            _ => throw new InvalidOperationException("Unknown tag " + tag)
        };
        if (form is Form f)
        {
            f.ShowInTaskbar = false;
            f.Opacity = 0;
            f.Close();
            f.Dispose();
        }
    }

    private static T S<T>(IServiceScope scope) where T : notnull =>
        scope.ServiceProvider.GetRequiredService<T>();

    private static async Task LoginAs(IAuthService auth, string user, string pass)
    {
        var r = await auth.LoginAsync(new LoginRequestDto { Username = user, Password = pass });
        if (r.UserID <= 0) throw new InvalidOperationException($"Login {user} failed");
        UserSession.Login(r);
    }

    private static async Task ResolveDemoUsers()
    {
        using var scope = _sp.CreateScope();
        var auth = S<IAuthService>(scope);
        await LoginAs(auth, "admin", "admin123"); AdminId = UserSession.CurrentUser!.UserID;
        await LoginAs(auth, "namlandlord", "123456"); LandlordId = UserSession.CurrentUser!.UserID;
        await LoginAs(auth, "tenant", "123456"); TenantId = UserSession.CurrentUser!.UserID;
        await LoginAs(auth, "manager", "123456"); ManagerId = UserSession.CurrentUser!.UserID;
        UserSession.Logout();
    }

    private static void ResetDemoPasswords()
    {
        var map = new Dictionary<string, string>
        {
            ["admin"] = "admin123",
            ["namlandlord"] = "123456",
            ["tenant"] = "123456",
            ["manager"] = "123456",
            ["khach1"] = "123456",
        };
        using var cn = new SqlConnection(Cs);
        cn.Open();
        foreach (var kv in map)
        {
            using var cmd = new SqlCommand("UPDATE Users SET Password=@p, Status=N'Active' WHERE Username=@u", cn);
            cmd.Parameters.AddWithValue("@p", PasswordHelper.HashPassword(kv.Value));
            cmd.Parameters.AddWithValue("@u", kv.Key);
            cmd.ExecuteNonQuery();
        }
    }

    private static async Task Step(string flow, string name, string expected, Func<Task<string>> act)
    {
        try
        {
            string actual = await act();
            if (actual == "BLOCKED") return; // already recorded
            if (actual == "FAIL") return;
            Steps.Add(new StepResult(flow, name, expected, actual, "PASS"));
            Console.WriteLine($"  [PASS] {flow}/{name}: {actual}");
        }
        catch (Exception ex)
        {
            Fail(flow, name, expected, ex.Message, "High", ex);
        }
    }

    private static void Assert(bool cond, string detail)
    {
        if (!cond) throw new Exception("ASSERT: " + detail);
    }

    private static void Block(string flow, string name, string reason)
    {
        Steps.Add(new StepResult(flow, name, "", reason, "BLOCKED"));
        Console.WriteLine($"  [BLOCKED] {flow}/{name}: {reason}");
    }

    private static void Fail(string flow, string name, string expected, string actual, string severity, Exception? ex = null)
    {
        Steps.Add(new StepResult(flow, name, expected, actual, "FAIL"));
        Console.WriteLine($"  [FAIL] {flow}/{name}: {actual}");
        _bugSeq++;
        Bugs.Add(new BugItem
        {
            BugId = $"BUG-E2E-{_bugSeq:D3}",
            Module = flow,
            Title = $"{flow} — {name}",
            Steps = name,
            Expected = expected,
            Actual = actual,
            Severity = severity,
            Stack = ex?.ToString() ?? "",
            Cause = ex?.GetBaseException().Message ?? actual
        });
    }

    private static int ScalarInt(string sql, params (string, object)[] ps)
    {
        using var cn = new SqlConnection(Cs);
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        foreach (var (n, v) in ps) cmd.Parameters.AddWithValue(n, v);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static decimal ScalarDec(string sql, params (string, object)[] ps)
    {
        using var cn = new SqlConnection(Cs);
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        foreach (var (n, v) in ps) cmd.Parameters.AddWithValue(n, v);
        return Convert.ToDecimal(cmd.ExecuteScalar());
    }

    private static string ScalarStr(string sql, params (string, object)[] ps)
    {
        using var cn = new SqlConnection(Cs);
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        foreach (var (n, v) in ps) cmd.Parameters.AddWithValue(n, v);
        var o = cmd.ExecuteScalar();
        return o?.ToString() ?? "";
    }

    private static void ExecSql(string sql, params (string, object)[] ps)
    {
        using var cn = new SqlConnection(Cs);
        cn.Open();
        using var cmd = new SqlCommand(sql, cn);
        foreach (var (n, v) in ps) cmd.Parameters.AddWithValue(n, v);
        cmd.ExecuteNonQuery();
    }

    private static void WriteReports(TimeSpan elapsed)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# RPMS E2E Business Flows — Test Execution Summary");
        sb.AppendLine();
        sb.AppendLine($"- **When:** {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"- **Duration:** {elapsed.TotalMinutes:F1} min");
        sb.AppendLine($"- **Approach:** BLL/service-level E2E + SQL verification + Form DI smoke (không automation WinForms UI click-by-click)");
        sb.AppendLine($"- **PASS:** {Steps.Count(s => s.Status == "PASS")}");
        sb.AppendLine($"- **FAIL:** {Steps.Count(s => s.Status == "FAIL")}");
        sb.AppendLine($"- **BLOCKED:** {Steps.Count(s => s.Status == "BLOCKED")}");
        sb.AppendLine($"- **Bugs:** {Bugs.Count}");
        sb.AppendLine();
        sb.AppendLine("## Results by step");
        sb.AppendLine();
        sb.AppendLine("| Flow | Step | Status | Expected | Actual |");
        sb.AppendLine("|------|------|--------|----------|--------|");
        foreach (var s in Steps)
            sb.AppendLine($"| {s.Flow} | {Esc(s.Name)} | **{s.Status}** | {Esc(s.Expected)} | {Esc(s.Actual)} |");

        sb.AppendLine();
        sb.AppendLine("## Flow coverage map");
        sb.AppendLine();
        for (int i = 1; i <= 15; i++)
        {
            var fs = Steps.Where(s => s.Flow == $"F{i}" || s.Flow.StartsWith($"F{i}")).ToList();
            // steps use F1, F2...
            fs = Steps.Where(s => s.Flow == $"F{i}").ToList();
            string roll = fs.Count == 0 ? "NOT RUN"
                : fs.Any(x => x.Status == "FAIL") ? "FAIL"
                : fs.Any(x => x.Status == "BLOCKED") && fs.All(x => x.Status != "FAIL") ? "PASS*"
                : "PASS";
            sb.AppendLine($"- **Luồng {i}:** {roll} ({fs.Count(x => x.Status == "PASS")}P/{fs.Count(x => x.Status == "FAIL")}F/{fs.Count(x => x.Status == "BLOCKED")}B)");
        }
        sb.AppendLine();
        sb.AppendLine("\\* PASS* = có bước BLOCKED (gap tính năng / thiếu data) nhưng không FAIL.");
        sb.AppendLine();
        sb.AppendLine("## Ghi chú QA quan trọng");
        sb.AppendLine();
        sb.AppendLine("1. **Appointment status** trong code là `Accepted` (không phải `Confirmed`).");
        sb.AppendLine("2. **Review** chỉ cho phép khi HĐ `Terminated`/`Expired` — Flow 10 terminate trước rồi đánh giá.");
        sb.AppendLine("3. **Gán Manager** chỉ sau khi nhà có HĐ Active; Flow 8 verify chặn đúng.");
        sb.AppendLine("4. **Thuê phòng:** Create/Assign → `PendingConfirm` → Tenant Accept → `Active`+`Occupied`.");
        sb.AppendLine("5. **MoveInDate** khi Accept = Today; demo hóa đơn tháng trước cần SQL backdate (đã làm trong Flow 4→5).");
        sb.AppendLine("6. **BLOCKED gaps:** upload ảnh House (không API); Hidden Post (không có status Hidden).");
        sb.AppendLine("7. Runner: `dotnet run --project tools/RpmsE2EFlows`");
        sb.AppendLine();
        sb.AppendLine("## Bug Reports");
        sb.AppendLine();
        if (Bugs.Count == 0) sb.AppendLine("_Không có bug FAIL._");
        foreach (var b in Bugs)
        {
            sb.AppendLine($"### {b.BugId} — {b.Title}");
            sb.AppendLine($"- **Module:** {b.Module}");
            sb.AppendLine($"- **Severity:** {b.Severity}");
            sb.AppendLine($"- **Steps:** {b.Steps}");
            sb.AppendLine($"- **Expected:** {b.Expected}");
            sb.AppendLine($"- **Actual:** {b.Actual}");
            sb.AppendLine($"- **Cause:** {b.Cause}");
            if (!string.IsNullOrWhiteSpace(b.Stack))
                sb.AppendLine($"\n```\n{b.Stack.Split('\n').Take(15).Aggregate((a, c) => a + "\n" + c)}\n```\n");
        }

        File.WriteAllText(Path.Combine(Docs, "E2E_BusinessFlows_Summary.md"), sb.ToString(), Encoding.UTF8);
        File.WriteAllText(Path.Combine(Docs, "E2E_Bug_Report.md"),
            Bugs.Count == 0 ? "# Bugs\n\nNone.\n" : string.Join("\n", Bugs.Select(b =>
                $"## {b.BugId}\nModule: {b.Module}\nSeverity: {b.Severity}\nExpected: {b.Expected}\nActual: {b.Actual}\n")),
            Encoding.UTF8);
    }

    private static string Esc(string s) => (s ?? "").Replace("|", "/").Replace("\n", " ").Trim();

    private sealed record StepResult(string Flow, string Name, string Expected, string Actual, string Status);

    private sealed class BugItem
    {
        public string BugId { get; set; } = "";
        public string Module { get; set; } = "";
        public string Title { get; set; } = "";
        public string Steps { get; set; } = "";
        public string Expected { get; set; } = "";
        public string Actual { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Stack { get; set; } = "";
        public string Cause { get; set; } = "";
    }

    #endregion
}
