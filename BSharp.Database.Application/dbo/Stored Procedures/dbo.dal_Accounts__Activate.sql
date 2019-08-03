CREATE PROCEDURE [dbo].[dal_Accounts__Activate] -- [dbo].[dal_Accounts__Activate] @Accounts = N'CashOnHand', @IsActive = 0
	@Ids dbo.[UuidList] READONLY,
	@IsActive bit
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
	WHEN MATCHED AND (t.IsActive <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]	= @IsActive,
			t.[ModifiedAt]	= @Now,
			t.[ModifiedById]= @UserId;
END;