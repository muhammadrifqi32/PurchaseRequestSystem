using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseRequestSystem.Models;

[Table("tbl_user_detail")]
public class UserDetail
{
    [Key]
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Account Account { get; set; } = null!;
}
