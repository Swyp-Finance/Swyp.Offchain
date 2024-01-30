using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Swyp.Data.Models;

namespace Swyp.Data;

public class SwypDbContext(DbContextOptions<SwypDbContext> options, IConfiguration configuration) : DbContext(options)
{
    private readonly IConfiguration _configuration = configuration;
    public DbSet<Block> Blocks { get; set; }
    public DbSet<TransactionOutput> TransactionOutputs { get; set; }
    public DbSet<TbcByAddress> TbcByAddress { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_configuration.GetConnectionString("SwypContextSchema"));
        modelBuilder.Entity<Block>().HasKey(b => new { b.Id, b.Number, b.Slot });
        modelBuilder.Entity<TransactionOutput>().HasKey(item => new { item.Id, item.Index });
        modelBuilder.Entity<TransactionOutput>().OwnsOne(item => item.Amount);
        modelBuilder.Entity<TbcByAddress>().HasKey(item => new { item.Address, item.Slot });
        modelBuilder.Entity<TbcByAddress>().OwnsOne(item => item.Amount);
        base.OnModelCreating(modelBuilder);
    }
}