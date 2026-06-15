# Phase 3 — Approval GM and Chairman Confirmation

This phase adds approval flow for Purchase Request.

Implemented endpoints:

```text
POST /api/purchase-requests/{id}/submit
POST /api/purchase-requests/{id}/approve-gm
POST /api/purchase-requests/{id}/reject-gm
POST /api/purchase-requests/{id}/request-revision-gm
POST /api/purchase-requests/{id}/record-chairman-approval
POST /api/purchase-requests/{id}/record-chairman-rejection
POST /api/purchase-requests/{id}/record-chairman-revision
GET  /api/purchase-requests/{id}/approval-history
```

## Important Database Setup

Run this SQL script before testing Phase 3:

```text
Database/Phase3_MSSQL_ApprovalPatch.sql
```

The script ensures these items exist:

```text
ApprovalStage: GM
ApprovalStage: Chairman
Status: PENDING_GM_APPROVAL
Status: GM_APPROVED
Status: PENDING_CHAIRMAN_CONFIRMATION
Status: CHAIRMAN_APPROVED
Dummy GM account: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3
```

The code can fall back to generic statuses such as `SUBMITTED`, `APPROVED`, `REJECTED`, and `REVISION_REQUIRED`, but specific Phase 3 statuses make testing easier to understand.

## Test Accounts

Requester:

```text
aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1
```

Procure:

```text
aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2
```

GM:

```text
aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3
```

## Testing Flow

Start from an existing Purchase Request with status `DRAFT` or `REVISION_REQUIRED`.

If you already have a Project or Non-Project Procurement Request from Phase 2, use its nested `purchaseRequest.purchaseRequestId`.

---

## 1. Submit Purchase Request to GM

Endpoint:

```text
POST /api/purchase-requests/{purchaseRequestId}/submit
```

Body:

```json
{
  "submittedBy": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
  "notes": "Submitted for GM approval"
}
```

Expected:

```text
200 OK
Purchase Request status = PENDING_GM_APPROVAL or SUBMITTED
Procurement Request status = PENDING_GM_APPROVAL or SUBMITTED
ApprovalRecord GM created with PENDING/SUBMITTED status
```

---

## 2. GM Approves Purchase Request

Endpoint:

```text
POST /api/purchase-requests/{purchaseRequestId}/approve-gm
```

Body:

```json
{
  "actionBy": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3",
  "notes": "Approved by GM"
}
```

Expected:

```text
200 OK
ApprovalRecord GM = APPROVED
Purchase Request status = PENDING_CHAIRMAN_CONFIRMATION / GM_APPROVED / APPROVED
```

---

## 3. GM Rejects Purchase Request

Use a different submitted Purchase Request for this negative path.

Endpoint:

```text
POST /api/purchase-requests/{purchaseRequestId}/reject-gm
```

Body:

```json
{
  "actionBy": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3",
  "notes": "Rejected by GM because budget is not approved"
}
```

Expected:

```text
200 OK
ApprovalRecord GM = REJECTED
Purchase Request status = REJECTED
Procurement Request status = REJECTED
```

Notes are required. Empty notes should return `400 Bad Request`.

---

## 4. GM Requests Revision

Use a different submitted Purchase Request for this negative path.

Endpoint:

```text
POST /api/purchase-requests/{purchaseRequestId}/request-revision-gm
```

Body:

```json
{
  "actionBy": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3",
  "notes": "Please revise the quantity and notes"
}
```

Expected:

```text
200 OK
ApprovalRecord GM = REVISION_REQUIRED
Purchase Request status = REVISION_REQUIRED
Procurement Request status = REVISION_REQUIRED
```

Notes are required. Empty notes should return `400 Bad Request`.

---

## 5. Record Chairman Approval

This endpoint is used after GM approval. Chairman does not login to the app. GM records Chairman offline decision.

Endpoint:

```text
POST /api/purchase-requests/{purchaseRequestId}/record-chairman-approval
```

Body:

```json
{
  "recordedBy": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3",
  "notes": "Chairman approved offline on 2026-06-12"
}
```

Expected:

```text
200 OK
ApprovalRecord Chairman = CHAIRMAN_APPROVED or APPROVED
Purchase Request status = APPROVED
Procurement Request status = APPROVED
Purchase Request is eligible for Purchase Order generation in the next phase
```

Notes are required. Empty notes should return `400 Bad Request`.

---

## 6. Record Chairman Rejection

Use a different GM-approved Purchase Request for this negative path.

Endpoint:

```text
POST /api/purchase-requests/{purchaseRequestId}/record-chairman-rejection
```

Body:

```json
{
  "recordedBy": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3",
  "notes": "Chairman rejected offline because budget is postponed"
}
```

Expected:

```text
200 OK
ApprovalRecord Chairman = REJECTED
Purchase Request status = REJECTED
Procurement Request status = REJECTED
```

---

## 7. Record Chairman Revision Request

Use a different GM-approved Purchase Request for this negative path.

Endpoint:

```text
POST /api/purchase-requests/{purchaseRequestId}/record-chairman-revision
```

Body:

```json
{
  "recordedBy": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3",
  "notes": "Chairman requested revision on item justification"
}
```

Expected:

```text
200 OK
ApprovalRecord Chairman = REVISION_REQUIRED
Purchase Request status = REVISION_REQUIRED
Procurement Request status = REVISION_REQUIRED
```

---

## 8. Approval History

Endpoint:

```text
GET /api/purchase-requests/{purchaseRequestId}/approval-history
```

Expected:

```json
{
  "statusCode": 200,
  "message": "Approval history retrieved successfully",
  "data": [
    {
      "approvalRecordId": "...",
      "procurementRequestId": "...",
      "approvalStage": "GM",
      "status": "APPROVED",
      "notes": "Approved by GM",
      "createdAt": "...",
      "createdBy": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3",
      "updatedAt": null,
      "updatedBy": null
    },
    {
      "approvalRecordId": "...",
      "procurementRequestId": "...",
      "approvalStage": "Chairman",
      "status": "APPROVED",
      "notes": "Chairman approved offline on 2026-06-12",
      "createdAt": "...",
      "createdBy": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3",
      "updatedAt": null,
      "updatedBy": null
    }
  ]
}
```

## Phase 3 Pass Criteria

Phase 3 is considered working if:

```text
Submit Purchase Request to GM returns 200
GM approve returns 200
Approval history shows GM APPROVED
Chairman approval returns 200
Approval history shows GM + Chairman records
Final Purchase Request status becomes APPROVED
GM rejection requires notes
GM revision requires notes
Chairman approval/rejection/revision requires notes
Chairman action before GM approval returns 400
Invalid ActionBy / RecordedBy returns 400
```

## Known Notes

1. Authentication is not implemented yet, so `SubmittedBy`, `ActionBy`, and `RecordedBy` are supplied manually in the request body.
2. Chairman does not login. GM records Chairman offline decision.
3. The code uses specific statuses if they exist. If they do not exist, it falls back to generic statuses where possible.
4. Approval records are stored per Procurement Request, not directly per Purchase Request.
5. One approval record is maintained per stage per Procurement Request. Repeating an action on the same stage updates the existing record.
