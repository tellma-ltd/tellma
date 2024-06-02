CREATE PROCEDURE [bll].[Agents_Validate__Save]
	@DefinitionId INT,
	@Entities [dbo].[AgentList] READONLY,
	@AgentUsers [dbo].[AgentUserList] READONLY,
	@Attachments [dbo].[AgentAttachmentList] READONLY,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	
	-- Grab the script
	DECLARE @ValidateScript NVARCHAR(MAX) = (SELECT [ValidateScript] FROM [map].[AgentDefinitions]() WHERE [Id] = @DefinitionId)

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
			------
			SELECT TOP (@Top) * FROM @ValidationErrors;
			';

		-- (2) Run the full Script
		BEGIN TRY
			INSERT INTO @ValidationErrors
			EXECUTE	dbo.sp_executesql @Script, N'
				@DefinitionId INT,
				@Entities [dbo].[AgentList] READONLY, 
				@AgentUsers [dbo].[AgentUserList] READONLY,
				@Top INT', 
				@DefinitionId = @DefinitionId,
				@Entities = @Entities,
				@AgentUsers = @AgentUsers,
				@Top = @Top;
		END TRY
		BEGIN CATCH
			DECLARE @ErrorNumber INT = 100000 + ERROR_NUMBER();
			DECLARE @ErrorMessage NVARCHAR (255) = ERROR_MESSAGE();
			DECLARE @ErrorState TINYINT = 99;
			THROW @ErrorNumber, @ErrorMessage, @ErrorState;
		END CATCH

	END

    -- Non zero Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Id',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE [Id] <> 0
	AND Id NOT IN (SELECT Id from [dbo].[Agents]);

	-- Code must be unique
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0]) 
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code
	FROM @Entities FE 
	JOIN [dbo].[Agents] BE ON FE.Code = BE.Code
	WHERE (BE.DefinitionId = @DefinitionId) AND ((FE.Id IS NULL) OR (FE.Id <> BE.Id));

		-- Code must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
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
			FROM [dbo].[AgentDefinitions] WHERE [Id] = @DefinitionId
		);
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Identifier',
		N'Error_TheIdentifier0IsUsed',
		@IdentifierLabel,
		FE.[Identifier]
	FROM @Entities FE 
	JOIN [dbo].[Agents] BE ON FE.[Identifier] = BE.[Identifier] AND BE.[DefinitionId] = @DefinitionId
	JOIN [dbo].[AgentDefinitions] RD ON BE.[DefinitionId] = RD.[Id]
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


	IF @DefinitionId <> dal.fn_AgentDefinitionCode__Id(N'Employee')
	BEGIN
		-- Name must be unique
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0]) 
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name',
			N'Error_TheName0IsUsed',
			FE.Name
		FROM @Entities FE 
		JOIN [dbo].[Agents] BE ON FE.Name = BE.Name
		WHERE (BE.DefinitionId = @DefinitionId) AND ((FE.Id IS NULL) OR (FE.Id <> BE.Id));

		-- Name must not be duplicated in the uploaded list
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + '].Name',
			N'Error_TheName0IsDuplicated',
			[Name]
		FROM @Entities
		WHERE [Name] IN (
			SELECT [Name]
			FROM @Entities
			WHERE [Name] IS NOT NULL
			GROUP BY [Name]
			HAVING COUNT(*) > 1
		);
	END
	-- call [bll].[AD__Validate] for new design
	
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;
GO

