BEGIN -- Cleanup

	Truncate Table [dbo].Settings;

	DECLARE @DocumentId int = 0, @State NVARCHAR (255), @DocumentType NVARCHAR (255), @SerialNumber int, @Mode NVARCHAR (255), @ResponsibleAgent int;
	DECLARE @LineNumber int = 0, @DocumentOffset int = 0;
	DECLARE @EntryNumber int = 0, @Operation int, @Memo NVARCHAR (255), @Reference NVARCHAR (255), @Account NVARCHAR (255), @Custody int, @Resource int, @Direction smallint, @Amount money, @Value money, @Note NVARCHAR (255);
	DECLARE @Documents DocumentList, @Lines LineList, @Entries EntryList, @ValidationMessage nvarchar(1024);
	
	-- List of Concepts
	DECLARE @EventDateTime datetimeoffset(7), @Supplier int, @Customer int, @Employee int, @Shareholder int, @Investment int, @Debtor int, @Creditor int;
	DECLARE @ReceivingWarehouse int, @IssuingWarehouse int, @ReceivingCashier int, @IssuingCashier int;

	DECLARE @item int, @Quantity money, @PriceVATExclusive money, @VAT money, @LineTotal money, @CashReceiptNumber NVARCHAR (255);
	DECLARE @Payment money, @AmountWithheld money, @WithholdingNumber NVARCHAR (255), @TaxableAmount money, @Warehouse int, @InvoiceDate datetimeoffset(7), @TypeOfTransaction NVARCHAR (255);
	DECLARE @SalaryAmount money, @Attendance money, @Department int, @EmployeeTaxableIncome money, @EmployeeIncomeTax money;

	DECLARE @MonthStarts datetimeoffset(7), @MonthEnds datetimeoffset(7), @StartDatetime datetimeoffset(7), @EndDatetime datetimeoffset(7);
	
	DECLARE @Organization int, @Currency int, @Date datetimeoffset(7), @BasicSalary money, @TransportationAllowance money, @NumberOfDays money;

	DECLARE @Cashier int, @ExpenseType NVARCHAR (255), @InvoiceNumber NVARCHAR (255), @MachineNumber NVARCHAR (255);
