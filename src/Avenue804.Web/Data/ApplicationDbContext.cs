using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<PropertyListing> PropertyListings => Set<PropertyListing>();
    public DbSet<PortfolioProject> PortfolioProjects => Set<PortfolioProject>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<OwnerListingRequest> OwnerListingRequests => Set<OwnerListingRequest>();
    public DbSet<PropertyListingInquiry> PropertyListingInquiries => Set<PropertyListingInquiry>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<LookupCategory> LookupCategories => Set<LookupCategory>();
    public DbSet<LookupValue> LookupValues => Set<LookupValue>();
    public DbSet<ContentBlock> ContentBlocks => Set<ContentBlock>();
    public DbSet<SavedProperty> SavedProperties => Set<SavedProperty>();
    public DbSet<PropertyRating> PropertyRatings => Set<PropertyRating>();
    public DbSet<SavedSearch> SavedSearches => Set<SavedSearch>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Developer> Developers => Set<Developer>();
    public DbSet<AreaGuide> AreaGuides => Set<AreaGuide>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<ProjectProgress> ProjectProgressItems => Set<ProjectProgress>();
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();
    public DbSet<ServiceReport> ServiceReports => Set<ServiceReport>();
    public DbSet<AdminNotification> AdminNotifications => Set<AdminNotification>();
    public DbSet<UaeArea> UaeAreas => Set<UaeArea>();
    public DbSet<ServiceQuoteRequest> ServiceQuoteRequests => Set<ServiceQuoteRequest>();
    public DbSet<MaintenanceTicket> MaintenanceTickets => Set<MaintenanceTicket>();
    public DbSet<AmcRequest> AmcRequests => Set<AmcRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<NewsletterSubscriber>(e =>
        {
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Preferences).HasMaxLength(200);
            e.HasIndex(x => x.Email).IsUnique();
        });

        builder.Entity<ServiceReport>(e =>
        {
            e.Property(x => x.ClientEmail).HasMaxLength(256);
            e.Property(x => x.ClientName).HasMaxLength(200);
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Description).HasMaxLength(4000);
            e.Property(x => x.ReportFileUrl).HasMaxLength(2000);
            e.HasOne(x => x.Ticket).WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<UaeArea>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Emirate).HasMaxLength(100);
            e.Property(x => x.City).HasMaxLength(100);
            e.HasIndex(x => new { x.Emirate, x.Name });
        });

        builder.Entity<AdminNotification>(e =>
        {
            e.Property(x => x.Message).HasMaxLength(500);
            e.Property(x => x.LinkUrl).HasMaxLength(500);
        });

        builder.Entity<ProjectProgress>(e =>
        {
            e.Property(x => x.ClientName).HasMaxLength(200);
            e.Property(x => x.ClientEmail).HasMaxLength(256);
            e.Property(x => x.ProjectTitle).HasMaxLength(300);
            e.Property(x => x.Location).HasMaxLength(400);
            e.Property(x => x.StageNotes).HasMaxLength(2000);
            e.Property(x => x.AccessToken).HasMaxLength(100);
        });

        builder.Entity<ServiceQuoteRequest>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Company).HasMaxLength(200);
            e.Property(x => x.Location).HasMaxLength(400);
            e.Property(x => x.Timeline).HasMaxLength(200);
            e.Property(x => x.ProjectDescription).HasMaxLength(4000);
            e.Property(x => x.EstimateRange).HasMaxLength(100);
            e.Property(x => x.AdminNotes).HasMaxLength(2000);
        });

        builder.Entity<MaintenanceTicket>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.BuildingOrLocation).HasMaxLength(400);
            e.Property(x => x.IssueDescription).HasMaxLength(4000);
            e.Property(x => x.AdminNotes).HasMaxLength(2000);
        });

        builder.Entity<AmcRequest>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Company).HasMaxLength(200);
            e.Property(x => x.Location).HasMaxLength(400);
            e.Property(x => x.ServicesNeeded).HasMaxLength(500);
            e.Property(x => x.EstimateRange).HasMaxLength(100);
            e.Property(x => x.AdminNotes).HasMaxLength(2000);
        });

        builder.Entity<Testimonial>(e =>
        {
            e.Property(x => x.AuthorName).HasMaxLength(200);
            e.Property(x => x.AuthorRole).HasMaxLength(200);
            e.Property(x => x.Quote).HasMaxLength(2000);
        });

        builder.Entity<Developer>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Slug).HasMaxLength(200);
            e.Property(x => x.LogoUrl).HasMaxLength(2000);
            e.Property(x => x.WebsiteUrl).HasMaxLength(2000);
            e.Property(x => x.Headquarters).HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(4000);
            e.HasIndex(x => x.Slug).IsUnique().HasFilter("[Slug] IS NOT NULL");
        });

        builder.Entity<AreaGuide>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Slug).HasMaxLength(200);
            e.Property(x => x.Emirate).HasMaxLength(100);
            e.Property(x => x.HeroImageUrl).HasMaxLength(2000);
            e.Property(x => x.Overview).HasMaxLength(20000);
            e.Property(x => x.PopularWith).HasMaxLength(300);
            e.Property(x => x.NearbyLandmarks).HasMaxLength(1000);
            e.Property(x => x.SchoolsNearby).HasMaxLength(1000);
            e.Property(x => x.TransportLinks).HasMaxLength(1000);
            e.HasIndex(x => x.Slug).IsUnique();
        });

        builder.Entity<Agent>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.WhatsAppNumber).HasMaxLength(50);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.AvatarUrl).HasMaxLength(2000);
            e.Property(x => x.Bio).HasMaxLength(1000);
            e.Property(x => x.ResponseTime).HasMaxLength(100);
            e.Property(x => x.UserId).HasMaxLength(450);
        });

        builder.Entity<PropertyListing>(e =>
        {
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Slug).HasMaxLength(320);
            e.Property(x => x.Currency).HasMaxLength(8);
            e.Property(x => x.Location).HasMaxLength(400);
            e.Property(x => x.MainImageUrl).HasMaxLength(2000);
            e.Property(x => x.FloorPlanUrl).HasMaxLength(2000);
            e.Property(x => x.VirtualTourUrl).HasMaxLength(2000);
            e.Property(x => x.SeoTitle).HasMaxLength(300);
            e.Property(x => x.SeoDescription).HasMaxLength(500);
            e.Property(x => x.RejectionReason).HasMaxLength(500);
            e.Property(x => x.Label).HasMaxLength(50);
            e.Property(x => x.HandoverDate).HasMaxLength(50);
            e.Property(x => x.PaymentPlan).HasMaxLength(300);
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasOne(x => x.Agent).WithMany(a => a.Listings).HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Developer).WithMany(d => d.Listings).HasForeignKey(x => x.DeveloperId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<PortfolioProject>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Slug).HasMaxLength(320);
            e.Property(x => x.CoverImageUrl).HasMaxLength(2000);
            e.HasIndex(x => x.Slug).IsUnique();
        });

        builder.Entity<Inquiry>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.Subject).HasMaxLength(300);
            e.Property(x => x.Message).HasMaxLength(8000);
            e.HasOne(x => x.TopicLookup)
                .WithMany()
                .HasForeignKey(x => x.TopicLookupValueId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<OwnerListingRequest>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.LocationOrTitle).HasMaxLength(500);
            e.Property(x => x.Details).HasMaxLength(8000);
        });

        builder.Entity<PropertyListingInquiry>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.OtherDetails).HasMaxLength(2000);
            e.Property(x => x.RequirementDetails).HasMaxLength(8000);
            e.Property(x => x.Area).HasMaxLength(300);
            e.Property(x => x.AgentNote).HasMaxLength(1000);
            e.Property(x => x.ExpectedMoveInDate).HasColumnType("date");
            /* SQL Server: multiple SET NULL from same child → LookupValues causes "cycles or multiple cascade paths". */
            e.HasOne(x => x.IAmLookup)
                .WithMany()
                .HasForeignKey(x => x.IAmLookupValueId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.WantToLookup)
                .WithMany()
                .HasForeignKey(x => x.WantToLookupValueId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.PropertyTypeLookup)
                .WithMany()
                .HasForeignKey(x => x.PropertyTypeLookupValueId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.PropertyDetailLookup)
                .WithMany()
                .HasForeignKey(x => x.PropertyDetailLookupValueId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.LocationLookup)
                .WithMany()
                .HasForeignKey(x => x.LocationLookupValueId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.BudgetLookup)
                .WithMany()
                .HasForeignKey(x => x.BudgetLookupValueId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<SiteSetting>(e =>
        {
            e.Property(x => x.Key).HasMaxLength(120);
            e.Property(x => x.Value).HasMaxLength(4000);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasIndex(x => x.Key).IsUnique();
        });

        builder.Entity<LookupCategory>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(80);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<LookupValue>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(80);
            e.Property(x => x.DisplayName).HasMaxLength(200);
            e.Property(x => x.Metadata).HasMaxLength(2000);
            e.HasIndex(x => new { x.CategoryId, x.Code }).IsUnique();
            e.HasOne(x => x.Category)
                .WithMany(x => x.Values)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ContentBlock>(e =>
        {
            e.Property(x => x.Slug).HasMaxLength(160);
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Body).HasMaxLength(20000);
            e.HasIndex(x => x.Slug).IsUnique();
        });

        builder.Entity<SavedProperty>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.ListingId }).IsUnique();
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Listing).WithMany().HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PropertyRating>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.ListingId }).IsUnique();
            e.Property(x => x.Review).HasMaxLength(2000);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Listing).WithMany().HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SavedSearch>(e =>
        {
            e.Property(x => x.Label).HasMaxLength(200);
            e.Property(x => x.Offer).HasMaxLength(20);
            e.Property(x => x.Location).HasMaxLength(200);
            e.Property(x => x.PropertyType).HasMaxLength(100);
            e.Property(x => x.Budget).HasMaxLength(20);
            e.Property(x => x.Keyword).HasMaxLength(300);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(x => x.DisplayName).HasMaxLength(200);
            e.Property(x => x.AvatarUrl).HasMaxLength(2000);
            e.Property(x => x.Bio).HasMaxLength(500);
        });
    }
}
