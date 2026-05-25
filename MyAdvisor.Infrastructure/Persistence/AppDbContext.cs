using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyAdvisor.Domain.Entities;
using MyAdvisor.Domain.Enums;
using MyAdvisor.Infrastructure.Identity;

namespace MyAdvisor.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public DbSet<User> DomainUsers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<FinancialDiary> FinancialDiaries { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionAiLog> TransactionAiLogs { get; set; }
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
        public DbSet<SpendingStatistic> SpendingStatistics { get; set; }
        public DbSet<CategoryStatistic> CategoryStatistics { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("DomainUsers");

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId);

            modelBuilder.Entity<Category>()
                .Navigation(c => c.SubCategories)
                .HasField("_subCategories")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<FinancialDiary>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Diary)
                .WithMany(d => d.Transactions)
                .HasForeignKey(t => t.DiaryId);

            modelBuilder.Entity<FinancialDiary>()
                .Navigation(d => d.Transactions)
                .HasField("_transactions")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany()
                .HasForeignKey(t => t.CategoryId);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.PaymentMethod)
                .HasConversion<string>();

            modelBuilder.Entity<TransactionAiLog>()
                .HasOne(l => l.Transaction)
                .WithMany()
                .HasForeignKey(l => l.TransactionId);

            modelBuilder.Entity<TransactionAiLog>()
                .HasOne(l => l.AiCategory)
                .WithMany()
                .HasForeignKey(l => l.AiCategoryId);

            modelBuilder.Entity<RecurringTransaction>()
                .Property(r => r.Frequency)
                .HasConversion<string>();

            modelBuilder.Entity<RecurringTransaction>()
                .Property(r => r.PaymentMethod)
                .HasConversion<string>();

            modelBuilder.Entity<RecurringTransaction>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<RecurringTransaction>()
                .HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId);

            modelBuilder.Entity<SpendingStatistic>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<CategoryStatistic>()
                .HasOne(cs => cs.Statistics)
                .WithMany(s => s.CategoryBreakdown)
                .HasForeignKey(cs => cs.StatisticsId);

            modelBuilder.Entity<SpendingStatistic>()
                .Navigation(s => s.CategoryBreakdown)
                .HasField("_categoryBreakdown")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<CategoryStatistic>()
                .Property(cs => cs.CategoryName)
                .HasMaxLength(255);

            modelBuilder.Entity<CategoryStatistic>()
                .HasOne(cs => cs.Category)
                .WithMany()
                .HasForeignKey(cs => cs.CategoryId)
                .IsRequired(false);

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Token).IsRequired();
                entity.HasIndex(x => x.Token).IsUnique();
            });
        }
    }
}
