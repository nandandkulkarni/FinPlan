using Microsoft.EntityFrameworkCore;

namespace FinPlan.ApiService.Data
{

    public class FinPlanDbContext : DbContext
    {
        public FinPlanDbContext(DbContextOptions<FinPlanDbContext> options) : base(options) { }

        public DbSet<FinPlanEntity> FinPlans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("finplan");
            modelBuilder.Entity<FinPlanEntity>().ToTable("FinPlan");
            base.OnModelCreating(modelBuilder);
        }
    }
}
