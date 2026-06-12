using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Common.Exceptions;
using PurchaseRequestSystem.Helpers;
using PurchaseRequestSystem.Interfaces;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Services;

public class StatusLookupService : IStatusLookupService
{
    private readonly IGenericRepository<Status> _statusRepository;

    public StatusLookupService(IGenericRepository<Status> statusRepository)
    {
        _statusRepository = statusRepository;
    }

    public async Task<Status> GetRequiredAsync(string statusName, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(statusName);
        var status = await _statusRepository.QueryActive()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.StatusName.ToUpper() == normalized, cancellationToken);

        return status ?? throw new BadRequestException($"Status '{normalized}' does not exist in master status.");
    }

    public async Task<Status> GetFirstExistingAsync(IEnumerable<string> statusNames, CancellationToken cancellationToken = default)
    {
        foreach (var name in statusNames)
        {
            var normalized = Normalize(name);
            var status = await _statusRepository.QueryActive()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.StatusName.ToUpper() == normalized, cancellationToken);

            if (status is not null)
                return status;
        }

        throw new BadRequestException($"None of these statuses exist in master status: {string.Join(", ", statusNames)}.");
    }

    public async Task<bool> IsAnyAsync(Guid statusId, IEnumerable<string> statusNames, CancellationToken cancellationToken = default)
    {
        var allowed = statusNames.Select(Normalize).ToList();
        return await _statusRepository.QueryActive()
            .AsNoTracking()
            .AnyAsync(x => x.StatusId == statusId && allowed.Contains(x.StatusName.ToUpper()), cancellationToken);
    }

    public async Task<string?> GetNameAsync(Guid statusId, CancellationToken cancellationToken = default)
    {
        return await _statusRepository.QueryActive()
            .AsNoTracking()
            .Where(x => x.StatusId == statusId)
            .Select(x => x.StatusName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string Normalize(string value)
        => InputHelper.Required(value, nameof(value)).Trim().ToUpperInvariant();
}
