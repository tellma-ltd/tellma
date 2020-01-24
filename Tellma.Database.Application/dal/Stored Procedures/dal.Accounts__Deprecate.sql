CREATE PROCEDURE [dal].[Accounts__Deprecate] -- [dbo].[dal_Accounts__Activate] @Accounts = N'CashOnHand', @IsActive = 0
	@Ids dbo.[IdList] READONLY,
	@IsDeprecated bit
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[Accounts] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.[IsDeprecated] <> @IsDeprecated)
	THEN
		UPDATE SET 
			t.[IsDeprecated]	= @IsDeprecated,
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId;
END;