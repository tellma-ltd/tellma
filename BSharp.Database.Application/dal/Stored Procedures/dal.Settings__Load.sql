-- Returns all the permissions of the current user
CREATE PROCEDURE [dal].[Settings__Load]
AS
	-- Whether responsibility centers are multiple or singleton 
	DECLARE @ResponsibilityCenterCount INT = (SELECT COUNT(*) FROM [dbo].[ResponsibilityCenters] WHERE [IsActive] = 1 AND [IsLeaf] = 1);
	SELECT CAST(IIF(@ResponsibilityCenterCount > 1, 1, 0) AS BIT);

	-- The settings
	SELECT [S].*, IIF(@ResponsibilityCenterCount > 1, 1, 0) AS [IsMultiResponsibilityCenters]
	FROM [dbo].[Settings] AS [S]

	-- The functional currency
	SELECT [C].* FROM [dbo].[Currencies] AS [C] 
	JOIN [dbo].[Settings] AS [S] ON [C].[Id] = [S].[FunctionalCurrencyId]

	-- Load the mappings
	SELECT N'TODO: change the code' AS [ResourceClassificationPath], 0 AS [EntryClassificationId]
