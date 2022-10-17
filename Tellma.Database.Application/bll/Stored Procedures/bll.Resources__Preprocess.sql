CREATE PROCEDURE [bll].[Resources__Preprocess]
	@DefinitionId INT,
	@Entities [dbo].[ResourceList] READONLY,
	@UserId INT,
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;

	-- Grab the script
	DECLARE @PreprocessScript NVARCHAR(MAX) = (SELECT [PreprocessScript] FROM map.[ResourceDefinitions]() WHERE [Id] = @DefinitionId)

	-- Execute it if not null
	DECLARE @PreprocessedEntities [dbo].[ResourceList];
	IF (@PreprocessScript IS NOT NULL)
	BEGIN
		-- (1) Prepare the full Script
		DECLARE @Script NVARCHAR(MAX) = N'
			SET NOCOUNT ON
			DECLARE @ProcessedEntities [dbo].[ResourceList];

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
				@Entities [dbo].[ResourceList] READONLY', 
				@DefinitionId = @DefinitionId,
				@Entities = @Entities;
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

	-- Set unique items to unit pure
	IF (SELECT UnitCardinality FROM dbo.ResourceDefinitions WHERE [Id] = @DefinitionId) = N'None'
	UPDATE @PreprocessedEntities
	SET UnitId = (SELECT MIN([Id]) FROM dbo.Units WHERE UnitType = N'Pure')

	IF (
		SELECT COUNT(*) FROM dbo.[Centers] [C]
		WHERE [C].[IsActive] = 1 AND [C].[IsLeaf] = 1
	) = 1
	UPDATE @PreprocessedEntities
	SET [CenterId] = (
			SELECT TOP (1) [C].[Id] FROM dbo.[Centers] [C]
			WHERE [C].[IsActive] = 1 AND [C].[IsLeaf] = 1
		);

	SELECT * FROM @PreprocessedEntities;
END;