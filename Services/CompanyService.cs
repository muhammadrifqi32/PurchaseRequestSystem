using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.DTOs;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class CompanyService : ICompanyService
{
    private readonly IGenericRepository<Company> _repository;

    public CompanyService(IGenericRepository<Company> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CompanyResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.QueryActive()
            .AsNoTracking()
            .OrderBy(x => x.CompanyCode)
            .Select(x => ToResponseDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<CompanyResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CompanyId == id, cancellationToken);

        return entity is null
            ? throw new NotFoundException("Company", id)
            : ToResponseDto(entity);
    }

    public async Task<CompanyResponseDto> CreateAsync(CreateCompanyDto dto, CancellationToken cancellationToken = default)
    {
        var code = InputHelper.NormalizeCode(dto.CompanyCode, nameof(dto.CompanyCode));
        var name = InputHelper.Required(dto.CompanyName, nameof(dto.CompanyName));

        if (await _repository.AnyAsync(x => x.CompanyCode == code, cancellationToken))
            throw new BadRequestException("Company code already exists.");

        var entity = new Company
        {
            CompanyId = Guid.NewGuid(),
            CompanyCode = code,
            CompanyName = name,
            IsDeleted = false
        };

        AuditHelper.SetCreatedAudit(entity);
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task<CompanyResponseDto> UpdateAsync(Guid id, UpdateCompanyDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.CompanyId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("Company", id);

        var code = InputHelper.NormalizeCode(dto.CompanyCode, nameof(dto.CompanyCode));
        var name = InputHelper.Required(dto.CompanyName, nameof(dto.CompanyName));

        if (await _repository.AnyAsync(x => x.CompanyCode == code && x.CompanyId != id, cancellationToken))
            throw new BadRequestException("Company code already exists.");

        entity.CompanyCode = code;
        entity.CompanyName = name;
        AuditHelper.SetUpdatedAudit(entity);

        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToResponseDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryActive()
            .FirstOrDefaultAsync(x => x.CompanyId == id, cancellationToken);

        if (entity is null)
            throw new NotFoundException("Company", id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static CompanyResponseDto ToResponseDto(Company entity) => new()
    {
        CompanyId = entity.CompanyId,
        CompanyCode = entity.CompanyCode,
        CompanyName = entity.CompanyName,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
