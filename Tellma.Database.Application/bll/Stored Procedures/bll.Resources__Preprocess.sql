﻿CREATE PROCEDURE [bll].[Resources__Preprocess]
	@DefinitionId NVARCHAR (255),
	@Entities [dbo].[ResourceList] READONLY
AS
SET NOCOUNT ON;
DECLARE @PreprocessedResources [dbo].[ResourceList];

INSERT INTO @PreprocessedResources
SELECT * FROM @Entities;

UPDATE @PreprocessedResources
SET
	[ResidualValue] = [ResidualMonetaryValue]
WHERE [CurrencyId]  = dbo.fn_FunctionalCurrencyId();

IF (
	SELECT COUNT(*) FROM dbo.ResponsibilityCenters
	WHERE ResponsibilityType = N'Investment' AND [IsActive] = 1
) = 1
UPDATE @PreprocessedResources
SET
	[InvestmentCenterId] = (
		SELECT [Id] FROM dbo.ResponsibilityCenters
		WHERE ResponsibilityType = N'Investment' AND [IsActive] = 1
	);


SELECT * FROM @PreprocessedResources;