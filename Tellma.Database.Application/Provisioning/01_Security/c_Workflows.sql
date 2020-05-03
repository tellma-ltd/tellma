

IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(0, @ManualLineDef,	+4); -- Reviewed

END
IF @DB = N'103' -- Lifan Cars, ETB, en/zh
BEGIN
	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(0, @ManualLineDef,	+4);

END
IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(0, @ManualLineDef,		+4);

	INSERT INTO @WorkflowSignatures([Index], [HeaderIndex], [RoleId])
	SELECT 0, 0, [Id] FROM dbo.Roles WHERE [Code] = N'AE';
END
IF @DB = N'105' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @Workflows([Index],
	[LineDefinitionId], ToState) Values
	(0, @ManualLineDef,		+3),
	(1, @ManualLineDef,		+4);

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
