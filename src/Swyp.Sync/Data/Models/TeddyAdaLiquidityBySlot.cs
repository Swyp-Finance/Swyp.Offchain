using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Cardano.Sync.Data.Models;
using Cardano.Sync.Data.Models.Datums;

namespace Swyp.Sync.Data.Models;

public record TeddyAdaLiquidityBySlot
{
    public ulong Slot { get; set; }
    public string TxHash { get; set; } = default!;
    public ulong TxIndex { get; set; }
    public Value Amount { get; set; } = default!;

    [NotMapped]
    public SpectrumLiquidityPool LiquidityPool { get; set; } = default!;

    public JsonElement LiquidityPoolJson
    {
        get
        {
            var jsonString = JsonSerializer.Serialize(LiquidityPool);
            return JsonDocument.Parse(jsonString).RootElement;
        }

        set
        {
            if (value.ValueKind == JsonValueKind.Undefined || value.ValueKind == JsonValueKind.Null)
            {
                throw new Exception("Invalid LiquidityPool");
            }
            else
            {
                LiquidityPool = JsonSerializer.Deserialize<SpectrumLiquidityPool>(value.GetRawText()) ?? throw new Exception("Invalid StakePoolJson");
            }
        }
    }
}