CREATE PROCEDURE [bll].[AccountTypes_Validate__DeleteWithDescendants]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @IndexesToDelete [IndexedIdList];
	-- CANNOT delete IsSystem
	WITH
	ParentNodesToDelete AS
	(
		SELECT [Node] FROM dbo.AccountTypes
		WHERE [Id] IN (SELECT [Id] FROM @Ids)
	),
	IdsToDelete AS
	(
		SELECT [Id]
		FROM dbo.AccountTypes C
		JOIN ParentNodesToDelete P
		ON C.[Node].IsDescendantOf(P.[Node]) = 1
	)
	INSERT INTO @IndexesToDelete
	SELECT * FROM @Ids
	WHERE [Id] IN (SELECT [Id] FROM IdsToDelete)

	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].IsSystem',
		N'Error_SystemTypesCannotBeDeleted'
	FROM @IndexesToDelete
	WHERE [Id] IN (
		SELECT [Id] FROM dbo.AccountTypes
		WHERE [IsSystem] = 1
	);

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheTypeIsUsedInAccount0',
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]) AS Account
	FROM @IndexesToDelete FE
	JOIN dbo.Accounts A ON FE.[Id] = A.[IfrsTypeId]

	SELECT TOP (@Top) * FROM @ValidationErrors;