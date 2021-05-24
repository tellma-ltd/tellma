CREATE PROCEDURE [bll].[AdminUsers_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@UserId INT,
	@Top INT = 200
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO

	SELECT TOP(@Top) * FROM @ValidationErrors;