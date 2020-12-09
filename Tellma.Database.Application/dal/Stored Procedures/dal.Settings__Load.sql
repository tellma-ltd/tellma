-- Returns all the permissions of the current user
CREATE PROCEDURE [dal].[Settings__Load]
AS
	-- Whether centers are multiple or singleton 
	DECLARE @BusinessUnitCount INT = (SELECT COUNT(*) FROM [dbo].[Centers] WHERE [CenterType] = N'BusinessUnit' AND [IsActive] = 1);
	SELECT CAST(IIF(@BusinessUnitCount <> 1, 1, 0) AS BIT) As [IsMultiBusinessUnit];

	-- The settings
	SELECT [S].* FROM [dbo].[Settings] AS [S]

	-- The functional currency
	SELECT [C].* FROM [dbo].[Currencies] AS [C] 
	JOIN [dbo].[Settings] AS [S] ON [C].[Id] = [S].[FunctionalCurrencyId]
