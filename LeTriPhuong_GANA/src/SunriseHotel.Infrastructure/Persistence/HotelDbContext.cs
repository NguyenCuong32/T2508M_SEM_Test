using Microsoft.EntityFrameworkCore;
using SunriseHotel.Application.Abstractions.Persistence;
using SunriseHotel.Domain.Entities;

namespace SunriseHotel.Infrastructure.Persistence;

public class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
}
