-- Returns all the permissions of the current user
CREATE PROCEDURE [dal].[Settings__Load]
AS
	-- Whether centers are multiple or singleton 
	DECLARE @CenterCount INT = (SELECT COUNT(*) FROM [dbo].[Centers] WHERE [IsActive] = 1 AND [IsLeaf] = 1);
	SELECT CAST(IIF(@CenterCount > 1, 1, 0) AS BIT);

	-- The settings
	SELECT [S].*, IIF(@CenterCount > 1, 1, 0) AS [IsMultiCenters]
	FROM [dbo].[Settings] AS [S]

	-- The functional currency
	SELECT [C].* FROM [dbo].[Currencies] AS [C] 
	JOIN [dbo].[Settings] AS [S] ON [C].[Id] = [S].[FunctionalCurrencyId]
