namespace Avenue804.Web.Infrastructure;

/// <summary>
/// Single source of truth for property amenities/features/parking options.
/// Used by the admin listing form (grouped checkboxes) and the public listing
/// detail page (code → label rendering). Codes are stored comma-separated in
/// <see cref="Domain.PropertyListing.AmenitiesJson"/>.
/// </summary>
public static class AmenityCatalog
{
    public sealed record Amenity(string Code, string Label);
    public sealed record Group(string Title, IReadOnlyList<Amenity> Items);

    public static readonly IReadOnlyList<Group> Groups = new[]
    {
        new Group("Community / Building Amenities", new Amenity[]
        {
            new("pool", "Shared Swimming Pool"),
            new("gym", "Gym / Fitness Center"),
            new("kids", "Children's Play Area"),
            new("gardens", "Landscaped Gardens"),
            new("bbq", "BBQ Area"),
            new("security", "24/7 Security"),
            new("cctv", "CCTV"),
            new("lobby", "Lobby / Reception Area"),
            new("parks", "Community Parks"),
            new("retail", "Retail Shops"),
            new("sports", "Sports Facilities"),
            new("concierge", "Concierge Service"),
            new("beach", "Beach Access"),
        }),
        new Group("Property Features", new Amenity[]
        {
            new("central_ac", "Central Air Conditioning"),
            new("wardrobes", "Built-in Wardrobes"),
            new("fitted_kitchen", "Fully Fitted Kitchen"),
            new("balcony", "Balcony / Terrace"),
            new("modern_finish", "Modern Finishing"),
            new("kitchen", "Open-Plan Kitchen"),
            new("closed_kitchen", "Closed Kitchen"),
            new("floor_windows", "Floor-to-Ceiling Windows"),
            new("maid", "Maid's Room"),
            new("driver", "Driver's Room"),
            new("smart_home", "Smart Home System"),
            new("quality_finish", "High-Quality Finishes"),
            new("laundry", "Laundry Room"),
            new("storage", "Storage Room"),
            new("upgraded", "Upgraded Interiors"),
            new("private_garden", "Private Garden"),
            new("private_pool", "Private Swimming Pool"),
        }),
        new Group("Parking Options", new Amenity[]
        {
            new("garage", "Private Garage"),
            new("parking", "Covered Parking"),
            new("basement_parking", "Basement Parking"),
            new("dedicated_parking", "Dedicated Parking Space"),
            new("visitor_parking", "Visitor Parking"),
            new("multilevel_parking", "Multi-level Parking"),
            new("ev_charging", "EV Charging Station"),
        }),
    };

    public static readonly IReadOnlyDictionary<string, string> Labels =
        Groups.SelectMany(g => g.Items).ToDictionary(a => a.Code, a => a.Label);

    // Legacy codes from the previous flat amenity list — kept so older listings
    // still render readable labels even though these are no longer selectable.
    private static readonly IReadOnlyDictionary<string, string> Legacy = new Dictionary<string, string>
    {
        ["pets"] = "Pets Allowed",
        ["view"] = "Sea / City View",
        ["furnished"] = "Fully Furnished",
        ["study"] = "Study / Home Office",
        ["lift"] = "Elevator / Lift",
    };

    public static string LabelFor(string code) =>
        Labels.TryGetValue(code, out var l) ? l
        : Legacy.TryGetValue(code, out var ll) ? ll
        : code;
}
