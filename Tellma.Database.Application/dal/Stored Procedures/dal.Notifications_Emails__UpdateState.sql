CREATE PROCEDURE [dal].[Notifications_Emails__UpdateState]
	@StateUpdates		    [dbo].[IdStateErrorList] READONLY,
	@EngagementUpdates		   [dbo].[IdStateErrorList] READONLY
AS
BEGIN
SET NOCOUNT ON;

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	-- (1) Update regular states
	UPDATE E
	SET E.[State] = U.[State],  E.[ErrorMessage] = U.[Error], E.[StateSince] = @Now
	FROM dbo.[Emails] E
	INNER JOIN @StateUpdates U ON E.[Id] = U.[Id]
	WHERE E.[State] <> E.[State] AND (U.[State] < 0 OR E.[State] < U.[State]) -- Positive states only advance forward
	
	-- (2) Update engagement states
	UPDATE E
	SET E.[EngagementState] = U.[State], E.[EngagementStateSince] = @Now
	FROM dbo.[Emails] E
	INNER JOIN @EngagementUpdates U ON E.[Id] = U.[Id]
	WHERE E.[EngagementState] <> E.[State] AND (U.[State] < 0 OR E.[EngagementState] < U.[State])

	-- TODO: If the user reports an email as spam, flag him/her to prevent any future emails
END
