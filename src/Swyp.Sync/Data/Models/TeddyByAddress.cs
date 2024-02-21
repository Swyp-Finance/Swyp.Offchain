namespace Swyp.Sync.Data.Models;

public record TeddyByAddress
{
    public string Address { get; set; } = default!;
    public ulong Amount { get; set; }
    public ulong Slot { get; set; }
}