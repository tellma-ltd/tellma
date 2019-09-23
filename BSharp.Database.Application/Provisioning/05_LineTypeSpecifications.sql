DECLARE @LineTypes TABLE(
	[id]				NVARCHAR (255) PRIMARY KEY
);
INSERT @LineTypes ([Id]) VALUES
	(N'ManualLine'),						-- Account, Direction, amount, Resource, Agent Account, ...

	-- cash payments and receipts are assumed to be in the transaction currency
	-- any currency conversions, bank fees, are to be added by a manual JV tab
	(N'CashReceipt.Functional'),			-- [received from:RelatedAccountAccount], [currency], amount, received in:AgentAccount, details:RelatedResource, memo
		(N'CheckReceipt.Functional'),		-- check receipt in hand
		(N'BankReceipt.Functional'),		-- direct deposit in bank	

	(N'CashIssue.Functional'),				-- [paid to:RelatedAccountAccount], [currency], amount, paid from:AgentAccount, details:RelatedResource, memo
		(N'CheckIssueToBeneficiary.Functional'),			-- existing or new
		(N'BankIssue.Functional'),			-- direct transfer

	(N'CashTransferToBank.Functional'),			-- [FromCashAccount],[ToCashAccount], [currency], amount, resource details
		(N'BankTransferToBank.Functional'),
		(N'CheckTransferToBank.Functional'),
		(N'CheckTransferToCustody.Functional'),		-- from custody to bank

	(N'EquityIssue'),						-- [investor], [currency], sharetype, qty, unit price, {total price}

	-- scheduling of receivables and payables
	(N'ReceivableDebit'),					-- [currency], amount, [debit a/c:AgentAccount], memo, settled by:
	(N'PayablesCredit'),					-- [currency], amount, [credit a/c:AgentAccount], memo, settled by:

	-- settling of receivables and payables. due date to be selected from available options
	(N'ReceivableCredit'),					-- [currency], amount, account:AgentAccount, memo, due date
	(N'PayableDebit'),						-- [currency], amount, account:AgentAccount, memo, due date
	
	-- withholding are assumed to be in the transaction currency
	-- any currency conversions is to be added by a manual JV tab
	(N'CustomerTaxWithholding'),			-- [customer], [currency], [invoice], taxable amount, percent withheld, {amount withheld}

	(N'GoodIssueWithInvoice.Functional'),	-- [customer], [currency], [warehouse], [invoice], [machine], item, qty, unit price, {total price}, percent VAT, {VAT}, {line total}
	(N'ServiceIssueWithInvoice'),			-- [customer], [currency], [invoice], [machine], item, qty, unit price, {total price}, percent VAT, {VAT}, {line total}
	(N'LeaseOutIssueWithInvoice'),			-- [customer], [currency], [invoice], [machine], lease starts, ppe, qty, {lease ends}, unit price, {total price}, percent VAT, {VAT}, {line total}
	(N'GoodServiceInvoiceWithoutIssue'),	-- [customer], [currency], [invoice], [machine], item, qty, unit price, {total price}, percent VAT, {VAT}, {line total}
	(N'LeaseOutInvoiceWithoutIssue'),		-- [customer], [currency], [invoice], [machine], lease starts, ppe, qty, {lease ends}, unit price, {total price}, percent VAT, {VAT}, {line total}
	(N'LeaseOutIssueWithoutInvoice'),		-- [customer], [currency], lease starts, ppe, qty, {lease ends}, unit price, {total price}	
	(N'GoodIssueWithoutInvoice'),			-- [customer], [currency], [warehouse], [machine], item, qty, unit price, {total price}
	(N'ServiceIssueWithoutInvoice'),		-- [customer], [currency], item, qty, unit price, {total price}
	
	-- withholding are assumed to be in the transaction currency
	-- any currency conversions is to be added by a manual JV tab
	(N'SupplierTaxWithholding'),			-- [supplier], [currency], [invoice], taxable amount, percent withheld, {amount withheld}

	(N'GoodReceiptInTransitWithInvoice'),	-- [supplier], [currency], [Transit Company], [LC], item, qty, unit price, {total price}, percent VAT, {VAT}, {line total}
	(N'GoodReceiptWithInvoice'),			-- [supplier], [currency], [warehouse], [invoice], [machine], item, qty, unit price, {total price}, percent VAT, {VAT}, {line total}
	(N'GoodReceiptIssueWithInvoice'),		-- [supplier], [currency], [responsibility center], [IFRSNote], [invoice], [machine], item, qty, unit price, {total price}, percent VAT, {VAT}, {line total}
	(N'ServiceReceiptWithInvoice'),			-- [supplier], [currency], [invoice], [machine], item, qty, unit price, {total price}, percent VAT, {VAT}, {line total}
	(N'LeaseInReceiptWithInvoice'),			-- [supplier], [currency], [invoice], [machine], lease starts, ppe, qty, {lease ends}, unit price, {total price}, percent VAT, {VAT}, {line total}
	(N'LeaseInReceiptWithoutInvoice'),		-- [supplier], [currency], lease starts, ppe, qty, {lease ends}, unit price, {total price}	
	(N'LeaseInInvoiceWithoutReceipt'),		-- [supplier], [currency], [invoice], [machine], lease starts, ppe, qty, {lease ends}, unit price, {total price}, percent VAT, {VAT}, {line total}

	-- Leaves, overtimes, and penalties must be posted during the payroll period [period starts] - [period ends] (= start + month - 1)
	(N'LaborOvertime'),						-- [employee], date/time starts, overtime hours, {overtime type lk}, {equiv. hours}
	(N'LaborUnpaidAbsences'),				-- employee, date start, absence days, absence type lk, description
	(N'LaborPenalty'),						-- employee, penalty, reason lk, description
	(N'LaborHourlyLeave'),					-- employee, leave date, from time, to time, paid/unpaid, reason
	(N'LaborDailyLeave'),					-- employee, from Date, to Date, leave type lk, {paid/semipaid/unpaid}, reason

	-- several items might be mapped to the same account, such as basic salary and allowances
	(N'EmployeePayslip'),					-- [currency], [period starts], employee, {basic salary}, {transportation}, {hardship}, {overtimes}, {penalties}, {social sec tax}, {social sec contribution}, {income tax withheld}, {net salary}, {loands deductions}, {net pay}
	(N'FixedAssetDepreciation'),			-- [period starts], [period ends], ppe, usage {life}, {responsibility center}, {depreciated value}
	(N'FixedAssetDisposal'),				-- [currency], [customer], ppe, sales value
	
	-- cash payments and receipts are assumed to be in the transaction currency
	-- any currency conversions, bank fees, are to be added by a manual JV tab
	(N'GoodReceipt'),						-- [received from:RelatedAccountAccount], item, Qty, received in:[ToCustodianAccount:AgentAccount], Batch:RelatedResource, memo
	(N'GoodIssue'),							-- [issued to:RelatedAccountAccount], item, Qty, issued from:[FromCustodianAccount:AgentAccount], Batch:RelatedResource, memo
	(N'GoodConsumption'),					-- [responsibility center], item, qty, [consumed by:AgentAccount]
	(N'GoodTransfer'),						-- [FromCustodyAccount],[ToCustodyAccount], item, qty, batch details
	(N'LaborReceipt'),						-- [received from:RelatedAccountAccount], item, Qty, received in:[ToCustodianAccount:AgentAccount], Batch:RelatedResource, memo
	(N'LaborIssue'),						-- [issued to:RelatedAccountAccount], item, Qty, issued from:[FromCustodianAccount:AgentAccount], Batch:RelatedResource, memo
	(N'LaborConsumption');					-- [responsibility center], item, qty, [consumed by:AgentAccount]
