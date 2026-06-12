namespace PurchaseRequestSystem.DTOs;

public class CreateStatusDto
{
    public string? StatusName { get; set; }
}

public class UpdateStatusDto
{
    public string? StatusName { get; set; }
}

public class StatusResponseDto
{
    public Guid StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
