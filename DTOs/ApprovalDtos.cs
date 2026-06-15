namespace PurchaseRequestSystem.DTOs;

public class SubmitPurchaseRequestDto
{
    public Guid SubmittedBy { get; set; }
    public string? Notes { get; set; }
}

public class ApprovalActionDto
{
    public Guid ActionBy { get; set; }
    public string? Notes { get; set; }
}

public class ChairmanConfirmationDto
{
    public Guid RecordedBy { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class ApprovalHistoryResponseDto
{
    public Guid ApprovalRecordId { get; set; }
    public Guid ProcurementRequestId { get; set; }
    public string ApprovalStage { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
