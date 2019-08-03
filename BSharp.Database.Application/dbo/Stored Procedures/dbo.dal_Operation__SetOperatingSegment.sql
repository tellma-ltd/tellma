CREATE PROCEDURE [dbo].[dal_Operation__SetOperatingSegment]
	@OperationId INT
AS
	DECLARE @Id INT, @Ids [dbo].[IdList], @NextIds [dbo].[IdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	--UPDATE dbo.[ResponsibilityCenters]
	--SET
	--	[IsOperatingSegment] = 1,
	--	[ModifiedAt] = @Now,
	--	[ModifiedById] = @UserId
	--WHERE [Id] = @OperationId
	--AND [IsOperatingSegment] = 0;

-- Reset all ancestors up to the root
	DECLARE @ParentId INT
	SELECT @ParentId = ParentId
	FROM dbo.[ResponsibilityCenters]
	WHERE Id = @OperationId

	WHILE @ParentId IS NOT NULL
	BEGIN
		--UPDATE dbo.[ResponsibilityCenters]
		--SET
		--	[IsOperatingSegment] = 0,
		--	[ModifiedAt] = @Now,
		--	[ModifiedById] = @UserId
		--WHERE [Id] = @ParentId
		--AND [IsOperatingSegment] = 1;

		SELECT @ParentId = ParentId
		FROM dbo.[ResponsibilityCenters]
		WHERE Id = @ParentId
	END

-- Reset all children
	INSERT INTO @Ids([Id])
	SELECT [Id] FROM dbo.[ResponsibilityCenters]
	WHERE ParentId = @OperationId;

	WHILE EXISTS (SELECT * FROM @Ids)
	BEGIN
		--UPDATE dbo.[ResponsibilityCenters]
		--SET 
		--	[IsOperatingSegment] = 0,
		--	[ModifiedAt] = @Now,
		--	[ModifiedById] = @UserId
		--WHERE [IsOperatingSegment] = 1
		--AND Id IN (SELECT [Id] FROM @Ids)
	
		DELETE FROM @NextIds;

		INSERT INTO @NextIds([Id])
		SELECT [Id] FROM @Ids;

		DELETE FROM @Ids;

		INSERT INTO @Ids
		SELECT [Id] FROM dbo.[ResponsibilityCenters]
		WHERE [ParentId] IN (SELECT [Id] FROM @NextIds);
	END;

	DELETE FROM @Ids;

	IF (
		SELECT ParentId 
		FROM dbo.[ResponsibilityCenters] 
		WHERE [Id] = @OperationId
	) IS NULL
		INSERT INTO @Ids
		SELECT [Id] FROM dbo.[ResponsibilityCenters]
		WHERE ParentId IS NULL
		AND [Id] > @OperationId
	ELSE
		INSERT INTO @Ids
		SELECT [Id] FROM dbo.[ResponsibilityCenters]
		WHERE ParentId = (
			SELECT ParentId 
			FROM dbo.[ResponsibilityCenters] 
			WHERE [Id] = @OperationId
		)
		AND [Id] > @OperationId;
	
	-- Set all older siblings as operating segments, recursively!
	WHILE EXISTS(SELECT * FROM @Ids)
	BEGIN
		SELECT @Id = MIN([Id]) FROM @Ids;
		EXEC [dbo].[dal_Operation__SetOperatingSegment] @OperationId = @Id;
		DELETE FROM @Ids WHERE [Id] = @Id;
	END