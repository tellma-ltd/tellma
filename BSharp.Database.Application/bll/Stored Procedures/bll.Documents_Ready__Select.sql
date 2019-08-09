CREATE PROCEDURE [bll].[Documents_Ready__Select]
	@Entities [dbo].[IdList] READONLY
AS
	SELECT [Id] FROM @Entities;