DECLARE @WideLineTypesSpecifications TABLE (
	[LineDefinitionId]					NVARCHAR (255) PRIMARY KEY,

	[DirectionIsVisible1]			BIT				NOT NULL DEFAULT 0,
	[DirectionExpression1]			NVARCHAR (255),
	[DirectionEntryNumber1]			NVARCHAR (255),
	[Direction1]					SMALLINT,

	[AccountIdIsVisible1]			BIT				NOT NULL DEFAULT 0,
	[AccountIdFilter1]				NVARCHAR (255),
	[AccountIdExpression1]			NVARCHAR (255),
	[AccountIdEntryNumber1]			INT,

	[IfrsEntryClassificationIdIsVisible1]			BIT				NOT NULL DEFAULT 0,
	[IfrsEntryClassificationIdExpression1]			NVARCHAR (255),
	[IfrsEntryClassificationIdEntryNumber1]		INT,
	[IfrsEntryClassificationId1]					NVARCHAR (255),

	[ResourceIdIsVisible1]			BIT				NOT NULL DEFAULT 0,
	[ResourceIdExpression1]			NVARCHAR (255),
	[ResourceIdEntryNumber1]		INT,
	[ResourceId1]					INT,

	[InstanceIdIsVisible1]			BIT				NOT NULL DEFAULT 0,
	[InstanceIdExpression1]			NVARCHAR (255),
	[InstanceIdEntryNumber1]		INT,
	
	[BatchCodeIsVisible1]			BIT				NOT NULL DEFAULT 0,
	[BatchCodeExpression1]			NVARCHAR (255),
	[BatchCodeEntryNumber1]			INT,

	[DueDateIsVisible1]				BIT				NOT NULL DEFAULT 0,
	[DueDateExpression1]			NVARCHAR (255),
	[DueDateEntryNumber1]			INT,

	[QuantityIsVisible1]			BIT				NOT NULL DEFAULT 0,
	[QuantityExpression1]			NVARCHAR (255),
	[QuantityEntryNumber1]			INT,
	--[Quantity1]						VTYPE,

	[MoneyAmountIsVisible1]			BIT				DEFAULT 0,
	[MoneyAmountExpression1]		NVARCHAR (255),
	[MoneyAmountEntryNumber1]		INT,

	[MassIsVisible1]				BIT				DEFAULT 0,
	[MassExpression1]				NVARCHAR (255),
	[MassEntryNumber1]				INT,

	[VolumeIsVisible1]				BIT				DEFAULT 0,
	[VolumeExpression1]				NVARCHAR (255),
	[VolumeEntryNumber1]			INT,

	[AreaIsVisible1]				BIT				DEFAULT 0,
	[AreaExpression1]				NVARCHAR (255),
	[AreaEntryNumber1]				INT,

	[LengthIsVisible1]				BIT				DEFAULT 0,
	[LengthExpression1]				NVARCHAR (255),
	[LengthEntryNumber1]			INT,

	[TimeIsVisible1]				BIT				DEFAULT 0,
	[TimeExpression1]				NVARCHAR (255),
	[TimeEntryNumber1]				INT,

	[CountIsVisible1]				BIT				DEFAULT 0,
	[CountExpression1]				NVARCHAR (255),
	[CountEntryNumber1]				INT,
	[Count1]						INT,
	
	[ValueIsVisible1]				BIT				NOT NULL DEFAULT 0,
	[ValueExpression1]				NVARCHAR (255),
	[ValueEntryNumber1]				INT,

	[MemoIsVisible1]				BIT				NOT NULL DEFAULT 0,
	[MemoExpression1]				NVARCHAR (255),
	[MemoEntryNumber1]				INT,

	[ExternalReferenceIsVisible1]	BIT				NOT NULL DEFAULT 0,
	[ExternalReferenceExpression1]	NVARCHAR (255),
	[ExternalReferenceEntryNumber1]	INT,

	[AdditionalReferenceIsVisible1]	BIT				NOT NULL DEFAULT 0,
	[AdditionalReferenceExpression1]NVARCHAR (255),
	[AdditionalReferenceEntryNumber1]INT,

	[RelatedResourceId1]			INT, -- Good, Service, Labor, Machine usage

	[RelatedAccountIsVisible1]		BIT				NOT NULL DEFAULT 0,
	[RelatedAccountExpression1]		NVARCHAR (255),
	[RelatedAccountEntryNumber1]		INT,
		
	[RelatedQuantity1]				MONEY ,			-- used in Tax accounts, to store the quantiy of taxable item
	[RelatedMoneyAmount1]			MONEY 				NOT NULL DEFAULT 0, -- e.g., amount subject to tax

	[DirectionIsVisible2]			BIT				NOT NULL DEFAULT 0,
	[DirectionExpression2]			NVARCHAR (255),
	[DirectionEntryNumber2]			NVARCHAR (255),
	[Direction2]					SMALLINT,

	[AccountIdIsVisible2]			BIT				NOT NULL DEFAULT 0,
	[AccountIdFilter2]				NVARCHAR (255),
	[AccountIdExpression2]			NVARCHAR (255),
	[AccountIdEntryNumber2]			INT,

	[IfrsEntryClassificationIdIsVisible2]			BIT				NOT NULL DEFAULT 0,
	[IfrsEntryClassificationIdExpression2]			NVARCHAR (255),
	[IfrsEntryClassificationIdEntryNumber2]		INT,
	[IfrsEntryClassificationId2]					NVARCHAR (255),

	[ResourceIdIsVisible2]			BIT				NOT NULL DEFAULT 0,
	[ResourceIdExpression2]			NVARCHAR (255),
	[ResourceIdEntryNumber2]		INT,
	[ResourceId2]					INT,

	[InstanceIdIsVisible2]			BIT				NOT NULL DEFAULT 0,
	[InstanceIdExpression2]			NVARCHAR (255),
	[InstanceIdEntryNumber2]		INT,
	
	[BatchCodeIsVisible2]			BIT				NOT NULL DEFAULT 0,
	[BatchCodeExpression2]			NVARCHAR (255),
	[BatchCodeEntryNumber2]			INT,

	[DueDateIsVisible2]				BIT				NOT NULL DEFAULT 0,
	[DueDateExpression2]			NVARCHAR (255),
	[DueDateEntryNumber2]			INT,

	[QuantityIsVisible2]			BIT				NOT NULL DEFAULT 0,
	[QuantityExpression2]			NVARCHAR (255),
	[QuantityEntryNumber2]			INT,
--	[Quantity2]						VTYPE,

	[MoneyAmountIsVisible2]			BIT				NOT NULL DEFAULT 0,
	[MoneyAmountExpression2]		NVARCHAR (255),
	[MoneyAmountEntryNumber2]		INT,

	[MassIsVisible2]				BIT,
	[MassExpression2]				NVARCHAR (255),
	[MassEntryNumber2]				INT,

	[VolumeIsVisible2]				BIT,
	[VolumeExpression2]				NVARCHAR (255),
	[VolumeEntryNumber2]			INT,

	[AreaIsVisible2]				BIT,
	[AreaExpression2]				NVARCHAR (255),
	[AreaEntryNumber2]				INT,

	[LengthIsVisible2]				BIT,
	[LengthExpression2]				NVARCHAR (255),
	[LengthEntryNumber2]			INT,

	[TimeIsVisible2]				BIT,
	[TimeExpression2]				NVARCHAR (255),
	[TimeEntryNumber2]				INT,

	[CountIsVisible2]				BIT,
	[CountExpression2]				NVARCHAR (255),
	[CountEntryNumber2]				INT,
	[Count2]						INT,
	
	[ValueIsVisible2]				BIT				NOT NULL DEFAULT 0,
	[ValueExpression2]				NVARCHAR (255),
	[ValueEntryNumber2]				INT,

	[MemoIsVisible2]				BIT				NOT NULL DEFAULT 0,
	[MemoExpression2]				NVARCHAR (255),
	[MemoEntryNumber2]				INT,

	[ExternalReferenceIsVisible2]	BIT				NOT NULL DEFAULT 0,
	[ExternalReferenceExpression2]	NVARCHAR (255),
	[ExternalReferenceEntryNumber2]	INT,

	[AdditionalReferenceIsVisible2]	BIT				NOT NULL DEFAULT 0,
	[AdditionalReferenceExpression2]NVARCHAR (255),
	[AdditionalReferenceEntryNumber2]INT,

	[RelatedResourceId2]			INT, -- Good, Service, Labor, Machine usage

	[RelatedAccountIsVisible2]		BIT				NOT NULL DEFAULT 0,
	[RelatedAccountExpression2]		NVARCHAR (255),
	[RelatedAccountEntryNumber2]		INT,

	[RelatedQuantity2]				MONEY ,			-- used in Tax accounts, to store the quantiy of taxable item
	[RelatedMoneyAmount2]			MONEY 			NOT NULL DEFAULT 0 -- e.g., amount subject to tax
);
-- ManualLine
INSERT INTO @WideLineTypesSpecifications (
	[LineDefinitionId], 
	[DirectionIsVisible1], [DirectionExpression1], 
	[AccountIdIsVisible1], [AccountIdExpression1],
	[IfrsEntryClassificationIdIsVisible1], [IfrsEntryClassificationIdExpression1],
	[ResourceIdIsVisible1], [ResourceIdExpression1],
	[QuantityIsVisible1], [QuantityExpression1],
	[MoneyAmountIsVisible1], [MoneyAmountExpression1],
	[MassIsVisible1], [MassExpression1],
	[ValueIsVisible1], [ValueExpression1]
) VALUES (
	N'ManualLine',
	1, N'Specified',
	1, N'Specified',
	NULL, N'Specified', -- Ifrs Note, depends on Account
	1, N'Specified',
	1, N'Specified',
	-- additional resource properties (money amount, mass, volume, area, length, ...)
	-- are either not defined, defined at a rate set by resource, or specified by user.
	-- if not defined, they will be invisible. If defined at a rate set by resource, they will be readonly
	-- if specified by user, they will be editable.
	NULL, N'Specified', -- Money amount, depends on resource
	NULL, N'Specified', -- Mass, depends on resource
	1, N'Specified'
);
-- CashReceipt.Functional
INSERT INTO @WideLineTypesSpecifications (
	[LineDefinitionId],
	[DirectionIsVisible1], [DirectionExpression1], [Direction1],
	[AccountIdIsVisible1], [AccountIdExpression1], [AccountIdFilter1],
	[IfrsEntryClassificationIdIsVisible1], [IfrsEntryClassificationIdExpression1],
	[ResourceIdIsVisible1], [ResourceIdExpression1], [ResourceId1],
	[QuantityIsVisible1], [QuantityExpression1],
	[MoneyAmountIsVisible1], [MoneyAmountExpression1], [MoneyAmountEntryNumber1],
	[ValueIsVisible1], [ValueExpression1], [ValueEntryNumber1],
	[MemoIsVisible1], [MemoExpression1],
	[RelatedAccountIsVisible1], [RelatedAccountExpression1]
) VALUES (
	N'CashReceipt.Functional', 
	0, N'Constant', +1,
	1, N'Specified', N'CashOnHand',
	1, N'Specified', -- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(), -- Currency
	1, N'Specified', -- Quantity
	0, N'!Quantity', 1,
	0, N'!Quantity', 1,
	1, N'Specified',
	1, N'Specified'
);
--		CheckReceipt.Functional
INSERT INTO @WideLineTypesSpecifications (
	[LineDefinitionId],
	[DirectionIsVisible1], [DirectionExpression1], [Direction1],
	[AccountIdIsVisible1], [AccountIdExpression1], [AccountIdFilter1],
	[IfrsEntryClassificationIdIsVisible1], [IfrsEntryClassificationIdExpression1],
	[ResourceIdIsVisible1], [ResourceIdExpression1], [ResourceId1],
	[InstanceIdIsVisible1], [InstanceIdExpression1],
	[QuantityIsVisible1], [QuantityExpression1], [QuantityEntryNumber1],
	[MoneyAmountIsVisible1], [MoneyAmountExpression1], [MoneyAmountEntryNumber1],
	[CountIsVisible1], [CountExpression1], [Count1],
	[ValueIsVisible1], [ValueExpression1], [ValueEntryNumber1],
	[MemoIsVisible1], [MemoExpression1],
	[RelatedAccountIsVisible1], [RelatedAccountExpression1],
	[ExternalReferenceIsVisible1], [ExternalReferenceExpression1], [ExternalReferenceEntryNumber1]
) VALUES (
	N'CheckReceipt.Functional', 
	0, N'Constant', 1, -- Direction
	1, N'Specified', N'BalancesWithBanks', -- Account
	1, N'Specified', -- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(),-- Currency
	1, N'SpecifiedNew', -- Check details
	0, N'!Instance.Amount', 1, -- Quantity
	0, N'!Instance.Amount', 1, -- Money Quantity
	0, N'Constant', 1, -- ONE Check
	0, N'!Instance.Amount', 1, -- Value
	1, N'Specified', -- Memo
	1, N'Specified', -- Relared Account
	0, N'!Instance.Code', 1
);
--		BankReceipt.Functional
INSERT INTO @WideLineTypesSpecifications (
	[LineDefinitionId],
	[DirectionIsVisible1], [DirectionExpression1], [Direction1],
	[AccountIdIsVisible1], [AccountIdExpression1], [AccountIdFilter1],
	[IfrsEntryClassificationIdIsVisible1], [IfrsEntryClassificationIdExpression1],
	[ResourceIdIsVisible1], [ResourceIdExpression1], [ResourceId1],
	[QuantityIsVisible1], [QuantityExpression1],
	[MoneyAmountIsVisible1], [MoneyAmountExpression1], [MoneyAmountEntryNumber1],
	[ValueIsVisible1], [ValueExpression1], [ValueEntryNumber1],
	[MemoIsVisible1], [MemoExpression1],
	[RelatedAccountIsVisible1], [RelatedAccountExpression1],
	[ExternalReferenceIsVisible1], [ExternalReferenceExpression1]
) VALUES (
	N'BankReceipt.Functional', 
	0, N'Constant', 1,
	1, N'Specified', N'BalancesWithBanks',
	1, N'Specified', -- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(),-- Currency
	1, N'Specified', -- Quantity
	0, N'!Quantity', 1,
	0, N'!Quantity', 1,
	1, N'Specified',
	1, N'Specified',
	1, N'Specified' -- External Reference: Deposit slip
);
-- CashIssue.Functional
INSERT INTO @WideLineTypesSpecifications (
	[LineDefinitionId],
	[DirectionIsVisible1], [DirectionExpression1], [Direction1],
	[AccountIdIsVisible1], [AccountIdExpression1], [AccountIdFilter1],
	[IfrsEntryClassificationIdIsVisible1], [IfrsEntryClassificationIdExpression1],
	[ResourceIdIsVisible1], [ResourceIdExpression1], [ResourceId1],
	[QuantityIsVisible1], [QuantityExpression1],
	[MoneyAmountIsVisible1], [MoneyAmountExpression1], [MoneyAmountEntryNumber1],
	[ValueIsVisible1], [ValueExpression1], [ValueEntryNumber1],
	[MemoIsVisible1], [MemoExpression1],
	[RelatedAccountIsVisible1], [RelatedAccountExpression1]
)	
VALUES (
	N'CashIssue.Functional', 
	0, N'Constant', -1,
	1, N'Specified', N'CashOnHand',
	1, N'Specified', -- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(), -- Currency
	1, N'Specified', -- Quantity
	0, N'!Quantity', 1,
	0, N'!Quantity', 1,
	1, N'Specified',
	1, N'Specified'
)
--		CheckIssueToBeneficiary.Functional
-- Here we do not define an instance, because the check is issued to the beneficiary.
-- If it were issued to the purchasing department, then we would define it as check transfer.
INSERT INTO @WideLineTypesSpecifications (
	[LineDefinitionId],
	[DirectionIsVisible1], [DirectionExpression1], [Direction1],
	[AccountIdIsVisible1], [AccountIdExpression1], [AccountIdFilter1],
	[IfrsEntryClassificationIdIsVisible1], [IfrsEntryClassificationIdExpression1],
	[ResourceIdIsVisible1], [ResourceIdExpression1], [ResourceId1],
	[QuantityIsVisible1], [QuantityExpression1],
	[MoneyAmountIsVisible1], [MoneyAmountExpression1], [MoneyAmountEntryNumber1],
	[ValueIsVisible1], [ValueExpression1], [ValueEntryNumber1],
	[MemoIsVisible1], [MemoExpression1],
	[RelatedAccountIsVisible1], [RelatedAccountExpression1],
	[ExternalReferenceIsVisible1], [ExternalReferenceExpression1]
)	
VALUES (
	N'CheckIssueToBeneficiary.Functional', 
	0, N'Constant', -1, -- Direction
	1, N'Specified', N'BalancesWithBanks', -- Account
	1, N'Specified', -- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(),-- Currency
	1, N'Specified', -- Quantity: Amount
	0, N'!Quantity', 1, -- Money Quantity
	0, N'!Quantity', 1, -- Value
	1, N'Specified',
	1, N'Specified',
	1, N'Specified' -- check number
);
--		BankIssue.Functional
INSERT INTO @WideLineTypesSpecifications (
	[LineDefinitionId],
	[DirectionIsVisible1], [DirectionExpression1], [Direction1],
	[AccountIdIsVisible1], [AccountIdExpression1], [AccountIdFilter1],
	[IfrsEntryClassificationIdIsVisible1], [IfrsEntryClassificationIdExpression1],
	[ResourceIdIsVisible1], [ResourceIdExpression1], [ResourceId1],
	[QuantityIsVisible1], [QuantityExpression1],
	[MoneyAmountIsVisible1], [MoneyAmountExpression1], [MoneyAmountEntryNumber1],
	[ValueIsVisible1], [ValueExpression1], [ValueEntryNumber1],
	[MemoIsVisible1], [MemoExpression1],
	[RelatedAccountIsVisible1], [RelatedAccountExpression1],
	[ExternalReferenceIsVisible1], [ExternalReferenceExpression1]
)	
VALUES (
	N'BankIssue.Functional', 
	0, N'Constant', -1,
	1, N'Specified', N'BalancesWithBanks',
	1, N'Specified', -- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(),-- Currency
	1, N'Specified', -- Quantity
	0, N'!Quantity', 1,
	0, N'!Quantity', 1,
	1, N'Specified',
	1, N'Specified',
	1, N'Specified' -- External Reference: Deposit slip
);
-- CashTransfer.Functional
INSERT INTO @WideLineTypesSpecifications (
	[LineDefinitionId],
	[DirectionIsVisible1], [DirectionExpression1], [Direction1],
	[AccountIdIsVisible1], [AccountIdExpression1], [AccountIdFilter1],
	[IfrsEntryClassificationIdIsVisible1], [IfrsEntryClassificationIdExpression1], [IfrsEntryClassificationId1],
	[ResourceIdIsVisible1], [ResourceIdExpression1], [ResourceId1],
	[QuantityIsVisible1], [QuantityExpression1],
	[MoneyAmountIsVisible1], [MoneyAmountExpression1], [MoneyAmountEntryNumber1],
	[ValueIsVisible1], [ValueExpression1], [ValueEntryNumber1],
	[MemoIsVisible1], [MemoExpression1],
	[RelatedAccountIsVisible1], [RelatedAccountExpression1], [RelatedAccountEntryNumber1],

	[DirectionIsVisible2], [DirectionExpression2], [Direction2],
	[AccountIdIsVisible2], [AccountIdExpression2], [AccountIdFilter2],
	[IfrsEntryClassificationIdIsVisible2], [IfrsEntryClassificationIdExpression2], [IfrsEntryClassificationId2],
	[ResourceIdIsVisible2], [ResourceIdExpression2], [ResourceId2],
	[QuantityIsVisible2], [QuantityExpression2], [QuantityEntryNumber2],
	[MoneyAmountIsVisible2], [MoneyAmountExpression2], [MoneyAmountEntryNumber2],
	[ValueIsVisible2], [ValueExpression2], [ValueEntryNumber2],
	[MemoIsVisible2], [MemoExpression2], [MemoEntryNumber2],
	[RelatedAccountIsVisible2], [RelatedAccountExpression2], [RelatedAccountEntryNumber2]
)	
VALUES (
	N'CashTransferToBank.Functional', 
	0, N'Constant', 1, -- Direction
	1, N'Specified', N'BalancesWithBanks', -- Account
	0, N'Constant', N'InternalCashTransfer',-- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(),-- Currency
	1, N'Specified', -- Quantity
	0, N'!Quantity', 1, -- Money Amount
	0, N'!Quantity', 1, -- Value
	1, N'Specified', -- Memo
	0, N'!Account', 2, -- Related Agent

	0, N'Constant', -1, --  Direction
	1, N'Specified', N'CashOnHand', -- Account
	0, N'Constant', N'InternalCashTransfer',-- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(),-- Currency
	0, N'!Quantity', 1,
	0, N'!Quantity', 1,
	0, N'!Quantity', 1,
	0, N'!Memo', 1,
	0, N'!Account', 1
)
--		BankTransferToBank.Functional
INSERT INTO @WideLineTypesSpecifications (
	[LineDefinitionId],
	[DirectionIsVisible1], [DirectionExpression1], [Direction1],
	[AccountIdIsVisible1], [AccountIdExpression1], [AccountIdFilter1],
	[IfrsEntryClassificationIdIsVisible1], [IfrsEntryClassificationIdExpression1], [IfrsEntryClassificationId1],
	[ResourceIdIsVisible1], [ResourceIdExpression1], [ResourceId1],
	[QuantityIsVisible1], [QuantityExpression1],
	[MoneyAmountIsVisible1], [MoneyAmountExpression1], [MoneyAmountEntryNumber1],
	[ValueIsVisible1], [ValueExpression1], [ValueEntryNumber1],
	[MemoIsVisible1], [MemoExpression1],
	[RelatedAccountIsVisible1], [RelatedAccountExpression1], [RelatedAccountEntryNumber1],

	[DirectionIsVisible2], [DirectionExpression2], [Direction2],
	[AccountIdIsVisible2], [AccountIdExpression2], [AccountIdFilter2],
	[IfrsEntryClassificationIdIsVisible2], [IfrsEntryClassificationIdExpression2], [IfrsEntryClassificationId2],
	[ResourceIdIsVisible2], [ResourceIdExpression2], [ResourceId2],
	[QuantityIsVisible2], [QuantityExpression2], [QuantityEntryNumber2],
	[MoneyAmountIsVisible2], [MoneyAmountExpression2], [MoneyAmountEntryNumber2],
	[ValueIsVisible2], [ValueExpression2], [ValueEntryNumber2],
	[MemoIsVisible2], [MemoExpression2], [MemoEntryNumber2],
	[RelatedAccountIsVisible2], [RelatedAccountExpression2], [RelatedAccountEntryNumber2]
)	
VALUES (
	N'BankTransferToBank.Functional', 
	0, N'Constant', 1,
	1, N'Specified', N'BalancesWithBanks', -- Destination
	0, N'Constant', N'InternalCashTransfer',-- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(),-- Currency
	1, N'Specified', -- Amount
	0, N'!Quantity', 1,
	0, N'!Quantity', 1,
	1, N'Specified', -- memo
	0, N'!Account', 2,

	0, N'Constant', -1,
	1, N'Specified', N'BalancesWithBanks', -- Source
	0, N'Constant', N'InternalCashTransfer',-- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(),-- Currency
	0, N'!Quantity', 1,
	0, N'!Quantity', 1,
	0, N'!Quantity', 1,
	0, N'!Memo', 1,
	0, N'!Account', 1
)
--		CheckTransferToBank.Functional
INSERT INTO @WideLineTypesSpecifications (
	[LineDefinitionId],
	[DirectionIsVisible1], [DirectionExpression1], [Direction1],
	[AccountIdIsVisible1], [AccountIdExpression1], [AccountIdFilter1],
	[IfrsEntryClassificationIdIsVisible1], [IfrsEntryClassificationIdExpression1], [IfrsEntryClassificationId1],
	[ResourceIdIsVisible1], [ResourceIdExpression1], [ResourceId1],
	[QuantityIsVisible1], [QuantityExpression1], [QuantityEntryNumber1],
	[MoneyAmountIsVisible1], [MoneyAmountExpression1], [MoneyAmountEntryNumber1],
	[ValueIsVisible1], [ValueExpression1], [ValueEntryNumber1],
	[MemoIsVisible1], [MemoExpression1],
	[RelatedAccountIsVisible1], [RelatedAccountExpression1], [RelatedAccountEntryNumber1],
	[ExternalReferenceIsVisible1], [ExternalReferenceExpression1],[ExternalReferenceEntryNumber1],

	[DirectionIsVisible2], [DirectionExpression2], [Direction2],
	[AccountIdIsVisible2], [AccountIdExpression2], [AccountIdFilter2],
	[IfrsEntryClassificationIdIsVisible2], [IfrsEntryClassificationIdExpression2], [IfrsEntryClassificationId2],
	[ResourceIdIsVisible2], [ResourceIdExpression2], [ResourceId2],
	[InstanceIdIsVisible2], [InstanceIdExpression2],
	[QuantityIsVisible2], [QuantityExpression2], [QuantityEntryNumber2],
	[MoneyAmountIsVisible2], [MoneyAmountExpression2], [MoneyAmountEntryNumber2],
	[CountIsVisible2], [CountExpression2], [Count2],
	[ValueIsVisible2], [ValueExpression2], [ValueEntryNumber2],
	[MemoIsVisible2], [MemoExpression2], [MemoEntryNumber2],
	[RelatedAccountIsVisible2], [RelatedAccountExpression2], [RelatedAccountEntryNumber2]
)	
VALUES (
	N'CheckTransferToBank.Functional', 
	0, N'Constant', 1,
	1, N'Specified', N'BalancesWithBanks', -- Destination
	0, N'Constant', N'InternalCashTransfer',-- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(),-- Currency
	0, N'!Instance.Amount', 2, -- Amount
	0, N'!Instance.Amount', 2,
	0, N'!Instance.Amount', 2,
	1, N'Specified', -- memo
	0, N'!Account', 2,
	0, N'!Instance.Code', 2,

	0, N'Constant', -1,
	1, N'Specified', N'CashOnHand', -- Source
	0, N'Constant', N'InternalCashTransfer',-- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(),-- Currency
	1, N'SpecifiedExisting',
	0, N'!Instance.Amount', 2,
	0, N'!Instance.Amount', 2,
	0, N'Constant', 1,
	0, N'!Instance.Amount', 2,
	0, N'!Memo', 1,
	0, N'!Account', 1
)
--		CheckTransferToCustody (e.g., to Purchasing)
INSERT INTO @WideLineTypesSpecifications (
	[LineDefinitionId],
	[DirectionIsVisible1], [DirectionExpression1], [Direction1],
	[AccountIdIsVisible1], [AccountIdExpression1], [AccountIdFilter1],
	[IfrsEntryClassificationIdIsVisible1], [IfrsEntryClassificationIdExpression1], [IfrsEntryClassificationId1],
	[ResourceIdIsVisible1], [ResourceIdExpression1], [ResourceId1],
	[InstanceIdIsVisible1], [InstanceIdExpression1],
	[QuantityIsVisible1], [QuantityExpression1], [QuantityEntryNumber1],
	[MoneyAmountIsVisible1], [MoneyAmountExpression1], [MoneyAmountEntryNumber1],
	[CountIsVisible1], [CountExpression1], [Count1],
	[ValueIsVisible1], [ValueExpression1], [ValueEntryNumber1],
	[MemoIsVisible1], [MemoExpression1],
	[RelatedAccountIsVisible1], [RelatedAccountExpression1], [RelatedAccountEntryNumber1],

	[DirectionIsVisible2], [DirectionExpression2], [Direction2],
	[AccountIdIsVisible2], [AccountIdExpression2], [AccountIdFilter2],
	[IfrsEntryClassificationIdIsVisible2], [IfrsEntryClassificationIdExpression2], [IfrsEntryClassificationId2],
	[ResourceIdIsVisible2], [ResourceIdExpression2], [ResourceId2],
	[QuantityIsVisible2], [QuantityExpression2], [QuantityEntryNumber2],
	[MoneyAmountIsVisible2], [MoneyAmountExpression2], [MoneyAmountEntryNumber2],
	[ValueIsVisible2], [ValueExpression2], [ValueEntryNumber2],
	[MemoIsVisible2], [MemoExpression2], [MemoEntryNumber2],
	[RelatedAccountIsVisible2], [RelatedAccountExpression2], [RelatedAccountEntryNumber2],
	[ExternalReferenceIsVisible2], [ExternalReferenceExpression2], [ExternalReferenceEntryNumber2]
) VALUES (
	N'CheckTransferToCustody.Functional',
	0, N'Constant', 1,
	1, N'Specified', N'CashOnHand', -- Destination
	0, N'Constant', N'InternalCashTransfer',-- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(),-- Currency
	1, N'SpecifiedNew',
	0, N'!Instance.Amount', 1, -- Quantity
	0, N'!Instance.Amount', 1, -- Money Amount
	0, N'Constant', 1, -- Count
	0, N'!Instance.Amount', 1, -- Value
	1, N'Specified', -- memo
	0, N'!Account', 2,

	0, N'Constant', -1,
	1, N'Specified', N'BalancesWithBanks', -- Source
	0, N'Constant', N'InternalCashTransfer',-- IfrsNote
	0, N'Constant', dbo.fn_FunctionalCurrency(),-- Currency
	0, N'!Instance.Amount', 1,
	0, N'!Instance.Amount', 1,
	0, N'!Instance.Amount', 1,
	0, N'!Memo', 1,
	0, N'!Account', 1,
	0, N'!Instance.Code', 1 -- 
);
--		GoodIssueWithInvoice
INSERT INTO @WideLineTypesSpecifications(
	[LineDefinitionId],
	[DirectionIsVisible1], [DirectionExpression1], [Direction1],
	[AccountIdIsVisible1], [AccountIdExpression1], [AccountIdFilter1],
	[IfrsEntryClassificationIdIsVisible1], [IfrsEntryClassificationIdExpression1], [IfrsEntryClassificationId1],
	[ResourceIdIsVisible1], [ResourceIdExpression1], [ResourceId1],
	[InstanceIdIsVisible1], [InstanceIdExpression1],
	[QuantityIsVisible1], [QuantityExpression1], [QuantityEntryNumber1],
	[MoneyAmountIsVisible1], [MoneyAmountExpression1], [MoneyAmountEntryNumber1],
	[CountIsVisible1], [CountExpression1], [Count1],
	[ValueIsVisible1], [ValueExpression1], [ValueEntryNumber1],
	[MemoIsVisible1], [MemoExpression1],
	[RelatedAccountIsVisible1], [RelatedAccountExpression1], [RelatedAccountEntryNumber1]
) VALUES (
	N'GoodIssueWithInvoice.Functional', -- Dr. COGS, Cr. Inventory, Cr. Revenues, Cr. VAT Sales
	0, N'Constant', 1, -- Direction, Dr. COGS
	0, N'!Resource.ExpenseAccountId', 1, -- COGS Account
	0, N'Constant', N'CostOfSales',-- IfrsNote
	0, N'!ResourceId', 2,-- Item
	0, N'!Quantity', 2, -- Quantity
	-- Other measures depends on resource.
	0, N'!Mass', 2, -- Mass
	0, N'!Volume', 2, -- volume
	0, N'!Area', 2, -- Area
	-- etc...
	0, N'!Value', 2, -- Value
	0, N'!Memo', 2, -- memo
-- Not sure when do we need to store the customer account here.
--	0, N'!RelatedAccount', 2, -- what do we need this for? Do we analyze profit per customer?!

	0, N'Constant', -1, -- Direction, Cr. Warehouse
	NULL, N'!Document.SourceCustodianAccountId', N'FinishedGoods', -- Warehouse Account
	0, N'Constant', N'InventoryIssueToSaleExtension',-- IfrsNote, extension useful to generate reports
	1, N'Specified', -- Item
	1, N'Specified', -- Quantity
-- Other measures depend on resource, and they affect the debit line
	NULL, N'Specified', -- Mass
	NULL, N'Specified', -- volume
	NULL, N'Specified', -- Area
	-- etc...
	0, N'Costed', 1, -- Value, using the inventory costing method
	1, N'Specified', -- memo
	-- depends on whether IsCommonCustomerAccountId is true or false.
	NULL, N'!Document.CustomerAccountId', 

	0, N'Constant', -1, -- Direction, Cr. VAT
	1, N'Specified', N'CurrentValueAddedTaxPayables', -- VAT Account
	0, N'Constant', dbo.fn_FunctionalCurrency(), -- Resource
	1, N'Specified', -- Quantity: VAT
	0, N'!Quantity', 3, -- Value,d
	1, N'Specified', -- memo
	-- depends on whether IsCommonInvoiceReference is true or false.
	NULL, N'!Document.InvoiceReference', -- ExternalReference
	0, N'!ResourceId', 2, -- RelatedResource
	-- depends on whether IsCommonCustomerAccountId is true or false.
	NULL, N'!Document.CustomerAccountId', -- Related Account

	0, N'Constant', -1, -- Direction, Cr. Revenues
	0, N'!Resource.RevenueAccountId', 1, -- Sales Account
	0, N'Constant', dbo.fn_FunctionalCurrency(), -- Resource
	1, N'Specified', -- Quantity: Price (VAT excl.)
	0, N'!Quantity', 3, -- Value,d
	1, N'Specified', -- memo
	-- depends on whether IsCommonInvoiceReference is true or false.
	NULL, N'!Document.StockIssueVoucherReference', -- ExternalReference
	-- depends on whether IsCommonCustomerAccountId is true or false.
	NULL, N'!Document.CustomerAccountId' -- Related Account
);
MERGE dbo.[LineDefinitions] AS t
USING (
	SELECT [id], [DocumentCategory] FROM @LineTypes
	) AS s
