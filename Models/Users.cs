using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookNest.Models
{
    [Table("Users")]
    public class Users
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("first_name")]
        [MaxLength(20)]
        public string FirstName { get; set; }

        [Column("last_name")]
        [MaxLength(20)]
        public string LastName { get; set; }

        [Column("email")]
        [MaxLength(256)]
        public string Email { get; set; }

        [Column("phone")]
        [MaxLength(10)]
        public string PhoneNumber { get; set; }

        [Column("password_hashed")]
        [MaxLength(256)] 
        public string PasswordHash { get; set; }

        [Column("type")]
        [MaxLength(10)]
        public string Type { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }
        public int IdentityUserId { get; set; }
    }
}
