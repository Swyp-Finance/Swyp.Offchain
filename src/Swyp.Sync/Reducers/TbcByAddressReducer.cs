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
public class TbcByAddressReducer(
    IDbContextFactory<SwypDbContext> dbContextFactory,
    IConfiguration configuration
) : IReducer
{
    private static string TbcOnePolicyId => "ab182ed76b669b49ee54a37dee0d0064ad4208a859cc4fdf3f906d87";
    private static string TbcTwoPolicyId => "da3562fad43b7759f679970fb4e0ec07ab5bebe5c703043acda07a3c";
    public async Task RollBackwardAsync(NextResponse response)
    {
        var _dbContext = dbContextFactory.CreateDbContext();
        var rollbackSlot = response.Block.Slot;
        var schema = configuration.GetConnectionString("CardanoContextSchema");
        _dbContext.TbcByAddress.RemoveRange(_dbContext.TbcByAddress.AsNoTracking().Where(b => b.Slot > rollbackSlot));
        await _dbContext.SaveChangesAsync();
        _dbContext.Dispose();
    }

    public async Task RollForwardAsync(NextResponse response)
    {
        var _dbContext = dbContextFactory.CreateDbContext();
        await ProcessInputAsync(response, _dbContext);
        await ProcessOutputAsync(response, _dbContext);
        await _dbContext.SaveChangesAsync();
        _dbContext.Dispose();
    }

    public async Task ProcessInputAsync(NextResponse response, SwypDbContext _dbContext)
    {
        foreach (var txBody in response.Block.TransactionBodies)
        {
            foreach (var input in txBody.Inputs)
            {
                var resolvedOutput = await _dbContext.TransactionOutputs.FirstOrDefaultAsync(o => o.Id == input.Id.ToHex() && o.Index == input.Index);

                if (resolvedOutput is null) continue;

                var currentTbc = _dbContext.TbcByAddress.Local.OrderByDescending(tba => tba.Slot).FirstOrDefault(t => t.Address == resolvedOutput.Address && t.Slot <= response.Block.Slot)
                    ?? await _dbContext.TbcByAddress.OrderByDescending(tba => tba.Slot).FirstOrDefaultAsync(t => t.Address == resolvedOutput.Address && t.Slot <= response.Block.Slot);

                var assets = new Dictionary<string, Dictionary<string, ulong>>();

                if (currentTbc is null)
                {
                    assets.Add(TbcOnePolicyId, []);
                    assets.Add(TbcTwoPolicyId, []);
                }
                else
                {
                    assets = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, ulong>>>(
                        JsonSerializer.Serialize(currentTbc.Amount.MultiAsset)
                    )!;
                }

                var utxo = resolvedOutput!;

                var hasUpdate = ProcessRemoveAssetsForPolicyId(utxo, TbcOnePolicyId, assets) ||
                    ProcessRemoveAssetsForPolicyId(utxo, TbcTwoPolicyId, assets);

                if(!hasUpdate) continue;

                if (currentTbc is null || currentTbc?.Slot != response.Block.Slot)
                {
                    _dbContext.TbcByAddress.Add(
                        new TbcByAddress
                        {
                            Address = resolvedOutput.Address,
                            Slot = response.Block.Slot,
                            Amount = new Value
                            {
                                Coin = 0,
                                MultiAsset = assets
                            }
                        }
                    );
                }
                else
                {
                    currentTbc.Amount = new Value
                    {
                        Coin = 0,
                        MultiAsset = assets
                    };
                }
            }
        }
    }

    public async Task ProcessOutputAsync(NextResponse response, SwypDbContext _dbContext)
    {
        var addressList = response.Block.TransactionBodies.SelectMany(txBody => txBody.Outputs).Select(output => output.Address.ToBech32()).Distinct().ToList();

        var latestTbcByAddress = await _dbContext.TbcByAddress
            .Where(tba => addressList.Contains(tba.Address))
            .GroupBy(tba => tba.Address).Select(g => g.OrderByDescending(tba => tba.Slot).FirstOrDefault())
            .ToListAsync();

        foreach (var txBody in response.Block.TransactionBodies)
        {
            foreach (var output in txBody.Outputs)
            {
                var address = output.Address.ToBech32();
                var slot = response.Block.Slot;

                var currentTbc = _dbContext.TbcByAddress.Local.OrderByDescending(tba => tba.Slot).FirstOrDefault(t => t.Address == address && t.Slot <= slot)
                    ?? latestTbcByAddress.FirstOrDefault(t => t is not null && t.Address == address && t.Slot <= slot);

                var assets = new Dictionary<string, Dictionary<string, ulong>>();

                if (currentTbc is null)
                {
                    assets.Add(TbcOnePolicyId, []);
                    assets.Add(TbcTwoPolicyId, []);
                }
                else
                {
                    assets = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, ulong>>>(
                        JsonSerializer.Serialize(currentTbc.Amount.MultiAsset)
                    )!;
                }

                var utxo = Utils.MapTransactionOutputEntity(txBody.Id.ToHex(), response.Block.Slot, output);

                var hasUpdate = ProcessAddAssetsForPolicyId(utxo, TbcOnePolicyId, assets) ||
                    ProcessAddAssetsForPolicyId(utxo, TbcTwoPolicyId, assets);

                if(!hasUpdate) continue;
                
                if (currentTbc is null || currentTbc?.Slot != slot)
                {
                    _dbContext.TbcByAddress.Add(
                        new TbcByAddress
                        {
                            Address = address,
                            Slot = slot,
                            Amount = new Value
                            {
                                Coin = 0,
                                MultiAsset = assets
                            }
                        }
                    );
                }
                else
                {
                    currentTbc.Amount = new Value
                    {
                        Coin = 0,
                        MultiAsset = assets
                    };
                }
            }
        }
    }

    private static bool ProcessAddAssetsForPolicyId(TransactionOutput utxo, string policyId, Dictionary<string, Dictionary<string, ulong>> assets)
    {
        var hasUpdate = false;
        if (utxo.Amount.MultiAsset.TryGetValue(policyId, out Dictionary<string, ulong>? value))
        {
            var amount = value.Where(v => v.Key != string.Empty).ToDictionary(k => k.Key, v => v.Value);
            if (amount.Count > 0)
            {
                // Merge with existing assets
                foreach (var (key, v) in amount)
                {
                    if (assets[policyId].TryGetValue(key, out ulong currentValue))
                    {
                        assets[policyId][key] = currentValue + v;
                    }
                    else
                    {
                        assets[policyId].Add(key, v);
                    }
                }

                hasUpdate = true;
            }
        }

        return hasUpdate;
    }

    // Same as ProcessAddAssetsForPolicyId but instead of adding the asset into the dictionary it removes it
    // if the asset value is 0 then it removes the asset from the dictionary
    private static bool ProcessRemoveAssetsForPolicyId(TransactionOutput utxo, string policyId, Dictionary<string, Dictionary<string, ulong>> assets)
    {
        var hasUpdate = false;
        if (utxo.Amount.MultiAsset.TryGetValue(policyId, out Dictionary<string, ulong>? value))
        {
            var amount = value.Where(v => v.Key != string.Empty).ToDictionary(k => k.Key, v => v.Value);
            if (amount.Count > 0)
            {
                // Merge with existing assets
                foreach (var (key, v) in amount)
                {
                    if (assets[policyId].TryGetValue(key, out ulong currentValue))
                    {
                        var newValue = currentValue - v;
                        if (newValue == 0)
                        {
                            assets[policyId].Remove(key);
                        }
                        else
                        {
                            assets[policyId][key] = newValue;
                        }
                    }
                }
                hasUpdate = true;
            }
        }

        return hasUpdate;
    }
}