CREATE PROCEDURE [dal].[Resources__Activate]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	MERGE INTO [dbo].[Resources] AS t
	USING (
		SELECT [Id]
		FROM @Ids
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED
	AND t.DefinitionId = @DefinitionId -- Added MA: 2022.05.03
	AND t.IsActive <> @IsActive
	THEN
	UPDATE SET 
		t.[IsActive]		= @IsActive,
		t.[ModifiedAt]		= @Now,
		t.[ModifiedById]	= @UserId;
END;