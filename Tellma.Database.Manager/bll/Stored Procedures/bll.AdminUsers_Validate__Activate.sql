CREATE PROCEDURE [bll].[AdminUsers_Validate__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@UserId INT,
	@Top INT = 200
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO

	SELECT TOP(@Top) * FROM @ValidationErrors;
END
