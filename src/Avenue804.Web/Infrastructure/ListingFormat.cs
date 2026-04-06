using System.Globalization;
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
}
