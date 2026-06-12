using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseRequestSystem.Models;

/// <summary>PROJECT flow only. Reviewed by Procure (status change + activity log).</summary>
[Table("tbl_proposal")]
public class Proposal
{
    public Guid ProposalId { get; set; }
    public string ProposalNo { get; set; } = string.Empty;
    public Guid RequesterId { get; set; }
    public DateTime ProposalDate { get; set; }
    public string? Purpose { get; set; }
    public Guid StatusId { get; set; }
    public string? ProposalAttachmentPath { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public Status Status { get; set; } = null!;
    public ICollection<ProposalDetail> Details { get; set; } = new List<ProposalDetail>();
    public ICollection<ProcurementRequest> ProcurementRequests { get; set; } = new List<ProcurementRequest>();
}
