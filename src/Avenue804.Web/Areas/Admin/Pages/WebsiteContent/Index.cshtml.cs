using Avenue804.Web.Configuration;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Avenue804.Web.Areas.Admin.Pages.WebsiteContent;

/// <summary>
/// Page-by-page CMS editor. Every public-facing surface (Home, Services hub,
/// service detail pages, About, etc.) gets its own tab containing every
/// CMS-overridable slug that page actually consumes. Admins should never have
/// to look up a slug by name — they pick the page and edit each labelled field.
/// </summary>
[Authorize(Policy = "AdminAccess")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public IndexModel(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public Dictionary<string, string> BlockBodies { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, SiteSetting> Settings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Content blocks present in the DB whose slug isn't currently wired to a
    /// page tab. Surfaced as a fallback "Other" tab so admins can edit
    /// everything, even legacy or experimental copy that hasn't been mapped to
    /// a public page yet.
    /// </summary>
    public List<ContentField> OrphanFields { get; set; } = [];

    public record ContentField(string Slug, string Label, bool IsTextarea = false, string? Hint = null);

    /// <summary>A logical "page" tab containing one or more editable fields.</summary>
    public record ContentPage(
        string Id,           // tab id (used in URL fragment)
        string Title,        // tab label
        string Description,  // shown above the form
        string? PreviewUrl,  // public URL to "Open page →"
        List<ContentField> Fields);

    /// <summary>
    /// Every public page → its CMS-overridable fields, in the order they appear
    /// on that page. Wire any new ContentBlockSlug here; the editor picks it up
    /// automatically, no markup changes required.
    /// </summary>
    public static readonly List<ContentPage> ContentPages =
    [
        new("page-home", "Home page",
            "Hero, service cards, stats, testimonials, CTA — the entire / landing page.",
            "/",
            [
                new(ContentBlockSlugs.HomeHeroEyebrow,    "Hero — eyebrow"),
                new(ContentBlockSlugs.HomeHeroTitle,      "Hero — title (HTML allowed)", true, "Use <em>…</em> for the orange accent."),
                new(ContentBlockSlugs.HomeHeroHeadline,   "Hero — headline (alt)"),
                new(ContentBlockSlugs.HomeHeroSubtitle,   "Hero — subtitle", true),
                new(ContentBlockSlugs.HomeHeroPropertiesLink, "Hero — properties link label"),
                new(ContentBlockSlugs.HomeServiceCardMaintLead, "Service card — Maintenance lead", true),
                new(ContentBlockSlugs.HomeServiceCardAmcLead,   "Service card — AMC lead", true),
                new(ContentBlockSlugs.HomeServiceCardContrLead, "Service card — Contracting lead", true),
                new(ContentBlockSlugs.HomeServiceCardFmLead,    "Service card — Facility Mgmt lead", true),
                new(ContentBlockSlugs.HomeAboutTag,       "About strip — tag"),
                new(ContentBlockSlugs.HomeAboutTitle,     "About strip — title"),
                new(ContentBlockSlugs.AboutStripLead,     "About strip — lead", true),
                new(ContentBlockSlugs.HomeAboutCards,     "About strip — cards (JSON)", true, "Optional structured JSON."),
                new(ContentBlockSlugs.HomeStatsItems,     "Stats strip — items (JSON)", true),
                new(ContentBlockSlugs.HomeReTag,          "Real estate strip — tag"),
                new(ContentBlockSlugs.HomeReTitle,        "Real estate strip — title"),
                new(ContentBlockSlugs.HomeReCards,        "Real estate strip — cards (JSON)", true),
                new(ContentBlockSlugs.HomeContractingTag, "Contracting strip — tag"),
                new(ContentBlockSlugs.HomeContractingTitle, "Contracting strip — title"),
                new(ContentBlockSlugs.HomeContractingLead,  "Contracting strip — lead", true),
                new(ContentBlockSlugs.HomeContractingCards, "Contracting strip — cards (JSON)", true),
                new(ContentBlockSlugs.HomeMaintenanceTag,   "Maintenance strip — tag"),
                new(ContentBlockSlugs.HomeMaintenanceTitle, "Maintenance strip — title"),
                new(ContentBlockSlugs.HomeMaintenanceLead,  "Maintenance strip — lead", true),
                new(ContentBlockSlugs.HomeMaintenanceCards, "Maintenance strip — cards (JSON)", true),
                new(ContentBlockSlugs.HomeWhyTag,    "Why-us strip — tag"),
                new(ContentBlockSlugs.HomeWhyTitle,  "Why-us strip — title"),
                new(ContentBlockSlugs.HomeWhyItems,  "Why-us strip — items (JSON)", true),
                new(ContentBlockSlugs.HomeTestiTag,   "Testimonials — tag"),
                new(ContentBlockSlugs.HomeTestiTitle, "Testimonials — title"),
                new(ContentBlockSlugs.HomeTestiItems, "Testimonials — items (JSON)", true),
                new(ContentBlockSlugs.HomeCtaTitle,   "Footer CTA — title"),
            ]),

        new("page-services", "Services hub",
            "The /Services landing page — hero, lead-in, and what every visitor sees first.",
            "/Services",
            [
                new(ContentBlockSlugs.PageServicesHubEyebrow,  "Hero — eyebrow"),
                new(ContentBlockSlugs.PageServicesHubTitle,    "Hero — title (HTML allowed)", true, "Use <em>…</em> for the orange accent."),
                new(ContentBlockSlugs.PageServicesHubSubtitle, "Hero — subtitle", true),
                new(ContentBlockSlugs.PageServicesHubLead,     "Cards section — lead", true),
            ]),

        new("page-about", "About",
            "Story, vision, mission, values, expertise and team copy on /About.",
            "/About",
            [
                new(ContentBlockSlugs.PageAboutHeroTitle,    "Hero — title"),
                new(ContentBlockSlugs.PageAboutHeroSubtitle, "Hero — subtitle", true),
                new(ContentBlockSlugs.AboutStoryTag,    "Story section — tag"),
                new(ContentBlockSlugs.AboutStoryTitle,  "Story section — title"),
                new(ContentBlockSlugs.AboutStoryBody,   "Story section — body", true),
                new(ContentBlockSlugs.AboutVmTag,       "Vision/mission — tag"),
                new(ContentBlockSlugs.AboutVisionBody,  "Vision statement", true),
                new(ContentBlockSlugs.AboutMissionBody, "Mission statement", true),
                new(ContentBlockSlugs.AboutValuesTag,   "Values — tag"),
                new(ContentBlockSlugs.AboutValuesItems, "Values — items (JSON)", true),
                new(ContentBlockSlugs.AboutExpertiseTag,   "Expertise — tag"),
                new(ContentBlockSlugs.AboutExpertiseLead,  "Expertise — lead", true),
                new(ContentBlockSlugs.AboutExpertiseItems, "Expertise — items (JSON)", true),
                new(ContentBlockSlugs.AboutTeamLead,    "Team — lead", true),
                new(ContentBlockSlugs.AboutTeamItems,   "Team — items (JSON)", true),
                new(ContentBlockSlugs.AboutWcuTag,      "Why choose us — tag"),
                new(ContentBlockSlugs.AboutWcuItems,    "Why choose us — items (JSON)", true),
            ]),

        new("page-contracting", "Contracting",
            "Service detail page at /Contracting — hero, individual services, process and CTA.",
            "/Contracting",
            [
                new(ContentBlockSlugs.PageContractingHeroTitle,    "Hero — title"),
                new(ContentBlockSlugs.PageContractingHeroSubtitle, "Hero — subtitle", true),
                new(ContentBlockSlugs.ContractingSvc1Title, "Service 1 — title"),
                new(ContentBlockSlugs.ContractingSvc1Body,  "Service 1 — body", true),
                new(ContentBlockSlugs.ContractingSvc2Title, "Service 2 — title"),
                new(ContentBlockSlugs.ContractingSvc2Body,  "Service 2 — body", true),
                new(ContentBlockSlugs.ContractingSvc3Title, "Service 3 — title"),
                new(ContentBlockSlugs.ContractingSvc3Body,  "Service 3 — body", true),
                new(ContentBlockSlugs.ContractingSvc4Title, "Service 4 — title"),
                new(ContentBlockSlugs.ContractingSvc4Body,  "Service 4 — body", true),
                new(ContentBlockSlugs.ContractingSvc5Title, "Service 5 — title"),
                new(ContentBlockSlugs.ContractingSvc5Body,  "Service 5 — body", true),
                new(ContentBlockSlugs.ContractingSvc6Title, "Service 6 — title"),
                new(ContentBlockSlugs.ContractingSvc6Body,  "Service 6 — body", true),
                new(ContentBlockSlugs.ContractingSvc7Title, "Service 7 — title"),
                new(ContentBlockSlugs.ContractingSvc7Body,  "Service 7 — body", true),
                new(ContentBlockSlugs.ContractingProcessTitle, "Process — title"),
                new(ContentBlockSlugs.ContractingProcessLead,  "Process — lead", true),
                new(ContentBlockSlugs.ContractingProcessSteps, "Process — steps (JSON)", true),
                new(ContentBlockSlugs.ContractingMaterialsItems, "Materials — items (JSON)", true),
                new(ContentBlockSlugs.ContractingCtaTitle, "CTA — title"),
                new(ContentBlockSlugs.ContractingCtaBody,  "CTA — body", true),
            ]),

        new("page-maintenance", "Maintenance",
            "Service detail page at /Maintenance — hero, individual services, AMC explainer and CTA.",
            "/Maintenance",
            [
                new(ContentBlockSlugs.PageMaintenanceHeroTitle,    "Hero — title"),
                new(ContentBlockSlugs.PageMaintenanceHeroSubtitle, "Hero — subtitle", true),
                new(ContentBlockSlugs.MaintenanceSvc1Title, "Service 1 — title"),
                new(ContentBlockSlugs.MaintenanceSvc1Body,  "Service 1 — body", true),
                new(ContentBlockSlugs.MaintenanceSvc2Title, "Service 2 — title"),
                new(ContentBlockSlugs.MaintenanceSvc2Body,  "Service 2 — body", true),
                new(ContentBlockSlugs.MaintenanceSvc3Title, "Service 3 — title"),
                new(ContentBlockSlugs.MaintenanceSvc3Body,  "Service 3 — body", true),
                new(ContentBlockSlugs.MaintenanceSvc4Title, "Service 4 — title"),
                new(ContentBlockSlugs.MaintenanceSvc4Body,  "Service 4 — body", true),
                new(ContentBlockSlugs.MaintenanceSvc5Title, "Service 5 — title"),
                new(ContentBlockSlugs.MaintenanceSvc5Body,  "Service 5 — body", true),
                new(ContentBlockSlugs.MaintenanceWhyItems,  "Why-us — items (JSON)", true),
                new(ContentBlockSlugs.MaintenanceContractTitle,    "AMC strip — title"),
                new(ContentBlockSlugs.MaintenanceContractBody,     "AMC strip — body", true),
                new(ContentBlockSlugs.MaintenanceContractFeatures, "AMC strip — features (JSON)", true),
                new(ContentBlockSlugs.MaintenanceCtaTitle, "CTA — title"),
                new(ContentBlockSlugs.MaintenanceCtaBody,  "CTA — body", true),
            ]),

        new("page-facility", "Facility Management",
            "Service detail page at /FacilityManagement — hero, services, why-FM and CTA.",
            "/FacilityManagement",
            [
                new(ContentBlockSlugs.PageFacilityHeroTitle,    "Hero — title"),
                new(ContentBlockSlugs.PageFacilityHeroSubtitle, "Hero — subtitle", true),
                new(ContentBlockSlugs.FacilitySvc1Title, "Service 1 — title"),
                new(ContentBlockSlugs.FacilitySvc1Body,  "Service 1 — body", true),
                new(ContentBlockSlugs.FacilitySvc2Title, "Service 2 — title"),
                new(ContentBlockSlugs.FacilitySvc2Body,  "Service 2 — body", true),
                new(ContentBlockSlugs.FacilityWhyTag,    "Why FM — tag"),
                new(ContentBlockSlugs.FacilityWhyTitle,  "Why FM — title"),
                new(ContentBlockSlugs.FacilityWhyItems,  "Why FM — items (JSON)", true),
                new(ContentBlockSlugs.FacilityWhyCard1Title, "Why FM — card 1 title"),
                new(ContentBlockSlugs.FacilityWhyCard1Body,  "Why FM — card 1 body", true),
                new(ContentBlockSlugs.FacilityWhyCard2Title, "Why FM — card 2 title"),
                new(ContentBlockSlugs.FacilityWhyCard2Body,  "Why FM — card 2 body", true),
                new(ContentBlockSlugs.FacilityWhyCard3Title, "Why FM — card 3 title"),
                new(ContentBlockSlugs.FacilityWhyCard3Body,  "Why FM — card 3 body", true),
                new(ContentBlockSlugs.FacilityFormTitle, "FM enquiry — section tag"),
                new(ContentBlockSlugs.FacilityFormBtn,   "FM enquiry — submit button"),
                new(ContentBlockSlugs.FacilityCtaTitle,  "CTA — title"),
                new(ContentBlockSlugs.FacilityCtaBody,   "CTA — body", true),
            ]),

        new("page-contact", "Contact",
            "Contact page hero, address rows, business hours, form labels and channels.",
            "/Contact",
            [
                new(ContentBlockSlugs.MetaContactTitle, "Meta — title"),
                new(ContentBlockSlugs.MetaContactDescription, "Meta — description", true),
                new(ContentBlockSlugs.ContactEyebrow, "Hero — eyebrow"),
                new(ContentBlockSlugs.ContactH1, "Hero — H1 (HTML allowed)", true),
                new(ContentBlockSlugs.ContactLead, "Hero — lead", true),
                new(ContentBlockSlugs.ContactRowAddressLabel, "Row — office address"),
                new(ContentBlockSlugs.ContactRowPhoneLabel, "Row — phone label"),
                new(ContentBlockSlugs.ContactRowEmailLabel, "Row — email label"),
                new(ContentBlockSlugs.ContactRowWhatsappLabel, "Row — WhatsApp label"),
                new(ContentBlockSlugs.ContactRowWhatsappSub, "Row — WhatsApp sub"),
                new(ContentBlockSlugs.ContactHoursTitle, "Hours — title"),
                new(ContentBlockSlugs.ContactHoursBody, "Hours — body (HTML)", true),
                new(ContentBlockSlugs.ContactFormTitle, "Form — title"),
                new(ContentBlockSlugs.ContactFormSub, "Form — subtitle", true),
                new(ContentBlockSlugs.ContactFormCallout, "Form — callout (HTML)", true),
                new(ContentBlockSlugs.ContactFormSecDetails, "Form — details section"),
                new(ContentBlockSlugs.ContactFormSecMessage, "Form — message section"),
                new(ContentBlockSlugs.ContactFormBtn, "Form — submit button"),
                new(ContentBlockSlugs.ContactFormPrivacy, "Form — privacy note (HTML)", true),
                new(ContentBlockSlugs.ContactChannelsTitle, "Channels — title (HTML)", true),
                new(ContentBlockSlugs.ContactChannelsSub, "Channels — sub", true),
            ]),

        new("page-properties", "Properties",
            "Properties listing page — header, filters, sort options, empty state.",
            "/Properties",
            [
                new(ContentBlockSlugs.MetaPropertiesTitle, "Meta — title"),
                new(ContentBlockSlugs.MetaPropertiesDescription, "Meta — description", true),
                new(ContentBlockSlugs.PropertiesHeader, "Header (HTML allowed)"),
                new(ContentBlockSlugs.PropertiesTabAll, "Tab — All"),
                new(ContentBlockSlugs.PropertiesTabBuy, "Tab — Buy"),
                new(ContentBlockSlugs.PropertiesTabRent, "Tab — Rent"),
                new(ContentBlockSlugs.PropertiesEmptyTitle, "Empty state — title"),
                new(ContentBlockSlugs.PropertiesEmptyBody, "Empty state — body", true),
                new(ContentBlockSlugs.PropertiesClearFilters, "Empty state — clear filters btn"),
                new(ContentBlockSlugs.PropertiesRecentTitle, "Recent views — title"),
                new(ContentBlockSlugs.PropertiesFilterAllCategories, "Filter — All categories"),
                new(ContentBlockSlugs.PropertiesFilterAnyEmirate, "Filter — Any emirate"),
                new(ContentBlockSlugs.PropertiesFilterAllTypes, "Filter — All types"),
                new(ContentBlockSlugs.PropertiesFilterSize, "Filter — Size label"),
                new(ContentBlockSlugs.PropertiesFilterAnySize, "Filter — Any size"),
                new(ContentBlockSlugs.PropertiesFilterPrice, "Filter — Price label"),
                new(ContentBlockSlugs.PropertiesFilterAnyPrice, "Filter — Any price"),
                new(ContentBlockSlugs.PropertiesFilterFeatures, "Filter — Features label"),
                new(ContentBlockSlugs.PropertiesFilterVerifiedOnly, "Filter — Verified only"),
                new(ContentBlockSlugs.PropertiesFilterPriceReduced, "Filter — Price reduced"),
                new(ContentBlockSlugs.PropertiesFilterApply, "Filter — Apply button"),
                new(ContentBlockSlugs.PropertiesSaveSearch, "Save search button"),
                new(ContentBlockSlugs.PropertiesSortLabel, "Sort — label"),
                new(ContentBlockSlugs.PropertiesSortNewest, "Sort — Newest first"),
                new(ContentBlockSlugs.PropertiesSortPriceAsc, "Sort — Price ascending"),
                new(ContentBlockSlugs.PropertiesSortPriceDesc, "Sort — Price descending"),
                new(ContentBlockSlugs.PropertiesSortMostViewed, "Sort — Most viewed"),
                new(ContentBlockSlugs.PropertiesSortVerifiedFirst, "Sort — Verified first"),
                new(ContentBlockSlugs.PropertiesViewGrid, "View — Grid title"),
                new(ContentBlockSlugs.PropertiesViewList, "View — List title"),
                new(ContentBlockSlugs.PropertiesMapView, "View — Map button"),
                new(ContentBlockSlugs.PropertiesFiltersLabel, "Mobile — Filters label"),
                new(ContentBlockSlugs.PropertiesCountSingular, "Count — singular"),
                new(ContentBlockSlugs.PropertiesCountPlural, "Count — plural"),
            ]),

        new("page-property-detail", "Property Detail",
            "Individual property detail page — share, calculators, reviews, mortgage.",
            "/Properties",
            [
                new(ContentBlockSlugs.PropertyDetailInquireBtn, "Inquire button"),
                new(ContentBlockSlugs.PropertyDetailPrintBtn, "Print/PDF button"),
                new(ContentBlockSlugs.PropertyDetailShareLabel, "Share — label"),
                new(ContentBlockSlugs.PropertyDetailCopyLinkLabel, "Share — Copy link"),
                new(ContentBlockSlugs.PropertyDetailPriceHistTitle, "Section — Price history"),
                new(ContentBlockSlugs.PropertyDetailAmenitiesTitle, "Section — Amenities"),
                new(ContentBlockSlugs.PropertyDetailFloorplanTitle, "Section — Floor plan"),
                new(ContentBlockSlugs.PropertyDetailFloorplanLink, "Floor plan — open link"),
                new(ContentBlockSlugs.PropertyDetailVtourTitle, "Section — Virtual tour"),
                new(ContentBlockSlugs.PropertyDetailVtourBtn, "Virtual tour — button"),
                new(ContentBlockSlugs.PropertyDetailRoiTitle, "ROI — title"),
                new(ContentBlockSlugs.PropertyDetailRoiPurchase, "ROI — Purchase price label"),
                new(ContentBlockSlugs.PropertyDetailRoiRent, "ROI — Rent label"),
                new(ContentBlockSlugs.PropertyDetailRoiServiceCharge, "ROI — Service charge label"),
                new(ContentBlockSlugs.PropertyDetailDistancesTitle, "Section — Distances"),
                new(ContentBlockSlugs.PropertyDetailDirectionsLink, "Distances — directions link"),
                new(ContentBlockSlugs.PropertyDetailReviewsTitle, "Section — Reviews"),
                new(ContentBlockSlugs.PropertyDetailReviewSignin, "Reviews — sign-in note"),
                new(ContentBlockSlugs.PropertyDetailLeaveReviewHeading, "Reviews — Leave heading"),
                new(ContentBlockSlugs.PropertyDetailUpdateReviewHeading, "Reviews — Update heading"),
                new(ContentBlockSlugs.PropertyDetailReviewLabel, "Reviews — text label"),
                new(ContentBlockSlugs.PropertyDetailSubmitReviewBtn, "Reviews — submit btn"),
                new(ContentBlockSlugs.PropertyDetailUpdateReviewBtn, "Reviews — update btn"),
                new(ContentBlockSlugs.PropertyDetailInvestScore, "Sidebar — Investment score"),
                new(ContentBlockSlugs.PropertyDetailMortgageTitle, "Sidebar — Mortgage title"),
                new(ContentBlockSlugs.PropertyDetailMortgagePrice, "Mortgage — Price label"),
                new(ContentBlockSlugs.PropertyDetailMortgageDown, "Mortgage — Down label"),
                new(ContentBlockSlugs.PropertyDetailMortgageRate, "Mortgage — Rate label"),
                new(ContentBlockSlugs.PropertyDetailMortgageTerm, "Mortgage — Term label"),
                new(ContentBlockSlugs.PropertyDetailMortgageYears, "Mortgage — Years suffix"),
                new(ContentBlockSlugs.PropertyDetailMortgageMonthly, "Mortgage — Monthly payment"),
                new(ContentBlockSlugs.PropertyDetailMortgageLoan, "Mortgage — Loan amount"),
                new(ContentBlockSlugs.PropertyDetailMortgageInterest, "Mortgage — Total interest"),
                new(ContentBlockSlugs.PropertyDetailMortgageDisclaimer, "Mortgage — Disclaimer", true),
                new(ContentBlockSlugs.PropertyDetailShareMortgageWa, "Mortgage — WA share btn"),
                new(ContentBlockSlugs.PropertyDetailShowPriceIn, "Sidebar — Show price in"),
                new(ContentBlockSlugs.PropertyDetailSimilarTag, "Similar — tag"),
                new(ContentBlockSlugs.PropertyDetailSimilarTitle, "Similar — title (HTML)"),
            ]),

        new("page-inquiry", "Inquiry",
            "Property inquiry page — eyebrow, hero, form labels.",
            "/Inquiry",
            [
                new(ContentBlockSlugs.MetaInquiryTitle, "Meta — title"),
                new(ContentBlockSlugs.MetaInquiryDescription, "Meta — description", true),
                new(ContentBlockSlugs.InquiryEyebrow, "Hero — eyebrow"),
                new(ContentBlockSlugs.InquiryHeroH1, "Hero — H1 (HTML)", true),
                new(ContentBlockSlugs.InquiryHeroLead, "Hero — lead", true),
                new(ContentBlockSlugs.InquirySubmitLabel, "Form — submit button"),
                new(ContentBlockSlugs.InquirySecureNote, "Form — secure note (HTML)", true),
            ]),

        new("page-list-property", "List Your Property",
            "List Your Property page — eyebrow, hero, form labels.",
            "/ListYourProperty",
            [
                new(ContentBlockSlugs.MetaListPropertyTitle, "Meta — title"),
                new(ContentBlockSlugs.MetaListPropertyDescription, "Meta — description", true),
                new(ContentBlockSlugs.ListPropertyEyebrow, "Hero — eyebrow"),
                new(ContentBlockSlugs.ListPropertyHeroH1, "Hero — H1 (HTML)", true),
                new(ContentBlockSlugs.ListPropertyHeroLead, "Hero — lead", true),
                new(ContentBlockSlugs.ListPropertySubmitLabel, "Form — submit button"),
                new(ContentBlockSlugs.ListPropertySecureNote, "Form — secure note (HTML)", true),
            ]),

        new("page-account", "Account Pages",
            "Login, Register, Dashboard, Profile, Forgot/Reset Password, Verify, Logout.",
            "/Account/Login",
            [
                new(ContentBlockSlugs.AccountBrandTagline, "Brand tagline (Login/Register)"),
                new(ContentBlockSlugs.AccountLoginTitle, "Login — Title"),
                new(ContentBlockSlugs.AccountLoginHeading, "Login — Heading"),
                new(ContentBlockSlugs.AccountLoginSub, "Login — Subtitle", true),
                new(ContentBlockSlugs.AccountLoginGoogle, "Login — Continue with Google"),
                new(ContentBlockSlugs.AccountLoginFacebook, "Login — Continue with Facebook"),
                new(ContentBlockSlugs.AccountLoginApple, "Login — Continue with Apple"),
                new(ContentBlockSlugs.AccountLoginOr, "Login — Or divider"),
                new(ContentBlockSlugs.AccountLoginTabEmail, "Login — Email tab"),
                new(ContentBlockSlugs.AccountLoginTabPhone, "Login — Phone tab"),
                new(ContentBlockSlugs.AccountLoginEmailLabel, "Login — Email label"),
                new(ContentBlockSlugs.AccountLoginPasswordLabel, "Login — Password label"),
                new(ContentBlockSlugs.AccountLoginForgotLink, "Login — Forgot link"),
                new(ContentBlockSlugs.AccountLoginRemember, "Login — Remember me"),
                new(ContentBlockSlugs.AccountLoginSubmit, "Login — Submit button"),
                new(ContentBlockSlugs.AccountLoginMobileLabel, "Login — Mobile label"),
                new(ContentBlockSlugs.AccountLoginSendOtp, "Login — Send OTP"),
                new(ContentBlockSlugs.AccountLoginCodeLabel, "Login — Code label"),
                new(ContentBlockSlugs.AccountLoginVerify, "Login — Verify button"),
                new(ContentBlockSlugs.AccountLoginOtpSent, "Login — OTP sent notice"),
                new(ContentBlockSlugs.AccountLoginFooterText, "Login — Footer text"),
                new(ContentBlockSlugs.AccountLoginCreateLink, "Login — Create account link"),
                new(ContentBlockSlugs.AccountRegisterTitle, "Register — Title"),
                new(ContentBlockSlugs.AccountRegisterHeading, "Register — Heading"),
                new(ContentBlockSlugs.AccountRegisterSub, "Register — Subtitle", true),
                new(ContentBlockSlugs.AccountRegisterFullName, "Register — Full name label"),
                new(ContentBlockSlugs.AccountRegisterPasswordHint, "Register — Password hint"),
                new(ContentBlockSlugs.AccountRegisterConfirmPassword, "Register — Confirm password"),
                new(ContentBlockSlugs.AccountRegisterSubmit, "Register — Submit button"),
                new(ContentBlockSlugs.AccountRegisterFooterText, "Register — Footer text"),
                new(ContentBlockSlugs.AccountRegisterSignInLink, "Register — Sign in link"),
                new(ContentBlockSlugs.AccountDashboardTitle, "Dashboard — Title"),
                new(ContentBlockSlugs.AccountDashboardHeroH1, "Dashboard — Hero H1 (HTML)"),
                new(ContentBlockSlugs.AccountDashboardHeroSub, "Dashboard — Hero sub", true),
                new(ContentBlockSlugs.AccountDashboardEditProfile, "Dashboard — Edit profile button"),
                new(ContentBlockSlugs.AccountDashboardTabSaved, "Dashboard — Tab Saved"),
                new(ContentBlockSlugs.AccountDashboardTabReviews, "Dashboard — Tab Reviews"),
                new(ContentBlockSlugs.AccountDashboardTabServices, "Dashboard — Tab Services"),
                new(ContentBlockSlugs.AccountDashboardTabSearches, "Dashboard — Tab Searches"),
                new(ContentBlockSlugs.AccountDashboardTabInquiries, "Dashboard — Tab Inquiries"),
                new(ContentBlockSlugs.AccountDashboardNoSaved, "Dashboard — Empty saved"),
                new(ContentBlockSlugs.AccountDashboardBrowseBtn, "Dashboard — Browse button"),
                new(ContentBlockSlugs.AccountDashboardNoReviews, "Dashboard — Empty reviews"),
                new(ContentBlockSlugs.AccountDashboardNoSearches, "Dashboard — Empty searches (HTML)", true),
                new(ContentBlockSlugs.AccountDashboardNoInquiries, "Dashboard — Empty inquiries"),
                new(ContentBlockSlugs.AccountProfileTitle, "Profile — Title"),
                new(ContentBlockSlugs.AccountProfileHeroH1, "Profile — Hero H1 (HTML)"),
                new(ContentBlockSlugs.AccountProfilePersonalHeading, "Profile — Personal heading"),
                new(ContentBlockSlugs.AccountProfileDisplayName, "Profile — Display name"),
                new(ContentBlockSlugs.AccountProfileAvatarUrl, "Profile — Avatar URL"),
                new(ContentBlockSlugs.AccountProfileBio, "Profile — Bio"),
                new(ContentBlockSlugs.AccountProfileSave, "Profile — Save button"),
                new(ContentBlockSlugs.AccountProfileChangePwdHeading, "Profile — Change pw heading"),
                new(ContentBlockSlugs.AccountProfileCurrentPassword, "Profile — Current password"),
                new(ContentBlockSlugs.AccountProfileNewPassword, "Profile — New password"),
                new(ContentBlockSlugs.AccountProfileConfirmNew, "Profile — Confirm new"),
                new(ContentBlockSlugs.AccountProfileUpdatePwd, "Profile — Update pw button"),
                new(ContentBlockSlugs.AccountForgotTitle, "Forgot — Title"),
                new(ContentBlockSlugs.AccountForgotHeading, "Forgot — Heading"),
                new(ContentBlockSlugs.AccountForgotSub, "Forgot — Sub", true),
                new(ContentBlockSlugs.AccountForgotSent, "Forgot — Sent message", true),
                new(ContentBlockSlugs.AccountForgotBackBtn, "Forgot — Back button"),
                new(ContentBlockSlugs.AccountForgotSubmit, "Forgot — Submit"),
                new(ContentBlockSlugs.AccountForgotBackLink, "Forgot — Back link"),
                new(ContentBlockSlugs.AccountResetTitle, "Reset — Title"),
                new(ContentBlockSlugs.AccountResetHeading, "Reset — Heading"),
                new(ContentBlockSlugs.AccountResetDone, "Reset — Done message"),
                new(ContentBlockSlugs.AccountResetConfirmPwd, "Reset — Confirm pw label"),
                new(ContentBlockSlugs.AccountVerifyTitle, "Verify — Title"),
                new(ContentBlockSlugs.AccountVerifySuccessHeading, "Verify — Success heading"),
                new(ContentBlockSlugs.AccountVerifySuccessSub, "Verify — Success sub", true),
                new(ContentBlockSlugs.AccountVerifyGoDashboard, "Verify — Go dashboard btn"),
                new(ContentBlockSlugs.AccountVerifyFailHeading, "Verify — Fail heading"),
                new(ContentBlockSlugs.AccountLogoutTitle, "Logout — Title"),
                new(ContentBlockSlugs.AccountLogoutMessage, "Logout — Message"),
                new(ContentBlockSlugs.AccountLogoutBackBtn, "Logout — Back button"),
                new(ContentBlockSlugs.AccountExternalCallbackTitle, "External — Title"),
                new(ContentBlockSlugs.AccountExternalCallbackMessage, "External — Message"),
            ]),

        new("page-misc", "Other Pages",
            "Off-Plan, Compare, Projects, Area Guides, Privacy, Error, OurServices, Project Tracker, etc.",
            "/",
            [
                new(ContentBlockSlugs.MetaOffPlanTitle, "Off-Plan — Meta title"),
                new(ContentBlockSlugs.MetaOffPlanDescription, "Off-Plan — Meta description", true),
                new(ContentBlockSlugs.OffPlanHeroH1, "Off-Plan — Hero H1 (HTML)"),
                new(ContentBlockSlugs.OffPlanHeroSub, "Off-Plan — Hero sub", true),
                new(ContentBlockSlugs.OffPlanEmptyText, "Off-Plan — Empty text"),
                new(ContentBlockSlugs.OffPlanHandoverLabel, "Off-Plan — Handover label"),
                new(ContentBlockSlugs.OffPlanPercentLabel, "Off-Plan — Percent label"),
                new(ContentBlockSlugs.OffPlanViewLink, "Off-Plan — View link"),
                new(ContentBlockSlugs.MetaCompareTitle, "Compare — Meta title"),
                new(ContentBlockSlugs.MetaCompareDescription, "Compare — Meta description", true),
                new(ContentBlockSlugs.CompareHeroH1, "Compare — Hero H1 (HTML)"),
                new(ContentBlockSlugs.CompareHeroSub, "Compare — Hero sub", true),
                new(ContentBlockSlugs.CompareEmptyText, "Compare — Empty text"),
                new(ContentBlockSlugs.CompareUnavailable, "Compare — Unavailable text"),
                new(ContentBlockSlugs.CompareTableFeature, "Compare — Feature col"),
                new(ContentBlockSlugs.CompareRowPrice, "Compare — Price row"),
                new(ContentBlockSlugs.CompareRowType, "Compare — Type row"),
                new(ContentBlockSlugs.CompareRemoveLabel, "Compare — Remove label"),
                new(ContentBlockSlugs.CompareInquireLabel, "Compare — Inquire label"),
                new(ContentBlockSlugs.MetaProjectsTitle, "Projects — Meta title"),
                new(ContentBlockSlugs.MetaProjectsDescription, "Projects — Meta description", true),
                new(ContentBlockSlugs.ProjectsHeroH1, "Projects — Hero H1 (HTML)"),
                new(ContentBlockSlugs.ProjectsHeroSub, "Projects — Hero sub", true),
                new(ContentBlockSlugs.ProjectsEmptyText, "Projects — Empty text"),
                new(ContentBlockSlugs.ProjectDetailSidebarTitle, "Project detail — Sidebar title"),
                new(ContentBlockSlugs.ProjectDetailBtnInquiry, "Project detail — Inquiry btn"),
                new(ContentBlockSlugs.ProjectDetailBtnContracting, "Project detail — Contracting btn"),
                new(ContentBlockSlugs.ProjectDetailBtnContact, "Project detail — Contact btn"),
                new(ContentBlockSlugs.MetaAreaGuidesTitle, "Area Guides — Meta title"),
                new(ContentBlockSlugs.MetaAreaGuidesDescription, "Area Guides — Meta description", true),
                new(ContentBlockSlugs.AreaGuidesHeroH1, "Area Guides — Hero H1 (HTML)"),
                new(ContentBlockSlugs.AreaGuidesHeroSub, "Area Guides — Hero sub", true),
                new(ContentBlockSlugs.AreaGuidesEmptyText, "Area Guides — Empty text"),
                new(ContentBlockSlugs.AreaGuideMarketTitle, "Area guide — Market title"),
                new(ContentBlockSlugs.AreaGuideAvgSale, "Area guide — Avg sale"),
                new(ContentBlockSlugs.AreaGuideAvgRent, "Area guide — Avg rent"),
                new(ContentBlockSlugs.AreaGuideMonthly, "Area guide — Monthly"),
                new(ContentBlockSlugs.AreaGuidePopular, "Area guide — Popular with"),
                new(ContentBlockSlugs.AreaGuideLandmarks, "Area guide — Landmarks"),
                new(ContentBlockSlugs.AreaGuideSchools, "Area guide — Schools"),
                new(ContentBlockSlugs.AreaGuideTransport, "Area guide — Transport"),
                new(ContentBlockSlugs.AreaGuideBrowseBtn, "Area guide — Browse btn"),
                new(ContentBlockSlugs.AreaGuideTalkBtn, "Area guide — Talk btn"),
                new(ContentBlockSlugs.MetaPrivacyTitle, "Privacy — Meta title"),
                new(ContentBlockSlugs.MetaPrivacyDescription, "Privacy — Meta description", true),
                new(ContentBlockSlugs.PrivacyHeroH1, "Privacy — Hero H1 (HTML)"),
                new(ContentBlockSlugs.PrivacyBody, "Privacy — Body (HTML)", true),
                new(ContentBlockSlugs.ErrorNotFoundTitle, "Error — Not found title"),
                new(ContentBlockSlugs.ErrorGenericTitle, "Error — Generic title"),
                new(ContentBlockSlugs.ErrorNotFoundH1, "Error — Not found H1 (HTML)"),
                new(ContentBlockSlugs.ErrorNotFoundSub, "Error — Not found sub", true),
                new(ContentBlockSlugs.ErrorGenericH1, "Error — Generic H1 (HTML)"),
                new(ContentBlockSlugs.ErrorGenericSub, "Error — Generic sub", true),
                new(ContentBlockSlugs.MetaOurServicesTitle, "OurServices — Meta title"),
                new(ContentBlockSlugs.MetaOurServicesDescription, "OurServices — Meta desc", true),
                new(ContentBlockSlugs.OurServicesHeroH1, "OurServices — Hero H1 (HTML)"),
                new(ContentBlockSlugs.OurServicesHeroSub, "OurServices — Hero sub", true),
                new(ContentBlockSlugs.OurServicesJourneyTitle, "OurServices — Journey title (HTML)"),
                new(ContentBlockSlugs.OurServicesJourneySub, "OurServices — Journey sub", true),
                new(ContentBlockSlugs.OurServicesBundleTitle, "OurServices — Bundle title"),
                new(ContentBlockSlugs.OurServicesBundleBody, "OurServices — Bundle body (HTML)", true),
                new(ContentBlockSlugs.OurServicesBundleBtn, "OurServices — Bundle button"),
                new(ContentBlockSlugs.MetaMortgageTitle, "Mortgage — Meta title"),
                new(ContentBlockSlugs.MetaMortgageDescription, "Mortgage — Meta desc", true),
                new(ContentBlockSlugs.MortgageHeroH1, "Mortgage — Hero H1 (HTML)"),
                new(ContentBlockSlugs.MortgageHeroSub, "Mortgage — Hero sub", true),
                new(ContentBlockSlugs.MetaMarketTrendsTitle, "MarketTrends — Meta title"),
                new(ContentBlockSlugs.MetaMarketTrendsDescription, "MarketTrends — Meta desc", true),
                new(ContentBlockSlugs.MarketTrendsHeroH1, "MarketTrends — Hero H1 (HTML)"),
                new(ContentBlockSlugs.MarketTrendsHeroSub, "MarketTrends — Hero sub", true),
                new(ContentBlockSlugs.MetaHomeValuationTitle, "HomeValuation — Meta title"),
                new(ContentBlockSlugs.MetaHomeValuationDescription, "HomeValuation — Meta desc", true),
                new(ContentBlockSlugs.HomeValuationHeroH1, "HomeValuation — Hero H1 (HTML)"),
                new(ContentBlockSlugs.HomeValuationHeroSub, "HomeValuation — Hero sub", true),
                new(ContentBlockSlugs.ProjectTrackerContactText, "ProjectTracker — Contact text"),
                new(ContentBlockSlugs.ProjectTrackerContactBtn, "ProjectTracker — Contact btn"),
            ]),

        new("page-chrome", "Navigation & Footer",
            "Global navigation labels, footer columns and shared chrome.",
            "/",
            [
                new(ContentBlockSlugs.NavLabelServices, "Nav — Services"),
                new(ContentBlockSlugs.NavLabelProperties, "Nav — Properties"),
                new(ContentBlockSlugs.NavLabelAbout, "Nav — About"),
                new(ContentBlockSlugs.NavLabelContact, "Nav — Contact"),
                new(ContentBlockSlugs.NavLabelListProperty, "Nav — List property"),
                new(ContentBlockSlugs.NavLabelInquiry, "Nav — Inquiry"),
                new(ContentBlockSlugs.NavDdMaintTitle, "Nav dropdown — Maintenance title"),
                new(ContentBlockSlugs.NavDdMaintSub, "Nav dropdown — Maintenance sub"),
                new(ContentBlockSlugs.NavDdContractingTitle, "Nav dropdown — Contracting title"),
                new(ContentBlockSlugs.NavDdContractingSub, "Nav dropdown — Contracting sub"),
                new(ContentBlockSlugs.NavDdFacilityTitle, "Nav dropdown — Facility title"),
                new(ContentBlockSlugs.NavDdFacilitySub, "Nav dropdown — Facility sub"),
                new(ContentBlockSlugs.NavDdAllServices, "Nav dropdown — All services link"),
                new(ContentBlockSlugs.NavDdOurProjects, "Nav dropdown — Our projects link"),
                new(ContentBlockSlugs.NavMobileServicesLabel, "Mobile — Services label"),
                new(ContentBlockSlugs.NavMobileServicesAll, "Mobile — All services link"),
                new(ContentBlockSlugs.NavMobileForOwnersLabel, "Mobile — For owners label"),
                new(ContentBlockSlugs.NavMobilePropertyInquiry, "Mobile — Property inquiry link"),
                new(ContentBlockSlugs.NavUserAgentPortal, "Nav user — Agent portal"),
                new(ContentBlockSlugs.NavUserDashboard, "Nav user — Dashboard"),
                new(ContentBlockSlugs.NavUserProfile, "Nav user — Profile"),
                new(ContentBlockSlugs.NavUserSignOut, "Nav user — Sign out"),
                new(ContentBlockSlugs.FooterColServicesTitle, "Footer — Services col title"),
                new(ContentBlockSlugs.FooterColPropertiesTitle, "Footer — Properties col title"),
                new(ContentBlockSlugs.FooterColCompanyTitle, "Footer — Company col title"),
                new(ContentBlockSlugs.FooterLinkContracting, "Footer — Contracting link"),
                new(ContentBlockSlugs.FooterLinkMaintenance, "Footer — Maintenance link"),
                new(ContentBlockSlugs.FooterLinkFacility, "Footer — Facility link"),
                new(ContentBlockSlugs.FooterLinkAllServices, "Footer — All services link"),
                new(ContentBlockSlugs.FooterLinkBuy, "Footer — Buy link"),
                new(ContentBlockSlugs.FooterLinkRent, "Footer — Rent link"),
                new(ContentBlockSlugs.FooterLinkOffPlan, "Footer — Off-Plan link"),
                new(ContentBlockSlugs.FooterLinkAreaGuides, "Footer — Area Guides link"),
                new(ContentBlockSlugs.FooterLinkAbout, "Footer — About link"),
                new(ContentBlockSlugs.FooterLinkContact, "Footer — Contact link"),
                new(ContentBlockSlugs.FooterLinkInquiry, "Footer — Inquiry link"),
                new(ContentBlockSlugs.FooterLinkListProperty, "Footer — List property link"),
                new(ContentBlockSlugs.FooterRightsText, "Footer — Rights notice"),
                new(ContentBlockSlugs.CardLabelForSale, "Card — For Sale label"),
                new(ContentBlockSlugs.CardLabelForRent, "Card — For Rent label"),
                new(ContentBlockSlugs.CardLabelVerified, "Card — Verified label"),
                new(ContentBlockSlugs.CardLabelCompare, "Card — Compare label"),
                new(ContentBlockSlugs.CardFallbackLocation, "Card — Fallback location"),
                new(ContentBlockSlugs.CardUnitBeds, "Card — Beds unit"),
                new(ContentBlockSlugs.CardUnitBaths, "Card — Baths unit"),
                new(ContentBlockSlugs.CardUnitArea, "Card — Area unit"),
                new(ContentBlockSlugs.ModalInquiryTitle, "Modal — Inquiry title"),
                new(ContentBlockSlugs.ModalInquirySub, "Modal — Inquiry sub", true),
                new(ContentBlockSlugs.ModalInquirySend, "Modal — Inquiry send btn"),
                new(ContentBlockSlugs.ModalListPropTitle, "Modal — List property title"),
                new(ContentBlockSlugs.ModalListPropSub, "Modal — List property sub", true),
                new(ContentBlockSlugs.ModalListPropSubmit, "Modal — List property submit btn"),
                new(ContentBlockSlugs.ModalListPropLegal, "Modal — List property legal (HTML)", true),
            ]),
    ];

    /// <summary>Brand &amp; contact details that appear in nav, footer and metadata.</summary>
    public static readonly List<(string Key, string Label, string? Hint)> BrandingFields =
    [
        (SiteSettingKeys.CompanyDisplayName, "Company name", "e.g. 804 Avenue"),
        (SiteSettingKeys.NavTagline,         "Navigation tagline", "Shown next to the logo in the top nav."),
        (SiteSettingKeys.FooterTagline,      "Footer tagline", null),
        (SiteSettingKeys.PhoneDisplay,       "Phone (display)", "+971 50 43 99 804"),
        (SiteSettingKeys.PhoneE164,          "Phone (E.164)", "+971504399804 — used for tel: links."),
        (SiteSettingKeys.Email,              "Email address", null),
        (SiteSettingKeys.Address,            "Office address", null),
        (SiteSettingKeys.WhatsAppDigits,     "WhatsApp number", "Digits only, e.g. 971504399804"),
        (SiteSettingKeys.WebsiteUrl,         "Website URL", null),
        (SiteSettingKeys.InstagramUrl,       "Instagram URL", null),
        (SiteSettingKeys.LinkedInUrl,        "LinkedIn URL", null),
        (SiteSettingKeys.FacebookUrl,        "Facebook URL", null),
        (SiteSettingKeys.CopyrightOwner,     "Copyright owner", "Shown in the footer copyright notice."),
        (SiteSettingKeys.DefaultMetaDescription, "Default meta description", "Used on pages without their own description."),
        (SiteSettingKeys.DefaultListingCurrency, "Default currency", "AED, USD, EUR…"),
    ];

    public async Task OnGetAsync(CancellationToken ct)
    {
        ViewData["AdminSection"] = "website";
        ViewData["Title"] = "Page Content";
        SuccessMessage = TempData["ToastOk"] as string;
        await LoadDataAsync(ct);
    }

    public async Task<IActionResult> OnPostContentAsync(CancellationToken ct)
    {
        ViewData["AdminSection"] = "website";
        var form = Request.Form;
        var pageId = form["__page"].ToString();
        int updated = 0;

        var allBlocks = await _db.ContentBlocks.ToListAsync(ct);
        var bySlug = allBlocks.ToDictionary(b => b.Slug, StringComparer.OrdinalIgnoreCase);

        IEnumerable<ContentField> fieldsToUpdate;
        if (pageId == "page-other")
        {
            // Orphan tab — accept any slug that came in via the form (we just
            // need to find it in the DB to update it).
            var orphans = await BuildOrphanFieldsAsync(ct);
            fieldsToUpdate = orphans;
        }
        else if (!string.IsNullOrEmpty(pageId))
        {
            var page = ContentPages.FirstOrDefault(p => p.Id == pageId);
            fieldsToUpdate = page?.Fields ?? Enumerable.Empty<ContentField>();
        }
        else
        {
            fieldsToUpdate = ContentPages.SelectMany(p => p.Fields);
        }

        foreach (var field in fieldsToUpdate)
        {
            var formKey = "cb_" + field.Slug.Replace('.', '_');
            if (!form.ContainsKey(formKey)) continue;

            var newBody = form[formKey].ToString().Trim();
            if (bySlug.TryGetValue(field.Slug, out var existing))
            {
                if (existing.Body != newBody)
                {
                    ContentBlockService.Invalidate(_cache, existing.Slug);
                    existing.Body = newBody;
                    existing.UpdatedAt = DateTimeOffset.UtcNow;
                    updated++;
                }
            }
            else if (!string.IsNullOrEmpty(newBody))
            {
                _db.ContentBlocks.Add(new ContentBlock
                {
                    Slug = field.Slug,
                    Body = newBody,
                    IsPublished = true,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
                updated++;
            }
        }

        if (updated > 0)
        {
            await _db.SaveChangesAsync(ct);
            TempData["ToastOk"] = $"{updated} field(s) saved.";
        }
        else
        {
            TempData["ToastOk"] = "No changes detected.";
        }

        return RedirectToPage(null, null, pageId);
    }

    public async Task<IActionResult> OnPostBrandingAsync(CancellationToken ct)
    {
        ViewData["AdminSection"] = "website";
        var form = Request.Form;
        int updated = 0;

        var allSettings = await _db.SiteSettings.ToListAsync(ct);
        var byKey = allSettings.ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var (key, _, _) in BrandingFields)
        {
            var formKey = "ss_" + key;
            if (!form.ContainsKey(formKey)) continue;

            var newVal = form[formKey].ToString().Trim();
            if (byKey.TryGetValue(key, out var existing))
            {
                if (existing.Value != newVal)
                {
                    existing.Value = newVal;
                    existing.UpdatedAt = DateTimeOffset.UtcNow;
                    updated++;
                }
            }
            else if (!string.IsNullOrEmpty(newVal))
            {
                _db.SiteSettings.Add(new SiteSetting
                {
                    Key = key,
                    Value = newVal,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
                updated++;
            }
        }

        if (updated > 0)
        {
            await _db.SaveChangesAsync(ct);
            _cache.Remove("site-branding-snapshot-v1");
            TempData["ToastOk"] = $"{updated} setting(s) saved.";
        }
        else
        {
            TempData["ToastOk"] = "No changes detected.";
        }

        return RedirectToPage(null, null, "branding");
    }

    private async Task LoadDataAsync(CancellationToken ct)
    {
        var blocks = await _db.ContentBlocks.AsNoTracking().ToListAsync(ct);
        BlockBodies = blocks.ToDictionary(b => b.Slug, b => b.Body, StringComparer.OrdinalIgnoreCase);

        var settings = await _db.SiteSettings.AsNoTracking().ToListAsync(ct);
        Settings = settings.ToDictionary(s => s.Key, s => s, StringComparer.OrdinalIgnoreCase);

        OrphanFields = await BuildOrphanFieldsAsync(ct);
    }

    private async Task<List<ContentField>> BuildOrphanFieldsAsync(CancellationToken ct)
    {
        var wired = ContentPages.SelectMany(p => p.Fields)
            .Select(f => f.Slug)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var allSlugs = await _db.ContentBlocks
            .AsNoTracking()
            .Select(b => b.Slug)
            .ToListAsync(ct);

        return allSlugs
            .Where(s => !wired.Contains(s))
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .Select(s => new ContentField(s, PrettifyLabel(s), IsTextarea: true,
                Hint: "Not yet wired to a page editor — slug-only edit."))
            .ToList();
    }

    /// <summary>Turn a slug like "page.about.story.body" into "Page · About · Story · Body".</summary>
    private static string PrettifyLabel(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return slug;
        var parts = slug.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var pretty = parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]);
        return string.Join(" · ", pretty);
    }
}
