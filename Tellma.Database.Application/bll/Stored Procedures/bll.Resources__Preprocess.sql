CREATE PROCEDURE [bll].[Resources__Preprocess]
	@DefinitionId INT,
	@Entities [dbo].[ResourceList] READONLY
AS
SET NOCOUNT ON;
DECLARE @PreprocessedResources [dbo].[ResourceList];

INSERT INTO @PreprocessedResources
SELECT * FROM @Entities;

--=-=-=-=-=-=-=-=-=-=-=-=-=-=- DONE IN C#
--UPDATE @PreprocessedResources
--SET
--	[ResidualValue] = [ResidualMonetaryValue]
--	WHERE [CurrencyId]  = dbo.fn_FunctionalCurrencyId();

--=-=-=-=-=-=-=-=-=-=-=-=-=-=-

IF (
	SELECT COUNT(*) FROM dbo.[Centers] [C]
	WHERE [C].[IsActive] = 1 AND [C].[IsLeaf] = 1
) = 1
UPDATE @PreprocessedResources
SET [CenterId] = (
		SELECT TOP (1) [C].[Id] FROM dbo.[Centers] [C]
		WHERE [C].[IsActive] = 1 AND [C].[IsLeaf] = 1
	);

SELECT * FROM @PreprocessedResources;