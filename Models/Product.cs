using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Mercadona4.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Label { get; set; } = null!;
        [Required]
        public string Description { get; set; } = null!;
        [Required]
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        [Required]
        public string Category { get; set; } = null!;

        [ForeignKey("Promotion")]
        public int? PromotionId { get; set; }

        public Promotion? Promotion { get; set; }
    }
}
