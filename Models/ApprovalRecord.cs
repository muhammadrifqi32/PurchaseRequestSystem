using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseRequestSystem.Models;

/// <summary>
/// One row per approval stage per procurement request.
/// Unique (ProcurementRequestId, ApprovalStageId) => one stage per request.
/// Used for GM and Chairman stages. Proposal "review" by Procure is tracked
/// on the Proposal status itself (the final ERD only references procurement_request_id here).
/// </summary>
[Table("tbl_approval_record")]
public class ApprovalRecord
{
    public Guid ApprovalRecordId { get; set; }
    public Guid ProcurementRequestId { get; set; }
    public Guid ApprovalStageId { get; set; }
    public Guid StatusId { get; set; }
    /// <summary>Required when stage = Chairman (offline decision recorded by GM).</summary>
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public ProcurementRequest ProcurementRequest { get; set; } = null!;
    public ApprovalStage ApprovalStage { get; set; } = null!;
    public Status Status { get; set; } = null!;
}
