CREATE PROCEDURE [dal].[Notifications_SmsMessages__Poll]
	@ExpiryInSeconds	INT,
	@Top				INT
AS
BEGIN
SET NOCOUNT ON;

	DECLARE @TooOld DATETIMEOFFSET(7) = DATEADD(second, -@ExpiryInSeconds, SYSDATETIMEOFFSET());

	SELECT TOP (@Top) [Id], [ToPhoneNumber], [Message] 
	FROM dbo.[SmsMessages] 
	WHERE [State] = 0 OR ([State] = 1 AND [StateSince] < @TooOld)
END
