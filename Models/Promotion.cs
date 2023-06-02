using System.ComponentModel.DataAnnotations;

namespace Mercadona4.Models
{
    public class Promotion
    {
        [Key]  // Cette propriété est une clé primaire dans la base de données.
        public int Id { get; set; }

        [Required]  // Cette propriété est obligatoire.
        public DateTime StartDate { get; set; }  // La date de début de la promotion.

        [Required]
        public DateTime EndDate { get; set; }  // La date de fin de la promotion.

        [Required]
        public decimal DiscountPercentage { get; set; }  // Le pourcentage de réduction offert par la promotion.

    }
}
