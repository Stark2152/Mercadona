using System.ComponentModel.DataAnnotations;

namespace Mercadona4.Models
{
    public class Promotion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public decimal DiscountPercentage { get; set; }
    }
}
