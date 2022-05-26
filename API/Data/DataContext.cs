using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : IdentityDbContext<
	AppUser,
	AppRole,
	int,
	IdentityUserClaim<int>,
	AppUserRole,
	IdentityUserLogin<int>,
	IdentityRoleClaim<int>,
	IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }
		
        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
			
			builder.Entity<AppUser>()
				.HasMany(u => u.UserRoles)
				.WithOne(u => u.User)
				.HasForeignKey(u => u.UserId)
				.IsRequired();
				
			builder.Entity<AppRole>()
				.HasMany(u => u.UserRoles)
				.WithOne(u => u.Role)
				.HasForeignKey(u => u.RoleId)
				.IsRequired();

            builder.Entity<UserLike>()
                .HasKey(k => new{k.SourceUserId, k.LikedUserId});
            
            builder.Entity<UserLike>()
                .HasOne(s => s.SourceUser)
                .WithMany(s => s.LikedUser)
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>()
                .HasOne(s => s.LikedUser)
                .WithMany(s => s.LikedByUsers)
                .HasForeignKey(s => s.LikedUserId)
                .OnDelete(DeleteBehavior.Cascade);

			builder.Entity<Message>()
				.HasOne(m => m.Sender)
				.WithMany(m => m.MessagesSent)
				.OnDelete(DeleteBehavior.Restrict);
			
			builder.Entity<Message>()
				.HasOne(m => m.Recipient)
				.WithMany(m => m.MessagesReceived)
				.OnDelete(DeleteBehavior.Restrict);
        }
    }
}
