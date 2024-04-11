using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hackaton_oauth.data.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedById { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedById { get; set; }

        public Guid? RoleId { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        public bool IsActive { get; set; }

        public virtual User CreatedBy { get; set; }

        public virtual User UpdatedBy { get; set; }

        public virtual Role Role { get; set; }

        public virtual ICollection<User> CreatedUsers { get; set; }

        public virtual ICollection<User> UpdatedUsers { get; set; }

        public virtual ICollection<Role> CreatedRoles { get; set; }

        public virtual ICollection<Role> UpdatedRoles { get; set; }
    }
}