ON s.[Id] = t.[Id] 
WHEN MATCHED THEN
UPDATE SET
	t.[DocumentCategory] = s.[DocumentCategory]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
INSERT ([Id], [DocumentCategory])
VALUES(s.[Id], s.[DocumentCategory]);
	
MERGE [dbo].[LineTypesSpecifications] AS t
USING (
SELECT
	[LineDefinitionId], 1 As [EntryNumber],
	[DirectionIsVisible1] AS [DirectionIsVisible], [DirectionExpression1] AS [DirectionExpression],
	[DirectionEntryNumber1] AS [DirectionEntryNumber], [Direction1] AS [Direction],
	[AccountIdIsVisible1] AS [AccountIdIsVisible], [AccountIdFilter1] AS [AccountIdIfrsFilter],
	[AccountIdExpression1] AS [AccountIdExpression], [AccountIdEntryNumber1] AS [AccountIdEntryNumber],
	[IfrsEntryClassificationIdIsVisible1] AS [IfrsEntryClassificationIdIsVisible], [IfrsEntryClassificationIdExpression1] AS [IfrsEntryClassificationIdExpression],
	[IfrsEntryClassificationIdEntryNumber1] AS [IfrsEntryClassificationIdEntryNumber], [IfrsEntryClassificationId1] AS [IfrsEntryClassificationId],
	[ResourceIdIsVisible1] AS [ResourceIdIsVisible], [ResourceIdExpression1] AS [ResourceIdExpression],
	[ResourceIdEntryNumber1] AS [ResourceIdEntryNumber], [ResourceId1] AS [ResourceId],
	[InstanceIdIsVisible1] AS [InstanceIdIsVisible], [InstanceIdExpression1] AS [InstanceIdExpression],
	[InstanceIdEntryNumber1] AS [InstanceIdEntryNumber], [BatchCodeIsVisible1] AS [BatchCodeIsVisible],
	[BatchCodeExpression1] AS [BatchCodeExpression], [BatchCodeEntryNumber1] AS [BatchCodeEntryNumber],
	[DueDateIsVisible1] AS [DueDateIsVisible], [DueDateExpression1] AS [DueDateExpression],
	[DueDateEntryNumber1] AS [DueDateEntryNumber], [QuantityIsVisible1] AS [QuantityIsVisible],
	[QuantityExpression1] AS [QuantityExpression], [QuantityEntryNumber1] AS [QuantityEntryNumber],
	[MoneyAmountIsVisible1] AS [MoneyAmountIsVisible], [MoneyAmountExpression1] AS [MoneyAmountExpression],
	[MoneyAmountEntryNumber1] AS [MoneyAmountEntryNumber], [MassIsVisible1] AS [MassIsVisible],
	[MassExpression1] AS [MassExpression], [MassEntryNumber1] AS [MassEntryNumber],
	[VolumeIsVisible1] AS [VolumeIsVisible], [VolumeExpression1] AS [VolumeExpression],
	[VolumeEntryNumber1] AS [VolumeEntryNumber], [AreaIsVisible1] AS [AreaIsVisible],
	[AreaExpression1] AS [AreaExpression],[AreaEntryNumber1] AS [AreaEntryNumber],
	[LengthIsVisible1] AS [LengthIsVisible],[LengthExpression1] AS [LengthExpression],
	[LengthEntryNumber1] AS [LengthEntryNumber],[TimeIsVisible1] AS [TimeIsVisible],
	[TimeExpression1] AS [TimeExpression], [TimeEntryNumber1] AS [TimeEntryNumber],
	[CountIsVisible1] AS [CountIsVisible], [CountExpression1] AS [CountExpression],
	[CountEntryNumber1] AS [CountEntryNumber], [ValueIsVisible1] AS [ValueIsVisible],
	[ValueExpression1] AS [ValueExpression], [ValueEntryNumber1] AS [ValueEntryNumber],
	[MemoIsVisible1] AS [MemoIsVisible], [MemoExpression1] AS [MemoExpression],
	[MemoEntryNumber1] AS [MemoEntryNumber], [ExternalReferenceIsVisible1] AS [ExternalReferenceIsVisible],
	[ExternalReferenceExpression1] AS [ExternalReferenceExpression],
	[ExternalReferenceEntryNumber1] AS [ExternalReferenceEntryNumber],
	[AdditionalReferenceIsVisible1] AS [AdditionalReferenceIsVisible],
	[AdditionalReferenceExpression1] AS [AdditionalReferenceExpression],
	[AdditionalReferenceEntryNumber1] AS [AdditionalReferenceEntryNumber],
	[RelatedResourceId1] AS [RelatedResourceId], [RelatedAccountIsVisible1] AS [RelatedAccountIsVisible],
	[RelatedAccountExpression1] AS [RelatedAccountExpression],[RelatedAccountEntryNumber1] AS [RelatedAccountEntryNumber],
	[RelatedQuantity1] AS [RelatedQuantity], [RelatedMoneyAmount1] AS [RelatedMoneyAmount]
FROM @WideLineTypesSpecifications 
UNION ALL
SELECT
	[LineDefinitionId], 2, [DirectionIsVisible2], [DirectionExpression2], [DirectionEntryNumber2],
	[Direction2], [AccountIdIsVisible2], [AccountIdFilter2], [AccountIdExpression2],
	[AccountIdEntryNumber2], [IfrsEntryClassificationIdIsVisible2], [IfrsEntryClassificationIdExpression2], [IfrsEntryClassificationIdEntryNumber2],
	[IfrsEntryClassificationId2], [ResourceIdIsVisible2], [ResourceIdExpression2],
	[ResourceIdEntryNumber2], [ResourceId2], [InstanceIdIsVisible2], [InstanceIdExpression2],
	[InstanceIdEntryNumber2], [BatchCodeIsVisible2], [BatchCodeExpression2], [BatchCodeEntryNumber2],
	[DueDateIsVisible2], [DueDateExpression2], [DueDateEntryNumber2], [QuantityIsVisible2],
	[QuantityExpression2], [QuantityEntryNumber2], [MoneyAmountIsVisible2], [MoneyAmountExpression2],
	[MoneyAmountEntryNumber2], [MassIsVisible2], [MassExpression2], [MassEntryNumber2], [VolumeIsVisible2],
	[VolumeExpression2], [VolumeEntryNumber2], [AreaIsVisible2], [AreaExpression2], [AreaEntryNumber2],
	[LengthIsVisible2], [LengthExpression2], [LengthEntryNumber2], [TimeIsVisible2], [TimeExpression2],
	[TimeEntryNumber2], [CountIsVisible2], [CountExpression2], [CountEntryNumber2], [ValueIsVisible2],
	[ValueExpression2], [ValueEntryNumber2], [MemoIsVisible2], [MemoExpression2], [MemoEntryNumber2],
	[ExternalReferenceIsVisible2], [ExternalReferenceExpression2], [ExternalReferenceEntryNumber2],
	[AdditionalReferenceIsVisible2], [AdditionalReferenceExpression2], [AdditionalReferenceEntryNumber2],
	[RelatedResourceId2], [RelatedAccountIsVisible2],	[RelatedAccountExpression2], [RelatedAccountEntryNumber2],
	[RelatedQuantity2], [RelatedMoneyAmount2]
FROM @WideLineTypesSpecifications
WHERE [AccountIdExpression2] IS NOT NULL
) AS s
ON s.[LineDefinitionId] = t.[LineDefinitionId]  AND s.[EntryNumber] = t.[EntryNumber]
WHEN MATCHED THEN
UPDATE SET
	t.[DirectionIsVisible] = s.[DirectionIsVisible],
	t.[DirectionExpression] = s.[DirectionExpression],
	t.[DirectionEntryNumber] = s.[DirectionEntryNumber],
	t.[Direction] = s.[Direction],
	t.[AccountIdIsVisible] = s.[AccountIdIsVisible],
	t.[AccountIdIfrsFilter] = s.[AccountIdIfrsFilter],
	t.[AccountIdExpression] = s.[AccountIdExpression],
	t.[AccountIdEntryNumber] = s.[AccountIdEntryNumber],
	t.[IfrsEntryClassificationIdIsVisible] = s.[IfrsEntryClassificationIdIsVisible],
	t.[IfrsEntryClassificationIdExpression] = s.[IfrsEntryClassificationIdExpression],
	t.[IfrsEntryClassificationIdEntryNumber] = s.[IfrsEntryClassificationIdEntryNumber],
	t.[IfrsEntryClassificationId] = s.[IfrsEntryClassificationId],
	t.[ResourceIdIsVisible] = s.[ResourceIdIsVisible],
	t.[ResourceIdExpression] = s.[ResourceIdExpression],
	t.[ResourceIdEntryNumber] = s.[ResourceIdEntryNumber],
	t.[ResourceId] = s.[ResourceId],
	t.[InstanceIdIsVisible] = s.[InstanceIdIsVisible],
	t.[InstanceIdExpression] = s.[InstanceIdExpression],
	t.[InstanceIdEntryNumber] = s.[InstanceIdEntryNumber],
	t.[BatchCodeIsVisible] = s.[BatchCodeIsVisible],
	t.[BatchCodeExpression] = s.[BatchCodeExpression],
	t.[BatchCodeEntryNumber] = s.[BatchCodeEntryNumber],
	t.[DueDateIsVisible] = s.[DueDateIsVisible],
	t.[DueDateExpression] = s.[DueDateExpression],
	t.[DueDateEntryNumber] = s.[DueDateEntryNumber],
	t.[QuantityIsVisible] = s.[QuantityIsVisible],
	t.[QuantityExpression] = s.[QuantityExpression],
	t.[QuantityEntryNumber] = s.[QuantityEntryNumber],
	t.[MoneyAmountIsVisible] = s.[MoneyAmountIsVisible],
	t.[MoneyAmountExpression] = s.[MoneyAmountExpression],
	t.[MoneyAmountEntryNumber] = s.[MoneyAmountEntryNumber],
	t.[MassIsVisible] = s.[MassIsVisible],
	t.[MassExpression] = s.[MassExpression],
	t.[MassEntryNumber] = s.[MassEntryNumber],
	t.[VolumeIsVisible] = s.[VolumeIsVisible],
	t.[VolumeExpression] = s.[VolumeExpression],
	t.[VolumeEntryNumber] = s.[VolumeEntryNumber],
	t.[AreaIsVisible] = s.[AreaIsVisible],
	t.[AreaExpression] = s.[AreaExpression],
	t.[AreaEntryNumber] = s.[AreaEntryNumber],
	t.[LengthIsVisible] = s.[LengthIsVisible],
	t.[LengthExpression] = s.[LengthExpression],
	t.[LengthEntryNumber] = s.[LengthEntryNumber],
	t.[TimeIsVisible] = s.[TimeIsVisible],
	t.[TimeExpression] = s.[TimeExpression],
	t.[TimeEntryNumber] = s.[TimeEntryNumber],
	t.[CountIsVisible] = s.[CountIsVisible],
	t.[CountExpression] = s.[CountExpression],
	t.[CountEntryNumber] = s.[CountEntryNumber],
	t.[ValueIsVisible] = s.[ValueIsVisible],
	t.[ValueExpression] = s.[ValueExpression],
	t.[ValueEntryNumber] = s.[ValueEntryNumber],
	t.[MemoIsVisible] = s.[MemoIsVisible],
	t.[MemoExpression] = s.[MemoExpression],
	t.[MemoEntryNumber] = s.[MemoEntryNumber],
	t.[ExternalReferenceIsVisible] = s.[ExternalReferenceIsVisible],
	t.[ExternalReferenceExpression] = s.[ExternalReferenceExpression],
	t.[ExternalReferenceEntryNumber] = s.[ExternalReferenceEntryNumber],
	t.[AdditionalReferenceIsVisible] = s.[AdditionalReferenceIsVisible],
	t.[AdditionalReferenceExpression] = s.[AdditionalReferenceExpression],
	t.[AdditionalReferenceEntryNumber] = s.[AdditionalReferenceEntryNumber],
	t.[RelatedResourceId] = s.[RelatedResourceId],
	t.[RelatedAccountIsVisible] = s.[RelatedAccountIsVisible],
	t.[RelatedAccountExpression] = s.[RelatedAccountExpression],
	t.[RelatedAccountEntryNumber] = s.[RelatedAccountEntryNumber],
	t.[RelatedQuantity] = s.[RelatedQuantity],
	t.[RelatedMoneyAmount] = s.[RelatedMoneyAmount]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
