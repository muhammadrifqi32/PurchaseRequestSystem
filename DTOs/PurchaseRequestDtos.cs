namespace PurchaseRequestSystem.DTOs;

public class PurchaseRequestResponseDto
{
    public Guid PurchaseRequestId { get; set; }
    public string PurchaseRequestNo { get; set; } = string.Empty;
    public Guid ProcurementRequestId { get; set; }
    public string? ProcurementRequestNo { get; set; }
    public Guid StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public List<PurchaseRequestDetailResponseDto> Details { get; set; } = new();
}

public class PurchaseRequestDetailResponseDto
{
    public Guid PurchaseRequestDetailId { get; set; }
    public Guid PurchaseRequestId { get; set; }
    public Guid? ProposalDetailId { get; set; }
    public Guid MaterialId { get; set; }
    public string? MaterialCode { get; set; }
    public string? MaterialName { get; set; }
    public Guid UomId { get; set; }
    public string? UomCode { get; set; }
    public string? UomName { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
