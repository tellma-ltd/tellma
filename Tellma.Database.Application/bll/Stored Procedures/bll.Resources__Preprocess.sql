CREATE PROCEDURE [bll].[Resources__Preprocess]
	@DefinitionId NVARCHAR (255),
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
--WHERE [CurrencyId]  = dbo.fn_FunctionalCurrencyId();

--=-=-=-=-=-=-=-=-=-=-=-=-=-=-

IF (
	SELECT COUNT(*) FROM dbo.ResponsibilityCenters
	WHERE [IsActive] = 1 AND [IsLeaf] = 1
) = 1
UPDATE @PreprocessedResources
SET [ExpenseCenterId] = (
		SELECT [Id] FROM dbo.ResponsibilityCenters
		WHERE [IsActive] = 1 AND [IsLeaf] = 1
	);
	
IF (
	SELECT COUNT(*) FROM dbo.ResponsibilityCenters
	WHERE ResponsibilityType = N'Investment' AND [IsActive] = 1 AND [IsLeaf] = 1
) = 1
UPDATE @PreprocessedResources
SET
	[InvestmentCenterId] = (
		SELECT [Id] FROM dbo.ResponsibilityCenters
		WHERE ResponsibilityType = N'Investment' AND [IsActive] = 1 AND [IsLeaf] = 1
	);


SELECT * FROM @PreprocessedResources;