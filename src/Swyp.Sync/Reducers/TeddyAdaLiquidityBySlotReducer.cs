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
    public async Task RollForwardAsync(NextResponse response)
    {
        var _dbContext = dbContextFactory.CreateDbContext();
        await ProcessInputAsync(response, _dbContext);
        await ProcessOutputAync(response);
        await _dbContext.SaveChangesAsync();
        _dbContext.Dispose();
    }

    public async Task RollBackwardAsync(NextResponse response)
    {
    }

    public async Task ProcessInputAsync(NextResponse response, SwypDbContext _dbContext)
    {
    }

    private Task ProcessOutputAync(NextResponse response)
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
                        if (output.Datum is not null && output.Datum.Type == PallasDotnet.Models.DatumType.InlineDatum)
                        {
                            var datum = output.Datum.Data;
                            try
                            {
                                var liquidityPool = CborConverter.Deserialize<SpectrumLiquidityPool>(datum);
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