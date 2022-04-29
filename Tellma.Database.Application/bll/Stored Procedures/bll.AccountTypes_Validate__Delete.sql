CREATE PROCEDURE [bll].[AccountTypes_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors dbo.[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotDeleteSystemRecords'
	FROM @Ids FE
    JOIN dbo.AccountTypes BE ON FE.[Id] = BE.[Id]
	WHERE BE.[IsSystem] = 1;

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
    SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccountType0IsUsedInAccount1',
		dbo.fn_Localize(BE.[Name], BE.[Name2], BE.[NAme3]),
		dbo.fn_Localize(A.[Name], A.[Name2], A.[NAme3])
	FROM @Ids FE
    JOIN dbo.AccountTypes BE ON FE.[Id] = BE.[Id]
    JOIN dbo.Accounts A ON FE.[Id] = A.[AccountTypeId]

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
    SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAccountType0IsUsedInLineDefinition1',
		dbo.fn_Localize(BE.[Name], BE.[Name2], BE.[NAme3]),
		dbo.fn_Localize(LD.[TitleSingular], LD.[TitleSingular2], LD.[TitleSingular3])
	FROM @Ids FE
    JOIN dbo.AccountTypes BE ON FE.[Id] = BE.[Id]
	JOIN dbo.LineDefinitionEntries LDE ON LDE.[ParentAccountTypeId] = FE.[Id]
    JOIN dbo.LineDefinitions LD ON LD.[Id] = LDE.[LineDefinitionId]

	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;