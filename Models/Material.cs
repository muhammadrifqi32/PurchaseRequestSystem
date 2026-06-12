using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseRequestSystem.Models;

[Table("tbl_material")]
public class Material
{
    public Guid MaterialId { get; set; }
    /// <summary>Default/recommended UoM. In transactions this is only a hint.</summary>
    public Guid UomId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Uom Uom { get; set; } = null!;
}
