CREATE FUNCTION [rpt].[Resources] (
-- SELECT * FROM [rpt].[Account__Statement](104, '01.01.2015', '01.01.2020')
	@Ids dbo.[IdList] READONLY
) RETURNS TABLE
AS 
RETURN
	SELECT 	
		[Id], [ResourceType], [Name], [IsActive], [IsBatch], [Code]
	FROM dbo.Resources
	WHERE [Id] IN (SELECT [Id] FROM @Ids)
GO