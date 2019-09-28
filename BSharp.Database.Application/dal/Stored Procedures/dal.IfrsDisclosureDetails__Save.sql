CREATE PROCEDURE [dal].[IfrsDisclosureDetails__Save]
	@Entities [IfrsDisclosureDetailList] READONLY
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[IfrsDisclosureDetails] AS t
	USING (
		SELECT [IfrsDisclosureId], [Concept], [ValidSince], [Value]
		FROM @Entities 
	) AS s 
	ON (
		t.[IfrsDisclosureId] = s.[IfrsDisclosureId]
		AND t.[Concept] = s.[Concept]
		AND t.[ValidSince] = s.[ValidSince] 
		AND t.[Value] <> s.[Value]
	)
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[Value]			= s.[Value],
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([IfrsDisclosureId], [Concept], [ValidSince], [Value])
		VALUES (s.[IfrsDisclosureId], s.[Concept], s.[ValidSince], s.[Value])
	OPTION (RECOMPILE);