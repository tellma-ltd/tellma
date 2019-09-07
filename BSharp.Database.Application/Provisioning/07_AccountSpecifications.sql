DECLARE @AccountSpecifications AS TABLE (
	[AccountId]				NVARCHAR (255),
	[Direction]				SMALLINT,
	[CustodyLabel]			NVARCHAR (50), -- Needed when hovering over the column.
	[CustodyFilter]			NVARCHAR (255), -- JS code, affecting the 
	[ResourceLabel]			NVARCHAR (50),
	[ResourceFilter]		NVARCHAR (255),
	[AmountLabel]			NVARCHAR (50),
	[ReferenceLabel]		NVARCHAR (50),
	[RelatedReferenceLabel]	NVARCHAR (255),
	[RelatedAgentLabel]		NVARCHAR (50),
	[RelatedAgentFilter]	NVARCHAR (255),
	[RelatedResourceLabel]	NVARCHAR (50),
	[RelatedResourceFilter]	NVARCHAR (255),
	[RelatedAmountLabel]	NVARCHAR (50)
	PRIMARY KEY NONCLUSTERED ([AccountId], [Direction])
);
INSERT INTO @AccountSpecifications(-- TODO: Is monetary resource type correct
[AccountId],				[Direction], [CustodyLabel], [CustodyFilter],				[ResourceLabel], [ResourceFilter],				[AmountLabel], [ReferenceLabel], [RelatedReferenceLabel], [RelatedAgentLabel], [RelatedAgentFilter], [RelatedResourceLabel], [RelatedResourceFilter], [RelatedAmountLabel]) VALUES
(N'BalancesWithBanks',			+1,		N'BankAccount',	N'CustodyType = N''BankAccount''', N'MonetaryValue',	N'ResourceType = N''MonetaryValue''', N'Deposit',	N'DepositSlipReference', N'CheckReference', N'Depositer',		NULL,					NULL,					NULL,					NULL),
(N'CurrentWithholdingTaxPayable', -1,		NULL,		N'SystemCode = N''TaxAgent''',	N'MonetaryValue',	N'SystemCode = N''Functional''', N'AmountWithheld',	N'WT Form #',	NULL,					N'Withholdee',		NULL,					NULL,					NULL,			N'Invoice Amount'),
(N'CurrentValueAddedTaxReceivables',+1,		NULL,		N'SystemCode = N''TaxAgent''',		NULL,		N'SystemCode = N''Functional''',	NULL,		N'Invoice #',		NULL,					N'Customer',		NULL,					NULL,					NULL,			N'Invoice Amount');
