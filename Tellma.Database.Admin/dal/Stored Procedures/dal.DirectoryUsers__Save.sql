CREATE PROCEDURE [dal].[DirectoryUsers__Save]
	@NewEmails [dbo].[StringList] READONLY,
	@OldEmails [dbo].[StringList] READONLY,
	@DatabaseId INT,
	@ReturnEmailsForCreation BIT = 0
AS
BEGIN
	SET NOCOUNT ON;

	-- This admin database SP is called whenever new users are added. Whether in tenant databases or in the admin databases
	-- The SP takes a list of emails, inserts them as DirectoryUsers if they are not already present
	--		and then returns the ones that are present and have an ExternalId so that their ExternalId can
	--		be set in the tenant database as well

	DECLARE @EmailsForCreation [dbo].[StringList];

	-- Merge new emails into DirectoryUsers and retrieve emails that NEVER existed before (@EmailsForCreation)
	INSERT INTO @EmailsForCreation([Id])
	SELECT x.[Email]
	FROM
	(
		MERGE INTO [dbo].[DirectoryUsers] AS t
			USING (
				SELECT [Id] as [Email] FROM @NewEmails 
			) AS s ON (t.[Email] = s.[Email])
			WHEN NOT MATCHED THEN
				INSERT ([Email]) VALUES (s.[Email])
				OUTPUT inserted.[Email]
	) AS x;

	-- Given the list of emails in @NewEmails, Insert into DirectoryUserMemberships all their corresponding UserIds
	WITH NewIds AS (
		SELECT [Id] FROM [dbo].[DirectoryUsers]
		WHERE [Email] IN (SELECT [Id] FROM @NewEmails)
	)
	MERGE INTO [dbo].[DirectoryUserMemberships] AS t
		USING (
			SELECT [Id] FROM NewIds 
		) AS s ON (t.[UserId] = s.[Id] AND t.[DatabaseId] = @DatabaseId)
		WHEN NOT MATCHED THEN
			INSERT ([UserId], [DatabaseId]) VALUES (s.[Id], @DatabaseId);

	-- Given the list of emails in @OldEmails, remove from DirectoryUserMemberships all their corresponding UserIds
	WITH OldIds AS (
		SELECT [Id] FROM [dbo].[DirectoryUsers]
		WHERE [Email] IN (SELECT [Id] FROM @OldEmails)
	)
	DELETE FROM [dbo].[DirectoryUserMemberships] 
	WHERE [UserId] IN (SELECT [Id] FROM OldIds)
	AND [DatabaseId] = @DatabaseId;

	-- If requested, return emails for creation
	IF (@ReturnEmailsForCreation = 1)
		SELECT [Id] AS [Email] FROM @EmailsForCreation;
END;
