using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NET_TASK.Models
{
    [Table("Catalogs")]
    public class Catalog
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ParentID { get; set; }
        public string Name { get; set; }


        [ForeignKey("ApplicationUser")]
        public string? UserID { get; set; }
        public User? User { get; set; }
    }
}