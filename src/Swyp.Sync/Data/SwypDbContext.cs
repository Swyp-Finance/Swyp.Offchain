using Cardano.Sync.Data;
using Microsoft.EntityFrameworkCore;
using Swyp.Sync.Data.Models;

namespace Swyp.Sync.Data;

public class SwypDbContext
(
    DbContextOptions<SwypDbContext> options,
    IConfiguration configuration
) : CardanoDbContext(options, configuration)
{
    public DbSet<TbcByAddress> TbcByAddress { get; set; }
    public DbSet<TeddyByAddress> TeddyByAddress { get; set; }
    
    override protected void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TbcByAddress>().HasKey(item => new { item.Address, item.Slot });
        modelBuilder.Entity<TbcByAddress>().OwnsOne(item => item.Amount);
        modelBuilder.Entity<TeddyByAddress>().HasKey(item => new { item.Address, item.Slot });
        base.OnModelCreating(modelBuilder);
    }
}