INSERT ([LineDefinitionId], [EntryNumber], [DirectionIsVisible], [DirectionExpression], [DirectionEntryNumber],
	[Direction], [AccountIdIsVisible], [AccountIdIfrsFilter], [AccountIdExpression],
	[AccountIdEntryNumber], [IfrsEntryClassificationIdIsVisible], [IfrsEntryClassificationIdExpression], [IfrsEntryClassificationIdEntryNumber],
	[IfrsEntryClassificationId], [ResourceIdIsVisible], [ResourceIdExpression],
	[ResourceIdEntryNumber], [ResourceId], [InstanceIdIsVisible], [InstanceIdExpression],
	[InstanceIdEntryNumber], [BatchCodeIsVisible], [BatchCodeExpression], [BatchCodeEntryNumber],
	[DueDateIsVisible], [DueDateExpression], [DueDateEntryNumber], [QuantityIsVisible],
	[QuantityExpression], [QuantityEntryNumber], [MoneyAmountIsVisible], [MoneyAmountExpression],
	[MoneyAmountEntryNumber], [MassIsVisible], [MassExpression], [MassEntryNumber], [VolumeIsVisible],
	[VolumeExpression], [VolumeEntryNumber], [AreaIsVisible], [AreaExpression], [AreaEntryNumber],
	[LengthIsVisible], [LengthExpression], [LengthEntryNumber], [TimeIsVisible], [TimeExpression],
	[TimeEntryNumber], [CountIsVisible], [CountExpression], [CountEntryNumber], [ValueIsVisible],
	[ValueExpression], [ValueEntryNumber], [MemoIsVisible], [MemoExpression], [MemoEntryNumber],
	[ExternalReferenceIsVisible], [ExternalReferenceExpression], [ExternalReferenceEntryNumber],
	[AdditionalReferenceIsVisible], [AdditionalReferenceExpression], [AdditionalReferenceEntryNumber],
	[RelatedResourceId], [RelatedAccountIsVisible],	[RelatedAccountExpression], [RelatedAccountEntryNumber],
	[RelatedQuantity], [RelatedMoneyAmount]
	)
