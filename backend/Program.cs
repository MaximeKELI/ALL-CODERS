using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

// Add email configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Add SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

// Configure CORS
app.UseCors("AllowAll");

// Serve static files
app.UseDefaultFiles();
app.UseStaticFiles();

// Contact endpoint
app.MapPost("/api/contact", async (ApplicationDbContext db, [FromBody] ContactMessage message) =>
{
    if (string.IsNullOrEmpty(message.Name))
        return Results.BadRequest(new { error = "Le nom est requis" });
        
    if (string.IsNullOrEmpty(message.Email))
        return Results.BadRequest(new { error = "L'email est requis" });
        
    if (string.IsNullOrEmpty(message.Message) || message.Message.Length < 10)
        return Results.BadRequest(new { error = "Le message doit contenir au moins 10 caractères" });

    try
    {
        // Save to database
        message.CreatedAt = DateTime.UtcNow;
        db.ContactMessages.Add(message);
        await db.SaveChangesAsync();

        // Send email
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Contact Form", "noreply@votresite.com"));
        emailMessage.To.Add(new MailboxAddress("Admin", "presidentnetero01@gmail.com"));
        emailMessage.Subject = $"Nouveau message de {message.Name}";
        
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
                <h2>Nouveau message de contact</h2>
                <p><strong>Nom:</strong> {message.Name}</p>
                <p><strong>Email:</strong> {message.Email}</p>
                <p><strong>Message:</strong></p>
                <p>{message.Message}</p>
                <p><small>Envoyé le {message.CreatedAt:dd/MM/yyyy HH:mm}</small></p>"
        };
        
        emailMessage.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync("presidentnetero01@gmail.com", app.Configuration["EmailSettings:Password"]);
        await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);

        // Log the message
        app.Logger.LogInformation($"Message reçu de {message.Name} ({message.Email}): {message.Message}");

        return Results.Ok(new { message = "Message envoyé avec succès!" });
    }
    catch (Exception ex)
    {
        app.Logger.LogError($"Erreur lors de l'envoi du message: {ex.Message}");
        return Results.BadRequest(new { error = "Une erreur est survenue lors de l'envoi du message." });
    }
});

// Configure port
app.Urls.Add("http://localhost:8000");

app.Run();

// Models
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
}

// Email Settings
public class EmailSettings
{
    public string Password { get; set; } = string.Empty;
}

// DbContext
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ContactMessage> ContactMessages { get; set; }
}
