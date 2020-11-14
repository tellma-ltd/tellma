CREATE PROCEDURE [bll].[Custodies__Preprocess]
	@DefinitionId INT,
	@Entities [dbo].[CustodyList] READONLY
AS
SET NOCOUNT ON;
DECLARE @PreprocessedEntities [dbo].[CustodyList];

--=-=-=-=-=-=-=-=-=-=-=-=-=-=- DONE IN C#

--=-=-=-=-=-=-=-=-=-=-=-=-=-=-

-- Grab the script
DECLARE @PreprocessScript NVARCHAR(MAX) = (SELECT [PreprocessScript] FROM map.[CustodyDefinitions]() WHERE [Id] = @DefinitionId)

-- Execute it if not null
IF (@PreprocessScript IS NOT NULL)
BEGIN
	-- (1) Prepare the full Script
	DECLARE @Script NVARCHAR(MAX) = N'
		SET NOCOUNT ON
		DECLARE @ProcessedEntities [dbo].[CustodyList];

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
		@Entities [dbo].[CustodyList] READONLY', 
		@DefinitionId = @DefinitionId,
		@Entities = @Entities;
END
ELSE
BEGIN
	INSERT INTO @PreprocessedEntities
	SELECT * FROM @Entities;
END


---- Any additional treatment for @PreprocessedEntities goes here


SELECT * FROM @PreprocessedEntities;