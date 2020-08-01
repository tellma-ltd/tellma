CREATE PROCEDURE [bll].[Units_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO: Make sure the unit is not in table Entries

	-- TODO: Make sure the unit is not in table Units

	-- TODO: Make sure the unit is not in table Resources

	-- TODO: Make sure the unit is not in table Budget Entries

	-- TODO: Make sure the unit is not in table Account Balances

	SELECT TOP(@Top) * FROM @ValidationErrors;