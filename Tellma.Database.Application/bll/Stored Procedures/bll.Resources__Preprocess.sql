CREATE PROCEDURE [bll].[Resources__Preprocess]
	@DefinitionId INT,
	@Entities [dbo].[ResourceList] READONLY
AS
SET NOCOUNT ON;
DECLARE @PreprocessedEntities [dbo].[ResourceList];

--=-=-=-=-=-=-=-=-=-=-=-=-=-=- DONE IN C#
--UPDATE @PreprocessedEntities
--SET
--	[ResidualValue] = [ResidualMonetaryValue]
--	WHERE [CurrencyId]  = dbo.fn_FunctionalCurrencyId();

--=-=-=-=-=-=-=-=-=-=-=-=-=-=-


-- Grab the script
DECLARE @PreprocessScript NVARCHAR(MAX) = (SELECT [PreprocessScript] FROM map.[ResourceDefinitions]() WHERE [Id] = @DefinitionId)

-- Execute it if not null
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
	INSERT INTO @PreprocessedEntities
	EXECUTE	sp_executesql @Script, N'
		@DefinitionId INT,
		@Entities [dbo].[ResourceList] READONLY', 
		@DefinitionId = @DefinitionId,
		@Entities = @Entities;
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

-- Handled by C#
--UPDATE @PreprocessedEntities
--SET
--	UnitId = COALESCE(UnitId, (SELECT [DefaultUnitId] FROM dbo.ResourceDefinitions WHERE [Id] = @DefinitionId)),
--	UnitMassUnitId = COALESCE(UnitMassUnitId, (SELECT [DefaultUnitMassUnitId] FROM dbo.ResourceDefinitions WHERE [Id] = @DefinitionId))

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