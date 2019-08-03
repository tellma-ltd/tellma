CREATE PROCEDURE [dal].[IfrsDisclosureDetails__Save]
	@Entities [IfrsDisclosureDetailList] READONLY
AS
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[IfrsDisclosureDetails] AS t
		USING (
			SELECT [Index], [Id], [IfrsDisclosureId], [Value], [ValidSince]
			FROM @Entities 
		) AS s 
		ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[IfrsDisclosureId]= s.[IfrsDisclosureId],
				t.[Value]			= s.[Value],
				t.[ValidSince]		= s.[ValidSince],
				t.[ModifiedAt]		= @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([IfrsDisclosureId], [Value], [ValidSince])
			VALUES (s.[IfrsDisclosureId], s.[Value], s.[ValidSince])
			OUTPUT s.[Index], inserted.[Id]
	) AS x
	OPTION (RECOMPILE);