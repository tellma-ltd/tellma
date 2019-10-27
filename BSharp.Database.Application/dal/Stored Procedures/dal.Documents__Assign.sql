CREATE PROCEDURE [dal].[Documents__Assign]
	@Documents [dbo].[IdList] READONLY,
	@AssigneeId INT,
	@Comment NVARCHAR(1024) = NULL
AS
BEGIN
	IF @AssigneeId IS NULL
		DELETE FROM dbo.DocumentAssignments
		WHERE DocumentId IN (SELECT [Id] FROM @Documents);
	ELSE BEGIN
		MERGE INTO [dbo].[DocumentAssignments] AS t
		USING (
			SELECT
				[Id]
			FROM @Documents 
		) AS s ON (t.[DocumentId] = s.Id)
		WHEN MATCHED THEN
			UPDATE SET
				t.[AssigneeId] = @AssigneeId,
				t.[Comment] = @Comment
		WHEN NOT MATCHED THEN
			INSERT ([DocumentId], [AssigneeId], [Comment])
			VALUES (s.[Id], @AssigneeId, @Comment);

		INSERT dbo.DocumentAssignmentsHistory([DocumentId], [AssigneeId], [Comment], [CreatedAt], [CreatedById])
		SELECT [DocumentId], [AssigneeId], [Comment], [CreatedAt], [CreatedById]
		FROM dbo.DocumentAssignments
		WHERE DocumentId IN (SELECT [Id] FROM @Documents);
	END
END;