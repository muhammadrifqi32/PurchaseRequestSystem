namespace PurchaseRequestSystem.DTOs;

public class CreateProjectProcurementRequestDto
{
    public Guid ProposalId { get; set; }
    public Guid RequesterId { get; set; }
    public DateTime RequestDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateProjectPurchaseRequestDetailDto> Details { get; set; } = new();
}

public class CreateProjectPurchaseRequestDetailDto
{
    public Guid ProposalDetailId { get; set; }
    public Guid MaterialId { get; set; }
    public Guid UomId { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateNonProjectProcurementRequestDto
{
    public Guid RequesterId { get; set; }
    public DateTime RequestDate { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseRequestDetailDto> Details { get; set; } = new();
}

public class CreatePurchaseRequestDetailDto
{
    public Guid MaterialId { get; set; }
    public Guid UomId { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class ProcurementRequestResponseDto
{
    public Guid ProcurementRequestId { get; set; }
    public string ProcurementRequestNo { get; set; } = string.Empty;
    public Guid RequestTypeId { get; set; }
    public string RequestTypeCode { get; set; } = string.Empty;
    public string RequestTypeName { get; set; } = string.Empty;
    public Guid? ProposalId { get; set; }
    public string? ProposalNo { get; set; }
    public Guid RequesterId { get; set; }
    public DateTime RequestDate { get; set; }
    public Guid StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public PurchaseRequestResponseDto? PurchaseRequest { get; set; }
}
