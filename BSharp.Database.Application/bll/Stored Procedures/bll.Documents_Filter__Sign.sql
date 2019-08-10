CREATE PROCEDURE [bll].[Documents_Filter__Sign]
	@Ids [dbo].[IdList] READONLY,
	@State NVARCHAR(30)
AS
SET NOCOUNT ON;
	-- Signing can be at any time
	-- We simply record the signature if
	-- It belongs to an agent
	-- It is required as per policy
	SELECT [Id] FROM @Ids
	WHERE [Id] <> 0;