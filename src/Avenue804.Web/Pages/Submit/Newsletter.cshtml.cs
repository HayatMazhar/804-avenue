using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Submit;

[EnableRateLimiting("submit")]
public class NewsletterModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public NewsletterModel(ApplicationDbContext db) => _db = db;

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(
        [Required, EmailAddress] string email,
        string? name,
        string? preferences,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(email))
        {
            TempData["ToastError"] = "Please enter a valid email address.";
            var referer = Request.Headers.Referer.ToString();
            return !string.IsNullOrEmpty(referer) && Url.IsLocalUrl(referer) ? LocalRedirect(referer) : RedirectToPage("/Index");
        }

        var exists = await _db.NewsletterSubscribers.AnyAsync(s => s.Email == email.Trim().ToLower(), ct);
        if (!exists)
        {
            _db.NewsletterSubscribers.Add(new NewsletterSubscriber
            {
                Email = email.Trim().ToLower(),
                Name = name?.Trim(),
                Preferences = preferences?.Trim()
            });
            await _db.SaveChangesAsync(ct);
        }

        TempData["ToastOk"] = "You're subscribed! We'll send property alerts to your inbox.";
        var back = Request.Headers.Referer.ToString();
        return !string.IsNullOrEmpty(back) && Url.IsLocalUrl(back) ? LocalRedirect(back) : RedirectToPage("/Index");
    }
}
