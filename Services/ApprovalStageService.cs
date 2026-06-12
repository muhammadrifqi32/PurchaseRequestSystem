using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class ApprovalStageService : IApprovalStageService
{
    private readonly IGenericRepository<ApprovalStage> _repository;

    public ApprovalStageService(IGenericRepository<ApprovalStage> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ApprovalStageResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.QueryActive()
            .AsNoTracking()
            .OrderBy(x => x.StageName)
            .Select(x => ToResponseDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<ApprovalStageResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApprovalStageId == id, cancellationToken);

        return entity is null
            ? throw new NotFoundException("ApprovalStage", id)
            : ToResponseDto(entity);
    }

    public async Task<ApprovalStageResponseDto> CreateAsync(CreateApprovalStageDto dto, CancellationToken cancellationToken = default)
    {
        var name = InputHelper.Required(dto.StageName, nameof(dto.StageName));

        if (await _repository.AnyAsync(x => x.StageName == name, cancellationToken))
            throw new BadRequestException("Approval stage name already exists.");

        var entity = new ApprovalStage
        {
            ApprovalStageId = Guid.NewGuid(),
            StageName = name,
            IsDeleted = false
        };

        AuditHelper.SetCreatedAudit(entity);
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task<ApprovalStageResponseDto> UpdateAsync(Guid id, UpdateApprovalStageDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.ApprovalStageId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("ApprovalStage", id);

        var name = InputHelper.Required(dto.StageName, nameof(dto.StageName));

        if (await _repository.AnyAsync(x => x.StageName == name && x.ApprovalStageId != id, cancellationToken))
            throw new BadRequestException("Approval stage name already exists.");

        entity.StageName = name;
        AuditHelper.SetUpdatedAudit(entity);

        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.ApprovalStageId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("ApprovalStage", id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static ApprovalStageResponseDto ToResponseDto(ApprovalStage entity) => new()
    {
        ApprovalStageId = entity.ApprovalStageId,
        StageName = entity.StageName,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
