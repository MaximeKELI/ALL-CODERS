using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Backend.Models;
using Backend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-256-bit-secret";
var jwtKeyBytes = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

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

// Configure middleware
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();

// Admin Login
app.MapPost("/api/admin/login", async (ApplicationDbContext db, [FromBody] LoginRequest login) =>
{
    var admin = await db.AdminUsers.FirstOrDefaultAsync(a => a.Username == login.Username);
    if (admin == null || !BCrypt.Net.BCrypt.Verify(login.Password, admin.PasswordHash))
        return Results.Unauthorized();

    var token = GenerateJwtToken(admin);
    admin.LastLogin = DateTime.UtcNow;
    await db.SaveChangesAsync();

    return Results.Ok(new { token });
});

// Newsletter Subscription
app.MapPost("/api/newsletter/subscribe", async (ApplicationDbContext db, [FromBody] NewsletterSubscriber subscriber) =>
{
    if (await db.NewsletterSubscribers.AnyAsync(n => n.Email == subscriber.Email))
        return Results.BadRequest(new { error = "Cette adresse email est déjà inscrite" });

    subscriber.UnsubscribeToken = Guid.NewGuid().ToString();
    db.NewsletterSubscribers.Add(subscriber);
    await db.SaveChangesAsync();

    // Envoyer email de confirmation
    // TODO: Implémenter l'envoi d'email de confirmation

    return Results.Ok(new { message = "Inscription réussie à la newsletter" });
});

// Contact endpoint with attachments
app.MapPost("/api/contact", async (ApplicationDbContext db, HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var message = new ContactMessage
    {
        Name = form["name"].ToString(),
        Email = form["email"].ToString(),
        Subject = form["subject"].ToString(),
        Message = form["message"].ToString()
    };

    if (string.IsNullOrEmpty(message.Name))
        return Results.BadRequest(new { error = "Le nom est requis" });
        
    if (string.IsNullOrEmpty(message.Email))
        return Results.BadRequest(new { error = "L'email est requis" });
        
    if (string.IsNullOrEmpty(message.Message) || message.Message.Length < 10)
        return Results.BadRequest(new { error = "Le message doit contenir au moins 10 caractères" });

    try
    {
        // Handle attachment
        var file = form.Files.GetFile("attachment");
        if (file != null)
        {
            var uploadPath = Path.Combine("wwwroot", "uploads");
            Directory.CreateDirectory(uploadPath);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            message.AttachmentPath = $"/uploads/{fileName}";
        }

        // Save to database
        message.CreatedAt = DateTime.UtcNow;
        db.ContactMessages.Add(message);
        await db.SaveChangesAsync();

        // Send email notification
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
                <p><strong>Sujet:</strong> {message.Subject}</p>
                <p><strong>Message:</strong></p>
                <p>{message.Message}</p>
                <p><small>Envoyé le {message.CreatedAt:dd/MM/yyyy HH:mm}</small></p>"
        };

        if (!string.IsNullOrEmpty(message.AttachmentPath))
        {
            var attachment = bodyBuilder.Attachments.Add(message.AttachmentPath);
        }
        
        emailMessage.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync("presidentnetero01@gmail.com", app.Configuration["EmailSettings:Password"]);
        await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);

        // Send auto-response
        var autoResponse = await db.AutoResponses.FirstOrDefaultAsync(ar => ar.IsActive);
        if (autoResponse != null)
        {
            var responseEmail = new MimeMessage();
            responseEmail.From.Add(new MailboxAddress("ALL-CODERS", "noreply@votresite.com"));
            responseEmail.To.Add(new MailboxAddress(message.Name, message.Email));
            responseEmail.Subject = autoResponse.Subject;
            
            var responseBody = autoResponse.Template.Replace("{Name}", message.Name);
            responseEmail.Body = new TextPart("html") { Text = responseBody };

            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("presidentnetero01@gmail.com", app.Configuration["EmailSettings:Password"]);
            await client.SendAsync(responseEmail);
            await client.DisconnectAsync(true);
        }

        return Results.Ok(new { message = "Message envoyé avec succès!" });
    }
    catch (Exception ex)
    {
        app.Logger.LogError($"Erreur lors de l'envoi du message: {ex.Message}");
        return Results.BadRequest(new { error = "Une erreur est survenue lors de l'envoi du message." });
    }
});

// Share statistics
app.MapPost("/api/share", async (ApplicationDbContext db, [FromBody] ShareStatistics share) =>
{
    var existing = await db.ShareStatistics
        .FirstOrDefaultAsync(s => s.Platform == share.Platform && s.Url == share.Url);

    if (existing != null)
    {
        existing.ShareCount++;
        existing.LastShared = DateTime.UtcNow;
    }
    else
    {
        share.ShareCount = 1;
        db.ShareStatistics.Add(share);
    }

    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Partage enregistré" });
});

// Protected admin endpoints
app.MapGet("/api/admin/messages", [Authorize] async (ApplicationDbContext db) =>
    await db.ContactMessages
        .OrderByDescending(m => m.CreatedAt)
        .ToListAsync());

app.MapGet("/api/admin/subscribers", [Authorize] async (ApplicationDbContext db) =>
    await db.NewsletterSubscribers
        .OrderByDescending(s => s.SubscribedAt)
        .ToListAsync());

app.MapGet("/api/admin/statistics", [Authorize] async (ApplicationDbContext db) =>
{
    var stats = new
    {
        TotalMessages = await db.ContactMessages.CountAsync(),
        TotalSubscribers = await db.NewsletterSubscribers.CountAsync(),
        TotalShares = await db.ShareStatistics.SumAsync(s => s.ShareCount),
        RecentMessages = await db.ContactMessages
            .OrderByDescending(m => m.CreatedAt)
            .Take(5)
            .ToListAsync(),
        PopularShares = await db.ShareStatistics
            .OrderByDescending(s => s.ShareCount)
            .Take(5)
            .ToListAsync()
    };
    return Results.Ok(stats);
});

// Configure port
app.Urls.Add("http://localhost:8000");

app.Run();

// Helper method for generating JWT tokens
string GenerateJwtToken(AdminUser admin)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "your-256-bit-secret");
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Role, admin.Role)
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}

// Request Models
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

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