VALUES(s.[LineDefinitionId], s.[EntryNumber], s.[DirectionIsVisible], s.[DirectionExpression], s.[DirectionEntryNumber],
	s.[Direction], s.[AccountIdIsVisible], s.[AccountIdIfrsFilter], s.[AccountIdExpression],
	s.[AccountIdEntryNumber], s.[IfrsEntryClassificationIdIsVisible], s.[IfrsEntryClassificationIdExpression], s.[IfrsEntryClassificationIdEntryNumber],
	s.[IfrsEntryClassificationId], s.[ResourceIdIsVisible], s.[ResourceIdExpression],
	s.[ResourceIdEntryNumber], s.[ResourceId], s.[InstanceIdIsVisible], s.[InstanceIdExpression],
	s.[InstanceIdEntryNumber], s.[BatchCodeIsVisible], s.[BatchCodeExpression], s.[BatchCodeEntryNumber],
	s.[DueDateIsVisible], s.[DueDateExpression], s.[DueDateEntryNumber], s.[QuantityIsVisible],
	s.[QuantityExpression], s.[QuantityEntryNumber], s.[MoneyAmountIsVisible], s.[MoneyAmountExpression],
	s.[MoneyAmountEntryNumber], s.[MassIsVisible], s.[MassExpression], s.[MassEntryNumber], s.[VolumeIsVisible],
	s.[VolumeExpression], s.[VolumeEntryNumber], s.[AreaIsVisible], s.[AreaExpression], s.[AreaEntryNumber],
	s.[LengthIsVisible], s.[LengthExpression], s.[LengthEntryNumber], s.[TimeIsVisible], s.[TimeExpression],
	s.[TimeEntryNumber], s.[CountIsVisible], s.[CountExpression], s.[CountEntryNumber], s.[ValueIsVisible],
	s.[ValueExpression], s.[ValueEntryNumber], s.[MemoIsVisible], s.[MemoExpression], s.[MemoEntryNumber],
	s.[ExternalReferenceIsVisible], s.[ExternalReferenceExpression], s.[ExternalReferenceEntryNumber],
	s.[AdditionalReferenceIsVisible], s.[AdditionalReferenceExpression], s.[AdditionalReferenceEntryNumber],
	s.[RelatedResourceId], s.[RelatedAccountIsVisible],	s.[RelatedAccountExpression], s.[RelatedAccountEntryNumber],
	s.[RelatedQuantity], s.[RelatedMoneyAmount]
);