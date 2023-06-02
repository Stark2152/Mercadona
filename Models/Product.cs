using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Mercadona4.Models
{
    public class Product
    {
        [Key]  // Attribut indiquant que cette propriété est une clé primaire dans la base de données.
        public int Id { get; set; }

        [Required]  // Attribut indiquant que cette propriété est obligatoire (ne peut pas être null) dans la base de données.
        public string Label { get; set; } = null!;  // Le libellé du produit. L'opérateur "null!" est utilisé pour dire au compilateur que cette propriété ne sera jamais null lors de l'exécution.

        [Required]
        public string Description { get; set; } = null!;  // La description du produit.

        [Required]
        public decimal Price { get; set; }  // Le prix du produit.

        public string? ImageUrl { get; set; }  // L'URL de l'image du produit.

        [Required]
        public string Category { get; set; } = null!;  // La catégorie du produit.

        [ForeignKey("Promotion")]  // Attribut indiquant que cette propriété est une clé étrangère, qui fait référence à la table "Promotion" dans la base de données.
        public int? PromotionId { get; set; }  // L'ID de la promotion associée au produit. Cette propriété est facultative (peut être null).

        public Promotion? Promotion { get; set; }  // L'entité de promotion associée au produit.
    }
}
