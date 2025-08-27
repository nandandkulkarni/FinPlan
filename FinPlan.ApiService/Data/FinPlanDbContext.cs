using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FinPlan.ApiService.Data
{
    // Entity for finplan.FinPlan table
    public class FinPlanEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string UserGuid { get; set; } = string.Empty;
        [Required]
        public string CalculatorType { get; set; } = string.Empty;
        [Required]
        public string Data { get; set; } = string.Empty;
    }

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
