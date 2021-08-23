CREATE FUNCTION [map].[IdentityServerUsers] ()
RETURNS TABLE
AS
RETURN (
	SELECT [Id], [Email], [EmailConfirmed], CAST(IIF([PasswordHash] IS NULL, 0, 1) AS BIT) AS [PasswordSet], [TwoFactorEnabled], [LockoutEnd] FROM [dbo].[AspNetUsers]
);
