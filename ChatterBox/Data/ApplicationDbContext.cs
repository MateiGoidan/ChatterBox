using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ChatterBox.Models;

namespace ChatterBox.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<BindChannelUser> BindChannelUserEntries { get; set; }
		public DbSet<BindRequestChannelUser> BindRequestChannelUserEntries { get; set; }
		public DbSet<ApplicationUser> AppUsers { get; set; }
		public DbSet<Channel> Channels { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Request> Requests { get; set; }
		public DbSet<Message> Messages { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<BindChannelUser>().HasKey(ac => new { ac.ChannelId, ac.UserId });
			builder.Entity<BindChannelUser>().HasOne(ac => ac.Channel).WithMany(ac => ac.BindChannelUser).HasForeignKey(ac => ac.ChannelId);
			builder.Entity<BindChannelUser>().HasOne(ac => ac.User).WithMany(ac => ac.BindChannelUsers).HasForeignKey(ac => ac.UserId);

			builder.Entity<BindRequestChannelUser>().HasKey(ac => new { ac.ChannelId, ac.UserId, ac.RequestId });
			builder.Entity<BindRequestChannelUser>().HasOne(ac => ac.Channel).WithMany(ac => ac.BindRequestChannelUser).HasForeignKey(ac => ac.ChannelId);
			builder.Entity<BindRequestChannelUser>().HasOne(ac => ac.User).WithMany(ac => ac.BindRequestChannelUsers).HasForeignKey(ac => ac.UserId);
			builder.Entity<BindRequestChannelUser>().HasOne(ac => ac.Request).WithMany(ac => ac.BindRequestChannelUsers).HasForeignKey(ac => ac.RequestId);
		}
	}
}
