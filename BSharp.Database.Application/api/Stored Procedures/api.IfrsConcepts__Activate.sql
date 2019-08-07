CREATE PROCEDURE [api].[IfrsConcepts__Activate]
	@Ids [dbo].[StringList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [dal].[IfrsConcepts__Activate] @Ids = @Ids, @IsActive = @IsActive;