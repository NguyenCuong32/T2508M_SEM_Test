using Microsoft.EntityFrameworkCore;

namespace ComicSystem.Models
{
    public class ComicDbContext :DbContext
    {
        public ComicDbContext(DbContextOptions<ComicDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<ComicBook> ComicBooks => Set<ComicBook>();
        public DbSet<Rental> Rentals => Set<Rental>();
        public DbSet<RentalDetail> RentalDetails => Set<RentalDetail>();
    }
}
