namespace Avenue804.Web.Domain;

/// <summary>
/// Market availability of a published listing — drives the public status badge
/// (e.g. "Under Offer", "Rented", "Sold"). Independent of the approval workflow
/// (<see cref="ListingApprovalStatus"/>) and of <c>IsPublished</c>.
/// </summary>
public enum ListingAvailabilityStatus
{
    Available = 0,
    UnderOffer = 1,
    Rented = 2,
    Sold = 3
}

/// <summary>
/// Current physical occupancy of the property. Primarily for internal/admin
/// tracking; can optionally be surfaced publicly.
/// </summary>
public enum OccupancyStatus
{
    Vacant = 0,
    Occupied = 1,
    OwnerOccupied = 2
}
