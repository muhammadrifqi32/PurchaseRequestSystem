using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class StatusService : IStatusService
{
    private readonly IGenericRepository<Status> _repository;

    public StatusService(IGenericRepository<Status> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<StatusResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.QueryActive()
            .AsNoTracking()
            .OrderBy(x => x.StatusName)
            .Select(x => ToResponseDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<StatusResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.StatusId == id, cancellationToken);

        return entity is null
            ? throw new NotFoundException("Status", id)
            : ToResponseDto(entity);
    }

    public async Task<StatusResponseDto> CreateAsync(CreateStatusDto dto, CancellationToken cancellationToken = default)
    {
        var name = InputHelper.Required(dto.StatusName, nameof(dto.StatusName)).ToUpperInvariant();

        if (await _repository.AnyAsync(x => x.StatusName == name, cancellationToken))
            throw new BadRequestException("Status name already exists.");

        var entity = new Status
        {
            StatusId = Guid.NewGuid(),
            StatusName = name,
            IsDeleted = false
        };

        AuditHelper.SetCreatedAudit(entity);
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task<StatusResponseDto> UpdateAsync(Guid id, UpdateStatusDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.StatusId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("Status", id);

        var name = InputHelper.Required(dto.StatusName, nameof(dto.StatusName)).ToUpperInvariant();

        if (await _repository.AnyAsync(x => x.StatusName == name && x.StatusId != id, cancellationToken))
            throw new BadRequestException("Status name already exists.");

        entity.StatusName = name;
        AuditHelper.SetUpdatedAudit(entity);

        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.StatusId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("Status", id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static StatusResponseDto ToResponseDto(Status entity) => new()
    {
        StatusId = entity.StatusId,
        StatusName = entity.StatusName,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
