CREATE FUNCTION [rpt].[Documents] (
-- SELECT * FROM [rpt].[Account__Statement](104, '01.01.2015', '01.01.2020')
	@Ids dbo.[IdList] READONLY
) RETURNS TABLE
AS 
RETURN
	SELECT 	
		D.[Id],
		D.[DocumentDate],
		D.[DocumentTypeId],
		DT.Prefix + 
		REPLICATE(N'0', DT.CodeWidth - 1 - FLOOR(LOG10(D.SerialNumber))) +
		CAST(D.SerialNumber AS NVARCHAR(30)) AS [S/N],
		D.[State],
		D.[VoucherNumericReference],
		AG.[Name] AS AssignedTo,
		D.[Memo]
	FROM dbo.Documents D
	JOIN dbo.DocumentTypes DT ON D.[DocumentTypeId] = DT.[Id]
	LEFT JOIN dbo.DocumentAssignments DA ON D.[Id] = DA.[DocumentId]
	LEFT JOIN dbo.Agents AG ON DA.AssigneeId = AG.Id
	WHERE D.[Id] IN (SELECT [Id] FROM @Ids)
GO