using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL;
using RPMS.BLL.Interfaces;
using RPMS.Common.Globals;
using RPMS.DAL;
using RPMS.DAL.Data;
using RPMS.DTO.Auth;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPMS.Smoke
{
    /// <summary>
    /// Headless smoke: DB + Auth + services + Form DI resolve.
    /// Run: dotnet run --project tools/RpmsSmoke/RpmsSmoke.csproj
    /// </summary>
    internal static class Program
    {
        private const string Cs =
            @"Server=.\SQLEXPRESS;Database=RPMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

        [STAThread]
        private static async Task<int> Main()
        {
            int fail = 0;
            void Ok(string m) => Console.WriteLine("[PASS] " + m);
            void Fail(string m) { fail++; Console.WriteLine("[FAIL] " + m); }

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();

            var services = new ServiceCollection();
            services.AddDataAccessLayer(Cs);
            services.AddBusinessLogicLayer();
            services.AddSingleton<IBackupService>(_ => new RPMS.BLL.Services.BackupService(Cs));

            // Forms used by MainForm navigation
            services.AddTransient<RPMS.WinForms.Forms.Auth.LoginForm>();
            services.AddTransient<RPMS.WinForms.Forms.Auth.RegisterForm>();
            services.AddTransient<RPMS.WinForms.Forms.Layout.MainForm>();
            services.AddTransient<RPMS.WinForms.Forms.Dashboard.DashboardForm>();
            services.AddTransient<RPMS.WinForms.Forms.Shared.NotificationCenterForm>();
            services.AddTransient<RPMS.WinForms.Forms.Shared.ProfileForm>();
            services.AddTransient<RPMS.WinForms.Forms.Shared.ChatForm>();
            services.AddTransient<RPMS.WinForms.Forms.Shared.CalendarForm>();
            services.AddTransient<RPMS.WinForms.Forms.Shared.ReportForm>();
            services.AddTransient<RPMS.WinForms.Forms.Admin.BackupForm>();
            services.AddTransient<RPMS.WinForms.Forms.Admin.UserManagementForm>();
            services.AddTransient<RPMS.WinForms.Forms.Admin.UserModalForm>();
            services.AddTransient<RPMS.WinForms.Forms.Admin.PostManagementForm>();
            services.AddTransient<RPMS.WinForms.Forms.Admin.PostDetailModalForm>();
            services.AddTransient<RPMS.WinForms.Forms.Admin.AssignmentManagementForm>();
            services.AddTransient<RPMS.WinForms.Forms.Admin.ActivityLogForm>();
            services.AddTransient<RPMS.WinForms.Forms.Admin.ReviewManagementForm>();
            services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordHouseForm>();
            services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordHouseModalForm>();
            services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordRoomForm>();
            services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordRoomModalForm>();
            services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordContractForm>();
            services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordAppointmentForm>();
            services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordPostForm>();
            services.AddTransient<RPMS.WinForms.Forms.Landlord.LandlordReviewForm>();
            services.AddTransient<RPMS.WinForms.Forms.Tenant.TenantHomeForm>();
            services.AddTransient<RPMS.WinForms.Forms.Tenant.TenantFavoriteForm>();
            services.AddTransient<RPMS.WinForms.Forms.Tenant.TenantContractForm>();
            services.AddTransient<RPMS.WinForms.Forms.Tenant.TenantInvoiceForm>();
            services.AddTransient<RPMS.WinForms.Forms.Tenant.TenantMaintenanceForm>();
            services.AddTransient<RPMS.WinForms.Forms.Tenant.TenantReviewForm>();
            services.AddTransient<RPMS.WinForms.Forms.Tenant.TenantAppointmentModalForm>();
            services.AddTransient<RPMS.WinForms.Forms.Manager.ManagerMeterForm>();
            services.AddTransient<RPMS.WinForms.Forms.Manager.ManagerMaintenanceForm>();

            await using var sp = services.BuildServiceProvider();

            // Allow hosted forms to resolve children the same way as the real app.
            var winProg = Type.GetType("RPMS.WinForms.Program, RPMS.WinForms")
                ?? throw new InvalidOperationException("Cannot find RPMS.WinForms.Program");
            winProg.GetProperty("ServiceProvider")!.SetValue(null, sp);

            // --- DB ---
            try
            {
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RPMSContext>();
                await DatabaseSchemaUpdater.EnsureUpdatedAsync(db);
                await DataSeeder.SeedAsync(db);
                Ok("Schema + Seed");
            }
            catch (Exception ex)
            {
                Fail("Schema/Seed: " + ex.Message);
                return 1;
            }

            // --- Auth all roles ---
            var accounts = new[]
            {
                ("admin", "admin123"),
                ("namlandlord", "123456"),
                ("tenant", "123456"),
                ("manager", "123456"),
            };
            foreach (var (user, pass) in accounts)
            {
                try
                {
                    using var scope = sp.CreateScope();
                    var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
                    var resp = await auth.LoginAsync(new LoginRequestDto { Username = user, Password = pass });
                    if (resp == null || resp.UserID <= 0) Fail($"Login {user}: empty response");
                    else Ok($"Login {user} (RoleID={resp.RoleID})");
                }
                catch (Exception ex)
                {
                    Fail($"Login {user}: {ex.Message}");
                }
            }

            // Bad password
            try
            {
                using var scope = sp.CreateScope();
                var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
                await auth.LoginAsync(new LoginRequestDto { Username = "admin", Password = "wrong" });
                Fail("Bad password should throw");
            }
            catch
            {
                Ok("Bad password rejected");
            }

            // --- Service smoke as each role ---
            await SmokeAdmin(sp, Ok, Fail);
            await SmokeLandlord(sp, Ok, Fail);
            await SmokeTenant(sp, Ok, Fail);
            await SmokeManager(sp, Ok, Fail);

            // --- Resolve every form ---
            var formTypes = new[]
            {
                typeof(RPMS.WinForms.Forms.Auth.LoginForm),
                typeof(RPMS.WinForms.Forms.Auth.RegisterForm),
                typeof(RPMS.WinForms.Forms.Layout.MainForm),
                typeof(RPMS.WinForms.Forms.Dashboard.DashboardForm),
                typeof(RPMS.WinForms.Forms.Shared.NotificationCenterForm),
                typeof(RPMS.WinForms.Forms.Shared.ProfileForm),
                typeof(RPMS.WinForms.Forms.Shared.ChatForm),
                typeof(RPMS.WinForms.Forms.Shared.CalendarForm),
                typeof(RPMS.WinForms.Forms.Shared.ReportForm),
                typeof(RPMS.WinForms.Forms.Admin.BackupForm),
                typeof(RPMS.WinForms.Forms.Admin.UserManagementForm),
                typeof(RPMS.WinForms.Forms.Admin.PostManagementForm),
                typeof(RPMS.WinForms.Forms.Admin.AssignmentManagementForm),
                typeof(RPMS.WinForms.Forms.Admin.ActivityLogForm),
                typeof(RPMS.WinForms.Forms.Admin.ReviewManagementForm),
                typeof(RPMS.WinForms.Forms.Landlord.LandlordHouseForm),
                typeof(RPMS.WinForms.Forms.Landlord.LandlordRoomForm),
                typeof(RPMS.WinForms.Forms.Landlord.LandlordContractForm),
                typeof(RPMS.WinForms.Forms.Landlord.LandlordAppointmentForm),
                typeof(RPMS.WinForms.Forms.Landlord.LandlordPostForm),
                typeof(RPMS.WinForms.Forms.Landlord.LandlordReviewForm),
                typeof(RPMS.WinForms.Forms.Tenant.TenantHomeForm),
                typeof(RPMS.WinForms.Forms.Tenant.TenantFavoriteForm),
                typeof(RPMS.WinForms.Forms.Tenant.TenantContractForm),
                typeof(RPMS.WinForms.Forms.Tenant.TenantInvoiceForm),
                typeof(RPMS.WinForms.Forms.Tenant.TenantMaintenanceForm),
                typeof(RPMS.WinForms.Forms.Tenant.TenantReviewForm),
                typeof(RPMS.WinForms.Forms.Manager.ManagerMeterForm),
                typeof(RPMS.WinForms.Forms.Manager.ManagerMaintenanceForm),
            };

            // Login admin for session-dependent forms
            try
            {
                using (var scope = sp.CreateScope())
                {
                    var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
                    var resp = await auth.LoginAsync(new LoginRequestDto { Username = "admin", Password = "admin123" });
                    UserSession.Login(resp);
                    Ok("Session admin for form resolve");
                }
            }
            catch (Exception ex)
            {
                Fail("Session admin: " + ex.Message);
                using var scope = sp.CreateScope();
                var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var resp = await auth.LoginAsync(new LoginRequestDto { Username = "namlandlord", Password = "123456" });
                UserSession.Login(resp);
            }

            foreach (var t in formTypes)
            {
                try
                {
                    var form = (Form)sp.GetRequiredService(t);
                    form.Dispose();
                    Ok("Resolve " + t.Name);
                }
                catch (Exception ex)
                {
                    Fail($"Resolve {t.Name}: {ex.GetBaseException().Message}");
                }
            }

            UserSession.Logout();
            Console.WriteLine();
            Console.WriteLine(fail == 0 ? "SMOKE RESULT: ALL PASSED" : $"SMOKE RESULT: {fail} FAILED");
            return fail == 0 ? 0 : 2;
        }

        private static async Task SmokeAdmin(ServiceProvider sp, Action<string> Ok, Action<string> Fail)
        {
            try
            {
                using var scope = sp.CreateScope();
                var users = scope.ServiceProvider.GetRequiredService<IUserService>();
                var posts = scope.ServiceProvider.GetRequiredService<IPostService>();
                var stats = scope.ServiceProvider.GetRequiredService<IStatisticService>();
                var assign = scope.ServiceProvider.GetRequiredService<IAssignmentService>();
                var report = scope.ServiceProvider.GetRequiredService<IReportService>();
                var list = (await users.GetAllUsersAsync()).ToList();
                var pending = (await posts.GetPendingPostsAsync()).ToList();
                _ = await stats.GetAdminDashboardStatsAsync();
                _ = (await assign.GetAllAsync()).ToList();
                _ = await report.GetAdminReportAsync();
                Ok($"Admin services (users={list.Count}, pendingPosts={pending.Count})");
            }
            catch (Exception ex) { Fail("Admin services: " + ex.Message); }
        }

        private static async Task SmokeLandlord(ServiceProvider sp, Action<string> Ok, Action<string> Fail)
        {
            try
            {
                using var scope = sp.CreateScope();
                var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var resp = await auth.LoginAsync(new LoginRequestDto { Username = "namlandlord", Password = "123456" });
                UserSession.Login(resp);
                var houses = scope.ServiceProvider.GetRequiredService<IHouseService>();
                var rooms = scope.ServiceProvider.GetRequiredService<IRoomService>();
                var contracts = scope.ServiceProvider.GetRequiredService<IContractService>();
                var landlord = scope.ServiceProvider.GetRequiredService<ILandlordService>();
                var h = (await houses.GetHousesByOwnerAsync(resp.UserID)).ToList();
                var rCount = 0;
                if (h.Count > 0)
                    rCount = (await rooms.GetRoomsByHouseAsync(h[0].HouseID)).Count();
                _ = (await contracts.GetContractsByLandlordAsync(resp.UserID)).ToList();
                _ = (await landlord.GetAppointmentsAsync(resp.UserID, null, "All", null, null)).ToList();
                Ok($"Landlord services (houses={h.Count}, roomsHouse0={rCount})");
            }
            catch (Exception ex) { Fail("Landlord services: " + ex.Message); }
            finally { UserSession.Logout(); }
        }

        private static async Task SmokeTenant(ServiceProvider sp, Action<string> Ok, Action<string> Fail)
        {
            try
            {
                using var scope = sp.CreateScope();
                var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var resp = await auth.LoginAsync(new LoginRequestDto { Username = "tenant", Password = "123456" });
                UserSession.Login(resp);
                var tenant = scope.ServiceProvider.GetRequiredService<ITenantService>();
                var contracts = scope.ServiceProvider.GetRequiredService<IContractService>();
                var fav = scope.ServiceProvider.GetRequiredService<ITenantInteractionService>();
                var posts = (await tenant.SearchRoomsAsync(new RPMS.DTO.Post.RoomSearchFilterDto())).ToList();
                var c = (await contracts.GetContractsByTenantAsync(resp.UserID)).ToList();
                _ = (await fav.GetFavoritesAsync(resp.UserID)).ToList();
                Ok($"Tenant services (posts={posts.Count}, contracts={c.Count})");
            }
            catch (Exception ex) { Fail("Tenant services: " + ex.Message); }
            finally { UserSession.Logout(); }
        }

        private static async Task SmokeManager(ServiceProvider sp, Action<string> Ok, Action<string> Fail)
        {
            try
            {
                using var scope = sp.CreateScope();
                var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var resp = await auth.LoginAsync(new LoginRequestDto { Username = "manager", Password = "123456" });
                UserSession.Login(resp);
                var invoice = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                var maint = scope.ServiceProvider.GetRequiredService<IMaintenanceService>();
                var assign = scope.ServiceProvider.GetRequiredService<IAssignmentService>();
                var a = (await assign.GetByManagerAsync(resp.UserID)).ToList();
                _ = (await maint.GetRequestsForManagerAsync(resp.UserID)).ToList();
                Ok($"Manager services (assignments={a.Count})");
                _ = invoice;
            }
            catch (Exception ex) { Fail("Manager services: " + ex.Message); }
            finally { UserSession.Logout(); }
        }
    }
}
