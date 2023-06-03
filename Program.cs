using System.Globalization;
using Mercadona4.Context;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Ajoute le service ApplicationDbContext au conteneur d'injection de d�pendances avec une configuration sp�cifique
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Utilise le fournisseur Npgsql (PostgreSQL) pour le contexte de base de donn�es
    // R�cup�re la cha�ne de connexion "DefaultConnection" � partir de la configuration de l'application
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add session services.
builder.Services.AddSession();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// D�finir les cultures support�es
var supportedCultures = new[] { "fr-FR" };

// Configurer les options de localisation de l'application
var localizationOptions = new RequestLocalizationOptions
{
    // Culture par d�faut pour le traitement des requ�tes
    DefaultRequestCulture = new RequestCulture("fr-FR"),

    // Liste des cultures support�es pour le contenu de l'application
    SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList(),

    // Liste des cultures support�es pour l'interface utilisateur
    SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList()
};

app.UseRequestLocalization(localizationOptions);

app.UseRouting();

// Use session.
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
