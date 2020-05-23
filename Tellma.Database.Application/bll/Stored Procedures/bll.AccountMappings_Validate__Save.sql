CREATE PROCEDURE [bll].[AccountMappings_Validate__Save]
	@Entities [dbo].[AccountMappingList] READONLY,
	@Top INT = 10,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
	--[AccountDesignationId]	INT,
	----[MapFunction]			SMALLINT,
	--[CenterId]				INT,
	--[ContractId]			INT,
	--[ResourceId]			INT,
	--[CurrencyId]			NCHAR (3),
	--[AccountId]				INT
	--=-=-=-=-=-=- [C# Validation]
	/* 
	
	 [ ] That tuples are unique within the arriving list

	*/

-- TODO: Add tests for every violation
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

    -- Non zero Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255)) AS [Id]
    FROM @Entities
    WHERE Id <> 0 AND Id NOT IN (SELECT Id from [dbo].[AccountMappings])

	-- No duplicate values in the uploaded list => Convert to C# code
	-- Code below has bug anyway, but...you got the idea
 --   INSERT INTO @ValidationErrors([Key], [ErrorName])
	--SELECT TOP (@Top)
	--	'[' + CAST([Index] AS NVARCHAR (255)) + ']',
	--	N'Error_DuplicateRecordsInList'
 --   FROM @Entities
	--GROUP BY [AccountDesignationId], [CenterId], [ContractId], [ResourceId], [CurrencyId]
	--HAVING COUNT(*) > 1

	-- values in the uploaded list must not be in DB already
    INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_RecordAlreadyInDatabase'
    FROM @Entities FE
	JOIN dbo.AccountMappings AM
	ON	FE.[AccountTypeId]	= AM.[DesignationId]
	AND	FE.[CenterId]				= AM.[CenterId]
	AND	FE.[ContractId]				= AM.[ContractId]
	AND	FE.[ResourceId]				= AM.[ResourceId]
	AND	FE.[CurrencyId]				= AM.[CurrencyId]
	WHERE (FE.[Id] <> AM.[Id])
	
	-- Some mapping values are inconsistent with Account properties
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_AccountNoConformantWithMappingDetails'
    FROM @Entities FE
	JOIN dbo.Accounts A
	ON	FE.[AccountId]	= A.[Id]
	WHERE
		(A.[AccountTypeId] <> FE.[AccountTypeId])
	OR	(A.[CenterId] IS NOT NULL AND FE.[CenterId] IS NOT NULL AND A.[CenterId] <> FE.[CenterId])
	OR	(A.[ContractId] IS NOT NULL AND FE.[ContractId] IS NOT NULL AND A.[ContractId] <> FE.[ContractId])
	OR	(A.[ResourceId] IS NOT NULL AND FE.[ResourceId] IS NOT NULL AND A.[ResourceId] <> FE.[CenterId])
	OR	(A.[CurrencyId] IS NOT NULL AND FE.[CurrencyId] IS NOT NULL AND A.[CurrencyId] <> FE.[CurrencyId])

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	SELECT TOP (@Top) * FROM @ValidationErrors;