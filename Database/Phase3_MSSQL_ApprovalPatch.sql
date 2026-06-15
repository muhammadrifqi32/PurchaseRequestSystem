/* =========================================================
   PHASE 3 APPROVAL PATCH - SAFE VERSION FOR SQL SERVER
   Purpose:
   1. Ensure GM and Chairman approval stages exist.
   2. Ensure specific approval statuses exist if the database wants to use them.
   3. Ensure dummy GM account exists for testing.

   Notes:
   - The code can fall back to generic statuses such as SUBMITTED, APPROVED,
     REJECTED, and REVISION_REQUIRED.
   - Adding these specific statuses makes the approval flow clearer in GET responses.
   ========================================================= */

DECLARE @Seeded DATETIME2 = '2024-01-01T00:00:00';

-- Approval Stages
IF NOT EXISTS (SELECT 1 FROM tbl_approval_stage WHERE UPPER(StageName) = 'GM')
BEGIN
    INSERT INTO tbl_approval_stage (ApprovalStageId, StageName, IsDeleted, CreatedAt, UpdatedAt)
    VALUES ('33333333-3333-3333-3333-333333333302', 'GM', 0, @Seeded, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM tbl_approval_stage WHERE UPPER(StageName) = 'CHAIRMAN')
BEGIN
    INSERT INTO tbl_approval_stage (ApprovalStageId, StageName, IsDeleted, CreatedAt, UpdatedAt)
    VALUES ('33333333-3333-3333-3333-333333333303', 'Chairman', 0, @Seeded, NULL);
END;

-- Specific approval statuses for Phase 3
IF NOT EXISTS (SELECT 1 FROM tbl_status WHERE UPPER(StatusName) = 'PENDING_GM_APPROVAL')
BEGIN
    INSERT INTO tbl_status (StatusId, StatusName, IsDeleted, CreatedAt, UpdatedAt)
    VALUES ('44444444-4444-4444-4444-444444444421', 'PENDING_GM_APPROVAL', 0, @Seeded, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM tbl_status WHERE UPPER(StatusName) = 'GM_APPROVED')
BEGIN
    INSERT INTO tbl_status (StatusId, StatusName, IsDeleted, CreatedAt, UpdatedAt)
    VALUES ('44444444-4444-4444-4444-444444444422', 'GM_APPROVED', 0, @Seeded, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM tbl_status WHERE UPPER(StatusName) = 'PENDING_CHAIRMAN_CONFIRMATION')
BEGIN
    INSERT INTO tbl_status (StatusId, StatusName, IsDeleted, CreatedAt, UpdatedAt)
    VALUES ('44444444-4444-4444-4444-444444444423', 'PENDING_CHAIRMAN_CONFIRMATION', 0, @Seeded, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM tbl_status WHERE UPPER(StatusName) = 'CHAIRMAN_APPROVED')
BEGIN
    INSERT INTO tbl_status (StatusId, StatusName, IsDeleted, CreatedAt, UpdatedAt)
    VALUES ('44444444-4444-4444-4444-444444444424', 'CHAIRMAN_APPROVED', 0, @Seeded, NULL);
END;

-- Dummy GM account for testing approval actions
DECLARE @GmAccountId UNIQUEIDENTIFIER = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3';
DECLARE @GmUserId UNIQUEIDENTIFIER = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3';
DECLARE @GmRoleId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111103';

IF NOT EXISTS (SELECT 1 FROM tbl_account WHERE AccountId = @GmAccountId)
BEGIN
    INSERT INTO tbl_account (
        AccountId,
        RoleId,
        Email,
        Password,
        LastLoginAt,
        IsDeleted,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy
    )
    VALUES (
        @GmAccountId,
        @GmRoleId,
        'gm@test.com',
        'DUMMY_PASSWORD_NOT_USED_YET',
        NULL,
        0,
        GETDATE(),
        NULL,
        NULL,
        NULL
    );
END;

IF NOT EXISTS (SELECT 1 FROM tbl_user_detail WHERE UserId = @GmUserId)
BEGIN
    INSERT INTO tbl_user_detail (
        UserId,
        AccountId,
        FullName,
        Phone,
        CreatedAt,
        UpdatedAt
    )
    VALUES (
        @GmUserId,
        @GmAccountId,
        'Test GM',
        '081234567892',
        GETDATE(),
        NULL
    );
END;

-- Verify
SELECT StatusId, StatusName
FROM tbl_status
WHERE StatusName IN (
    'PENDING_GM_APPROVAL',
    'GM_APPROVED',
    'PENDING_CHAIRMAN_CONFIRMATION',
    'CHAIRMAN_APPROVED',
    'APPROVED',
    'REJECTED',
    'REVISION_REQUIRED',
    'SUBMITTED'
)
ORDER BY StatusName;

SELECT ApprovalStageId, StageName
FROM tbl_approval_stage
WHERE StageName IN ('GM', 'Chairman')
ORDER BY StageName;

SELECT 
    a.AccountId,
    a.Email,
    r.RoleCode,
    ud.FullName
FROM tbl_account a
JOIN tbl_role r ON r.RoleId = a.RoleId
LEFT JOIN tbl_user_detail ud ON ud.AccountId = a.AccountId
WHERE a.Email IN ('requester@test.com', 'procure@test.com', 'gm@test.com')
ORDER BY a.Email;
