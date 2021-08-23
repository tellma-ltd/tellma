CREATE PROCEDURE [dal].[Lookups__Activate]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	-- TODO: Restrict action only to the given @DefinitionId

	MERGE INTO [dbo].[Lookups] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED AND (t.[IsActive] <> @IsActive)
	THEN
		UPDATE SET 
			t.[IsActive]		= @IsActive,
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId;
END;
