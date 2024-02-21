using System.Text.Json;
using Cardano.Sync;
using Cardano.Sync.Reducers;
using Microsoft.EntityFrameworkCore;
using PallasDotnet.Models;
using Swyp.Sync.Data;
using Swyp.Sync.Data.Models;
using Value = Cardano.Sync.Data.Models.Value;
using TransactionOutput = Cardano.Sync.Data.Models.TransactionOutput;
using Address = CardanoSharp.Wallet.Models.Addresses.Address;
using CardanoSharp.Wallet.Extensions.Models;
using Cardano.Sync.Data.Models.Datums;

namespace Swyp.Sync.Reducers;

[ReducerDepends(typeof(TransactionOutputReducer<>))]
public class TeddyAdaLiquidityBySlotReducer(
    IDbContextFactory<SwypDbContext> dbContextFactory,
    IConfiguration configuration,
    ILogger<TeddyAdaLiquidityBySlotReducer> logger
) : IReducer
{
    private readonly string _teddyAdaPoolIdentityPolicyId = "1c0ad45d50bd0a8c9bb851a9c59c3cb3e1ab2e2a29bd4d61b0e967ca";
    private readonly string _teddyAdaPoolIdentityAssetName = "544544595f4144415f504f4f4c5f4944454e54495459";

    public async Task RollForwardAsync(NextResponse response)
    {
        var _dbContext = dbContextFactory.CreateDbContext();
        await ProcessOutputAync(response, _dbContext);
        await _dbContext.SaveChangesAsync();
        _dbContext.Dispose();
    }

    public async Task RollBackwardAsync(NextResponse response)
    {
        var _dbContext = dbContextFactory.CreateDbContext();
        var rollbackSlot = response.Block.Slot;
        _dbContext.TeddyAdaLiquidityBySlot.RemoveRange(_dbContext.TeddyAdaLiquidityBySlot.AsNoTracking().Where(b => b.Slot > rollbackSlot));
        await _dbContext.SaveChangesAsync();
        _dbContext.Dispose();
    }

    private Task ProcessOutputAync(NextResponse response, SwypDbContext _dbContext)
    {
        foreach (var txBody in response.Block.TransactionBodies)
        {
            foreach (var output in txBody.Outputs)
            {
                var addressBech32 = output.Address.ToBech32();
                if (addressBech32.StartsWith("addr"))
                {
                    var address = new Address(addressBech32);
                    var pkh = Convert.ToHexString(address.GetPublicKeyHash()).ToLowerInvariant();
                    if (pkh == configuration["TeddySwapPoolValidatorHash"])
                    {
                        var outputEntity = Utils.MapTransactionOutputEntity(txBody.Id.ToHex(), response.Block.Slot, output);
                        outputEntity.Amount.MultiAsset.TryGetValue(_teddyAdaPoolIdentityPolicyId, out var tokenBundle);
                        var hasAdaTeddyPoolIdentityAsset = tokenBundle?.ContainsKey(_teddyAdaPoolIdentityAssetName) ?? false;

                        if (hasAdaTeddyPoolIdentityAsset && output.Datum is not null && output.Datum.Type == PallasDotnet.Models.DatumType.InlineDatum)
                        {
                            var datum = output.Datum.Data;
                            try
                            {
                                var liquidityPool = CborConverter.Deserialize<SpectrumLiquidityPool>(datum);
                                var teddyAdaLiquidityBySlot = new TeddyAdaLiquidityBySlot()
                                {
                                    Slot = response.Block.Slot,
                                    TxHash = txBody.Id.ToHex(),
                                    TxIndex = output.Index,
                                    Amount = outputEntity.Amount,
                                    LiquidityPool = liquidityPool
                                };

                                _dbContext.TeddyAdaLiquidityBySlot.Add(teddyAdaLiquidityBySlot);
                            }
                            catch
                            {
                                logger.LogError("Error deserializing liquidity pool datum: {datum} for {txHash}#{txIndex}",
                                    Convert.ToHexString(datum).ToLowerInvariant(),
                                    txBody.Id.ToHex(),
                                    output.Index
                                );
                            }
                        }
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
}