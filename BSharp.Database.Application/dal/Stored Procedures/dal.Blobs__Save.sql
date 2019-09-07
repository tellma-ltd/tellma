CREATE PROCEDURE [dal].[Blobs__Save]
	@Name NVARCHAR(450),
	@Blob VARBINARY(MAX)
AS
MERGE INTO [dbo].[Blobs] AS t
	USING (
		SELECT @Name as [Id], @Blob as [Content]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED 
	THEN
		UPDATE SET t.[Content] = s.[Content]
	WHEN NOT MATCHED THEN
		INSERT ([Id], [Content])
		VALUES (s.[Id], s.[Content]);
