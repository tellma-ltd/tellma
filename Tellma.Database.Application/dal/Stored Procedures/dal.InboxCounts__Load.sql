CREATE PROCEDURE [dal].[InboxCounts__Load]
	@UserIds dbo.[IdList] READONLY
AS
	SELECT 
		[ExternalId],
		(SELECT COUNT(*) FROM [dbo].[DocumentAssignments] WHERE [AssigneeId] = U.Id) AS [Count],
		(SELECT COUNT(*) FROM [dbo].[DocumentAssignments] WHERE [AssigneeId] = U.Id AND [OpenedAt] IS NULL AND (U.[LastInboxCheck] IS NULL OR [CreatedAt] > U.[LastInboxCheck])) AS [UnknownCount]
	FROM [dbo].[Users] U 
	JOIN @UserIds UI ON U.Id = UI.Id
	WHERE U.ExternalId IS NOT NULL;