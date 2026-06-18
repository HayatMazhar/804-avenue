using System.Text;
using System.Text.Json;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Listings;

[Authorize(Policy = "AdminAccess")]
public class BulkImportModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public BulkImportModel(ApplicationDbContext db) => _db = db;

    public ImportResults? Results { get; set; }
    public bool IsDryRun { get; set; }

    public class ImportResults
    {
        public int Created { get; set; }
        public int Skipped { get; set; }
        public List<ImportError> Errors { get; } = [];
    }

    public class ImportError
    {
        public int Row { get; set; }
        public string Message { get; set; } = "";
        public string Raw { get; set; } = "";
    }

    public IActionResult OnGet(string? download)
    {
        ViewData["AdminSection"] = "listings";

        if (download == "template")
        {
            var csv = new StringBuilder();
            csv.AppendLine("Title,OfferType,Price,Currency,Location,Description,Beds,Baths,AreaSqft,MainImageUrl,GalleryUrls,Label,IsOffPlan,HandoverDate,PaymentPlan,IsVerified,IsPublished,Amenities");
            csv.AppendLine("\"Luxury 2BR Apartment Al Reem\",Sale,1500000,AED,\"Al Reem Island, Abu Dhabi\",\"Spacious apartment with sea views\",2,2,1200,https://example.com/photo1.jpg,https://example.com/photo2.jpg|https://example.com/photo3.jpg,,false,,,false,false,\"pool,gym,parking\"");
            csv.AppendLine("\"Office Space Business Bay\",Rent,120000,AED,\"Business Bay, Dubai\",,,,1500,,,,false,,,false,true,");
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "804-avenue-listings-template.csv");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? csvFile, bool dryRun, CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "listings";
        IsDryRun = dryRun;

        if (csvFile == null || csvFile.Length == 0)
        {
            ModelState.AddModelError("", "Please select a CSV file.");
            return Page();
        }

        Results = new ImportResults();
        var lines = new List<string>();

        using var reader = new StreamReader(csvFile.OpenReadStream(), Encoding.UTF8);
        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
            lines.Add(line);

        if (lines.Count < 2)
        {
            ModelState.AddModelError("", "CSV is empty or has no data rows.");
            return Page();
        }

        // Parse header
        var header = ParseCsvLine(lines[0]);
        var colIdx = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < header.Count; i++)
            colIdx[header[i].Trim()] = i;

        string? Get(List<string> cols, string name)
        {
            if (!colIdx.TryGetValue(name, out var idx)) return null;
            return idx < cols.Count ? cols[idx].Trim() : null;
        }

        var toCreate = new List<PropertyListing>();
        var maxRows = Math.Min(lines.Count - 1, 500);

        for (int i = 1; i <= maxRows; i++)
        {
            var rowLine = lines[i];
            if (string.IsNullOrWhiteSpace(rowLine)) { Results.Skipped++; continue; }

            var cols = ParseCsvLine(rowLine);
            var title = Get(cols, "Title");
            var offerTypeStr = Get(cols, "OfferType");

            if (string.IsNullOrWhiteSpace(title))
            {
                Results.Errors.Add(new ImportError { Row = i + 1, Message = "Title is required.", Raw = rowLine[..Math.Min(80, rowLine.Length)] });
                continue;
            }
            if (!Enum.TryParse<ListingOfferType>(offerTypeStr, ignoreCase: true, out var offerType))
            {
                Results.Errors.Add(new ImportError { Row = i + 1, Message = $"Invalid OfferType '{offerTypeStr}'. Use Sale or Rent.", Raw = rowLine[..Math.Min(80, rowLine.Length)] });
                continue;
            }

            decimal? price = null;
            if (decimal.TryParse(Get(cols, "Price"), out var p)) price = p;
            int? beds = null, baths = null, areaSqft = null;
            if (int.TryParse(Get(cols, "Beds"), out var b)) beds = b;
            if (int.TryParse(Get(cols, "Baths"), out var ba)) baths = ba;
            if (int.TryParse(Get(cols, "AreaSqft"), out var a)) areaSqft = a;
            bool.TryParse(Get(cols, "IsOffPlan"), out var isOffPlan);
            bool.TryParse(Get(cols, "IsVerified"), out var isVerified);
            bool.TryParse(Get(cols, "IsPublished"), out var isPublished);

            // Gallery URLs from pipe-separated
            var galleryRaw = Get(cols, "GalleryUrls");
            string? galleryJson = null;
            if (!string.IsNullOrWhiteSpace(galleryRaw))
            {
                var urls = galleryRaw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(u => u.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToArray();
                if (urls.Length > 0) galleryJson = JsonSerializer.Serialize(urls);
            }

            // Amenities
            var amenitiesRaw = Get(cols, "Amenities");
            string? amenitiesJson = null;
            if (!string.IsNullOrWhiteSpace(amenitiesRaw))
                amenitiesJson = amenitiesRaw.Trim().Trim('"');

            var slug = SlugGenerator.FromTitle(title);
            // Ensure unique slug
            var slugBase = slug;
            var attempt = 0;
            while (toCreate.Any(x => x.Slug == slug) || await _db.PropertyListings.AnyAsync(x => x.Slug == slug, ct))
            {
                attempt++;
                slug = $"{slugBase}-{attempt}";
            }

            toCreate.Add(new PropertyListing
            {
                Title = title.Trim(),
                Slug = slug,
                OfferType = offerType,
                Price = price,
                Currency = Get(cols, "Currency")?.Trim() is { Length: > 0 } c ? c : "AED",
                Location = Get(cols, "Location")?.Trim(),
                Description = Get(cols, "Description")?.Trim(),
                Beds = beds, Baths = baths, AreaSqft = areaSqft,
                MainImageUrl = Get(cols, "MainImageUrl")?.Trim(),
                GalleryImagesJson = galleryJson,
                Label = Get(cols, "Label")?.Trim(),
                IsOffPlan = isOffPlan,
                HandoverDate = Get(cols, "HandoverDate")?.Trim(),
                PaymentPlan = Get(cols, "PaymentPlan")?.Trim(),
                IsVerified = isVerified,
                IsPublished = isPublished,
                AmenitiesJson = amenitiesJson,
                ApprovalStatus = isPublished ? ListingApprovalStatus.Approved : ListingApprovalStatus.Draft,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        Results.Created = toCreate.Count;

        if (!dryRun && toCreate.Count > 0)
        {
            _db.PropertyListings.AddRange(toCreate);
            await _db.SaveChangesAsync(ct);
        }

        TempData["ToastOk"] = dryRun
            ? $"Dry run: {Results.Created} rows valid, {Results.Errors.Count} errors."
            : $"Imported {Results.Created} listings, {Results.Errors.Count} errors.";

        return Page();
    }

    // Proper CSV parser that handles quoted fields
    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var cur = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                { cur.Append('"'); i++; }
                else inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            { result.Add(cur.ToString()); cur.Clear(); }
            else cur.Append(c);
        }
        result.Add(cur.ToString());
        return result;
    }
}
