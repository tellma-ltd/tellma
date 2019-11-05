CREATE PROCEDURE [bll].[DocumentLines__RelevantIndexIds]
	@DocLinesIndexedIds dbo.[IndexedIdList] READONLY,
	@Roles dbo.[IdList] READONLY,
	@ToState NVARCHAR(30)
AS
	DECLARE @DocLinesIds dbo.[IdList]
	INSERT INTO @DocLinesIds([Id]) SELECT [Id] FROM @DocLinesIndexedIds;
	DECLARE @DocLinesMissingSignatures dbo.[DocumentLineRoleList];
	INSERT INTO @DocLinesMissingSignatures([DocumentLineId], [RoleId])
	SELECT [DocumentLineId], [RoleId]
	FROM [rpt].[DocumentLines_ToState_Roles__Missing] (@DocLinesIds, @Roles, @ToState);

	-- Find additional role signatures required for documents satisfying workflow criteria
	DECLARE @DocLinesMissingConditionalSignatures dbo.[DocumentLineRoleList];
	INSERT INTO @DocLinesMissingConditionalSignatures([DocumentLineId], [RoleId])
	SELECT [DocumentLineId], [RoleId]
	FROM [rpt].[DocumentLines_ToState_Roles__MissingConditional] (@DocLinesIds, @Roles, @ToState);

	DECLARE	@DocLinesWithNoDefinedWorkflows dbo.[IdList];
	INSERT INTO @DocLinesWithNoDefinedWorkflows([Id])
	SELECT [DocumentLineId] FROM [rpt].[DocumentLinesWithNoDefinedWorkflows](@DocLinesIds);

	DECLARE @RelevantIndexedDocLines dbo.[IndexedIdList];
	INSERT INTO @RelevantIndexedDocLines([Index], [Id])
	SELECT [Index], [Id]
	FROM @DocLinesIndexedIds
	WHERE [Id] IN (
		SELECT [DocumentLineId] FROM @DocLinesMissingSignatures
		UNION
		SELECT [DocumentLineId]	FROM @DocLinesMissingConditionalSignatures
		UNION 
		SELECT [Id] FROM @DocLinesWithNoDefinedWorkflows
	);