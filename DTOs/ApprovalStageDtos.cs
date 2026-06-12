namespace PurchaseRequestSystem.DTOs;

public class CreateApprovalStageDto
{
    public string? StageName { get; set; }
}

public class UpdateApprovalStageDto
{
    public string? StageName { get; set; }
}

public class ApprovalStageResponseDto
{
    public Guid ApprovalStageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
