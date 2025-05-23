using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookNest.Data.AuthInfomation
{
	[Table("Roles")]
	public class ApplicationRole : IdentityRole<int>  
	{
		[Column("role")]
		[MaxLength(10)]
		public string RoleName { get; set; }
	}
}
