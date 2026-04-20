using System.Globalization;
using System.Text.Json;
using Avenue804.Web.Domain;

namespace Avenue804.Web.Infrastructure;

public static class ListingFormat
{
    private const string FallbackImage =
        "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=1200&q=80";

    public static string PricePrimary(decimal? price, string currency, ListingOfferType offerType)
    {
        if (price is null)
            return "Price on request";

        var cur = string.IsNullOrWhiteSpace(currency) ? "AED" : currency.Trim();
        var amount = price.Value.ToString("N0", CultureInfo.InvariantCulture);
        return $"{cur} {amount}";
    }

    public static bool ShowRentSuffix(ListingOfferType offerType) => offerType == ListingOfferType.Rent;

    public static string ListingImageUrl(string? mainImageUrl) =>
        string.IsNullOrWhiteSpace(mainImageUrl) ? FallbackImage : mainImageUrl.Trim();

    public static IReadOnlyList<string> GalleryUrls(PropertyListing listing)
    {
        var list = new List<string>();
        var main = ListingImageUrl(listing.MainImageUrl);
        list.Add(main);

        if (!string.IsNullOrWhiteSpace(listing.GalleryImagesJson))
        {
            try
            {
                var extras = JsonSerializer.Deserialize<string[]>(listing.GalleryImagesJson);
                if (extras != null)
                    foreach (var url in extras)
                        if (!string.IsNullOrWhiteSpace(url) && url.Trim() != main)
                            list.Add(url.Trim());
            }
            catch { /* ignore malformed JSON */ }
        }

        return list;
    }
}
