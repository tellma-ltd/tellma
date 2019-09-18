CREATE PROCEDURE [dal].[Currencies__Save]
	@Entities [CurrencyList] READONLY,
	@ReturnIds BIT = 0
AS
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Currencies] AS t
		USING (
			SELECT
				[Index], [Id], [Name], [Name2], [Name3],
				[Description], [Description2], [Description3], [E]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 
				t.[Name]			= s.[Name],
				t.[Name2]			= s.[Name2],
				t.[Name3]			= s.[Name3],
				t.[Description]		= s.[Description],
				t.[Description2]	= s.[Description2],
				t.[Description3]	= s.[Description3],
				t.[E]				= s.[E],
				t.[ModifiedAt]		= @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([Name], [Name2], [Name3], [Description], [Description2], [Description3], [E])
			VALUES (s.[Name], s.[Name2], s.[Name3], s.[Description], s.[Description2], s.[Description3], s.[E])
		OUTPUT s.[Index], inserted.[Id]
	) AS x
	OPTION (RECOMPILE);

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
