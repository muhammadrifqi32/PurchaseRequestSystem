using PurchaseRequestSystem.DTOs;

namespace PurchaseRequestSystem.Interfaces;

public interface IPurchaseOrderService
{
    Task<List<PurchaseOrderResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PurchaseOrderResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PurchaseOrderResponseDto> GetByPurchaseRequestIdAsync(Guid purchaseRequestId, CancellationToken cancellationToken = default);
    Task<PurchaseOrderResponseDto> GenerateFromPurchaseRequestAsync(Guid purchaseRequestId, GeneratePurchaseOrderDto dto, CancellationToken cancellationToken = default);
    Task<PurchaseOrderResponseDto> UpdateAsync(Guid id, UpdatePurchaseOrderDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
