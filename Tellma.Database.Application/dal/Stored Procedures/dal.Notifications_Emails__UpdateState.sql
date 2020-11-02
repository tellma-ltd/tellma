CREATE PROCEDURE [dal].[Notifications_Emails__UpdateState]
	@Updates	[dbo].[IdStateErrorTimestampList] READONLY
AS
BEGIN
SET NOCOUNT ON;

	UPDATE E
	SET 
		E.[State] = U.[State],
		E.[ErrorMessage] = U.[Error],
		E.[StateSince] = U.[Timestamp],
		E.[DeliveredAt] = IIF(E.[State] < 3 AND U.[State] >= 3, U.[Timestamp], E.[DeliveredAt]),
		E.[OpenedAt] = IIF(E.[State] < 4 AND U.[State] >= 4, U.[Timestamp], E.[OpenedAt])
	FROM dbo.[Emails] E
	INNER JOIN @Updates U ON E.[Id] = U.[Id]
	WHERE E.[State] <> U.[State] AND (U.[State] < 0 OR E.[State] < U.[State]) -- Positive states only advance forward
END
