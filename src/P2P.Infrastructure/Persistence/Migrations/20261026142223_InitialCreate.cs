using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2P.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "procurement");

            migrationBuilder.EnsureSchema(
                name: "infrastructure");

            migrationBuilder.EnsureSchema(
                name: "payables");

            migrationBuilder.CreateTable(
                name: "ApprovalPolicies",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CostCentreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    RequiredRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEntries",
                schema: "infrastructure",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", maxLength: 400, nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", maxLength: 400, nullable: true),
                    ChangedColumns = table.Column<string>(type: "nvarchar(max)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CostCentres",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DepartmentHeadUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCentres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentSequences",
                schema: "infrastructure",
                columns: table => new
                {
                    DocumentType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    NextValue = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSequences", x => new { x.DocumentType, x.Year });
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceipts",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    DeliveryNoteReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsReversed = table.Column<bool>(type: "bit", nullable: false),
                    ReversalReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                schema: "payables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InternalNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    VendorInvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VendorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MatchedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ApprovedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountPaidCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "payables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    VendorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorBankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    AuthorisedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorisedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    VendorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequisitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BuyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    OrderDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentTermsDays = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IssuedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ShipToCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShipToCountryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    ShipToLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShipToLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShipToPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShipToState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Requisitions",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequesterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CostCentreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Justification = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NeededBy = table.Column<DateOnly>(type: "date", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requisitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TradingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TaxIdentifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DefaultCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PaymentTermsDays = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoldReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RemitToCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RemitToCountryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    RemitToLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RemitToLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RemitToPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RemitToState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiptLines",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodsReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityAccepted = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityRejected = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiptLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptLines_GoodsReceipts_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalSchema: "procurement",
                        principalTable: "GoodsReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLines",
                schema: "payables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxRatePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "payables",
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchExceptions",
                schema: "payables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ExpectedValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ActualValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchExceptions_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "payables",
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAllocations",
                schema: "payables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "payables",
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderLines",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    QuantityOrdered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TaxRatePercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityReceived = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    QuantityInvoiced = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "procurement",
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalSteps",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequisitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    RequiredRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ActionedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActionedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_Requisitions_RequisitionId",
                        column: x => x.RequisitionId,
                        principalSchema: "procurement",
                        principalTable: "Requisitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequisitionLines",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequisitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SuggestedVendorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisitionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequisitionLines_Requisitions_RequisitionId",
                        column: x => x.RequisitionId,
                        principalSchema: "procurement",
                        principalTable: "Requisitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortCodeOrSwift = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccounts_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalSchema: "procurement",
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalPolicies_Active_Currency_Amount",
                schema: "procurement",
                table: "ApprovalPolicies",
                columns: new[] { "IsActive", "Currency", "MinimumAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_Status_Role",
                schema: "procurement",
                table: "ApprovalSteps",
                columns: new[] { "Status", "RequiredRole" });

            migrationBuilder.CreateIndex(
                name: "UX_ApprovalSteps_Requisition_Sequence",
                schema: "procurement",
                table: "ApprovalSteps",
                columns: new[] { "RequisitionId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_OccurredAt",
                schema: "infrastructure",
                table: "AuditEntries",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_Table_Entity",
                schema: "infrastructure",
                table: "AuditEntries",
                columns: new[] { "TableName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_Vendor",
                schema: "procurement",
                table: "BankAccounts",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "UX_CostCentres_Code",
                schema: "procurement",
                table: "CostCentres",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptLines_GoodsReceipt",
                schema: "procurement",
                table: "GoodsReceiptLines",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_PurchaseOrder",
                schema: "procurement",
                table: "GoodsReceipts",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "UX_GoodsReceipts_Number",
                schema: "procurement",
                table: "GoodsReceipts",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_Invoice",
                schema: "payables",
                table: "InvoiceLines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PurchaseOrder",
                schema: "payables",
                table: "Invoices",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Vendor_Status",
                schema: "payables",
                table: "Invoices",
                columns: new[] { "VendorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Vendor_VendorInvoiceNumber",
                schema: "payables",
                table: "Invoices",
                columns: new[] { "VendorId", "VendorInvoiceNumber" });

            migrationBuilder.CreateIndex(
                name: "UX_Invoices_InternalNumber",
                schema: "payables",
                table: "Invoices",
                column: "InternalNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchExceptions_Invoice",
                schema: "payables",
                table: "MatchExceptions",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_Invoice",
                schema: "payables",
                table: "PaymentAllocations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_Payment",
                schema: "payables",
                table: "PaymentAllocations",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Vendor_Status",
                schema: "payables",
                table: "Payments",
                columns: new[] { "VendorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_Payments_Number",
                schema: "payables",
                table: "Payments",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_PurchaseOrder",
                schema: "procurement",
                table: "PurchaseOrderLines",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_Requisition",
                schema: "procurement",
                table: "PurchaseOrders",
                column: "RequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_Vendor_Status",
                schema: "procurement",
                table: "PurchaseOrders",
                columns: new[] { "VendorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_PurchaseOrders_Number",
                schema: "procurement",
                table: "PurchaseOrders",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequisitionLines_Requisition",
                schema: "procurement",
                table: "RequisitionLines",
                column: "RequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_Requester_Status",
                schema: "procurement",
                table: "Requisitions",
                columns: new[] { "RequesterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_Status_Submitted",
                schema: "procurement",
                table: "Requisitions",
                columns: new[] { "Status", "SubmittedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_Requisitions_Number",
                schema: "procurement",
                table: "Requisitions",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_Status",
                schema: "procurement",
                table: "Vendors",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "UX_Vendors_Code",
                schema: "procurement",
                table: "Vendors",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalPolicies",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "ApprovalSteps",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "AuditEntries",
                schema: "infrastructure");

            migrationBuilder.DropTable(
                name: "BankAccounts",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "CostCentres",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "DocumentSequences",
                schema: "infrastructure");

            migrationBuilder.DropTable(
                name: "GoodsReceiptLines",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "InvoiceLines",
                schema: "payables");

            migrationBuilder.DropTable(
                name: "MatchExceptions",
                schema: "payables");

            migrationBuilder.DropTable(
                name: "PaymentAllocations",
                schema: "payables");

            migrationBuilder.DropTable(
                name: "PurchaseOrderLines",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "RequisitionLines",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "Vendors",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "GoodsReceipts",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "Invoices",
                schema: "payables");

            migrationBuilder.DropTable(
                name: "Payments",
                schema: "payables");

            migrationBuilder.DropTable(
                name: "PurchaseOrders",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "Requisitions",
                schema: "procurement");
        }
    }
}
