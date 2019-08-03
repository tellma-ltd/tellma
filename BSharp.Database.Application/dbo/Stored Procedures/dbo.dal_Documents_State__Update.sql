CREATE PROCEDURE [dbo].[dal_Documents_State__Update]
	@Entities [dbo].[UiidWithStateList] READONLY,
	@State NVARCHAR (255)
AS
BEGIN
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	UPDATE D
	SET
		[State] = E.[State],
		ModifiedAt = @Now,
		ModifiedById = @UserId
	FROM dbo.Documents D
	JOIN @Entities E ON D.Id = E.[Id]
	Where D.[State] <> E.[State];
END;