CREATE PROCEDURE [bll].[Agents__Preprocess]
	@DefinitionId INT,
	@Entities [dbo].[AgentList] READONLY,
	@UserId INT,
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;

	-- Grab the script
	DECLARE @PreprocessScript NVARCHAR(MAX) = (SELECT [PreprocessScript] FROM map.[AgentDefinitions]() WHERE [Id] = @DefinitionId)

	-- Execute it if not null
	DECLARE @PreprocessedEntities [dbo].[AgentList];
	IF (@PreprocessScript IS NOT NULL)
	BEGIN
		-- (1) Prepare the full Script
		DECLARE @Script NVARCHAR(MAX) = N'
			SET NOCOUNT ON
			DECLARE @ProcessedEntities [dbo].[AgentList];

			INSERT INTO @ProcessedEntities
			SELECT * FROM @Entities;
			------
			'
			+ @PreprocessScript + 
			N'
			-----
			SELECT * FROM @ProcessedEntities;			
			';
		
		-- (2) Run the full Script
		BEGIN TRY
			INSERT INTO @PreprocessedEntities
			EXECUTE	dbo.sp_executesql @Script, N'
				@DefinitionId INT,
				@Entities [dbo].[AgentList] READONLY,
				@UserId INT', 
				@DefinitionId = @DefinitionId,
				@Entities = @Entities,
				@UserId = @UserId; -- MA: added 2023.07.17
		END TRY
		BEGIN CATCH
			DECLARE @ErrorNumber INT = 100000 + ERROR_NUMBER();
			DECLARE @ErrorMessage NVARCHAR (255) = ERROR_MESSAGE();
			DECLARE @ErrorState TINYINT = 99;
			THROW @ErrorNumber, @ErrorMessage, @ErrorState;
		END CATCH
	END
	ELSE
	BEGIN
		INSERT INTO @PreprocessedEntities
		SELECT * FROM @Entities;
	END

	---- Any additional treatment for @PreprocessedEntities goes here

	SELECT * FROM @PreprocessedEntities;
END;