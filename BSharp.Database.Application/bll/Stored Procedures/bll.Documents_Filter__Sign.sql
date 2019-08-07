CREATE PROCEDURE [bll].[Documents_Filter__Sign]
	@Entities [dbo].[IdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Signing can be at any time
	-- We simply record the signature if
	-- It belongs to an agent
	-- It is required as per policy
	SELECT [Id] FROM @Entities
	WHERE [Id] <> 0;