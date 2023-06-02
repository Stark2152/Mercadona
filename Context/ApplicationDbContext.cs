// Importation du paquet Microsoft.EntityFrameworkCore qui fournit les fonctionnalités de Entity Framework Core, 
// un framework populaire pour l'accès et la gestion de bases de données dans .NET.
using Microsoft.EntityFrameworkCore;

// Importation du modèle de données de l'application (Produit et Promotion).
using Mercadona4.Models;

namespace Mercadona4.Context
{
    // Définition de la classe ApplicationDbContext qui hérite de la classe DbContext de Entity Framework Core.
    // Cette classe est la principale classe qui coordonne les fonctionnalités d'Entity Framework pour un modèle de données.
    public class ApplicationDbContext : DbContext
    {
        // Constructeur de la classe qui prend les options de configuration de DbContext comme argument et les passe au constructeur de la classe de base.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Propriétés représentant les tables de la base de données. Chaque DbSet correspond à une table.
        // Un DbSet peut être utilisé pour interroger et sauvegarder des instances de la classe associée.

        // Propriété représentant la table des produits dans la base de données.
        public DbSet<Product> Products { get; set; }

        // Propriété représentant la table des promotions dans la base de données.
        public DbSet<Promotion> Promotions { get; set; }
    }
}
