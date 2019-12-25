CREATE FUNCTION [map].[DocumentSignatures] ()
RETURNS TABLE
AS
RETURN (
	SELECT
		MIN(LS.[Id]) AS [Id],
		L.[DocumentId],
		LS.[SignedAt],
		LS.[OnBehalfOfUserId],
		LS.[RoleId],
		LS.[CreatedAt],
		LS.[CreatedById]
	FROM dbo.LineSignatures LS
	JOIN dbo.Lines L ON L.[Id] = LS.[LineId]
	WHERE
		ToState > 0 AND
		RevokedById IS NULL
	GROUP BY
		L.[DocumentId],
		LS.[SignedAt],
		LS.[OnBehalfOfUserId],
		LS.[RoleId],
		LS.[CreatedAt],
		LS.[CreatedById]
);