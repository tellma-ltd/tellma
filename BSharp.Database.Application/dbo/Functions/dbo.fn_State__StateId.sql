CREATE FUNCTION [dbo].[fn_State__StateId]
(
	@State NVARCHAR(30)
)
RETURNS INT
AS
BEGIN
	RETURN CASE
		WHEN @State = N'Draft'		THEN 0
		WHEN @State = N'Void'		THEN -10
		WHEN @State = N'Requested'	THEN +10
		WHEN @State = N'Rejected'	THEN -20
		WHEN @State = N'Authorized'	THEN +20
		WHEN @State = N'Failed'		THEN -30
		WHEN @State = N'Completed'	THEN +30
		WHEN @State = N'Invalid'	THEN -100
		WHEN @State = N'Reviewed'	THEN +100
	END
END
