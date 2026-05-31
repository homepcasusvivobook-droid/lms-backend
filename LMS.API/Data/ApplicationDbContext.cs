using LMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Member> Members { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<Rack> Racks { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<BookIssue> BookIssues { get; set; }
        public DbSet<BookReturn> BookReturns { get; set; }
        public DbSet<BookPurchase> BookPurchases { get; set; }
        public DbSet<BookPurchaseDetail> BookPurchaseDetails { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<MembershipRenewal> MembershipRenewals { get; set; }
        public DbSet<MemberType> MemberTypes { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BookCopy>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCopies)
                .HasForeignKey(bc => bc.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookCopy>()
                .HasOne(bc => bc.Shelf)
                .WithMany()
                .HasForeignKey(bc => bc.ShelfId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookCopy>()
                .HasOne(bc => bc.Rack)
                .WithMany()
                .HasForeignKey(bc => bc.RackId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookIssue>()
                .HasOne(bi => bi.Member)
                .WithMany()
                .HasForeignKey(bi => bi.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookIssue>()
                .HasOne(bi => bi.BookCopy)
                .WithMany()
                .HasForeignKey(bi => bi.BookCopyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookReturn>()
                .HasOne(br => br.BookIssue)
                .WithMany()
                .HasForeignKey(br => br.BookIssueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookReturn>()
                .Property(br => br.FineAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BookPurchase>()
                .Property(x => x.TotalCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BookPurchaseDetail>()
                .Property(x => x.Cost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BookPurchaseDetail>()
                .HasOne(x => x.BookPurchase)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.BookPurchaseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookPurchaseDetail>()
                .HasOne(x => x.Book)
                .WithMany()
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookPurchaseDetail>()
                .HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookPurchaseDetail>()
                .HasOne(x => x.Shelf)
                .WithMany()
                .HasForeignKey(x => x.ShelfId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookPurchaseDetail>()
                .HasOne(x => x.Rack)
                .WithMany()
                .HasForeignKey(x => x.RackId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookPurchase>()
                .Property(x => x.TotalCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BookPurchase>()
                .Property(x => x.ConversionRate)
                .HasPrecision(18, 6);

            modelBuilder.Entity<BookPurchase>()
                .Property(x => x.TotalCostAed)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BookPurchaseDetail>()
                .Property(x => x.Cost)
                .HasPrecision(18, 2);
        }
    }
}