CREATE PROCEDURE [dal].[Settings__Load]
AS
	-- The settings
	SELECT * FROM [dbo].[AdminSettings] AS [S]
