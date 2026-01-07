CREATE PROCEDURE [dal].[InboxCounts__Load]
	@UserIds [dbo].[IdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		U.[ExternalId],
		ISNULL(Summary.[Count], 0) AS [Count],
		ISNULL(Summary.[UnknownCount], 0) AS [UnknownCount]
	FROM [dbo].[Users] U
	INNER JOIN @UserIds UI ON U.[Id] = UI.[Id]
	LEFT JOIN (
		SELECT 
			DA.[AssigneeId],
			COUNT(*) AS [Count],
			SUM(CASE 
				WHEN DA.[OpenedAt] IS NULL 
				AND (U2.[LastInboxCheck] IS NULL OR DA.[CreatedAt] > U2.[LastInboxCheck]) 
				THEN 1 ELSE 0 END) AS [UnknownCount]
		FROM [dbo].[DocumentAssignments] DA
		JOIN [dbo].[Users] U2 ON DA.[AssigneeId] = U2.[Id]
		WHERE DA.[AssigneeId] IN (SELECT [Id] FROM @UserIds)
		GROUP BY DA.[AssigneeId]
	) AS Summary ON U.[Id] = Summary.[AssigneeId]
	WHERE U.[ExternalId] IS NOT NULL;
END;