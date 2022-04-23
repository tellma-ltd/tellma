CREATE FUNCTION [dal].[fn_Center__ExpenseByFunction_AgentId] (
	@CenterId INT
)
RETURNS INT
AS
BEGIN
	DECLARE @CenterType NVARCHAR (255) = dal.fn_Center__CenterType(@CenterId);

	DECLARE @ExpenseByFunctionCode NVARCHAR (255) = [dbo].[fn_CenterType__ExpenseByFunction](@CenterType)

	RETURN dal.fn_AgentDefinition_Code__Id(
		N'ExpenseByFunction',
		@ExpenseByFunctionCode
	)
END
