using System.Net;
using System.Net.Mail;
using AllCodersWeb.Models;

namespace AllCodersWeb.Services;

public interface IEmailService
{
    Task<bool> SendContactEmailAsync(Contact contact);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly string _adminPhone;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Récupération des paramètres de configuration
        _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? throw new ArgumentNullException("SmtpServer configuration missing");
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? throw new ArgumentNullException("SenderEmail configuration missing");
        _senderName = _configuration["EmailSettings:SenderName"] ?? "ALL CODERS";
        _adminPhone = _configuration["AdminSettings:PhoneNumber"] ?? "";
    }

    public async Task<bool> SendContactEmailAsync(Contact contact)
    {
        try
        {
            // Validation supplémentaire des données
            if (string.IsNullOrWhiteSpace(contact.Email) || string.IsNullOrWhiteSpace(contact.Name))
            {
                _logger.LogWarning("Tentative d'envoi d'email avec des données invalides");
                return false;
            }

            var body = GenerateEmailBody(contact);

            using var message = new MailMessage();
            message.From = new MailAddress(_senderEmail, _senderName);
            message.To.Add(_senderEmail);
            message.Subject = $"Nouveau contact de {WebUtility.HtmlEncode(contact.Name)}";
            message.Body = body;
            message.IsBodyHtml = true;

            // Récupération du mot de passe depuis les variables d'environnement
            var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
            if (string.IsNullOrEmpty(emailPassword))
            {
                _logger.LogError("Le mot de passe email n'est pas configuré dans les variables d'environnement");
                return false;
            }

            using var client = new SmtpClient(_smtpServer, _smtpPort);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(_senderEmail, emailPassword);

            await client.SendMailAsync(message);
            _logger.LogInformation($"Email envoyé avec succès pour {contact.Email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erreur lors de l'envoi de l'email: {ex.Message}");
            return false;
        }
    }

    private string GenerateEmailBody(Contact contact)
    {
        // Encodage HTML pour prévenir les attaques XSS
        var encodedName = WebUtility.HtmlEncode(contact.Name);
        var encodedEmail = WebUtility.HtmlEncode(contact.Email);
        var encodedMessage = WebUtility.HtmlEncode(contact.Message);

        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #333;'>Nouveau message de contact</h2>
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px;'>
                    <p><strong>Nom:</strong> {encodedName}</p>
                    <p><strong>Email:</strong> {encodedEmail}</p>
                    <p><strong>Message:</strong></p>
                    <p style='white-space: pre-wrap;'>{encodedMessage}</p>
                </div>
                <hr style='border: 1px solid #eee; margin: 20px 0;'>
                <div style='background-color: #e9ecef; padding: 15px; border-radius: 5px;'>
                    <p style='margin: 5px 0;'><strong>Pour répondre:</strong></p>
                    <p style='margin: 5px 0;'>Email: {_senderEmail}</p>
                    {(!string.IsNullOrEmpty(_adminPhone) ? $"<p style='margin: 5px 0;'>Téléphone: {_adminPhone}</p>" : "")}
                </div>
            </div>";
    }
} 