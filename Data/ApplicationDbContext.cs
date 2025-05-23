using BookNest.Data.AuthInfomation;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BookNest.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			builder.Entity<ApplicationUser>().ToTable("Users");
			builder.Entity<ApplicationRole>().ToTable("Roles");
        }

	}
}
