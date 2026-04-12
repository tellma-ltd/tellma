CREATE PROCEDURE [dal].[DirectoryUsers__Save]
	@NewEmailsOrClientIds [dbo].[StringList] READONLY,
	@OldEmailsOrClientIds [dbo].[StringList] READONLY,
	@DatabaseId INT,
	@ReturnEmailsOrClientIdsForCreation BIT = 0
AS
BEGIN
	SET NOCOUNT ON;

	-- This admin database SP is called whenever new users are added. Whether in tenant databases or in the admin databases
	-- The SP takes a list of emails/clientIds, inserts them as DirectoryUsers if they are not already present
	--		and then returns the ones that are present and have an ExternalId so that their ExternalId can
	--		be set in the tenant database as well

	DECLARE @EmailsOrClientIdsForCreation [dbo].[StringList];

	-- Merge new emails/clientIds into DirectoryUsers and retrieve ones that NEVER existed before (@EmailsOrClientIdsForCreation)
	INSERT INTO @EmailsOrClientIdsForCreation([Id])
	SELECT x.[EmailOrClientId]
	FROM
	(
		MERGE INTO [dbo].[DirectoryUsers] AS t
			USING (
				SELECT [Id] as [EmailOrClientId] FROM @NewEmailsOrClientIds
			) AS s ON (t.[EmailOrClientId] = s.[EmailOrClientId])
			WHEN NOT MATCHED THEN
				INSERT ([EmailOrClientId]) VALUES (s.[EmailOrClientId])
				OUTPUT inserted.[EmailOrClientId]
	) AS x;

	-- Given the list of emails/clientIds in @NewEmailsOrClientIds, Insert into DirectoryUserMemberships all their corresponding UserIds
	WITH NewIds AS (
		SELECT [Id] FROM [dbo].[DirectoryUsers]
		WHERE [EmailOrClientId] IN (SELECT [Id] FROM @NewEmailsOrClientIds)
	)
	MERGE INTO [dbo].[DirectoryUserMemberships] AS t
		USING (
			SELECT [Id] FROM NewIds
		) AS s ON (t.[UserId] = s.[Id] AND t.[DatabaseId] = @DatabaseId)
		WHEN NOT MATCHED THEN
			INSERT ([UserId], [DatabaseId]) VALUES (s.[Id], @DatabaseId);

	-- Given the list of emails/clientIds in @OldEmailsOrClientIds, remove from DirectoryUserMemberships all their corresponding UserIds
	WITH OldIds AS (
		SELECT [Id] FROM [dbo].[DirectoryUsers]
		WHERE [EmailOrClientId] IN (SELECT [Id] FROM @OldEmailsOrClientIds)
	)
	DELETE FROM [dbo].[DirectoryUserMemberships]
	WHERE [UserId] IN (SELECT [Id] FROM OldIds)
	AND [DatabaseId] = @DatabaseId;

	-- If requested, return emails/clientIds for creation
	IF (@ReturnEmailsOrClientIdsForCreation = 1)
		SELECT [Id] AS [EmailOrClientId] FROM @EmailsOrClientIdsForCreation;
END;
