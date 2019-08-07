CREATE PROCEDURE [dbo].[dal_ResourceLookup1s__Delete]
	@Ids [dbo].[IdList] READONLY,
	@IsDeleted BIT = 1
AS
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	DELETE [dbo].ResourceLookup1s WHERE [Id] IN (SELECT [Id] FROM @Ids);