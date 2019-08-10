CREATE PROCEDURE [bll].[Documents_Ready__Select]
	@Entities [dbo].[IdList] READONLY,
	@State NVARCHAR(30)
AS
-- Assumes that every document type has a required set of signatures, and the document has been signed at least once
	SELECT R.[Id]
	FROM
	(
		SELECT D.[Id], COUNT(WS.[RoleId]) AS SignaturesCount
		FROM dbo.[Documents] D
		JOIN dbo.Workflows W ON D.DocumentTypeId = W.DocumentTypeId
		JOIN dbo.WorkflowSignatories WS ON W.Id = WS.RoleId
		WHERE D.[Id] IN (SELECT [Id] FROM @Entities)
		AND W.ToState = @State
		AND WS.RevokedById IS NULL
		AND W.RevokedById IS NULL
		GROUP BY D.[Id]
	) R -- Required signature to move
	INNER JOIN
	(
		SELECT D.[Id], COUNT(DS.[RoleId]) AS SignaturesCount
		FROM dbo.[Documents] D
		JOIN dbo.DocumentSignatures DS ON D.Id = DS.DocumentId
		WHERE D.[Id] IN (SELECT [Id] FROM @Entities)
		AND DS.[State] = @State
		AND DS.RevokedById IS NULL
		GROUP BY D.[Id]
	) A -- Available signatures to move
	ON R.[Id] = A.[Id] AND R.SignaturesCount = A.SignaturesCount