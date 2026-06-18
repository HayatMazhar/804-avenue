IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [DisplayName] nvarchar(max) NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE TABLE [Inquiries] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Phone] nvarchar(50) NULL,
        [Subject] nvarchar(300) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Inquiries] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE TABLE [OwnerListingRequests] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Phone] nvarchar(50) NOT NULL,
        [Intent] int NOT NULL,
        [LocationOrTitle] nvarchar(500) NULL,
        [Details] nvarchar(max) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_OwnerListingRequests] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE TABLE [PortfolioProjects] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(300) NOT NULL,
        [Slug] nvarchar(320) NULL,
        [Summary] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [CoverImageUrl] nvarchar(2000) NULL,
        [IsPublished] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_PortfolioProjects] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE TABLE [PropertyListings] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(300) NOT NULL,
        [Slug] nvarchar(320) NULL,
        [Price] decimal(18,2) NULL,
        [Currency] nvarchar(8) NOT NULL,
        [OfferType] int NOT NULL,
        [Location] nvarchar(400) NULL,
        [Description] nvarchar(max) NULL,
        [Beds] int NULL,
        [Baths] int NULL,
        [AreaSqft] int NULL,
        [MainImageUrl] nvarchar(2000) NULL,
        [IsPublished] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NULL,
        CONSTRAINT [PK_PropertyListings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PropertyListings_Slug] ON [PropertyListings] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104012_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260406104012_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406104039_PropertyListingPricePrecision'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260406104039_PropertyListingPricePrecision', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    DROP INDEX [IX_PropertyListings_Slug] ON [PropertyListings];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    ALTER TABLE [Inquiries] ADD [TopicLookupValueId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    CREATE TABLE [ContentBlocks] (
        [Id] int NOT NULL IDENTITY,
        [Slug] nvarchar(160) NOT NULL,
        [Title] nvarchar(300) NULL,
        [Body] nvarchar(max) NOT NULL,
        [IsPublished] bit NOT NULL,
        [UpdatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_ContentBlocks] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    CREATE TABLE [LookupCategories] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(80) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [SortOrder] int NOT NULL,
        CONSTRAINT [PK_LookupCategories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    CREATE TABLE [SiteSettings] (
        [Id] int NOT NULL IDENTITY,
        [Key] nvarchar(120) NOT NULL,
        [Value] nvarchar(4000) NOT NULL,
        [Description] nvarchar(500) NULL,
        [UpdatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_SiteSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    CREATE TABLE [LookupValues] (
        [Id] int NOT NULL IDENTITY,
        [CategoryId] int NOT NULL,
        [Code] nvarchar(80) NOT NULL,
        [DisplayName] nvarchar(200) NOT NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [Metadata] nvarchar(2000) NULL,
        CONSTRAINT [PK_LookupValues] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LookupValues_LookupCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [LookupCategories] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PropertyListings_Slug] ON [PropertyListings] ([Slug]) WHERE [Slug] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PortfolioProjects_Slug] ON [PortfolioProjects] ([Slug]) WHERE [Slug] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    CREATE INDEX [IX_Inquiries_TopicLookupValueId] ON [Inquiries] ([TopicLookupValueId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ContentBlocks_Slug] ON [ContentBlocks] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LookupCategories_Code] ON [LookupCategories] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LookupValues_CategoryId_Code] ON [LookupValues] ([CategoryId], [Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SiteSettings_Key] ON [SiteSettings] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    ALTER TABLE [Inquiries] ADD CONSTRAINT [FK_Inquiries_LookupValues_TopicLookupValueId] FOREIGN KEY ([TopicLookupValueId]) REFERENCES [LookupValues] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406113351_SiteSettingsLookupsContent'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260406113351_SiteSettingsLookupsContent', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406120000_SlugUniqueIndexes'
)
BEGIN
    DROP INDEX [IX_PropertyListings_Slug] ON [PropertyListings];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406120000_SlugUniqueIndexes'
)
BEGIN
    DROP INDEX [IX_PortfolioProjects_Slug] ON [PortfolioProjects];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406120000_SlugUniqueIndexes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PropertyListings_Slug] ON [PropertyListings] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406120000_SlugUniqueIndexes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PortfolioProjects_Slug] ON [PortfolioProjects] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406120000_SlugUniqueIndexes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260406120000_SlugUniqueIndexes', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406120706_PropertyListingInquiries'
)
BEGIN
    CREATE TABLE [PropertyListingInquiries] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Phone] nvarchar(50) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [IAmLookupValueId] int NULL,
        [WantToLookupValueId] int NULL,
        [PropertyTypeLookupValueId] int NULL,
        [PropertyDetailLookupValueId] int NULL,
        [OtherDetails] nvarchar(2000) NULL,
        [LocationLookupValueId] int NULL,
        [Area] nvarchar(300) NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_PropertyListingInquiries] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PropertyListingInquiries_LookupValues_IAmLookupValueId] FOREIGN KEY ([IAmLookupValueId]) REFERENCES [LookupValues] ([Id]),
        CONSTRAINT [FK_PropertyListingInquiries_LookupValues_LocationLookupValueId] FOREIGN KEY ([LocationLookupValueId]) REFERENCES [LookupValues] ([Id]),
        CONSTRAINT [FK_PropertyListingInquiries_LookupValues_PropertyDetailLookupValueId] FOREIGN KEY ([PropertyDetailLookupValueId]) REFERENCES [LookupValues] ([Id]),
        CONSTRAINT [FK_PropertyListingInquiries_LookupValues_PropertyTypeLookupValueId] FOREIGN KEY ([PropertyTypeLookupValueId]) REFERENCES [LookupValues] ([Id]),
        CONSTRAINT [FK_PropertyListingInquiries_LookupValues_WantToLookupValueId] FOREIGN KEY ([WantToLookupValueId]) REFERENCES [LookupValues] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406120706_PropertyListingInquiries'
)
BEGIN
    CREATE INDEX [IX_PropertyListingInquiries_IAmLookupValueId] ON [PropertyListingInquiries] ([IAmLookupValueId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406120706_PropertyListingInquiries'
)
BEGIN
    CREATE INDEX [IX_PropertyListingInquiries_LocationLookupValueId] ON [PropertyListingInquiries] ([LocationLookupValueId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406120706_PropertyListingInquiries'
)
BEGIN
    CREATE INDEX [IX_PropertyListingInquiries_PropertyDetailLookupValueId] ON [PropertyListingInquiries] ([PropertyDetailLookupValueId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406120706_PropertyListingInquiries'
)
BEGIN
    CREATE INDEX [IX_PropertyListingInquiries_PropertyTypeLookupValueId] ON [PropertyListingInquiries] ([PropertyTypeLookupValueId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406120706_PropertyListingInquiries'
)
BEGIN
    CREATE INDEX [IX_PropertyListingInquiries_WantToLookupValueId] ON [PropertyListingInquiries] ([WantToLookupValueId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406120706_PropertyListingInquiries'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260406120706_PropertyListingInquiries', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406121327_PropertyListingInquiryV2Fields'
)
BEGIN
    ALTER TABLE [PropertyListingInquiries] ADD [BudgetLookupValueId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406121327_PropertyListingInquiryV2Fields'
)
BEGIN
    ALTER TABLE [PropertyListingInquiries] ADD [ExpectedMoveInDate] date NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406121327_PropertyListingInquiryV2Fields'
)
BEGIN
    ALTER TABLE [PropertyListingInquiries] ADD [RequirementDetails] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406121327_PropertyListingInquiryV2Fields'
)
BEGIN
    CREATE INDEX [IX_PropertyListingInquiries_BudgetLookupValueId] ON [PropertyListingInquiries] ([BudgetLookupValueId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406121327_PropertyListingInquiryV2Fields'
)
BEGIN
    ALTER TABLE [PropertyListingInquiries] ADD CONSTRAINT [FK_PropertyListingInquiries_LookupValues_BudgetLookupValueId] FOREIGN KEY ([BudgetLookupValueId]) REFERENCES [LookupValues] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406121327_PropertyListingInquiryV2Fields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260406121327_PropertyListingInquiryV2Fields', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410060506_UserPlatform'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'DisplayName');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [AspNetUsers] ALTER COLUMN [DisplayName] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410060506_UserPlatform'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [AvatarUrl] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410060506_UserPlatform'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Bio] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410060506_UserPlatform'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [CreatedAt] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410060506_UserPlatform'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [IsPublicUser] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410060506_UserPlatform'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [PhoneVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410060506_UserPlatform'
)
BEGIN
    CREATE TABLE [PropertyRatings] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ListingId] int NOT NULL,
        [Stars] int NOT NULL,
        [Review] nvarchar(2000) NULL,
        [IsApproved] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_PropertyRatings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PropertyRatings_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PropertyRatings_PropertyListings_ListingId] FOREIGN KEY ([ListingId]) REFERENCES [PropertyListings] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410060506_UserPlatform'
)
BEGIN
    CREATE TABLE [SavedProperties] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ListingId] int NOT NULL,
        [SavedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_SavedProperties] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SavedProperties_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SavedProperties_PropertyListings_ListingId] FOREIGN KEY ([ListingId]) REFERENCES [PropertyListings] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410060506_UserPlatform'
)
BEGIN
    CREATE INDEX [IX_PropertyRatings_ListingId] ON [PropertyRatings] ([ListingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410060506_UserPlatform'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PropertyRatings_UserId_ListingId] ON [PropertyRatings] ([UserId], [ListingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410060506_UserPlatform'
)
BEGIN
    CREATE INDEX [IX_SavedProperties_ListingId] ON [SavedProperties] ([ListingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410060506_UserPlatform'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SavedProperties_UserId_ListingId] ON [SavedProperties] ([UserId], [ListingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410060506_UserPlatform'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410060506_UserPlatform', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410062022_GalleryImages'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [GalleryImagesJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410062022_GalleryImages'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410062022_GalleryImages', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410062717_AgentsAndSavedSearches'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [AgentId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410062717_AgentsAndSavedSearches'
)
BEGIN
    CREATE TABLE [Agents] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Title] nvarchar(200) NULL,
        [Phone] nvarchar(50) NULL,
        [WhatsAppNumber] nvarchar(50) NULL,
        [Email] nvarchar(256) NULL,
        [AvatarUrl] nvarchar(2000) NULL,
        [Bio] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Agents] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410062717_AgentsAndSavedSearches'
)
BEGIN
    CREATE TABLE [SavedSearches] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Label] nvarchar(200) NULL,
        [Offer] nvarchar(20) NULL,
        [Location] nvarchar(200) NULL,
        [PropertyType] nvarchar(100) NULL,
        [Budget] nvarchar(20) NULL,
        [Keyword] nvarchar(300) NULL,
        [AlertEnabled] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [LastAlertedAt] datetimeoffset NULL,
        CONSTRAINT [PK_SavedSearches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SavedSearches_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410062717_AgentsAndSavedSearches'
)
BEGIN
    CREATE INDEX [IX_PropertyListings_AgentId] ON [PropertyListings] ([AgentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410062717_AgentsAndSavedSearches'
)
BEGIN
    CREATE INDEX [IX_SavedSearches_UserId] ON [SavedSearches] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410062717_AgentsAndSavedSearches'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD CONSTRAINT [FK_PropertyListings_Agents_AgentId] FOREIGN KEY ([AgentId]) REFERENCES [Agents] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410062717_AgentsAndSavedSearches'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410062717_AgentsAndSavedSearches', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410065304_FeaturesViewsLabels'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [Label] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410065304_FeaturesViewsLabels'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [ViewCount] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410065304_FeaturesViewsLabels'
)
BEGIN
    CREATE TABLE [Testimonials] (
        [Id] int NOT NULL IDENTITY,
        [AuthorName] nvarchar(200) NOT NULL,
        [AuthorRole] nvarchar(200) NULL,
        [Quote] nvarchar(2000) NOT NULL,
        [Stars] int NOT NULL,
        [IsActive] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Testimonials] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410065304_FeaturesViewsLabels'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410065304_FeaturesViewsLabels', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410071430_ServiceRequestsPhase1'
)
BEGIN
    CREATE TABLE [AmcRequests] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Phone] nvarchar(50) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Company] nvarchar(200) NULL,
        [BuildingType] int NOT NULL,
        [NumberOfFloors] int NULL,
        [NumberOfUnits] int NULL,
        [TotalAreaSqm] decimal(18,2) NULL,
        [Location] nvarchar(400) NULL,
        [ServicesNeeded] nvarchar(500) NULL,
        [EstimateRange] nvarchar(100) NULL,
        [Status] int NOT NULL,
        [AdminNotes] nvarchar(2000) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_AmcRequests] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410071430_ServiceRequestsPhase1'
)
BEGIN
    CREATE TABLE [MaintenanceTickets] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Phone] nvarchar(50) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [BuildingOrLocation] nvarchar(400) NOT NULL,
        [IssueDescription] nvarchar(4000) NOT NULL,
        [Priority] int NOT NULL,
        [Status] int NOT NULL,
        [AdminNotes] nvarchar(2000) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [ResolvedAt] datetimeoffset NULL,
        CONSTRAINT [PK_MaintenanceTickets] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410071430_ServiceRequestsPhase1'
)
BEGIN
    CREATE TABLE [ServiceQuoteRequests] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Phone] nvarchar(50) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Company] nvarchar(200) NULL,
        [ServiceType] int NOT NULL,
        [ProjectDescription] nvarchar(4000) NULL,
        [AreaSqm] decimal(18,2) NULL,
        [Location] nvarchar(400) NULL,
        [Timeline] nvarchar(200) NULL,
        [Budget] decimal(18,2) NULL,
        [EstimateRange] nvarchar(100) NULL,
        [Status] int NOT NULL,
        [AdminNotes] nvarchar(2000) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_ServiceQuoteRequests] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410071430_ServiceRequestsPhase1'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410071430_ServiceRequestsPhase1', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [AmenitiesJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [CompletionPercent] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [DeveloperId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [FloorPlanUrl] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [HandoverDate] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [IsOffPlan] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [PaymentPlan] nvarchar(300) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [PreviousPrice] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [VirtualTourUrl] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    CREATE TABLE [AreaGuides] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Slug] nvarchar(200) NOT NULL,
        [Emirate] nvarchar(100) NULL,
        [HeroImageUrl] nvarchar(2000) NULL,
        [Overview] nvarchar(max) NULL,
        [AvgPriceSaleSqft] decimal(18,2) NULL,
        [AvgRentYearly] decimal(18,2) NULL,
        [PopularWith] nvarchar(300) NULL,
        [NearbyLandmarks] nvarchar(1000) NULL,
        [SchoolsNearby] nvarchar(1000) NULL,
        [TransportLinks] nvarchar(1000) NULL,
        [IsPublished] bit NOT NULL,
        [UpdatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_AreaGuides] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    CREATE TABLE [Developers] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Slug] nvarchar(200) NULL,
        [LogoUrl] nvarchar(2000) NULL,
        [WebsiteUrl] nvarchar(2000) NULL,
        [Description] nvarchar(4000) NULL,
        [Headquarters] nvarchar(200) NULL,
        [EstablishedYear] int NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Developers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    CREATE INDEX [IX_PropertyListings_DeveloperId] ON [PropertyListings] ([DeveloperId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AreaGuides_Slug] ON [AreaGuides] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Developers_Slug] ON [Developers] ([Slug]) WHERE [Slug] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD CONSTRAINT [FK_PropertyListings_Developers_DeveloperId] FOREIGN KEY ([DeveloperId]) REFERENCES [Developers] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410073136_Phase2Properties'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410073136_Phase2Properties', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410074910_Phase3Features'
)
BEGIN
    CREATE TABLE [ProjectProgressItems] (
        [Id] int NOT NULL IDENTITY,
        [ClientName] nvarchar(200) NOT NULL,
        [ClientEmail] nvarchar(256) NOT NULL,
        [ProjectTitle] nvarchar(300) NULL,
        [Location] nvarchar(400) NULL,
        [CurrentStage] int NOT NULL,
        [StageNotes] nvarchar(2000) NULL,
        [ProgressPercent] int NOT NULL,
        [EstimatedCompletion] datetimeoffset NULL,
        [AccessToken] nvarchar(100) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_ProjectProgressItems] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410074910_Phase3Features'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410074910_Phase3Features', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410121011_Phase4QuickWins'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [IsVerified] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410121011_Phase4QuickWins'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [PriceHistoryJson] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410121011_Phase4QuickWins'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [SeoDescription] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410121011_Phase4QuickWins'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [SeoTitle] nvarchar(300) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410121011_Phase4QuickWins'
)
BEGIN
    ALTER TABLE [Agents] ADD [LastActiveAt] datetimeoffset NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410121011_Phase4QuickWins'
)
BEGIN
    ALTER TABLE [Agents] ADD [ResponseRatePct] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410121011_Phase4QuickWins'
)
BEGIN
    ALTER TABLE [Agents] ADD [ResponseTime] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410121011_Phase4QuickWins'
)
BEGIN
    CREATE TABLE [AdminNotifications] (
        [Id] int NOT NULL IDENTITY,
        [Type] int NOT NULL,
        [Message] nvarchar(500) NOT NULL,
        [LinkUrl] nvarchar(500) NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_AdminNotifications] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410121011_Phase4QuickWins'
)
BEGIN
    CREATE TABLE [NewsletterSubscribers] (
        [Id] int NOT NULL IDENTITY,
        [Email] nvarchar(256) NOT NULL,
        [Name] nvarchar(200) NULL,
        [Preferences] nvarchar(200) NULL,
        [IsActive] bit NOT NULL,
        [SubscribedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_NewsletterSubscribers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410121011_Phase4QuickWins'
)
BEGIN
    CREATE TABLE [ServiceReports] (
        [Id] int NOT NULL IDENTITY,
        [TicketId] int NULL,
        [ClientEmail] nvarchar(256) NOT NULL,
        [ClientName] nvarchar(200) NOT NULL,
        [Title] nvarchar(300) NOT NULL,
        [Description] nvarchar(4000) NULL,
        [ReportFileUrl] nvarchar(2000) NULL,
        [VisitDate] datetimeoffset NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_ServiceReports] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ServiceReports_MaintenanceTickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [MaintenanceTickets] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410121011_Phase4QuickWins'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NewsletterSubscribers_Email] ON [NewsletterSubscribers] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410121011_Phase4QuickWins'
)
BEGIN
    CREATE INDEX [IX_ServiceReports_TicketId] ON [ServiceReports] ([TicketId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410121011_Phase4QuickWins'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410121011_Phase4QuickWins', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410123052_SprintA_UaeAreasAutocompleteApproval'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [ApprovalStatus] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410123052_SprintA_UaeAreasAutocompleteApproval'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [RejectionReason] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410123052_SprintA_UaeAreasAutocompleteApproval'
)
BEGIN
    CREATE TABLE [UaeAreas] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Emirate] nvarchar(100) NOT NULL,
        [City] nvarchar(100) NULL,
        [Type] int NOT NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_UaeAreas] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410123052_SprintA_UaeAreasAutocompleteApproval'
)
BEGIN
    CREATE INDEX [IX_UaeAreas_Emirate_Name] ON [UaeAreas] ([Emirate], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410123052_SprintA_UaeAreasAutocompleteApproval'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410123052_SprintA_UaeAreasAutocompleteApproval', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410124401_SprintB_ImageUploadAgentCRM'
)
BEGIN
    ALTER TABLE [PropertyListingInquiries] ADD [AgentNote] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410124401_SprintB_ImageUploadAgentCRM'
)
BEGIN
    ALTER TABLE [Agents] ADD [UserId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260410124401_SprintB_ImageUploadAgentCRM'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260410124401_SprintB_ImageUploadAgentCRM', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511000000_PropertyCategoryAndType'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [Emirates] nvarchar(80) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511000000_PropertyCategoryAndType'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [PropertyCategory] nvarchar(20) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511000000_PropertyCategoryAndType'
)
BEGIN
    ALTER TABLE [PropertyListings] ADD [PropertyType] nvarchar(80) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511000000_PropertyCategoryAndType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260511000000_PropertyCategoryAndType', N'8.0.11');
END;
GO

COMMIT;
GO

