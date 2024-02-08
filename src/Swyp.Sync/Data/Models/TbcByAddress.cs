using Cardano.Sync.Data.Models;

namespace Swyp.Sync.Data.Models;

public record class TbcByAddress
{
    public string Address { get; set; } = default!;
    public Value Amount { get; set; } = default!;
    public ulong Slot { get; set; }
};