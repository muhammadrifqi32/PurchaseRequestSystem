# Phase 2 - Proposal, Procurement Request, Purchase Request

This patch continues the existing ASP.NET Core Web API project and keeps the current MSSQL setup.

## Added scope

- Proposal CRUD
- Proposal submit
- Proposal review
- Project Procurement Request creation from approved Proposal
- Non-Project Procurement Request creation without Proposal
- Purchase Request submit
- Project quantity validation against approved Proposal Detail quantity

## Important model additions

To enforce Project flow quantity validation accurately, this phase adds:

- `ProposalDetail.StatusId` nullable
- `PurchaseRequest.PurchaseRequestNo` nullable, generated as `PR-YYYYMM-0001`
- `PurchaseRequestDetail.ProposalDetailId` nullable

Preferred migration command:

```bash
dotnet ef migrations add Phase2ProposalProcurementPurchaseRequest
dotnet ef database update
```

If you need to patch an existing local MSSQL DB manually, see:

```text
/Database/Phase2_MSSQL_ManualPatch.sql
```

## POST request examples

### Create Proposal

`POST /api/proposals`

```json
{
  "requesterId": "11111111-1111-1111-1111-111111111111",
  "proposalDate": "2026-06-11",
  "purpose": "Project material request",
  "proposalAttachmentPath": "uploads/proposals/file.pdf",
  "details": [
    {
      "materialId": "22222222-2222-2222-2222-222222222222",
      "uomId": "33333333-3333-3333-3333-333333333333",
      "description": "Cement for project",
      "requestedQty": 500
    }
  ]
}
```

### Submit Proposal

`POST /api/proposals/{id}/submit`

No request body.

### Review Proposal

`POST /api/proposals/{id}/review`

```json
{
  "status": "APPROVED",
  "notes": "Approved by Procure",
  "reviewedBy": "11111111-1111-1111-1111-111111111111",
  "details": [
    {
      "proposalDetailId": "44444444-4444-4444-4444-444444444444",
      "approvedQty": 400,
      "status": "APPROVED"
    }
  ]
}
```

### Create Project Procurement Request

`POST /api/procurement-requests/project`

```json
{
  "proposalId": "55555555-5555-5555-5555-555555555555",
  "requesterId": "11111111-1111-1111-1111-111111111111",
  "requestDate": "2026-06-11",
  "notes": "First batch",
  "details": [
    {
      "proposalDetailId": "44444444-4444-4444-4444-444444444444",
      "materialId": "22222222-2222-2222-2222-222222222222",
      "uomId": "33333333-3333-3333-3333-333333333333",
      "description": "Cement request batch 1",
      "quantity": 200,
      "notes": "First batch"
    }
  ]
}
```

### Create Non-Project Procurement Request

`POST /api/procurement-requests/non-project`

```json
{
  "requesterId": "11111111-1111-1111-1111-111111111111",
  "requestDate": "2026-06-11",
  "notes": "Office equipment request",
  "details": [
    {
      "materialId": "22222222-2222-2222-2222-222222222222",
      "uomId": "33333333-3333-3333-3333-333333333333",
      "description": "Office chair",
      "quantity": 10,
      "notes": "For office use"
    }
  ]
}
```

### Submit Purchase Request

`POST /api/purchase-requests/{id}/submit`

No request body.
