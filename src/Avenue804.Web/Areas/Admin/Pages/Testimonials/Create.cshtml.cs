using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages.Testimonials;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public TestimonialForm Form { get; set; } = new();

    public void OnGet() { ViewData["AdminSection"] = "testimonials"; }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return Page();
        _db.Testimonials.Add(new Testimonial
        {
            AuthorName = Form.AuthorName.Trim(),
            AuthorRole = Form.AuthorRole?.Trim(),
            Quote = Form.Quote.Trim(),
            Stars = Form.Stars,
            SortOrder = Form.SortOrder,
            IsActive = Form.IsActive
        });
        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Testimonial saved.";
        return RedirectToPage("./Index");
    }

    public class TestimonialForm
    {
        [Required, StringLength(200)] public string AuthorName { get; set; } = string.Empty;
        [StringLength(200)] public string? AuthorRole { get; set; }
        [Required, StringLength(2000)] public string Quote { get; set; } = string.Empty;
        [Range(1, 5)] public int Stars { get; set; } = 5;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
