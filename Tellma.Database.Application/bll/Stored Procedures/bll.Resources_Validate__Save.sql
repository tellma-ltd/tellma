﻿CREATE PROCEDURE [bll].[Resources_Validate__Save]
	@DefinitionId INT,
	@Entities [dbo].[ResourceList] READONLY,
	@ResourceUnits [dbo].[ResourceUnitList] READONLY,
	@Attachments [dbo].[ResourceAttachmentList] READONLY,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
			
	-- Grab the script
	DECLARE @ValidateScript NVARCHAR(MAX) = (SELECT [ValidateScript] FROM map.[ResourceDefinitions]() WHERE [Id] = @DefinitionId)

	-- Execute it if not null
	IF (@ValidateScript IS NOT NULL)
	BEGIN
		-- (1) Prepare the full Script
		DECLARE @Script NVARCHAR(MAX) = N'
			SET NOCOUNT ON
			DECLARE @ValidationErrors [dbo].[ValidationErrorList];
			------
			' 
			+ @ValidateScript + 
			N'
			-----
			SELECT TOP (@Top) * FROM @ValidationErrors;
			';

		-- (2) Run the full Script
		BEGIN TRY
			INSERT INTO @ValidationErrors
			EXECUTE	dbo.sp_executesql @Script, N'
				@DefinitionId INT,
				@Entities [dbo].[ResourceList] READONLY, 
				@ResourceUnits [dbo].[ResourceUnitList] READONLY,
				@Top INT', 
				@DefinitionId = @DefinitionId,
				@Entities = @Entities,
				@ResourceUnits = @ResourceUnits,
				@Top = @Top;
		END TRY
		BEGIN CATCH
			DECLARE @ErrorNumber INT = 100000 + ERROR_NUMBER();
			DECLARE @ErrorMessage NVARCHAR (255) = ERROR_MESSAGE();
			DECLARE @ErrorState TINYINT = 99;
			THROW @ErrorNumber, @ErrorMessage, @ErrorState;
		END CATCH
	END

	DECLARE @TitleSingular NVARCHAR (50);
	SELECT @TitleSingular = [dbo].[fn_Localize](TitleSingular, TitleSingular2, TitleSingular3)
	FROM [dbo].[ResourceDefinitions]
	WHERE [Id] = @DefinitionId

	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotModifyInactiveItem'
    FROM @Entities
    WHERE [Id] IN (SELECT [Id] from [dbo].[Resources] WHERE [IsActive] = 0);

    -- Non zero Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE [Id] <> 0 AND [Id] NOT IN (SELECT [Id] from [dbo].[Resources])

	-- Code must be unique
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code
	FROM @Entities FE 
	JOIN [dbo].[Resources] BE ON FE.Code = BE.Code AND BE.[DefinitionId] = @DefinitionId
	WHERE (FE.[Id] <> BE.[Id]);

	-- Code must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsDuplicated',
		[Code]
	FROM @Entities
	WHERE [Code] IN (
		SELECT [Code]
		FROM @Entities
		WHERE [Code] IS NOT NULL
		GROUP BY [Code]
		HAVING COUNT(*) > 1
	);

