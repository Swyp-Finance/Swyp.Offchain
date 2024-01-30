using Microsoft.EntityFrameworkCore;
using PallasDotnet.Models;
using Swyp.Data;
using BlockEntity = Swyp.Data.Models.Block;
namespace Swyp.Sync.Reducers;

public class BlockReducer(IDbContextFactory<SwypDbContext> dbContextFactory, ILogger<BlockReducer> logger) : IBlockReducer
{
    private SwypDbContext _dbContext = default!;
    private readonly ILogger<BlockReducer> _logger = logger;

    public async Task RollBackwardAsync(NextResponse response)
    {
        _dbContext = dbContextFactory.CreateDbContext();
        _dbContext.Blocks.RemoveRange(_dbContext.Blocks.AsNoTracking().Where(b => b.Slot > response.Block.Slot));
        await _dbContext.SaveChangesAsync();
        _dbContext.Dispose();
    }

    public async Task RollForwardAsync(NextResponse response)
    {
        _dbContext = dbContextFactory.CreateDbContext();
        _dbContext.Blocks.Add(new BlockEntity(
            response.Block.Hash.ToHex(),
            response.Block.Number,
            response.Block.Slot
        ));

        await _dbContext.SaveChangesAsync();
        _dbContext.Dispose();
    }
}