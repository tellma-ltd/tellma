CREATE PROCEDURE [dbo].[bll_SalesOrder__Discount]
	@FunctionId INT,
	@Discount	DECIMAL  = NULL OUTPUT
AS
-- Execute with readonly privilige
IF @FunctionId = N'1211AX'
BEGIN


	SET @Discount = 0
END