using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Submit;

public class RatingModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public RatingModel(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(
        int listingId,
        [Range(1, 5)] int stars,
        [StringLength(2000)] string? review,
        string? returnUrl = null,
        CancellationToken ct = default)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            TempData["ToastError"] = "Please sign in to leave a review.";
            return RedirectToPage("/Account/Login");
        }

        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (stars < 1 || stars > 5) { TempData["ToastError"] = "Invalid rating."; return Redirect(returnUrl ?? "/"); }

        var existing = await _db.PropertyRatings
            .FirstOrDefaultAsync(r => r.UserId == user.Id && r.ListingId == listingId, ct);

        if (existing != null)
        {
            existing.Stars = stars;
            existing.Review = review?.Trim();
            existing.IsApproved = false;
            existing.CreatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            _db.PropertyRatings.Add(new PropertyRating
            {
                UserId = user.Id,
                ListingId = listingId,
                Stars = stars,
                Review = review?.Trim(),
                IsApproved = false
            });
        }

        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Thank you for your review! It will appear after moderation.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        var listing = await _db.PropertyListings.FindAsync(new object[] { listingId }, ct);
        return RedirectToPage("/Properties/Detail", new { slug = listing?.Slug });
    }
}