-- Identifier must be unique
	DECLARE @IdentifierLabel NVARCHAR (255) =  (
			SELECT [dbo].[fn_Localize]([IdentifierLabel], [IdentifierLabel2], [IdentifierLabel3])
			FROM [dbo].[ResourceDefinitions] WHERE [Id] = @DefinitionId
		);
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Identifier',
		N'Error_TheIdentifier0IsUsed',
		@IdentifierLabel,
		FE.[Identifier]
	FROM @Entities FE 
	JOIN [dbo].[Resources] BE ON FE.[Identifier] = BE.[Identifier] AND BE.[DefinitionId] = @DefinitionId
	JOIN [dbo].[ResourceDefinitions] RD ON BE.[DefinitionId] = RD.[Id]
	WHERE (FE.[Id] <> BE.[Id]);

	-- Identifier must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Identifier',
		N'Error_TheIdentifier0IsDuplicated',
		@IdentifierLabel,
		[Identifier]
	FROM @Entities
	WHERE [Identifier] IN (
		SELECT [Identifier]
		FROM @Entities
		WHERE [Identifier] IS NOT NULL
		GROUP BY [Identifier]
		HAVING COUNT(*) > 1
	);

	-- Name must not exist in the db
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name',
		N'Error_TheName0IsUsed',
		FE.[Name]
	FROM @Entities FE 
	JOIN [dbo].[Resources] BE ON FE.[Name] = BE.[Name]
	AND (FE.[Identifier] IS NULL AND BE.[Identifier] IS NULL OR FE.[Identifier] = BE.[Identifier])
	WHERE BE.[DefinitionId] = @DefinitionId AND  (FE.[Id] <> BE.[Id]);

	-- Name2 must not exist in the db
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name2',
		N'Error_TheName0IsUsed',
		FE.[Name2]
	FROM @Entities FE 
	JOIN [dbo].[Resources] BE ON FE.[Name2] = BE.[Name2]
	AND (FE.[Identifier] IS NULL AND BE.[Identifier] IS NULL OR FE.[Identifier] = BE.[Identifier])
	WHERE BE.[DefinitionId] = @DefinitionId AND (FE.[Id] <> BE.[Id]);

	-- Name3 must not exist in the db
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name3',
		N'Error_TheName0IsUsed',
		FE.[Name3]
	FROM @Entities FE 
	JOIN [dbo].[Resources] BE ON FE.[Name3] = BE.[Name3]
	AND (FE.[Identifier] IS NULL AND BE.[Identifier] IS NULL OR FE.[Identifier] = BE.[Identifier])
	WHERE BE.[DefinitionId] = @DefinitionId AND (FE.[Id] <> BE.[Id]);

	-- Name must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Name',
		N'Error_TheName0IsDuplicated',
		[Name]
	FROM @Entities
	WHERE [Name] IN (
		SELECT [Name]
		FROM @Entities
		GROUP BY [Name], [Identifier]
		HAVING COUNT(*) > 1
	);

	-- Name2 must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Name2',
		N'Error_TheName0IsDuplicated',
		[Name2]
	FROM @Entities
	WHERE [Name2] IN (
		SELECT [Name2]
		FROM @Entities
		WHERE [Name2] IS NOT NULL
		GROUP BY [Name2], [Identifier]
		HAVING COUNT(*) > 1
	);

	-- Name3 must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Name3',
		N'Error_TheName0IsDuplicated',
		[Name3]
	FROM @Entities
	WHERE [Name3] IN (
		SELECT [Name3]
		FROM @Entities
		WHERE [Name3] IS NOT NULL
		GROUP BY [Name3], [Identifier]
		HAVING COUNT(*) > 1
	);

	-- Unit in ResourceUnits must be of same type of Header unit or be of type Mass
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(R.[Index] AS NVARCHAR (255)) + '].Units[' + CAST(RU.[Index] AS NVARCHAR(255)) + '].UnitId',
		N'Error_TheUnit0HasIncompatibleUnitTypeMustBeType1',
		[dbo].[fn_Localize](URU.[Name], URU.[Name2], URU.[Name3]) AS [NameOfIncompatibleUnitName],
		N'localize:Unit_' + UR.[UnitType] as [ExpectedType]
	FROM @Entities R
	JOIN [dbo].[Units] UR ON R.[UnitId] = UR.[Id]
	JOIN @ResourceUnits RU ON R.[Index] = RU.[HeaderIndex]
	JOIN [dbo].[Units] URU ON RU.[UnitId] = URU.[Id]
	WHERE URU.[UnitType] <> UR.[UnitType];
	-- AND URU.[UnitType] <> N'Mass'. MA: 2025-03-05 Commented Out

	-- Cannot change currency if resource is already used in Entries with different currency
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(R.[Index] AS NVARCHAR (255)) + '].CurrencyId',
		N'Error_TheResource0WasUsedInDocument1WithCurrency2',
		@TitleSingular,
		D.[Code],
		E.[CurrencyId]
	FROM @Entities R
	JOIN [dbo].[Entries] E ON R.[Id] = E.[ResourceId]
	JOIN [dbo].[Lines] L ON E.[LineId] = L.[Id]
	JOIN [map].[Documents]() D ON D.[Id] = L.[DocumentId]
	WHERE R.[CurrencyId] IS NOT NULL AND E.[CurrencyId] <> R.[CurrencyId]

	-- Cannot change currency if resource is already used in Account with different currency
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(R.[Index] AS NVARCHAR (255)) + '].CurrencyId',
		N'Error_TheResource0WasUsedInAccount1WithCurrency2',
		@TitleSingular,
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]),
		A.[CurrencyId]
	FROM @Entities R
	JOIN [dbo].[Accounts] A ON R.[Id] = A.[ResourceId]
	WHERE R.[CurrencyId] IS NOT NULL AND A.[CurrencyId] <> R.[CurrencyId]

	-- TODO: Cannot change unit type if resource is already used in Entries with different unit type
	-- Nafkot changed Nails from pcs to Kg and the expense capitalization failed

	/*
	-- Cannot change Center if resource is already used in Entries with different Center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(R.[Index] AS NVARCHAR (255)) + '].CenterId',
		N'Error_TheResource0WasUsedInDocument1WithCenter2',
		@TitleSingular,
		D.[Code],
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS BusinessUnit
	FROM @Entities R
	JOIN [dbo].[Entries] E ON R.[Id] = E.[ResourceId]
	JOIN [dbo].[Lines] L ON E.[LineId] = L.[Id]
	JOIN [map].[Documents]() D ON D.[Id] = L.[DocumentId]
	JOIN [map].[Accounts]() A ON E.AccountId = A.[Id]
	JOIN [dbo].[Centers] C ON E.[CenterId] = C.[Id]
	WHERE R.[CenterId] IS NOT NULL AND E.[CenterId] <> R.[CenterId]
	AND A.[IsBusinessUnit] = 1

	-- Only business units may be assigned to resources
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(R.[Index] AS NVARCHAR (255)) + '].CenterId',
		N'Error_CenterMustBeBusinessUnit'
	FROM @Entities R
	JOIN [dbo].[Centers] C ON R.[CenterId] = C.[Id]
	WHERE R.[CenterId] IS NOT NULL AND C.[CenterType] <> N'BusinessUnit'
*/
	-- Cannot change Center if resource is already used in Account with different Center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(R.[Index] AS NVARCHAR (255)) + '].CenterId',
		N'Error_TheResource0WasUsedInAccount1WithCenter2',
		@TitleSingular,
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]),
		A.[CenterId]
	FROM @Entities R
	JOIN [dbo].[Accounts] A ON R.[Id] = A.[ResourceId]
	WHERE R.[CenterId] IS NOT NULL AND A.[CenterId] <> R.[CenterId]

	-- Cannot assign an inactive Agent1
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Agent1Id',
		N'Error_TheAgent01IsInactive',
		[dbo].[fn_Localize](AGD.[TitleSingular], AGD.[TitleSingular2], AGD.[TitleSingular3]),
		[dbo].[fn_Localize](AG.[Name], AG.[Name2], AG.[Name3])
	FROM @Entities FE
	JOIN [dbo].[Agents] AG ON FE.[Agent1Id] = AG.[Id]
	JOIN [dbo].[AgentDefinitions] AGD ON AG.[DefinitionId] = AGD.[Id]
	WHERE AG.[IsActive] = 0

	-- Cannot assign an inactive  Agent2
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Agent2Id',
		N'Error_TheAgent01IsInactive',
		[dbo].[fn_Localize](AGD.[TitleSingular], AGD.[TitleSingular2], AGD.[TitleSingular3]),
		[dbo].[fn_Localize](AG.[Name], AG.[Name2], AG.[Name3])
	FROM @Entities FE
	JOIN [dbo].[Agents] AG ON FE.[Agent2Id] = AG.[Id]
	JOIN [dbo].[AgentDefinitions] AGD ON AG.[DefinitionId] = AGD.[Id]
	WHERE AG.[IsActive] = 0

	-- Cannot assign an inactive center
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].CenterId',
		N'Error_TheCenter0IsInactive',
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3])
	FROM @Entities FE
	JOIN [dbo].[Centers] C ON FE.[CenterId] = C.[Id]
	WHERE C.[IsActive] = 0

	-- Cannot assign an inactive currency
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].CurrencyId',
		N'Error_TheCurrency0IsInactive',
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3])
	FROM @Entities FE
	JOIN [dbo].[Currencies] C ON FE.CurrencyId = C.[Id]
	WHERE C.[IsActive] = 0
	
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;
