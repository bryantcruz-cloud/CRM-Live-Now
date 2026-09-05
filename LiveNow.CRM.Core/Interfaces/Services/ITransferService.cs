using LiveNow.CRM.Core.DTOs;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface ITransferService
{
    Task<IReadOnlyList<SlotTransferDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SlotTransferDto> TransferSlotAsync(
        Guid slotId,
        SlotTransferRequestDto dto,
        CancellationToken cancellationToken = default);
}
