CREATE FUNCTION [bll].[fn_Line_Criteria__Satisfied] (
	@Id INT,
	@Criteria NVARCHAR(1024)
)
-- This is a place holder function. It is actually computed in C#
RETURNS BIT
AS
BEGIN
	RETURN 1;
END