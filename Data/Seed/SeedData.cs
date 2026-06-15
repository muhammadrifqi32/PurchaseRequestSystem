using Microsoft.EntityFrameworkCore;
using PurchaseRequestSystem.Models;

namespace PurchaseRequestSystem.Data.Seed;

/// <summary>
/// Deterministic seed data baked into the migration via HasData.
/// All Ids and timestamps are static (required by HasData).
/// </summary>
public static class SeedData
{
    private static readonly DateTime Seeded = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // Roles
    public static readonly Guid RoleRequester    = Guid.Parse("11111111-1111-1111-1111-111111111101");
    public static readonly Guid RoleProcure      = Guid.Parse("11111111-1111-1111-1111-111111111102");
    public static readonly Guid RoleGm           = Guid.Parse("11111111-1111-1111-1111-111111111103");
    public static readonly Guid RoleChairman     = Guid.Parse("11111111-1111-1111-1111-111111111104");
    public static readonly Guid RoleProjectAdmin = Guid.Parse("11111111-1111-1111-1111-111111111105");
    public static readonly Guid RoleAdmin        = Guid.Parse("11111111-1111-1111-1111-111111111106");

    // Request types
    public static readonly Guid RequestTypeProject    = Guid.Parse("22222222-2222-2222-2222-222222222201");
    public static readonly Guid RequestTypeNonProject = Guid.Parse("22222222-2222-2222-2222-222222222202");

    // Approval stages
    public static readonly Guid StageProcure  = Guid.Parse("33333333-3333-3333-3333-333333333301");
    public static readonly Guid StageGm       = Guid.Parse("33333333-3333-3333-3333-333333333302");
    public static readonly Guid StageChairman = Guid.Parse("33333333-3333-3333-3333-333333333303");

    // Statuses (flat dictionary shared by all document types)
    public static readonly Guid StatusDraft             = Guid.Parse("44444444-4444-4444-4444-444444444401");
    public static readonly Guid StatusPending           = Guid.Parse("44444444-4444-4444-4444-444444444402");
    public static readonly Guid StatusSubmitted         = Guid.Parse("44444444-4444-4444-4444-444444444403");
    public static readonly Guid StatusUnderReview       = Guid.Parse("44444444-4444-4444-4444-444444444404");
    public static readonly Guid StatusApproved          = Guid.Parse("44444444-4444-4444-4444-444444444405");
    public static readonly Guid StatusRejected          = Guid.Parse("44444444-4444-4444-4444-444444444406");
    public static readonly Guid StatusRevisionRequired  = Guid.Parse("44444444-4444-4444-4444-444444444407");
    public static readonly Guid StatusPartiallyApproved = Guid.Parse("44444444-4444-4444-4444-444444444408");
    public static readonly Guid StatusPoOpen            = Guid.Parse("44444444-4444-4444-4444-444444444409");
    public static readonly Guid StatusPartiallyReceived = Guid.Parse("44444444-4444-4444-4444-444444444410");
    public static readonly Guid StatusFullyReceived     = Guid.Parse("44444444-4444-4444-4444-444444444411");
    public static readonly Guid StatusUnpaid            = Guid.Parse("44444444-4444-4444-4444-444444444412");
    public static readonly Guid StatusPartiallyPaid     = Guid.Parse("44444444-4444-4444-4444-444444444413");
    public static readonly Guid StatusPaid              = Guid.Parse("44444444-4444-4444-4444-444444444414");
    public static readonly Guid StatusPendingGmApproval = Guid.Parse("44444444-4444-4444-4444-444444444421");
    public static readonly Guid StatusGmApproved        = Guid.Parse("44444444-4444-4444-4444-444444444422");
    public static readonly Guid StatusPendingChairman   = Guid.Parse("44444444-4444-4444-4444-444444444423");
    public static readonly Guid StatusChairmanApproved  = Guid.Parse("44444444-4444-4444-4444-444444444424");

