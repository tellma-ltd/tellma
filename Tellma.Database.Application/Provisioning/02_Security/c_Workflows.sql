DECLARE @WorkflowId INT;
DECLARE @Workflows dbo.[WorkflowList];
DECLARE @WorkflowSignatures dbo.WorkflowSignatureList;

IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(0, N'ManualLine',		+3),
	(1, N'ManualLine',		+4);

	INSERT INTO @WorkflowSignatures([Index], [HeaderIndex], [RoleId]) VALUES
	(0, 0, @1Comptroller),
	(0, 1, @1GeneralManager);

	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(2, N'CashPayment',		+1),
	(3, N'CashPayment',		+3),
	(4, N'CashPayment',		+4);

	INSERT INTO @WorkflowSignatures([Index], [HeaderIndex], [RoleId]) VALUES
--	(0, 2, @1Reader),
	(0, 3, @1GeneralManager),
	(0, 4, @1Comptroller);

END
IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	--(N'ManualLine',	N'Reviewed');
	(0, N'ManualLine',		+4);

END
IF @DB = N'103' -- Lifan Cars, ETB, en/zh
BEGIN
	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(0, N'ManualLine',		+4);

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
		WS.[RoleId],
		WS.[Criteria],
		WS.[ProxyRoleId]
	FROM @WorkflowSignatures WS
	JOIN @IndexedIds W ON WS.[HeaderIndex] = W.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED THEN
		UPDATE SET
			t.[RoleId]		= s.[RoleId],	
			t.[Criteria]	= s.[Criteria],	
			t.[ProxyRoleId]	= s.[ProxyRoleId],
			t.[SavedById]	= @AdminUserId
	WHEN NOT MATCHED THEN
	INSERT ([WorkflowId], [RoleId], [Criteria], [ProxyRoleId])
	VALUES (s.[WorkflowId], s.[RoleId], s.[Criteria], s.[ProxyRoleId]);
