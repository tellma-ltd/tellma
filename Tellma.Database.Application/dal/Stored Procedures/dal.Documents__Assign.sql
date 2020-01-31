CREATE PROCEDURE [dal].[Documents__Assign]
	@Ids [dbo].[IdList] READONLY,
	@AssigneeId INT,
	@Comment NVARCHAR(1024) = NULL
AS
BEGIN
	IF @AssigneeId IS NULL
		DELETE FROM dbo.DocumentAssignments
		WHERE DocumentId IN (SELECT [Id] FROM @Ids);
	ELSE BEGIN
		MERGE INTO [dbo].[DocumentAssignments] AS t
		USING (
			SELECT
				[Id]
			FROM @Ids 
		) AS s ON (t.[DocumentId] = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[AssigneeId] = @AssigneeId,
				t.[Comment] = @Comment,
				t.[CreatedAt] = SYSDATETIMEOFFSET(),
				t.[CreatedById] = CONVERT(INT, SESSION_CONTEXT(N'UserId'))
		WHEN NOT MATCHED THEN
			INSERT ([DocumentId], [AssigneeId], [Comment])
			VALUES (s.[Id], @AssigneeId, @Comment);

		INSERT dbo.DocumentAssignmentsHistory([DocumentId], [AssigneeId], [Comment], [CreatedAt], [CreatedById])
		SELECT [DocumentId], [AssigneeId], [Comment], [CreatedAt], [CreatedById]
		FROM dbo.DocumentAssignments
		WHERE DocumentId IN (SELECT [Id] FROM @Ids)
		AND ( -- do not add the first assignment to history. It is redundant.
			[AssigneeId] <> [CreatedById] OR 
			DocumentId IN (SELECT [DocumentId] FROM dbo.DocumentAssignmentsHistory)
		)
	END
END;