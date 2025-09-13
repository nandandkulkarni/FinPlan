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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("finplan");
            modelBuilder.Entity<FinPlanEntity>().ToTable("FinPlan");
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<UserRegistration>().ToTable("UserRegistration");
            base.OnModelCreating(modelBuilder);
        }
    }
}
