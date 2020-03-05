DECLARE @WorkflowId INT;
DECLARE @Workflows dbo.[WorkflowList];
DECLARE @WorkflowSignatures dbo.WorkflowSignatureList;

IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(0, N'ManualLine',		+4);

	INSERT INTO @WorkflowSignatures([Index], [HeaderIndex], [RuleType], [RoleId]) VALUES
	(0, 0, N'ByRole', @1Comptroller);

	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(2, N'CashPaymentToSupplierAndPurchaseInvoiceVAT',	+1),-- Requested
	(3, N'CashPaymentToSupplierAndPurchaseInvoiceVAT',	+2),-- Authorized
	(4, N'CashPaymentToSupplierAndPurchaseInvoiceVAT',	+3),-- Completed
	(5, N'CashPaymentToSupplierAndPurchaseInvoiceVAT',	+4);-- Reviewed

	INSERT INTO @WorkflowSignatures([Index], [HeaderIndex],
	[RuleType],			[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
	(0, 2, N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
	(0, 3, N'ByRole',	@1GeneralManager,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
	(0, 4, N'ByAgent',	NULL,				0,				@1Comptroller), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
	(0, 5, N'ByRole',	@1Comptroller,		NULL,			NULL); -- Comptroller only can review

	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(6, N'CashPaymentToOther',	+1),-- Requested
	(7, N'CashPaymentToOther',	+2),-- Authorized
	(8, N'CashPaymentToOther',	+3),-- Completed
	(9, N'CashPaymentToOther',	+4);-- Reviewed

	INSERT INTO @WorkflowSignatures([Index], [HeaderIndex],
	[RuleType],			[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
	(0, 6, N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
	(0, 7, N'ByRole',	@1GeneralManager,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
	(0, 8, N'ByAgent',	NULL,				0,				@1Comptroller), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
	(0, 9, N'ByRole',	@1Comptroller,		NULL,			NULL); -- Comptroller only can review

		INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(10, N'LeaseOutIssueAndSalesInvoiceNoVAT',	+3),-- Requested
	(11, N'LeaseOutIssueAndSalesInvoiceNoVAT',	+4);-- Reviewed

	INSERT INTO @WorkflowSignatures([Index], [HeaderIndex],
	[RuleType],			[RoleId],			[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
	(0, 10, N'ByRole',	@1AccountManager,	NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
	(0, 11, N'ByRole',	@1Comptroller,		NULL,			NULL); -- Comptroller only can review


	--INSERT INTO @Workflows([Index],
	--[LineDefinitionId],		ToState) Values
	--(6, N'PettyCashPayment',+2),
	--(7, N'PettyCashPayment',+3),
	--(8, N'PettyCashPayment',+4);

	--INSERT INTO @WorkflowSignatures([Index], [HeaderIndex],
	--[RuleType],			[RoleId],	[RuleTypeEntryIndex],	[PredicateType],[PredicateTypeEntryIndex], [Value]) VALUES
	--(0, 6, N'ByRole',	@1GeneralManager,	NULL,			N'ValueGreaterOrEqual',0,					500),
	--(0, 7, N'ByAgent',	NULL,				0,				NULL,			NULL,						NULL), -- Agent0: Cash custodian
	--(0, 8, N'ByRole',	@1Comptroller,		NULL,			NULL,			NULL,						NULL);
END
IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(0, N'ManualLine',	+4); -- Reviewed

END
IF @DB = N'103' -- Lifan Cars, ETB, en/zh
BEGIN
	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(0, N'ManualLine',	+4);

END
IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(0, N'ManualLine',		+4);

	INSERT INTO @WorkflowSignatures([Index], [HeaderIndex], [RoleId])
	SELECT 0, 0, [Id] FROM dbo.Roles WHERE [Code] = N'AE';
END
IF @DB = N'105' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(0, N'ManualLine',		+3),
	(1, N'ManualLine',		+4);

	INSERT INTO @WorkflowSignatures([Index], [HeaderIndex], [RoleId])
	SELECT 0, 0, [Id] FROM dbo.Roles WHERE [Code] = N'AC' UNION
	SELECT 0, 1, [Id] FROM dbo.Roles WHERE [Code] = N'GM';
END
DECLARE @IndexedIds [dbo].[IndexedIdList];
INSERT INTO @IndexedIds([Index], [Id])
SELECT x.[Index], x.[Id]
FROM
(
	MERGE [dbo].[Workflows] AS t
	USING (
		SELECT
			[Index],
			[Id],
			[LineDefinitionId],
			[ToState]	
		FROM @Workflows W
	) AS s
	ON s.Id = t.Id
	WHEN MATCHED THEN
		UPDATE SET
			t.[LineDefinitionId]= s.[LineDefinitionId],
			t.[ToState]			= s.[ToState]
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ([LineDefinitionId],	[ToState])
		VALUES (s.[LineDefinitionId], s.[ToState])
		OUTPUT s.[Index], inserted.[Id] 
) As x;

WITH BE AS (
	SELECT * FROM dbo.[WorkflowSignatures]
	WHERE [WorkflowId] IN (SELECT [Id] FROM @IndexedIds)
)
MERGE INTO BE AS t
USING (
	SELECT
		WS.[Id], 
		W.[Id] AS [WorkflowId],
		WS.[RuleType],
		WS.[RuleTypeEntryIndex],
		WS.[RoleId],
		WS.[Userid],
		WS.[PredicateType],
		WS.[PredicateTypeEntryIndex],
		WS.[Value],
		WS.[ProxyRoleId]
	FROM @WorkflowSignatures WS
	JOIN @IndexedIds W ON WS.[HeaderIndex] = W.[Index]
	) AS s ON (t.Id = s.Id)
WHEN MATCHED THEN
	UPDATE SET
		t.[RuleType]				= s.[RuleType],
		t.[RuleTypeEntryIndex]		= s.[RuleTypeEntryIndex],
		t.[RoleId]					= s.[RoleId],
		t.[Userid]					= s.[Userid],
		t.[PredicateType]			= s.[PredicateType],
		t.[PredicateTypeEntryIndex]	= s.[PredicateTypeEntryIndex],
		t.[Value]					= s.[Value],
		t.[ProxyRoleId]				= s.[ProxyRoleId],
		t.[SavedById]				= CONVERT(INT, SESSION_CONTEXT(N'UserId'))
WHEN NOT MATCHED THEN
	INSERT ([WorkflowId],	[RuleType],		[RuleTypeEntryIndex],	[RoleId], [Userid],		[PredicateType], [PredicateTypeEntryIndex], [Value], [ProxyRoleId])
	VALUES (s.[WorkflowId], s.[RuleType], s.[RuleTypeEntryIndex], s.[RoleId], s.[Userid], s.[PredicateType], s.[PredicateTypeEntryIndex], s.[Value], s.[ProxyRoleId]);
