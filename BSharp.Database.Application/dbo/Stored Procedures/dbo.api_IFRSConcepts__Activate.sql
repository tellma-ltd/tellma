CREATE PROCEDURE [dbo].[api_IfrsConcepts__Activate]
	@Ids [dbo].[StringList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [dbo].[dal_IfrsConcepts__Activate] @Ids = @Ids, @IsActive = @IsActive;