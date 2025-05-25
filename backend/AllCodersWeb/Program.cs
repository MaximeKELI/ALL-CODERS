using AllCodersWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IEmailService, EmailService>();

// Configuration CORS plus sécurisée
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? 
    new[] { "http://localhost:5000", "https://localhost:5001" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithMethods("POST", "GET", "OPTIONS")
              .WithHeaders("Content-Type")
              .WithExposedHeaders("X-Custom-Header")
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// Ajout de la sécurité des en-têtes HTTP
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Middleware de sécurité
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["X-XSS-Protection"] = "1; mode=block";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("DefaultPolicy");

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Déplacer tous les fichiers du frontend vers wwwroot
var sourceDir = Path.Combine(app.Environment.ContentRootPath, "..", "..", "frontend");
var targetDir = Path.Combine(app.Environment.WebRootPath);

if (Directory.Exists(sourceDir))
{
    // Copier les fichiers du frontend vers wwwroot
    CopyDirectory(sourceDir, targetDir);
}

app.Run();

void CopyDirectory(string sourceDir, string targetDir)
{
    Directory.CreateDirectory(targetDir);

    foreach (var file in Directory.GetFiles(sourceDir))
    {
        var fileName = Path.GetFileName(file);
        var destFile = Path.Combine(targetDir, fileName);
        File.Copy(file, destFile, true);
    }

    foreach (var directory in Directory.GetDirectories(sourceDir))
    {
        var dirName = Path.GetFileName(directory);
        var destDir = Path.Combine(targetDir, dirName);
        CopyDirectory(directory, destDir);
    }
}
