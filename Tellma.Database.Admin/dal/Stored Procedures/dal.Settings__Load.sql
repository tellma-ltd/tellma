CREATE PROCEDURE [dal].[Settings__Load]
AS
BEGIN
	-- The settings
	SELECT * FROM [dbo].[AdminSettings] AS [S];
END;
