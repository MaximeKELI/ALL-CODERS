using System.ComponentModel.DataAnnotations;

namespace AllCodersWeb.Models;

public class Contact
{
    [Required(ErrorMessage = "Le nom est requis")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le message est requis")]
    [MinLength(10, ErrorMessage = "Le message doit contenir au moins 10 caractères")]
    public string Message { get; set; } = string.Empty;
}

public class ContactResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
} 