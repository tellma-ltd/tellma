CREATE PROCEDURE [bll].[Agents_Validate__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

SELECT TOP(@Top) * FROM @ValidationErrors;