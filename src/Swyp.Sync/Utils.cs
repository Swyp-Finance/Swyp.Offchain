using PallasDotnet.Models;
using TransactionOutputEntity = Swyp.Data.Models.TransactionOutput;
using ValueEntity = Swyp.Data.Models.Value;

namespace Swyp;

public static class Utils
{
    public static TransactionOutputEntity MapTransactionOutput(string TransactionId, ulong slot, TransactionOutput output)
    {
        return new TransactionOutputEntity
        {
            Id = TransactionId,
            Address = output.Address.ToBech32(),
            Slot = slot,
            Index = Convert.ToUInt32(output.Index),
            Amount = new ValueEntity
            {
                Coin = output.Amount.Coin,
                MultiAsset = output.Amount.MultiAsset.ToDictionary(
                    k => k.Key.ToHex(),
                    v => v.Value.ToDictionary(
                        k => k.Key.ToHex(),
                        v => v.Value
                    )
                )
            }
        };
    }
}