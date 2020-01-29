DECLARE @WorkflowId INT;
DECLARE @Workflows dbo.[WorkflowList];
DECLARE @WorkflowSignatures dbo.WorkflowSignatureList;

INSERT INTO @Workflows([Index],
[LineDefinitionId], FromState, ToState) Values
--(N'ManualLine',	N'Draft',	N'Reviewed');
(0, N'ManualLine',		0,			+4);

--IF @DB = N'101' -- Banan SD, USD, en

--IF @DB = N'102' -- Banan ET, ETB, en

--IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh

IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @WorkflowSignatures([Index], [HeaderIndex], [RoleId])
	SELECT 0, 0, [Id] FROM dbo.Roles WHERE [Code] = N'AE';

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
			[FromState],
			[ToState]		
		FROM @Workflows W
	) AS s
	ON s.Id = t.Id
	WHEN MATCHED THEN
		UPDATE SET
			t.[LineDefinitionId]= s.[LineDefinitionId],
			t.[FromState]		= s.[FromState],
			t.[ToState]			= s.[ToState]
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ([LineDefinitionId],		[FromState], [ToState])
		VALUES (s.[LineDefinitionId], s.[FromState], s.[ToState])
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
			

