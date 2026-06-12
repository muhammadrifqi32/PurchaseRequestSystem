using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class RequestTypeService : IRequestTypeService
{
    private readonly IGenericRepository<RequestType> _repository;

    public RequestTypeService(IGenericRepository<RequestType> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<RequestTypeResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.QueryActive()
            .AsNoTracking()
            .OrderBy(x => x.RequestTypeCode)
            .Select(x => ToResponseDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<RequestTypeResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RequestTypeId == id, cancellationToken);

        return entity is null
            ? throw new NotFoundException("RequestType", id)
            : ToResponseDto(entity);
    }

    public async Task<RequestTypeResponseDto> CreateAsync(CreateRequestTypeDto dto, CancellationToken cancellationToken = default)
    {
        var code = InputHelper.NormalizeCode(dto.RequestTypeCode, nameof(dto.RequestTypeCode));
        var name = InputHelper.Required(dto.RequestTypeName, nameof(dto.RequestTypeName));
        var description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        if (await _repository.AnyAsync(x => x.RequestTypeCode == code, cancellationToken))
            throw new BadRequestException("Request type code already exists.");

        var entity = new RequestType
        {
            RequestTypeId = Guid.NewGuid(),
            RequestTypeCode = code,
            RequestTypeName = name,
            Description = description
        };

        AuditHelper.SetCreatedAudit(entity);
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task<RequestTypeResponseDto> UpdateAsync(Guid id, UpdateRequestTypeDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.RequestTypeId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("RequestType", id);

        var code = InputHelper.NormalizeCode(dto.RequestTypeCode, nameof(dto.RequestTypeCode));
        var name = InputHelper.Required(dto.RequestTypeName, nameof(dto.RequestTypeName));
        var description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        if (await _repository.AnyAsync(x => x.RequestTypeCode == code && x.RequestTypeId != id, cancellationToken))
            throw new BadRequestException("Request type code already exists.");

        entity.RequestTypeCode = code;
        entity.RequestTypeName = name;
        entity.Description = description;
        AuditHelper.SetUpdatedAudit(entity);

        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.RequestTypeId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("RequestType", id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static RequestTypeResponseDto ToResponseDto(RequestType entity) => new()
    {
        RequestTypeId = entity.RequestTypeId,
        RequestTypeCode = entity.RequestTypeCode,
        RequestTypeName = entity.RequestTypeName,
        Description = entity.Description,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
