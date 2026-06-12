using System.ComponentModel.DataAnnotations.Schema;
using PurchaseRequestSystem.Common.Enums;

namespace PurchaseRequestSystem.Models;

[Table("tbl_activity_log")]
public class ActivityLog
{
    public Guid ActivityLogId { get; set; }
    public Guid AccountId { get; set; }
    public DocumentType DocumentType { get; set; }
    public Guid DocumentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }
}
