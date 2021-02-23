CREATE PROCEDURE [rpt].[Documents__NoAttachments]
@Since DATE
AS
	SELECT D.[Code], U.[Name], D.PostingDate
	from map.Documents() D
	JOIN dbo.DocumentDefinitions DD ON D.[DefinitionId] = DD.[Id]
	JOIN dbo.Users U ON D.[ModifiedById] = U.[Id]
	LEFT JOIN dbo.Attachments A ON D.[Id] = A.[DocumentId]
	WHERE A.[Id] IS NULL
	AND D.[State] = 1
	AND DD.Prefix IN (N'RA', N'SA', N'SMV', N'CRSI', N'CRV', N'CSI', N'SRV', N'CPV' )
	AND D.PostingDate > @Since
	ORDER BY U.Name, D.Code