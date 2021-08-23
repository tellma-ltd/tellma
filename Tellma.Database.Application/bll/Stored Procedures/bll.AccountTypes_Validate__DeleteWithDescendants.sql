CREATE PROCEDURE [bll].[AccountTypes_Validate__DeleteWithDescendants]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @IndexesToDelete [IndexedIdList];

	-- CANNOT delete IsSystem
	WITH
	ParentNodesToDelete AS
	(
		SELECT [Node] FROM dbo.[AccountTypes]
		WHERE [Id] IN (SELECT [Id] FROM @Ids)
	),
	IdsToDelete AS
	(
		SELECT [Id]
		FROM dbo.[AccountTypes] C
		JOIN ParentNodesToDelete P
		ON C.[Node].IsDescendantOf(P.[Node]) = 1
	)
	INSERT INTO @IndexesToDelete
	SELECT * FROM @Ids
	WHERE [Id] IN (SELECT [Id] FROM IdsToDelete)

	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotDeleteSystemRecords'
	FROM @IndexesToDelete
	WHERE [Id] IN (
		SELECT [Id] FROM dbo.[AccountTypes]
	);

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_AccountType0IsUsedInAccount1',
		[dbo].[fn_Localize](T.[Name], T.[Name2], T.[Name3]) AS [AccountType],
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS [Account]
	FROM @IndexesToDelete FE
	JOIN dbo.[AccountTypes] T ON FE.[Id] = T.[Id]
	JOIN dbo.[Accounts] A ON FE.[Id] = A.[AccountTypeId]
	
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;