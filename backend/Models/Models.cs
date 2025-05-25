using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le message est requis")]
        [MinLength(10, ErrorMessage = "Le message doit contenir au moins 10 caractères")]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? Subject { get; set; }
        public string? AttachmentPath { get; set; }
        public bool IsAnswered { get; set; }
        public string? ResponseMessage { get; set; }
    }

    public class NewsletterSubscriber
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; } = string.Empty;

        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string? UnsubscribeToken { get; set; }
    }

    public class NewsletterCampaign
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le titre est requis")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le contenu est requis")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
        public int SentCount { get; set; }
        public string Status { get; set; } = "Draft";
    }

    public class ShareStatistics
    {
        public int Id { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int ShareCount { get; set; }
        public DateTime LastShared { get; set; } = DateTime.UtcNow;
    }

    public class AdminUser
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom d'utilisateur est requis")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Admin";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
    }

    public class AutoResponse
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Template { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
} 