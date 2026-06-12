using PurchaseRequestSystem.DTOs;

namespace PurchaseRequestSystem.Interfaces;

public interface IProposalService
{
    Task<IReadOnlyList<ProposalResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProposalResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProposalResponseDto> CreateAsync(CreateProposalDto dto, CancellationToken cancellationToken = default);
    Task<ProposalResponseDto> UpdateAsync(Guid id, UpdateProposalDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SubmitResponseDto> SubmitAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProposalResponseDto> ReviewAsync(Guid id, ReviewProposalDto dto, CancellationToken cancellationToken = default);
}
