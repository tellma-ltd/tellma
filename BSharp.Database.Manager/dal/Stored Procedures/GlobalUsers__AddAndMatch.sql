-- This admin database SP is called when a tenant administrator adds new emails as tenant users
-- The SP takes a list of emails, inserts them as GlobalUsers if they are not already present
--		and then returns the ones that are present and have an ExternalId so that their ExternalId can
--		be set in the tenant database as well

CREATE PROCEDURE [dbo].[GlobalUsers__AddAndMatch]
	@Emails [dbo].[StringList] READONLY,
	@DatabaseId INT
AS
SET NOCOUNT ON;

DECLARE @Ids [dbo].[IdList];

-- Insert new users
INSERT INTO @Ids([Id])
SELECT x.[Id]
FROM
(
    MERGE INTO [dbo].[GlobalUsers] AS t
    USING (
        SELECT [Code] as [Email] FROM @Emails 
    ) AS s ON (t.[Email] = s.[Email])
    WHEN NOT MATCHED THEN
        INSERT ([Email]) VALUES (s.[Email])
        OUTPUT inserted.[Id] 
) As x;

-- Insert memberships
INSERT INTO [dbo].[GlobalUserMemberships] ([UserId], [DatabaseId])
SELECT [Id], @DatabaseId FROM @Ids;

-- Return global users from the list of emails who already have External Ids
SELECT [GU].[Email], [GU].[ExternalId] FROM [dbo].[GlobalUsers] [GU]
INNER JOIN @Emails E ON [GU].[Email] = [E].[Code]
WHERE [GU].[ExternalId] IS NOT NULL
