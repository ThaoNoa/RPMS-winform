using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL;
using RPMS.BLL.Interfaces;
using RPMS.BLL.Services;
using RPMS.DAL;
using RPMS.DAL.Data;
using RPMS.WinForms.Forms.Auth;
using RPMS.WinForms.Forms.Layout;
using System;
using System.Windows.Forms;

namespace RPMS.WinForms
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;
        public static string ConnectionString { get; private set; } =
            @"Server=(localdb)\mssqllocaldb;Database=RPMS;Trusted_Connection=True;TrustServerCertificate=True;";

        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            try
            {
                using var scope = ServiceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RPMSContext>();
                DatabaseSchemaUpdater.EnsureUpdatedAsync(db).GetAwaiter().GetResult();
                DataSeeder.SeedAsync(db).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể khởi tạo/cập nhật database.\n" + ex.Message +
                    "\n\nKiểm tra LocalDB: (localdb)\\mssqllocaldb",
                    "Lỗi Database",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            bool keepRunning = true;
            while (keepRunning)
            {
                using (var loginForm = ServiceProvider.GetRequiredService<LoginForm>())
                {
                    var result = loginForm.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        using (var mainForm = ServiceProvider.GetRequiredService<MainForm>())
                        {
                            var mainResult = mainForm.ShowDialog();
                            if (mainResult != DialogResult.Retry)
                                keepRunning = false;
                        }
                    }
                    else
                        keepRunning = false;
                }
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddDataAccessLayer(ConnectionString);
            services.AddBusinessLogicLayer();
            services.AddSingleton<IBackupService>(_ => new BackupService(ConnectionString));

            services.AddTransient<LoginForm>();
            services.AddTransient<RegisterForm>();
            services.AddTransient<MainForm>();
            services.AddTransient<Forms.Admin.UserManagementForm>();
            services.AddTransient<Forms.Admin.UserModalForm>();
            services.AddTransient<Forms.Admin.PostManagementForm>();
            services.AddTransient<Forms.Admin.PostDetailModalForm>();
            services.AddTransient<Forms.Admin.AssignmentManagementForm>();
            services.AddTransient<Forms.Admin.ActivityLogForm>();
            services.AddTransient<Forms.Admin.ReviewManagementForm>();
            services.AddTransient<Forms.Admin.BackupForm>();
            services.AddTransient<Forms.Landlord.LandlordHouseForm>();
            services.AddTransient<Forms.Landlord.LandlordHouseModalForm>();
            services.AddTransient<Forms.Landlord.LandlordRoomForm>();
            services.AddTransient<Forms.Landlord.LandlordRoomModalForm>();
            services.AddTransient<Forms.Landlord.LandlordContractForm>();
            services.AddTransient<Forms.Landlord.LandlordAppointmentForm>();
            services.AddTransient<Forms.Landlord.LandlordPostForm>();
            services.AddTransient<Forms.Landlord.LandlordReviewForm>();
            services.AddTransient<Forms.Tenant.TenantHomeForm>();
            services.AddTransient<Forms.Tenant.TenantAppointmentModalForm>();
            services.AddTransient<Forms.Tenant.TenantContractForm>();
            services.AddTransient<Forms.Tenant.TenantFavoriteForm>();
            services.AddTransient<Forms.Tenant.TenantInvoiceForm>();
            services.AddTransient<Forms.Tenant.TenantMaintenanceForm>();
            services.AddTransient<Forms.Tenant.TenantReviewForm>();
            services.AddTransient<Forms.Manager.ManagerMeterForm>();
            services.AddTransient<Forms.Manager.ManagerMaintenanceForm>();
            services.AddTransient<Forms.Dashboard.DashboardForm>();
            services.AddTransient<Forms.Shared.NotificationCenterForm>();
            services.AddTransient<Forms.Shared.ProfileForm>();
            services.AddTransient<Forms.Shared.ChatForm>();
            services.AddTransient<Forms.Shared.CalendarForm>();
            services.AddTransient<Forms.Shared.ReportForm>();
        }
    }
}
