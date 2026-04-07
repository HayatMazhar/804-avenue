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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PropertyListing>(e =>
        {
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Slug).HasMaxLength(320);
            e.Property(x => x.Currency).HasMaxLength(8);
            e.Property(x => x.Location).HasMaxLength(400);
            e.Property(x => x.MainImageUrl).HasMaxLength(2000);
            e.HasIndex(x => x.Slug).IsUnique();
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
    }
}
