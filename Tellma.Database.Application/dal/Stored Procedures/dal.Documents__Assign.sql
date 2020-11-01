CREATE PROCEDURE [dal].[Documents__Assign]
	@Ids [dbo].[IdList] READONLY,
	@AssigneeId INT,
	@Comment NVARCHAR(1024) = NULL,
	@ManualAssignment BIT = 0
AS
BEGIN
	DECLARE @AffectedUsers dbo.IdList;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Retrieve the affected users whose documents will be re-assigned
	INSERT INTO @AffectedUsers
	SELECT DISTINCT [AssigneeId]
	FROM [dbo].[DocumentAssignments]
	WHERE (@AssigneeId IS NULL OR [AssigneeId] <> @AssigneeId)
		AND [DocumentId] IN (SELECT [Id] FROM @Ids);

	IF (@AssigneeId IS NOT NULL)
		INSERT INTO @AffectedUsers ([Id]) VALUES (@AssigneeId);

	IF @AssigneeId IS NULL
		DELETE FROM dbo.DocumentAssignments
		WHERE DocumentId IN (SELECT [Id] FROM @Ids);
	ELSE BEGIN
		MERGE INTO [dbo].[DocumentAssignments] AS t
		USING (
			SELECT
				[Id]
			FROM @Ids WHERE [Id] IN (SELECT [Id] FROM dbo.Documents) -- If trying to open a document that does not exist anymore.
		) AS s ON (t.[DocumentId] = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[AssigneeId] = @AssigneeId,
				t.[Comment] = @Comment,
				t.[CreatedAt] = SYSDATETIMEOFFSET(),
				t.[CreatedById] = @UserId,
				t.[OpenedAt] = IIF(@AssigneeId = @UserId, @Now, NULL) -- Self assigned documents are automatically marked opened
		WHEN NOT MATCHED THEN
			INSERT ([DocumentId], [AssigneeId], [Comment], [OpenedAt])
			VALUES (s.[Id], @AssigneeId, @Comment, IIF(@AssigneeId = @UserId, @Now, NULL));

		IF (@ManualAssignment = 1)
			INSERT dbo.DocumentAssignmentsHistory([DocumentId], [AssigneeId], [Comment], [CreatedAt], [CreatedById], [OpenedAt])
			SELECT [DocumentId], [AssigneeId], [Comment], [CreatedAt], [CreatedById], [OpenedAt]
			FROM dbo.DocumentAssignments
			WHERE DocumentId IN (SELECT [Id] FROM @Ids WHERE [Id] IN (SELECT [Id] FROM dbo.Documents))
	END

	-- Return Notification info
	EXEC [dal].[InboxCounts__Load] @UserIds = @AffectedUsers;

	-- Return contact info of the user, for notification purposeses
	IF (@ManualAssignment = 1)
		SELECT 
			[Name],
			[Name2],
			[Name3],
			[PreferredLanguage],
			[ContactEmail], 
			[ContactMobile], 
			[PushEndpoint],	
			[PushP256dh],
			[PushAuth],	
			[PreferredChannel],	
			[EmailNewInboxItem],
			[SmsNewInboxItem],
			[PushNewInboxItem]
		FROM dbo.[Users]
		WHERE [Id] = @AssigneeId;
END;