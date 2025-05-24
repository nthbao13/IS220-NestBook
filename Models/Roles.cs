using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookNest.Models
{
    [Table("Roles")]
    public class Roles
    {
        [Key]
        public int Id { get; set; }
        [Column("role")]
        public string role { get; set; }
    }
}
