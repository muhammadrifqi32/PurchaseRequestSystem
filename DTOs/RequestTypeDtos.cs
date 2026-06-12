namespace PurchaseRequestSystem.DTOs;

public class CreateRequestTypeDto
{
    public string? RequestTypeCode { get; set; }
    public string? RequestTypeName { get; set; }
    public string? Description { get; set; }
}

public class UpdateRequestTypeDto
{
    public string? RequestTypeCode { get; set; }
    public string? RequestTypeName { get; set; }
    public string? Description { get; set; }
}

public class RequestTypeResponseDto
{
    public Guid RequestTypeId { get; set; }
    public string RequestTypeCode { get; set; } = string.Empty;
    public string RequestTypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
