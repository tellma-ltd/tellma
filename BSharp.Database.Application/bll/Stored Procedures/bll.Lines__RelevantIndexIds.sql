CREATE PROCEDURE [bll].[Lines__RelevantIndexIds]
	@DocLinesIndexedIds dbo.[IndexedIdList] READONLY,
	@Roles dbo.[IdList] READONLY,
	@ToState NVARCHAR(30)
AS
	DECLARE @DocLinesIds dbo.[IdList]
	INSERT INTO @DocLinesIds([Id]) SELECT [Id] FROM @DocLinesIndexedIds;
	DECLARE @DocLinesMissingSignatures dbo.[LineRoleList];
	INSERT INTO @DocLinesMissingSignatures([LineId], [RoleId])
	SELECT [LineId], [RoleId]
	FROM [rpt].[Lines_ToState_Roles__Missing] (@DocLinesIds, @Roles, @ToState);

	-- Find additional role signatures required for documents satisfying workflow criteria
	DECLARE @DocLinesMissingConditionalSignatures dbo.[LineRoleList];
	INSERT INTO @DocLinesMissingConditionalSignatures([LineId], [RoleId])
	SELECT [LineId], [RoleId]
	FROM [rpt].[Lines_ToState_Roles__MissingConditional] (@DocLinesIds, @Roles, @ToState);

	DECLARE	@DocLinesWithNoDefinedWorkflows dbo.[IdList];
	INSERT INTO @DocLinesWithNoDefinedWorkflows([Id])
	SELECT [LineId] FROM [rpt].[LinesWithNoDefinedWorkflows](@DocLinesIds);

	DECLARE @RelevantIndexedDocLines dbo.[IndexedIdList];
	INSERT INTO @RelevantIndexedDocLines([Index], [Id])
	SELECT [Index], [Id]
	FROM @DocLinesIndexedIds
	WHERE [Id] IN (
		SELECT [LineId] FROM @DocLinesMissingSignatures
		UNION
		SELECT [LineId]	FROM @DocLinesMissingConditionalSignatures
		UNION 
		SELECT [Id] FROM @DocLinesWithNoDefinedWorkflows
	);