using Microsoft.EntityFrameworkCore;
using RealEstateApi.Models;

namespace RealEstateApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<NewsArticle> NewsArticles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.PropertiesAsSeller)
                .WithOne(p => p.Seller)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.PropertiesAsAgent)
                .WithOne(p => p.Agent)
                .HasForeignKey(p => p.AgentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Agent)
                .WithOne(a => a.User)
                .HasForeignKey<Agent>(a => a.Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Favorites)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.NewsArticles)
                .WithOne(n => n.Author)
                .HasForeignKey(n => n.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Property relationships
            modelBuilder.Entity<Property>()
                .HasMany(p => p.Favorites)
                .WithOne(f => f.Property)
                .HasForeignKey(f => f.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Favorite unique constraint
            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.PropertyId })
                .IsUnique();

            // Indexes for performance
            modelBuilder.Entity<Property>()
                .HasIndex(p => p.Status);

            modelBuilder.Entity<Property>()
                .HasIndex(p => p.Location);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed users
            var user1 = new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Email = "seller@example.com",
                PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>().HashPassword(null, "Password123!"),
                FirstName = "John",
                LastName = "Smith",
                Phone = "+1234567890",
                Role = Models.UserRole.Seller,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var user2 = new User
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Email = "agent@example.com",
                PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>().HashPassword(null, "Password123!"),
                FirstName = "Jane",
                LastName = "Doe",
                Phone = "+1234567891",
                Role = Models.UserRole.Agent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<User>().HasData(user1, user2);

            // Seed agent profile
            modelBuilder.Entity<Agent>().HasData(new Agent
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Same as user2
                LicenseNumber = "REA12345",
                Specialties = "Residential,Commercial",
                YearsOfExperience = 5,
                Bio = "Experienced real estate agent specializing in residential properties.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            // Seed properties
            var properties = new[]
            {
                new Property
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Title = "Modern Downtown Apartment",
                    Description = "Beautiful modern apartment in the heart of downtown with stunning city views.",
                    Price = 450000,
                    Location = "Downtown, City Center",
                    Latitude = -37.8136M,
                    Longitude = 144.9631M,
                    PropertyType = Models.PropertyType.Apartment,
                    Bedrooms = 2,
                    Bathrooms = 1,
                    LandSize = 0,
                    Status = Models.PropertyStatus.Active,
                    SellerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    AgentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    ImagesJson = "[\"https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?w=500\"]",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Property
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Title = "Spacious Family Home",
                    Description = "Perfect family home with large backyard, modern kitchen, and 4 bedrooms.",
                    Price = 750000,
                    Location = "Suburbia, Residential Area",
                    Latitude = -37.8200M,
                    Longitude = 144.9500M,
                    PropertyType = Models.PropertyType.House,
                    Bedrooms = 4,
                    Bathrooms = 2,
                    LandSize = 800,
                    Status = Models.PropertyStatus.Active,
                    SellerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    AgentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    ImagesJson = "[\"https://images.unsplash.com/photo-1570129477492-45c003edd2be?w=500\"]",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Property
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Title = "Luxury Townhouse",
                    Description = "Elegant townhouse with premium finishes, private courtyard, and garage.",
                    Price = 620000,
                    Location = "Inner City, Trendy Neighborhood",
                    Latitude = -37.8100M,
                    Longitude = 144.9600M,
                    PropertyType = Models.PropertyType.Townhouse,
                    Bedrooms = 3,
                    Bathrooms = 2,
                    LandSize = 200,
                    Status = Models.PropertyStatus.Active,
                    SellerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    ImagesJson = "[\"https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=500\"]",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Property
                {
                    Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    Title = "Cozy Studio Apartment",
                    Description = "Perfect starter home or investment property in quiet neighborhood.",
                    Price = 280000,
                    Location = "Quiet Suburb, Family Friendly",
                    Latitude = -37.8300M,
                    Longitude = 144.9400M,
                    PropertyType = Models.PropertyType.Apartment,
                    Bedrooms = 1,
                    Bathrooms = 1,
                    LandSize = 0,
                    Status = Models.PropertyStatus.Active,
                    SellerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    ImagesJson = "[\"https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=500\"]",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            modelBuilder.Entity<Property>().HasData(properties);
        }
    }
}