END
-- get acceptable document types; and user permissions and general settings;
IF (1=1)-- Journal Vouchers
IF (1=0)-- Purchase Order
BEGIN 
	SELECT @DocumentId = @DocumentId + 1, @State = N'Order', @DocumentType = N'Purchase', @Mode = N'Draft';

	INSERT INTO @Documents([Id], [State], [DocumentType], [SerialNumber], [Mode], [FolderId], [Memo], [ResponsibleAgentId],
  [StartDateTime], [EndDateTime], [LinesAgentId1], [LinesAgentId2], [LinesAgentId3], [LinesReference1],	[LinesReference2], [LinesReference3])
	VALUES(@DocumentId, @State, @DocumentType, @SerialNumber, @Mode, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

	SELECT @ResponsibleAgent = @AyelechHora, @Supplier = @Lifan, @StartDatetime = '2018.01.02', @EndDatetime = DATEADD(D, 1, @StartDatetime);
-- Line 1: Camry
	SELECT @LineNumber = @LineNumber + 1, @Operation = @Existing, @item = @Camry2018, @Quantity = 2, @PriceVATExclusive = 30000;
	SELECT @VAT = 0.15 * @PriceVATExclusive, @LineTotal = @PriceVATExclusive + @VAT;
	INSERT INTO @WideLines(DocumentId, LineNumber, DocumentType, ResponsibleAgentId, StartDateTime, EndDateTime, Operation1, AgentId1, ResourceId1, Amount1, Amount2, RelatedAmount2, Amount3)
	VALUES(@DocumentId, @LineNumber, @DocumentType, @ResponsibleAgent, @StartDateTime, @EndDateTime, @Operation, @Supplier, @item, @Quantity, @VAT, @PriceVATExclusive, @LineTotal);	
-- Line 2: Teddy bear

	SELECT @LineNumber = @LineNumber + 1, @Operation = @Existing, @item = @TeddyBear, @Quantity = 5, @PriceVATExclusive = 500;
	SELECT @VAT = 0.15 * @PriceVATExclusive, @LineTotal = @PriceVATExclusive + @VAT;
	INSERT INTO @WideLines(DocumentId, LineNumber, DocumentType, ResponsibleAgentId, StartDateTime, EndDateTime, Operation1, AgentId1, ResourceId1, Amount1, Amount2, RelatedAmount2, Amount3)
	VALUES(@DocumentId, @LineNumber, @DocumentType, @ResponsibleAgent, @StartDateTime, @EndDateTime, @Operation, @Supplier, @item, @Quantity, @VAT, @PriceVATExclusive, @LineTotal);	
	
	EXEC ui_Documents_WideLines__Validate @Documents = @Documents, @WideLines = @WideLines, @ValidationMessage = @ValidationMessage OUTPUT
	IF @ValidationMessage IS NOT NULL GOTO UI_Error;
END
IF (1=0)-- Purchase Event
BEGIN
	SELECT @DocumentId = @DocumentId + 1, @State = N'Voucher', @DocumentType = N'CashIssueToSupplier', @Mode = N'Draft';
	INSERT INTO @Documents([Id], [State], [DocumentType], [SerialNumber], [Mode], [FolderId], [Memo], [ResponsibleAgentId],
  [StartDateTime], [EndDateTime], [LinesAgentId1], [LinesAgentId2], [LinesAgentId3], [LinesReference1],	[LinesReference2], [LinesReference3])
	VALUES(@DocumentId, @State, @DocumentType, @SerialNumber, @Mode, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

	SELECT @ResponsibleAgent = @TizitaNigussie, @Supplier = @Lifan, @StartDatetime = '2018.01.03', @EndDatetime = DATEADD(D, 1, @StartDatetime);

-- Payment
	SELECT @LineNumber = @LineNumber + 1, @Operation = @WSI, @Payment = 34465, @Cashier = @TigistNegash, @CashReceiptNumber = N'7023'
	INSERT INTO @WideLines(DocumentId, LineNumber, [DocumentType], ResponsibleAgentId, StartDateTime, EndDateTime, Operation1, AgentId1, Amount1, AgentId2, Reference2)
	VALUES(@DocumentId, @LineNumber, @DocumentType, @ResponsibleAgent, @StartDateTime, @EndDateTime, @Operation, @Supplier, @Payment, @Cashier, @CashReceiptNumber);

	SELECT @DocumentId = @DocumentId + 1, @State = N'Voucher', @DocumentType = N'PurchaseWitholdingTax', @Mode = N'Draft';
	INSERT INTO @Documents([Id], [State], [DocumentType], [SerialNumber], [Mode], [FolderId], [Memo], [ResponsibleAgentId],
  [StartDateTime], [EndDateTime], [LinesAgentId1], [LinesAgentId2], [LinesAgentId3], [LinesReference1],	[LinesReference2], [LinesReference3])
	VALUES(@DocumentId, @State, @DocumentType, @SerialNumber, @Mode, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

	SELECT @ResponsibleAgent = @TizitaNigussie, @Supplier = @Lifan, @StartDatetime = '2018.01.03', @EndDatetime = DATEADD(D, 1, @StartDatetime), @Memo = N'Assets Purchase';

-- Witholding tax: 
	SELECT @LineNumber = @LineNumber + 1, @Operation = @WSI, @Supplier = @Lifan, @AmountWithheld = 610, @WithholdingNumber = N'0006';
	SELECT @TaxableAmount = @AmountWithheld/0.02;
	INSERT INTO @WideLines(DocumentId, LineNumber, [DocumentType], ResponsibleAgentId, StartDateTime, EndDateTime, Memo, 
		Operation1, AgentId1, Amount1, Reference2, RelatedAmount2, RelatedReference2)
	VALUES(@DocumentId,	@LineNumber, @DocumentType, @ResponsibleAgent, @StartDateTime, @EndDateTime, @Memo,
		@Operation, @Supplier, @AmountWithheld, @WithholdingNumber, @TaxableAmount, @TypeOfTransaction);
	/*
	SELECT @LineType = N'StockReceiptFromSupplier', @ResponsibleAgent = @AyelechHora, @Supplier = @Lifan, @StartDatetime = @RecordedOnDateTime, @EndDatetime = DATEADD(D, 1, @RecordedOnDateTime);
-- Stock receipt
-- Camry
	SELECT @LineNumber = @LineNumber + 1, @Operation = @Expansion, @item = @Camry2018, @Quantity = 2, @Warehouse = @FinishedGoodsWarehouse;
	INSERT INTO @WideLines(DocumentId, LineNumber, LineType, ResponsibleAgentId, StartDateTime, EndDateTime, Operation1, AgentId1, ResourceId1, Amount1, AgentId2)
	VALUES(@DocumentId, @LineNumber, @LineType, @ResponsibleAgent, @StartDateTime, @EndDateTime, @Operation, @Warehouse, @item, @Quantity, @Supplier);

-- Teddy bear
	SELECT @LineNumber = @LineNumber + 1, @Operation = @Existing, @item = @TeddyBear, @Quantity = 5, @Warehouse = @RawMaterialsWarehouse;
	INSERT INTO @WideLines(DocumentId, LineNumber, LineType, ResponsibleAgentId, StartDateTime, EndDateTime, Operation1, AgentId1, ResourceId1, Amount1, AgentId2)
	VALUES(@DocumentId, @LineNumber, @LineType, @ResponsibleAgent, @StartDateTime, @EndDateTime, @Operation, @Warehouse, @item, @Quantity, @Supplier);
	*/

	SELECT @DocumentId = @DocumentId + 1, @State = N'Voucher', @DocumentType = N'Purchase', @Mode = N'Draft';
		
	INSERT INTO @Documents([Id], [State], [DocumentType], [SerialNumber], [Mode], [FolderId], [Memo], [ResponsibleAgentId],
  [StartDateTime], [EndDateTime], [LinesAgentId1], [LinesAgentId2], [LinesAgentId3], [LinesReference1],	[LinesReference2], [LinesReference3])
	VALUES(@DocumentId, @State, @DocumentType, @SerialNumber, @Mode, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

	SELECT @ResponsibleAgent = @AyelechHora, @Supplier = @Lifan, @StartDatetime = '2018.01.31', @EndDatetime = DATEADD(D, 1, @StartDatetime);
-- Purchase invoice
	SELECT @LineNumber = @LineNumber + 1, @Operation = @Expansion, @VAT = 4575, @InvoiceNumber = N'0913', @MachineNumber = N'fs4512219',
			@item = @Camry2018, @Quantity = 2, @PriceVATExclusive = 300000;
	INSERT INTO @WideLines(DocumentId, LineNumber, [DocumentType], ResponsibleAgentId, StartDateTime, EndDateTime, 
			Operation1, AgentId1, ResourceId1, Amount1,	Value1,				Amount2, Reference2, RelatedReference2)
	VALUES(@DocumentId, @LineNumber, @DocumentType, @ResponsibleAgent, @StartDateTime, @EndDateTime,			
			@Operation,	@Supplier,@item,	@Quantity, @PriceVATExclusive, @VAT, @InvoiceNumber, @MachineNumber);
END
IF (1=0)-- Employment Contract
BEGIN
	SELECT @DocumentId = @DocumentId + 1, @State = N'Order', @DocumentType = N'Labor', @Mode = N'Draft';

	INSERT INTO @Documents([Id], [State], [DocumentType], [SerialNumber], [Mode], [FolderId], [Memo], [ResponsibleAgentId],
  [StartDateTime], [EndDateTime], [LinesAgentId1], [LinesAgentId2], [LinesAgentId3], [LinesReference1],	[LinesReference2], [LinesReference3])
	VALUES(@DocumentId, @State, @DocumentType, @SerialNumber, @Mode, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

	SELECT @ResponsibleAgent = @BadegeKebede, @Employee = @MohamadAkra, 
		@StartDatetime = '2019.01.01', @EndDatetime = DATEADD(MONTH, 24, @StartDatetime);

-- Line 1: MA, Basic
	SELECT @LineNumber = @LineNumber + 1, @Operation = @Existing, @Employee = @MohamadAkra, @BasicSalary = 7000, @TransportationAllowance = 1750
	INSERT INTO @WideLines(DocumentId, LineNumber, [DocumentType], ResponsibleAgentId, StartDateTime, EndDateTime, Operation1, AgentId1, Amount2, Amount3)
	VALUES(@DocumentId, @LineNumber, @DocumentType, @ResponsibleAgent, @StartDateTime, @EndDateTime, @Operation, @Employee, @BasicSalary, @TransportationAllowance);
-- Line 3: AA, Basic
	SELECT @LineNumber = @LineNumber + 1, @Operation = @Existing, @Employee = @AhmadAkra, @BasicSalary = 7000, @TransportationAllowance = 0
	INSERT INTO @WideLines(DocumentId, LineNumber, [DocumentType], ResponsibleAgentId, StartDateTime, EndDateTime, Memo, Operation1, AgentId1, Amount2, Amount3)
	VALUES(@DocumentId, @LineNumber, @DocumentType, @ResponsibleAgent, @StartDateTime, @EndDateTime, @Memo, @Operation, @Employee, @BasicSalary, @TransportationAllowance);
END
IF (1=0)-- Attendance Event
BEGIN
	DELETE FROM @WideLines;
	SELECT @State = N'Voucher', @DocumentType = N'Payroll';

	SELECT @DocumentType = N'LaborReceiptFromEmployee';
-- Labor receipt
-- MA
	SELECT @LineNumber = @LineNumber + 1, @Operation = @Expansion, @Employee = @MohamadAkra, @Attendance = 208, @Department = @ExecOffice;
	INSERT INTO @WideLines(DocumentId, LineNumber, [DocumentType], ResponsibleAgentId, StartDateTime, EndDateTime, Memo, Operation1, AgentId1, Amount1, AgentId2)
	VALUES(@DocumentId, @LineNumber, @DocumentType, @ResponsibleAgent, @StartDateTime, @EndDateTime, @Memo, @Operation, @Department, @Attendance, @Employee);
-- Ahmad
	SELECT @LineNumber = @LineNumber + 1, @Operation = @Expansion, @Employee = @AhmadAkra, @Attendance = 208, @Department = @ProductionDept;
	INSERT INTO @WideLines(DocumentId, LineNumber, [DocumentType], ResponsibleAgentId, StartDateTime, EndDateTime, Memo, Operation1, AgentId1, Amount1, AgentId2)
	VALUES(@DocumentId, @LineNumber, @DocumentType, @ResponsibleAgent, @StartDateTime, @EndDateTime, @Memo, @Operation, @Department, @Attendance, @Employee);

	SELECT @DocumentType = N'EmployeeIncomeTax';
	SELECT @LineNumber = @LineNumber + 1, @Operation = @WSI, @Employee = @MohamadAkra, @EmployeeTaxableIncome = 7000, @EmployeeIncomeTax = 1105;
	INSERT INTO @WideLines(DocumentId, LineNumber, [DocumentType], ResponsibleAgentId, StartDateTime, EndDateTime, Memo, Operation1, AgentId1, Amount1, RelatedAmount2)
	VALUES(@DocumentId, @LineNumber, @DocumentType, @ResponsibleAgent, @StartDateTime, @EndDateTime, @Memo, @Operation, @Employee, @EmployeeIncomeTax, @EmployeeTaxableIncome);

	SELECT @LineNumber = @LineNumber + 1, @Operation = @WSI, @Employee = @AhmadAkra, @EmployeeTaxableIncome = 7000, @EmployeeIncomeTax = 1105;
	INSERT INTO @WideLines(DocumentId, LineNumber, [DocumentType], ResponsibleAgentId, StartDateTime, EndDateTime, Memo, Operation1, AgentId1, Amount1, RelatedAmount2)
	VALUES(@DocumentId, @LineNumber, @DocumentType, @ResponsibleAgent, @StartDateTime, @EndDateTime, @Memo, @Operation, @Employee, @EmployeeIncomeTax, @EmployeeTaxableIncome);
/*
--	SELECT @LineType = N'CashPaymentToEmployee';
-- Payment
	SELECT @LineNumber = @LineNumber + 1, @Operation = @WSI, @Supplier = @Lifan, @Payment = 34465, @Cashier = @TigistNegash, @CashReceiptNumber = N'7023'
	INSERT INTO @WideLines(LineNumber, LineType, Operation1, AgentId1, Amount1, AgentId2, Reference2)
	VALUES(		@LineNumber, @LineType, @Operation, @Supplier, @Payment, @Cashier, @CashReceiptNumber);
*/
END
IF (1=0)-- Inventory transfer order
BEGIN
	DELETE FROM @WideLines;
	SELECT @State = N'Order', @DocumentType = N'InventoryTransfer';

	SELECT @DocumentType = N'InventoryTransferOrder';
-- Payment
	SELECT @LineNumber = @LineNumber + 1, @Operation = @WSI, @IssuingWarehouse = @RawMaterialsWarehouse, @ReceivingWarehouse = @FinishedGoodsWarehouse, @Item = @TeddyBear, @Quantity = 10, @Value = 100, @EventDateTime = '2018.01.02'
	INSERT INTO @WideLines(DocumentId, LineNumber, [DocumentType], ResponsibleAgentId, StartDateTime, EndDateTime, Memo, Operation1, AgentId2, AgentId1, ResourceId1, Amount1, Value1)
	VALUES(@DocumentId, @LineNumber, @DocumentType, @ResponsibleAgent, @StartDateTime, @EndDateTime, @Memo, @Operation, @IssuingWarehouse, @ReceivingWarehouse, @Item, @Quantity, @Value);
END
--	IF (1=0)-- Inventory transfer event

EXEC [dbo].[api_Documents_WideLines__Save] @Documents = @Documents, @WideLines = @WideLines, @Lines = @Lines, @Entries = @Entries, @DocumentOffset = @DocumentOffset Output
EXEC [dbo].[api_Transactions__Post] @Documents = @Documents;
RETURN
UI_Error:
	Print @ValidationMessage;
RETURN