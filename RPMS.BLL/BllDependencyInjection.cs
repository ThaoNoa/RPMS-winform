using Microsoft.Extensions.DependencyInjection;
using RPMS.BLL.Interfaces;
using RPMS.BLL.Services;

namespace RPMS.BLL
{
    public static class BllDependencyInjection
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
        {
            // Đăng ký AutoMapper – scan assembly hiện tại để tìm các Profile
            services.AddAutoMapper(typeof(BllDependencyInjection).Assembly);

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IHouseService, HouseService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IAmenityService, AmenityService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IMaintenanceService, MaintenanceService>();
            services.AddScoped<IStatisticService, StatisticService>();
            services.AddScoped<ITenantInteractionService, TenantInteractionService>();
            services.AddScoped<ILandlordService, LandlordService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAssignmentService, AssignmentService>();
            services.AddScoped<IActivityLogService, ActivityLogService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<ICalendarService, CalendarService>();
            services.AddScoped<IReportService, ReportService>();

            return services;
        }
    }
}