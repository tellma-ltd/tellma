CREATE PROCEDURE [dal].[Notifications_SmsMessages__UpdateState]
	@Id					INT,
	@NewState			SMALLINT,
	@Error				NVARCHAR (2048)
AS
BEGIN
SET NOCOUNT ON;

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	UPDATE dbo.[SmsMessages]
	SET [State] = @NewState, [StateSince] = @Now, [ErrorMessage] = @Error
	WHERE [Id] = @Id AND @NewState <> [State] AND (@NewState < 0 OR [State] < @NewState)
END
