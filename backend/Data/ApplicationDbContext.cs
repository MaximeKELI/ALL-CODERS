using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<NewsletterSubscriber> NewsletterSubscribers { get; set; }
        public DbSet<NewsletterCampaign> NewsletterCampaigns { get; set; }
        public DbSet<ShareStatistics> ShareStatistics { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<AutoResponse> AutoResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuration des index
            modelBuilder.Entity<NewsletterSubscriber>()
                .HasIndex(n => n.Email)
                .IsUnique();

            modelBuilder.Entity<AdminUser>()
                .HasIndex(a => a.Username)
                .IsUnique();

            // Données initiales pour l'admin
            modelBuilder.Entity<AdminUser>().HasData(
                new AdminUser
                {
                    Id = 1,
                    Username = "admin",
                    // Note: Dans un vrai projet, utilisez un hash sécurisé
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Réponse automatique par défaut
            modelBuilder.Entity<AutoResponse>().HasData(
                new AutoResponse
                {
                    Id = 1,
                    Subject = "Réception de votre message",
                    Template = @"
                        <h2>Merci pour votre message</h2>
                        <p>Cher(e) {Name},</p>
                        <p>Nous avons bien reçu votre message et nous vous en remercions.</p>
                        <p>Notre équipe va l'examiner et vous répondra dans les plus brefs délais.</p>
                        <p>Cordialement,<br>L'équipe ALL-CODERS</p>
                    ",
                    IsActive = true
                }
            );
        }
    }
} 