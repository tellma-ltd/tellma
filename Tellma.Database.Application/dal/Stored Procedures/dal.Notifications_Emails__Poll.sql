CREATE PROCEDURE [dal].[Notifications_Emails__Poll]
	@ExpiryInSeconds	INT,
	@Top				INT
AS
BEGIN
SET NOCOUNT ON;

	DECLARE @TooOld DATETIMEOFFSET(7) = DATEADD(second, -@ExpiryInSeconds, SYSDATETIMEOFFSET());

	-- Get the Ids of the emails
	DECLARE @Ids [dbo].[IdList];
	INSERT INTO @Ids ([Id])
	SELECT TOP (@Top) [Id]
	FROM [dbo].[Emails] 
	WHERE [State] = 0 OR ([State] = 1 AND [StateSince] < @TooOld) 

	-- Select the Emails
	SELECT TOP (@Top) [Id], [To], [Cc], [Bcc], [Subject], [BodyBlobId] 
	FROM [dbo].[Emails] 
	WHERE [Id] IN (SELECT [Id] FROM @Ids)

	-- Select the attachments
	SELECT [EmailId], [Name], [ContentBlobId]
	FROM [dbo].[EmailAttachments] WHERE [EmailId] IN (SELECT [Id] FROM @Ids)
END
