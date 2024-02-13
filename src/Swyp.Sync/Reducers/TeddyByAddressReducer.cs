using System.Text.Json;
using Cardano.Sync;
using Cardano.Sync.Reducers;
using Microsoft.EntityFrameworkCore;
using PallasDotnet.Models;
using Swyp.Sync.Data;
using Swyp.Sync.Data.Models;
using Value = Cardano.Sync.Data.Models.Value;
using TransactionOutput = Cardano.Sync.Data.Models.TransactionOutput;

namespace Swyp.Sync.Reducers;

[ReducerDepends(typeof(TransactionOutputReducer<>))]
public class TeddyAddressReducer(
    IDbContextFactory<SwypDbContext> dbContextFactory,
    IConfiguration configuration
) : IReducer
{
    private static string TedyPolicyId => "f6696363e9196289ef4f2b4bf34bc8acca5352cdc7509647afe6888f";
    private static string TedyAssetName => "54454459";

    public async Task RollForwardAsync(NextResponse response)
    {
        var _dbContext = dbContextFactory.CreateDbContext();
        await ProcessInputAsync(response, _dbContext);
        await ProcessOutputAsync(response, _dbContext);
        await _dbContext.SaveChangesAsync();
        _dbContext.Dispose();
    }

    public async Task RollBackwardAsync(NextResponse response)
    {
        var _dbContext = dbContextFactory.CreateDbContext();
        var rollbackSlot = response.Block.Slot;
        var schema = configuration.GetConnectionString("CardanoContextSchema");
        _dbContext.TeddyByAddress.RemoveRange(_dbContext.TeddyByAddress.AsNoTracking().Where(b => b.Slot > rollbackSlot));
        await _dbContext.SaveChangesAsync();
        _dbContext.Dispose();
    }

    public async Task ProcessInputAsync(NextResponse response, SwypDbContext _dbContext)
    {
        foreach (var txBody in response.Block.TransactionBodies)
        {
            foreach (var input in txBody.Inputs)
            {
                var resolvedInputOutput = await _dbContext.TransactionOutputs.FirstOrDefaultAsync(o => o.Id == input.Id.ToHex() && o.Index == input.Index);
                if (resolvedInputOutput is not null)
                {
                    if (resolvedInputOutput.Amount.MultiAsset.TryGetValue(TedyPolicyId, out Dictionary<string, ulong>? tokenBundle))
                    {
                        var teddyAsset = tokenBundle?.FirstOrDefault(a => a.Key == TedyAssetName).Value;

                        if (teddyAsset is not null)
                        {
                            var latestTedyByAddress = _dbContext.TeddyByAddress.Local.OrderByDescending(tba => tba.Slot).FirstOrDefault(tba => tba.Address == resolvedInputOutput.Address) ??
                                await _dbContext.TeddyByAddress.OrderByDescending(tba => tba.Slot).FirstOrDefaultAsync(tba => tba.Address == resolvedInputOutput.Address);

                            var latestAmount = latestTedyByAddress?.Amount - teddyAsset ?? 0 - (ulong)teddyAsset;

                            if (latestTedyByAddress is not null && latestTedyByAddress.Slot == response.Block.Slot)
                            {
                                latestTedyByAddress.Amount = latestAmount;
                            }
                            else
                            {
                                _dbContext.TeddyByAddress.Add(new()
                                {
                                    Address = resolvedInputOutput.Address,
                                    Amount = latestAmount,
                                    Slot = response.Block.Slot
                                });
                            }
                        }
                    }
                }
            }
        }
    }

    public async Task ProcessOutputAsync(NextResponse response, SwypDbContext _dbContext)
    {
        var uniqueAddresses = response.Block.TransactionBodies
            .SelectMany(txBody => txBody.Outputs)
            .Select(output => output.Address.ToBech32())
            .Distinct();

        var latestTeddyByAddresses = await _dbContext.TeddyByAddress
            .Where(tba => uniqueAddresses.Contains(tba.Address))
            .GroupBy(tba => tba.Address)
            .Select(g => g.OrderByDescending(tba => tba.Slot).First())
            .ToListAsync();

        foreach (var txBody in response.Block.TransactionBodies)
        {
            foreach (var output in txBody.Outputs)
            {
                var outputEntity = Utils.MapTransactionOutputEntity(txBody.Id.ToHex(), response.Block.Slot, output);
                if (outputEntity.Amount.MultiAsset.TryGetValue(TedyPolicyId, out Dictionary<string, ulong>? tokenBundle))
                {
                    var teddyAsset = tokenBundle?.FirstOrDefault(a => a.Key == TedyAssetName).Value;

                    if (teddyAsset is not null)
                    {
                        var latestTeddyByAddress = _dbContext.TeddyByAddress.Local.OrderByDescending(tba => tba.Slot).FirstOrDefault(tba => tba.Address == outputEntity.Address) 
                            ?? latestTeddyByAddresses.FirstOrDefault(tba => tba.Address == outputEntity.Address);

                        var latestAmount = latestTeddyByAddress?.Amount + teddyAsset ?? (ulong)teddyAsset;

                        if (latestTeddyByAddress is not null && latestTeddyByAddress.Slot == response.Block.Slot)
                        {
                            latestTeddyByAddress.Amount = latestAmount;
                        }
                        else
                        {
                            _dbContext.TeddyByAddress.Add(new()
                            {
                                Address = outputEntity.Address,
                                Amount = latestAmount,
                                Slot = response.Block.Slot
                            });
                        }
                    }
                }
            }
        }
    }
}