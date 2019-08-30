CREATE PROCEDURE [dal].[GetUserPermissionsVersion]
AS
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

    SELECT [PermissionsVersion] 
    FROM [dbo].[Users]
    WHERE [Id] = @UserId
