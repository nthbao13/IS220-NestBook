using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace BookNest.Data.AuthInfomation
{
	[Table("Users")]
	public class ApplicationUser : IdentityUser<int>
	{
		[Column("first_name")]
		[MaxLength(20)]
		public string FirstName { get; set; }

		[Column("last_name")]
		[MaxLength(20)]
		public string LastName { get; set; }

		[Column("email")]
		[MaxLength(20)]
		public override string Email { get; set; }

		[Column("phone")]
		[MaxLength(10)]
		public override string PhoneNumber { get; set; }

		[Column("password_hashed")]
		[MaxLength(256)] // Tăng lên để phù hợp với Identity
		public override string PasswordHash { get; set; }

		[Column("type")]
		[MaxLength(10)]
		public string Type { get; set; }

		[Column("role_id")]
		public int RoleId { get; set; }

		[ForeignKey("RoleId")]
		public ApplicationRole Role { get; set; }
	}
}
