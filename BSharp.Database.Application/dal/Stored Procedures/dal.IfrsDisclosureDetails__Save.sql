CREATE PROCEDURE [dal].[IfrsDisclosureDetails__Save]
	@Entities [IfrsDisclosureDetailList] READONLY
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[IfrsDisclosureDetails] AS t
	USING (
		SELECT [IfrsDisclosureId], [ValidSince], [Value]
		FROM @Entities 
	) AS s 
	ON (t.[IfrsDisclosureId] = s.[IfrsDisclosureId] AND t.[ValidSince] = s.[ValidSince])
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[Value]			= s.[Value],
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([IfrsDisclosureId], [ValidSince], [Value])
		VALUES (s.[IfrsDisclosureId], s.[ValidSince], s.[Value])
	OPTION (RECOMPILE);