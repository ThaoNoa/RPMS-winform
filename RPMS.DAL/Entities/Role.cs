using System.Collections.Generic;

namespace RPMS.DAL.Entities
{
    public class Role
    {
        public Role()
        {
            Users = new HashSet<User>();
        }

        public int RoleID { get; set; }
        public string RoleName { get; set; } = "";

        public virtual ICollection<User> Users { get; set; }
    }
}