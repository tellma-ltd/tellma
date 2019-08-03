CREATE PROCEDURE [dbo].[bll_Documents_Validate__Sign]
	@Entities [dbo].[UuidList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Signing can be at any time
	-- We simply record the signature if
	-- It belongs to an agent
	-- It is required as per policy