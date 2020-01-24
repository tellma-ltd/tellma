CREATE PROCEDURE [dal].[OnConnect]
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50),
	@ExternalUserId NVARCHAR(255),
	@UserEmail NVARCHAR(255)
AS
    -- Set the global values of the session context
    EXEC [sys].[sp_set_session_context] @key = N'Culture', @value = @Culture;
    EXEC [sys].[sp_set_session_context] @key = N'NeutralCulture', @value = @NeutralCulture;

    -- Get the User Id
    DECLARE 
        @UserId INT, 
        @ExternalId NVARCHAR(450), 
        @Email NVARCHAR(255);

    SELECT
        @UserId = [Id],
        @ExternalId = [ExternalId],
        @Email = [Email]
    FROM [dbo].[GlobalUsers] 
    WHERE [ExternalId] = @ExternalUserId OR [Email] = @UserEmail;

    -- Set the User Id
    EXEC [sys].[sp_set_session_context] @key = N'UserId', @value = @UserId;

    -- Return the user and tenant information
    SELECT 
		-- Global User Info
        @UserId AS [UserId], 
        @ExternalId AS [ExternalId], 
        @Email AS [Email]
