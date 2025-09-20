using FinPlan.ApiService.Data.FinPlan.ApiService.Data;
using Microsoft.EntityFrameworkCore;

namespace FinPlan.ApiService.Data
{

    public class FinPlanDbContext : DbContext
    {
        public FinPlanDbContext(DbContextOptions<FinPlanDbContext> options) : base(options) { }

        public DbSet<FinPlanEntity> FinPlans { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRegistration> UserRegistrations { get; set; }
        public DbSet<SurveyResponse> SurveyResponses { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("finplan");
            modelBuilder.Entity<FinPlanEntity>().ToTable("FinPlan");
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<UserRegistration>().ToTable("UserRegistration");

            // Ensure SurveyResponses mapping and avoid EF Core generating OUTPUT clauses by
            // configuring the key as not value-generated. This prevents SQL Server INSERT/UPDATE
            // statements from including an OUTPUT clause which is blocked when the table has triggers.
            modelBuilder.Entity<SurveyResponse>(b =>
            {
                b.ToTable("SurveyResponses");
                b.HasKey(e => e.Id);
                b.Property(e => e.Id).ValueGeneratedNever();

                // Make sure CreatedAt/UpdatedAt are treated as regular properties (not computed)
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

            base.OnModelCreating(modelBuilder);
        }
    }
}