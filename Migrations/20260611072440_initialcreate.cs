using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PurchaseRequestSystem.Migrations
{
    /// <inheritdoc />
    public partial class initialcreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_approval_stage",
                columns: table => new
                {
                    ApprovalStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_approval_stage", x => x.ApprovalStageId);
                });

            migrationBuilder.CreateTable(
                name: "tbl_request_type",
                columns: table => new
                {
                    RequestTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestTypeCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_request_type", x => x.RequestTypeId);
                });

            migrationBuilder.CreateTable(
                name: "tbl_role",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_role", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "tbl_status",
                columns: table => new
                {
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_status", x => x.StatusId);
                });

            migrationBuilder.CreateTable(
                name: "tbl_uom",
                columns: table => new
                {
                    UomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UomCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UomName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_uom", x => x.UomId);
                });

            migrationBuilder.CreateTable(
                name: "tbl_account",
                columns: table => new
                {
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_account", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_tbl_account_tbl_account_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_account_tbl_account_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_account_tbl_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "tbl_role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_material",
                columns: table => new
                {
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_material", x => x.MaterialId);
                    table.ForeignKey(
                        name: "FK_tbl_material_tbl_uom_UomId",
                        column: x => x.UomId,
                        principalTable: "tbl_uom",
                        principalColumn: "UomId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_activity_log",
                columns: table => new
                {
                    ActivityLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_activity_log", x => x.ActivityLogId);
                    table.ForeignKey(
                        name: "FK_tbl_activity_log_tbl_account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_company",
                columns: table => new
                {
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_company", x => x.CompanyId);
                    table.ForeignKey(
                        name: "FK_tbl_company_tbl_account_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_company_tbl_account_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_proposal",
                columns: table => new
                {
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalDate = table.Column<DateTime>(type: "date", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalAttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_proposal", x => x.ProposalId);
                    table.ForeignKey(
                        name: "FK_tbl_proposal_tbl_account_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_proposal_tbl_account_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_proposal_tbl_account_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_proposal_tbl_status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "tbl_status",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_user_detail",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_detail", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_tbl_user_detail_tbl_account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_vendor",
                columns: table => new
                {
                    VendorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VendorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_vendor", x => x.VendorId);
                    table.ForeignKey(
                        name: "FK_tbl_vendor_tbl_account_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_vendor_tbl_account_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_procurement_request",
                columns: table => new
                {
                    ProcurementRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcurementRequestNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RequestTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "date", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_procurement_request", x => x.ProcurementRequestId);
                    table.ForeignKey(
                        name: "FK_tbl_procurement_request_tbl_account_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_procurement_request_tbl_account_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_procurement_request_tbl_account_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_procurement_request_tbl_proposal_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "tbl_proposal",
                        principalColumn: "ProposalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_procurement_request_tbl_request_type_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalTable: "tbl_request_type",
                        principalColumn: "RequestTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_procurement_request_tbl_status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "tbl_status",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_proposal_detail",
                columns: table => new
                {
                    ProposalDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ApprovedQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_proposal_detail", x => x.ProposalDetailId);
                    table.ForeignKey(
                        name: "FK_tbl_proposal_detail_tbl_material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "tbl_material",
                        principalColumn: "MaterialId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_proposal_detail_tbl_proposal_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "tbl_proposal",
                        principalColumn: "ProposalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_proposal_detail_tbl_uom_UomId",
                        column: x => x.UomId,
                        principalTable: "tbl_uom",
                        principalColumn: "UomId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_approval_record",
                columns: table => new
                {
                    ApprovalRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcurementRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovalStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_approval_record", x => x.ApprovalRecordId);
                    table.ForeignKey(
                        name: "FK_tbl_approval_record_tbl_account_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_approval_record_tbl_account_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_approval_record_tbl_approval_stage_ApprovalStageId",
                        column: x => x.ApprovalStageId,
                        principalTable: "tbl_approval_stage",
                        principalColumn: "ApprovalStageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_approval_record_tbl_procurement_request_ProcurementRequestId",
                        column: x => x.ProcurementRequestId,
                        principalTable: "tbl_procurement_request",
                        principalColumn: "ProcurementRequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_approval_record_tbl_status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "tbl_status",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_purchase_request",
                columns: table => new
                {
                    PurchaseRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcurementRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_purchase_request", x => x.PurchaseRequestId);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_request_tbl_account_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_request_tbl_account_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_request_tbl_procurement_request_ProcurementRequestId",
                        column: x => x.ProcurementRequestId,
                        principalTable: "tbl_procurement_request",
                        principalColumn: "ProcurementRequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_request_tbl_status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "tbl_status",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_purchase_order",
                columns: table => new
                {
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VendorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PoDate = table.Column<DateTime>(type: "date", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubtotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrandtotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PurchaseOrderAttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_purchase_order", x => x.PurchaseOrderId);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_tbl_account_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_tbl_account_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_tbl_company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "tbl_company",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_tbl_purchase_request_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "tbl_purchase_request",
                        principalColumn: "PurchaseRequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_tbl_status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "tbl_status",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_tbl_vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "tbl_vendor",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_purchase_request_detail",
                columns: table => new
                {
                    PurchaseRequestDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_purchase_request_detail", x => x.PurchaseRequestDetailId);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_request_detail_tbl_account_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_request_detail_tbl_account_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_request_detail_tbl_material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "tbl_material",
                        principalColumn: "MaterialId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_request_detail_tbl_purchase_request_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "tbl_purchase_request",
                        principalColumn: "PurchaseRequestId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_request_detail_tbl_uom_UomId",
                        column: x => x.UomId,
                        principalTable: "tbl_uom",
                        principalColumn: "UomId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_goods_receipt",
                columns: table => new
                {
                    GoodsReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodsReceiptNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "date", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HasDiscrepancy = table.Column<bool>(type: "bit", nullable: false),
                    DiscrepancyNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_goods_receipt", x => x.GoodsReceiptId);
                    table.ForeignKey(
                        name: "FK_tbl_goods_receipt_tbl_account_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_goods_receipt_tbl_account_ReceivedBy",
                        column: x => x.ReceivedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_goods_receipt_tbl_account_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_goods_receipt_tbl_purchase_order_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "tbl_purchase_order",
                        principalColumn: "PurchaseOrderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_goods_receipt_tbl_status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "tbl_status",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_purchase_order_detail",
                columns: table => new
                {
                    PurchaseOrderDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DetailNo = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SubtotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_purchase_order_detail", x => x.PurchaseOrderDetailId);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_detail_tbl_material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "tbl_material",
                        principalColumn: "MaterialId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_detail_tbl_purchase_order_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "tbl_purchase_order",
                        principalColumn: "PurchaseOrderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_detail_tbl_uom_UomId",
                        column: x => x.UomId,
                        principalTable: "tbl_uom",
                        principalColumn: "UomId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_purchase_order_payment",
                columns: table => new
                {
                    PurchaseOrderPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodsReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "date", nullable: true),
                    DueDate = table.Column<DateTime>(type: "date", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "date", nullable: true),
                    PaymentReferenceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentProofPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_purchase_order_payment", x => x.PurchaseOrderPaymentId);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_payment_tbl_account_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_payment_tbl_account_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_payment_tbl_goods_receipt_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalTable: "tbl_goods_receipt",
                        principalColumn: "GoodsReceiptId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_payment_tbl_purchase_order_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "tbl_purchase_order",
                        principalColumn: "PurchaseOrderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_purchase_order_payment_tbl_status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "tbl_status",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_goods_receipt_detail",
                columns: table => new
                {
                    GoodsReceiptDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodsReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DetailNo = table.Column<int>(type: "int", nullable: false),
                    ReceivedQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsMatchPo = table.Column<bool>(type: "bit", nullable: false),
                    DiscrepancyType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GoodsReceiptAttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_goods_receipt_detail", x => x.GoodsReceiptDetailId);
                    table.ForeignKey(
                        name: "FK_tbl_goods_receipt_detail_tbl_account_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_goods_receipt_detail_tbl_account_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tbl_account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_goods_receipt_detail_tbl_goods_receipt_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalTable: "tbl_goods_receipt",
                        principalColumn: "GoodsReceiptId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_goods_receipt_detail_tbl_material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "tbl_material",
                        principalColumn: "MaterialId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_goods_receipt_detail_tbl_purchase_order_detail_PurchaseOrderDetailId",
                        column: x => x.PurchaseOrderDetailId,
                        principalTable: "tbl_purchase_order_detail",
                        principalColumn: "PurchaseOrderDetailId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_goods_receipt_detail_tbl_uom_UomId",
                        column: x => x.UomId,
                        principalTable: "tbl_uom",
                        principalColumn: "UomId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "tbl_approval_stage",
                columns: new[] { "ApprovalStageId", "CreatedAt", "IsDeleted", "StageName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333301"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Procure", null },
                    { new Guid("33333333-3333-3333-3333-333333333302"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "GM", null },
                    { new Guid("33333333-3333-3333-3333-333333333303"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Chairman", null }
                });

            migrationBuilder.InsertData(
                table: "tbl_request_type",
                columns: new[] { "RequestTypeId", "CreatedAt", "Description", "RequestTypeCode", "RequestTypeName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222201"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Starts from an approved Proposal", "PROJECT", "Project", null },
                    { new Guid("22222222-2222-2222-2222-222222222202"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Created directly by Procurement", "NON_PROJECT", "Non-Project", null }
                });

            migrationBuilder.InsertData(
                table: "tbl_role",
                columns: new[] { "RoleId", "CreatedAt", "Description", "RoleCode", "RoleName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Creates proposals and project purchase requests", "REQUESTER", "Requester", null },
                    { new Guid("11111111-1111-1111-1111-111111111102"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Reviews proposals and creates non-project requests / POs", "PROCURE", "Procurement", null },
                    { new Guid("11111111-1111-1111-1111-111111111103"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Approves purchase requests and records Chairman decisions", "GM", "General Manager", null },
                    { new Guid("11111111-1111-1111-1111-111111111104"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Final offline approver", "CHAIRMAN", "Chairman", null },
                    { new Guid("11111111-1111-1111-1111-111111111105"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generates purchase orders", "PROJECT_ADMIN", "Project Admin", null },
                    { new Guid("11111111-1111-1111-1111-111111111106"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System administrator", "ADMIN", "Administrator", null }
                });

            migrationBuilder.InsertData(
                table: "tbl_status",
                columns: new[] { "StatusId", "CreatedAt", "IsDeleted", "StatusName", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444401"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "DRAFT", null },
                    { new Guid("44444444-4444-4444-4444-444444444402"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "PENDING", null },
                    { new Guid("44444444-4444-4444-4444-444444444403"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "SUBMITTED", null },
                    { new Guid("44444444-4444-4444-4444-444444444404"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "UNDER_REVIEW", null },
                    { new Guid("44444444-4444-4444-4444-444444444405"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "APPROVED", null },
                    { new Guid("44444444-4444-4444-4444-444444444406"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "REJECTED", null },
                    { new Guid("44444444-4444-4444-4444-444444444407"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "REVISION_REQUIRED", null },
                    { new Guid("44444444-4444-4444-4444-444444444408"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "PARTIALLY_APPROVED", null },
                    { new Guid("44444444-4444-4444-4444-444444444409"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "OPEN", null },
                    { new Guid("44444444-4444-4444-4444-444444444410"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "PARTIALLY_RECEIVED", null },
                    { new Guid("44444444-4444-4444-4444-444444444411"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "FULLY_RECEIVED", null },
                    { new Guid("44444444-4444-4444-4444-444444444412"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "UNPAID", null },
                    { new Guid("44444444-4444-4444-4444-444444444413"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "PARTIALLY_PAID", null },
                    { new Guid("44444444-4444-4444-4444-444444444414"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "PAID", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_account_CreatedBy",
                table: "tbl_account",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_account_Email",
                table: "tbl_account",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_account_RoleId",
                table: "tbl_account",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_account_UpdatedBy",
                table: "tbl_account",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_activity_log_AccountId",
                table: "tbl_activity_log",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_activity_log_DocumentType_DocumentId",
                table: "tbl_activity_log",
                columns: new[] { "DocumentType", "DocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_approval_record_ApprovalStageId",
                table: "tbl_approval_record",
                column: "ApprovalStageId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_approval_record_CreatedBy",
                table: "tbl_approval_record",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_approval_record_ProcurementRequestId_ApprovalStageId",
                table: "tbl_approval_record",
                columns: new[] { "ProcurementRequestId", "ApprovalStageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_approval_record_StatusId",
                table: "tbl_approval_record",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_approval_record_UpdatedBy",
                table: "tbl_approval_record",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_company_CompanyCode",
                table: "tbl_company",
                column: "CompanyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_company_CreatedBy",
                table: "tbl_company",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_company_UpdatedBy",
                table: "tbl_company",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goods_receipt_CreatedBy",
                table: "tbl_goods_receipt",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goods_receipt_GoodsReceiptNo",
                table: "tbl_goods_receipt",
                column: "GoodsReceiptNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goods_receipt_PurchaseOrderId",
                table: "tbl_goods_receipt",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goods_receipt_ReceivedBy",
                table: "tbl_goods_receipt",
                column: "ReceivedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goods_receipt_StatusId",
                table: "tbl_goods_receipt",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goods_receipt_UpdatedBy",
                table: "tbl_goods_receipt",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goods_receipt_detail_CreatedBy",
                table: "tbl_goods_receipt_detail",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goods_receipt_detail_GoodsReceiptId",
                table: "tbl_goods_receipt_detail",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goods_receipt_detail_MaterialId",
                table: "tbl_goods_receipt_detail",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goods_receipt_detail_PurchaseOrderDetailId",
                table: "tbl_goods_receipt_detail",
                column: "PurchaseOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goods_receipt_detail_UomId",
                table: "tbl_goods_receipt_detail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_goods_receipt_detail_UpdatedBy",
                table: "tbl_goods_receipt_detail",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_material_MaterialCode",
                table: "tbl_material",
                column: "MaterialCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_material_UomId",
                table: "tbl_material",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_procurement_request_CreatedBy",
                table: "tbl_procurement_request",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_procurement_request_ProcurementRequestNo",
                table: "tbl_procurement_request",
                column: "ProcurementRequestNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_procurement_request_ProposalId",
                table: "tbl_procurement_request",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_procurement_request_RequesterId",
                table: "tbl_procurement_request",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_procurement_request_RequestTypeId",
                table: "tbl_procurement_request",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_procurement_request_StatusId",
                table: "tbl_procurement_request",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_procurement_request_UpdatedBy",
                table: "tbl_procurement_request",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_proposal_CreatedBy",
                table: "tbl_proposal",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_proposal_ProposalNo",
                table: "tbl_proposal",
                column: "ProposalNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_proposal_RequesterId",
                table: "tbl_proposal",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_proposal_StatusId",
                table: "tbl_proposal",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_proposal_UpdatedBy",
                table: "tbl_proposal",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_proposal_detail_MaterialId",
                table: "tbl_proposal_detail",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_proposal_detail_ProposalId",
                table: "tbl_proposal_detail",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_proposal_detail_UomId",
                table: "tbl_proposal_detail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_CompanyId",
                table: "tbl_purchase_order",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_CreatedBy",
                table: "tbl_purchase_order",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_PurchaseOrderNo",
                table: "tbl_purchase_order",
                column: "PurchaseOrderNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_PurchaseRequestId",
                table: "tbl_purchase_order",
                column: "PurchaseRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_StatusId",
                table: "tbl_purchase_order",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_UpdatedBy",
                table: "tbl_purchase_order",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_VendorId",
                table: "tbl_purchase_order",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_detail_MaterialId",
                table: "tbl_purchase_order_detail",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_detail_PurchaseOrderId",
                table: "tbl_purchase_order_detail",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_detail_UomId",
                table: "tbl_purchase_order_detail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_payment_CreatedBy",
                table: "tbl_purchase_order_payment",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_payment_GoodsReceiptId",
                table: "tbl_purchase_order_payment",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_payment_PurchaseOrderId",
                table: "tbl_purchase_order_payment",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_payment_StatusId",
                table: "tbl_purchase_order_payment",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_order_payment_UpdatedBy",
                table: "tbl_purchase_order_payment",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_request_CreatedBy",
                table: "tbl_purchase_request",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_request_ProcurementRequestId",
                table: "tbl_purchase_request",
                column: "ProcurementRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_request_StatusId",
                table: "tbl_purchase_request",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_request_UpdatedBy",
                table: "tbl_purchase_request",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_request_detail_CreatedBy",
                table: "tbl_purchase_request_detail",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_request_detail_MaterialId",
                table: "tbl_purchase_request_detail",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_request_detail_PurchaseRequestId",
                table: "tbl_purchase_request_detail",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_request_detail_UomId",
                table: "tbl_purchase_request_detail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_purchase_request_detail_UpdatedBy",
                table: "tbl_purchase_request_detail",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_request_type_RequestTypeCode",
                table: "tbl_request_type",
                column: "RequestTypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_role_RoleCode",
                table: "tbl_role",
                column: "RoleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_uom_UomCode",
                table: "tbl_uom",
                column: "UomCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_detail_AccountId",
                table: "tbl_user_detail",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_vendor_CreatedBy",
                table: "tbl_vendor",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_vendor_UpdatedBy",
                table: "tbl_vendor",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_vendor_VendorCode",
                table: "tbl_vendor",
                column: "VendorCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_activity_log");

            migrationBuilder.DropTable(
                name: "tbl_approval_record");

            migrationBuilder.DropTable(
                name: "tbl_goods_receipt_detail");

            migrationBuilder.DropTable(
                name: "tbl_proposal_detail");

            migrationBuilder.DropTable(
                name: "tbl_purchase_order_payment");

            migrationBuilder.DropTable(
                name: "tbl_purchase_request_detail");

            migrationBuilder.DropTable(
                name: "tbl_user_detail");

            migrationBuilder.DropTable(
                name: "tbl_approval_stage");

            migrationBuilder.DropTable(
                name: "tbl_purchase_order_detail");

            migrationBuilder.DropTable(
                name: "tbl_goods_receipt");

            migrationBuilder.DropTable(
                name: "tbl_material");

            migrationBuilder.DropTable(
                name: "tbl_purchase_order");

            migrationBuilder.DropTable(
                name: "tbl_uom");

            migrationBuilder.DropTable(
                name: "tbl_company");

            migrationBuilder.DropTable(
                name: "tbl_purchase_request");

            migrationBuilder.DropTable(
                name: "tbl_vendor");

            migrationBuilder.DropTable(
                name: "tbl_procurement_request");

            migrationBuilder.DropTable(
                name: "tbl_proposal");

            migrationBuilder.DropTable(
                name: "tbl_request_type");

            migrationBuilder.DropTable(
                name: "tbl_account");

            migrationBuilder.DropTable(
                name: "tbl_status");

            migrationBuilder.DropTable(
                name: "tbl_role");
        }
    }
}
