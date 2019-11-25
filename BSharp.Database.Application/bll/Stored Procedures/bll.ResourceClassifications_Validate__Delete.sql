CREATE PROCEDURE [bll].[ResourceClassifications_Validate__Delete]
	@DefinitionId NVARCHAR(50),
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];



	SELECT TOP (@Top) * FROM @ValidationErrors;
