using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AllCodersWeb.Models;
using AllCodersWeb.Services;

namespace AllCodersWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEmailService _emailService;

    public HomeController(ILogger<HomeController> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [Route("/api/contact")]
    public async Task<IActionResult> Contact([FromBody] Contact contact)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ContactResponse 
            { 
                Success = false, 
                Message = "Données invalides" 
            });
        }

        var success = await _emailService.SendContactEmailAsync(contact);
        
        if (success)
        {
            return Ok(new ContactResponse 
            { 
                Success = true, 
                Message = "Message envoyé avec succès" 
            });
        }

        return BadRequest(new ContactResponse 
        { 
            Success = false, 
            Message = "Erreur lors de l'envoi du message" 
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
} 