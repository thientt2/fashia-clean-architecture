namespace Fashia.Domain.Enums;

public enum InventoryTransactionType
{
    Import = 0,
    Sale = 1,
    Return = 2,
    Adjustment = 3,
    Transfer = 4,
    Initialize = 5,
    Increase = 6,
    Decrease = 7,
    TransferOut = 8,
    TransferIn = 9,
    Reserve = 10,
    ReleaseReservation = 11,
    CommitReservation = 12,
}
