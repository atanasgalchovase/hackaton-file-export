using hackaton_oauth.data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hackaton_oauth.data
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to postgres with connection string from app settings
            options.UseNpgsql(Configuration.GetConnectionString("WebApiDatabase"));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                // Property.
                b.Property(x => x.Id).ValueGeneratedNever();
                b.Property(x => x.Username).IsRequired().HasMaxLength(255);
                b.Property(x => x.Email).HasMaxLength(320);
                b.Property(x => x.CreatedAt).IsRequired();

                // Index.
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.Username);
                b.HasIndex(x => x.IsActive);
                b.HasIndex(x => x.RoleId);
                b.HasIndex(x => x.CreatedById);
                b.HasIndex(x => x.UpdatedById);

                // Relation.
                b.HasOne(x => x.Role).WithMany(y => y.Users).HasForeignKey(z => z.RoleId);
                b.HasOne(x => x.CreatedBy).WithMany(x => x.CreatedUsers).HasForeignKey(x => x.CreatedById);
                b.HasOne(x => x.UpdatedBy).WithMany(x => x.UpdatedUsers).HasForeignKey(x => x.UpdatedById);
            });

            modelBuilder.Entity<Role>(b =>
            {
                // Property.
                b.Property(x => x.Id).ValueGeneratedNever();
                b.Property(x => x.Name).IsRequired().HasMaxLength(64);

                // Index.
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.Name).IsUnique();
                b.HasIndex(x => x.CreatedById);
                b.HasIndex(x => x.UpdatedById);

                // Relation.
                b.HasOne(x => x.CreatedBy).WithMany(x => x.CreatedRoles).HasForeignKey(x => x.CreatedById);
                b.HasOne(x => x.UpdatedBy).WithMany(x => x.UpdatedRoles).HasForeignKey(x => x.UpdatedById);
            });
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }
    }
}
