CREATE FUNCTION [rpt].[Resources] (
-- SELECT * FROM [rpt].[Account__Statement](104, '01.01.2015', '01.01.2020')
	@Ids dbo.[IdList] READONLY
) RETURNS TABLE
AS 
RETURN
	SELECT 	
		R.[Id], RC.[Name] AS CLassification, R.[Name], R.[IsActive], R.[Code]
	FROM dbo.Resources R
	LEFT JOIN dbo.ResourceClassifications RC ON R.ResourceClassificationId = RC.Id
	WHERE R.[Id] IN (SELECT [Id] FROM @Ids)
GO