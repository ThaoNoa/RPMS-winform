using System;
using System.Collections.Generic;

namespace RPMS.DAL.Entities
{
    public class User
    {
        public User()
        {
            Houses = new HashSet<House>();
            ApprovedPosts = new HashSet<Post>();
            Favorites = new HashSet<Favorite>();
            Appointments = new HashSet<Appointment>();
            TenantContracts = new HashSet<Contract>();
            CreatedContracts = new HashSet<Contract>();
            CreatedMeterReadings = new HashSet<MeterReading>();
            AssignedMaintenanceRequests = new HashSet<MaintenanceRequest>();
            Assignments = new HashSet<Assignment>();
            Notifications = new HashSet<Notification>();
            ActivityLogs = new HashSet<ActivityLog>();
        }

        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string FullName { get; set; } = "";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string? Address { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual ICollection<House> Houses { get; set; }
        public virtual ICollection<Post> ApprovedPosts { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<Contract> TenantContracts { get; set; }
        public virtual ICollection<Contract> CreatedContracts { get; set; }
        public virtual ICollection<MeterReading> CreatedMeterReadings { get; set; }
        public virtual ICollection<MaintenanceRequest> AssignedMaintenanceRequests { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<ActivityLog> ActivityLogs { get; set; }
    }
}