    public static void Apply(ModelBuilder b)
    {
        b.Entity<Role>().HasData(
            new Role { RoleId = RoleRequester,    RoleCode = "REQUESTER",     RoleName = "Requester",     Description = "Creates proposals and project purchase requests", CreatedAt = Seeded },
            new Role { RoleId = RoleProcure,      RoleCode = "PROCURE",       RoleName = "Procurement",   Description = "Reviews proposals and creates non-project requests / POs", CreatedAt = Seeded },
            new Role { RoleId = RoleGm,           RoleCode = "GM",            RoleName = "General Manager", Description = "Approves purchase requests and records Chairman decisions", CreatedAt = Seeded },
            new Role { RoleId = RoleChairman,     RoleCode = "CHAIRMAN",      RoleName = "Chairman",      Description = "Final offline approver", CreatedAt = Seeded },
            new Role { RoleId = RoleProjectAdmin, RoleCode = "PROJECT_ADMIN", RoleName = "Project Admin", Description = "Generates purchase orders", CreatedAt = Seeded },
            new Role { RoleId = RoleAdmin,        RoleCode = "ADMIN",         RoleName = "Administrator", Description = "System administrator", CreatedAt = Seeded }
        );

        b.Entity<RequestType>().HasData(
            new RequestType { RequestTypeId = RequestTypeProject,    RequestTypeCode = "PROJECT",     RequestTypeName = "Project",     Description = "Starts from an approved Proposal", CreatedAt = Seeded },
            new RequestType { RequestTypeId = RequestTypeNonProject, RequestTypeCode = "NON_PROJECT", RequestTypeName = "Non-Project", Description = "Created directly by Procurement", CreatedAt = Seeded }
        );

        b.Entity<ApprovalStage>().HasData(
            new ApprovalStage { ApprovalStageId = StageProcure,  StageName = "Procure",  IsDeleted = false, CreatedAt = Seeded },
            new ApprovalStage { ApprovalStageId = StageGm,       StageName = "GM",       IsDeleted = false, CreatedAt = Seeded },
            new ApprovalStage { ApprovalStageId = StageChairman, StageName = "Chairman", IsDeleted = false, CreatedAt = Seeded }
        );

        b.Entity<Status>().HasData(
            new Status { StatusId = StatusDraft,             StatusName = "DRAFT",              CreatedAt = Seeded },
            new Status { StatusId = StatusPending,           StatusName = "PENDING",            CreatedAt = Seeded },
            new Status { StatusId = StatusSubmitted,         StatusName = "SUBMITTED",          CreatedAt = Seeded },
            new Status { StatusId = StatusUnderReview,       StatusName = "UNDER_REVIEW",       CreatedAt = Seeded },
            new Status { StatusId = StatusApproved,          StatusName = "APPROVED",           CreatedAt = Seeded },
            new Status { StatusId = StatusRejected,          StatusName = "REJECTED",           CreatedAt = Seeded },
            new Status { StatusId = StatusRevisionRequired,  StatusName = "REVISION_REQUIRED",  CreatedAt = Seeded },
            new Status { StatusId = StatusPartiallyApproved, StatusName = "PARTIALLY_APPROVED", CreatedAt = Seeded },
            new Status { StatusId = StatusPoOpen,            StatusName = "OPEN",               CreatedAt = Seeded },
            new Status { StatusId = StatusPartiallyReceived, StatusName = "PARTIALLY_RECEIVED", CreatedAt = Seeded },
            new Status { StatusId = StatusFullyReceived,     StatusName = "FULLY_RECEIVED",     CreatedAt = Seeded },
            new Status { StatusId = StatusUnpaid,            StatusName = "UNPAID",             CreatedAt = Seeded },
            new Status { StatusId = StatusPartiallyPaid,     StatusName = "PARTIALLY_PAID",     CreatedAt = Seeded },
            new Status { StatusId = StatusPaid,              StatusName = "PAID",               CreatedAt = Seeded },
            new Status { StatusId = StatusPendingGmApproval, StatusName = "PENDING_GM_APPROVAL", CreatedAt = Seeded },
            new Status { StatusId = StatusGmApproved,        StatusName = "GM_APPROVED",         CreatedAt = Seeded },
            new Status { StatusId = StatusPendingChairman,   StatusName = "PENDING_CHAIRMAN_CONFIRMATION", CreatedAt = Seeded },
            new Status { StatusId = StatusChairmanApproved,  StatusName = "CHAIRMAN_APPROVED",   CreatedAt = Seeded }
        );
    }
}
