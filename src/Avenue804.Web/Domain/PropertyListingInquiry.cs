namespace Avenue804.Web.Domain;

/// <summary>Buyer/tenant property interest submitted from the public site (modal or page).</summary>
public class PropertyListingInquiry
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>Legacy field; no longer on public form.</summary>
    public int? IAmLookupValueId { get; set; }
    public LookupValue? IAmLookup { get; set; }

    public int? WantToLookupValueId { get; set; }
    public LookupValue? WantToLookup { get; set; }

    public int? PropertyTypeLookupValueId { get; set; }
    public LookupValue? PropertyTypeLookup { get; set; }

    public int? PropertyDetailLookupValueId { get; set; }
    public LookupValue? PropertyDetailLookup { get; set; }

    /// <summary>Legacy short note; superseded by <see cref="RequirementDetails"/>.</summary>
    public string? OtherDetails { get; set; }

    /// <summary>Describe needs, timeline, must-haves, etc.</summary>
    public string? RequirementDetails { get; set; }

    public int? BudgetLookupValueId { get; set; }
    public LookupValue? BudgetLookup { get; set; }

    /// <summary>Legacy Emirates dropdown; no longer on public form.</summary>
    public int? LocationLookupValueId { get; set; }
    public LookupValue? LocationLookup { get; set; }

    public string? Area { get; set; }

    public DateOnly? ExpectedMoveInDate { get; set; }

    public InquiryStatus Status { get; set; } = InquiryStatus.New;

    // Agent CRM
    public string? AgentNote { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
