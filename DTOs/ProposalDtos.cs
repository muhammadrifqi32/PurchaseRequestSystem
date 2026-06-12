namespace PurchaseRequestSystem.DTOs;

public class CreateProposalDto
{
    public Guid RequesterId { get; set; }
    public DateTime ProposalDate { get; set; }
    public string? Purpose { get; set; }
    public string? ProposalAttachmentPath { get; set; }
    public List<CreateProposalDetailDto> Details { get; set; } = new();
}

public class CreateProposalDetailDto
{
    public Guid MaterialId { get; set; }
    public Guid UomId { get; set; }
    public string? Description { get; set; }
    public decimal RequestedQty { get; set; }
}

public class UpdateProposalDto
{
    public Guid RequesterId { get; set; }
    public DateTime ProposalDate { get; set; }
    public string? Purpose { get; set; }
    public string? ProposalAttachmentPath { get; set; }
    public List<UpdateProposalDetailDto> Details { get; set; } = new();
}

public class UpdateProposalDetailDto
{
    public Guid? ProposalDetailId { get; set; }
    public Guid MaterialId { get; set; }
    public Guid UomId { get; set; }
    public string? Description { get; set; }
    public decimal RequestedQty { get; set; }
}

public class ReviewProposalDto
{
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public Guid? ReviewedBy { get; set; }
    public List<ReviewProposalDetailDto> Details { get; set; } = new();
}

public class ReviewProposalDetailDto
{
    public Guid ProposalDetailId { get; set; }
    public decimal ApprovedQty { get; set; }
    public string? Status { get; set; }
}

public class ProposalResponseDto
{
    public Guid ProposalId { get; set; }
    public string ProposalNo { get; set; } = string.Empty;
    public Guid RequesterId { get; set; }
    public DateTime ProposalDate { get; set; }
    public string? Purpose { get; set; }
    public Guid StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? ProposalAttachmentPath { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public List<ProposalDetailResponseDto> Details { get; set; } = new();
}

public class ProposalDetailResponseDto
{
    public Guid ProposalDetailId { get; set; }
    public Guid ProposalId { get; set; }
    public Guid MaterialId { get; set; }
    public string? MaterialCode { get; set; }
    public string? MaterialName { get; set; }
    public Guid UomId { get; set; }
    public string? UomCode { get; set; }
    public string? UomName { get; set; }
    public string? Description { get; set; }
    public decimal RequestedQty { get; set; }
    public decimal ApprovedQty { get; set; }
    public decimal RemainingQty { get; set; }
    public Guid? StatusId { get; set; }
    public string? StatusName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SubmitResponseDto
{
    public Guid Id { get; set; }
    public string DocumentNo { get; set; } = string.Empty;
    public Guid StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
}
