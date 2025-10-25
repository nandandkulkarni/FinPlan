using FinPlan.Web.Data.Models;
using FinPlan.Shared.Models.LivingCosts;
using Microsoft.EntityFrameworkCore;

namespace FinPlan.Web.Data
{
    public class FinPlanDbContext : DbContext
    {
        public FinPlanDbContext(DbContextOptions<FinPlanDbContext> options) : base(options) { }

        public DbSet<FinPlanEntity> FinPlans { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRegistration> UserRegistrations { get; set; }
        public DbSet<SurveyResponse> SurveyResponses { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        
        // Cost of Living Templates
        public DbSet<CityTemplate> CityTemplates { get; set; }
        public DbSet<DemographicProfile> DemographicProfiles { get; set; }
        public DbSet<UserDemographics> UserDemographics { get; set; }

        // Page views tracking
        public DbSet<PageView> PageViews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("finplan");
            
            // Configure FinPlanEntity
            modelBuilder.Entity<FinPlanEntity>(b =>
            {
                b.ToTable("FinPlan");
                b.HasKey(e => e.Id);
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.CreatedAt).ValueGeneratedNever();
                b.Property(e => e.UpdatedAt).ValueGeneratedNever();
                b.Property(e => e.IpAddress).HasMaxLength(45); // Support IPv6 addresses
            });
            
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<UserRegistration>().ToTable("UserRegistration");

            // Ensure SurveyResponses mapping
            modelBuilder.Entity<SurveyResponse>(b =>
            {
                b.ToTable("SurveyResponses");
                b.HasKey(e => e.Id);
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.CreatedAt).ValueGeneratedNever();
                b.Property(e => e.UpdatedAt).ValueGeneratedNever();
            });

            // Configure ContactMessage entity
            modelBuilder.Entity<ContactMessage>(b =>
            {
                b.ToTable("ContactMessages");
                b.HasKey(e => e.Id);
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.CreatedAt).ValueGeneratedNever();
            });

            // Configure CityTemplate entity
            modelBuilder.Entity<CityTemplate>(b =>
            {
                b.ToTable("CityTemplates");
                b.HasKey(e => e.CityId);
                b.Property(e => e.CityId).ValueGeneratedNever().HasMaxLength(50);
                b.Property(e => e.CityName).IsRequired().HasMaxLength(200);
                b.Property(e => e.Country).IsRequired().HasMaxLength(100);
                b.Property(e => e.Currency).IsRequired().HasMaxLength(10);
                b.Property(e => e.CostOfLivingIndex).HasPrecision(10, 2);
                b.Property(e => e.CreatedAt).ValueGeneratedNever();
                b.Property(e => e.UpdatedAt).ValueGeneratedNever();
                b.Property(e => e.CreatedBy).HasMaxLength(100);
            });

            // Configure DemographicProfile entity
            modelBuilder.Entity<DemographicProfile>(b =>
            {
                b.ToTable("DemographicProfiles");
                b.HasKey(e => e.ProfileId);
                b.Property(e => e.ProfileId).ValueGeneratedNever().HasMaxLength(50);
                b.Property(e => e.CityId).IsRequired().HasMaxLength(50);
                b.Property(e => e.ProfileName).IsRequired().HasMaxLength(200);
                b.Property(e => e.ChildrenAgesJSON).HasColumnType("nvarchar(max)");
                b.Property(e => e.SampleExpensesJSON).HasColumnType("nvarchar(max)");
                b.Property(e => e.CreatedAt).ValueGeneratedNever();
                b.Property(e => e.UpdatedAt).ValueGeneratedNever();
                
                // Foreign key relationship
                b.HasOne<CityTemplate>()
                    .WithMany()
                    .HasForeignKey(e => e.CityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UserDemographics entity
            modelBuilder.Entity<UserDemographics>(b =>
            {
                b.ToTable("UserDemographics");
                b.HasKey(e => e.UserGuid);
                b.Property(e => e.UserGuid).ValueGeneratedNever().HasMaxLength(50);
                b.Property(e => e.ChildrenAgesJSON).HasColumnType("nvarchar(max)");
                b.Property(e => e.PreferredCurrency).HasMaxLength(10);
                b.Property(e => e.SelectedCityId).HasMaxLength(50);
                b.Property(e => e.CreatedAt).ValueGeneratedNever();
                b.Property(e => e.UpdatedAt).ValueGeneratedNever();
                
                // Foreign key relationship (nullable)
                b.HasOne<CityTemplate>()
                    .WithMany()
                    .HasForeignKey(e => e.SelectedCityId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
            });

            // Configure PageView entity
            modelBuilder.Entity<PageView>(b =>
            {
                b.ToTable("PageViews");
                b.HasKey(e => e.Id);
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.Page).IsRequired().HasMaxLength(120);
                b.Property(e => e.Route).HasMaxLength(512);
                b.Property(e => e.UserGuid).HasMaxLength(100);
                b.Property(e => e.IpAddress).HasMaxLength(45);
                b.Property(e => e.UserAgent).HasMaxLength(512);
                b.Property(e => e.Referrer).HasMaxLength(512);
                b.Property(e => e.CreatedAt).ValueGeneratedNever();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
