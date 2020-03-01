CREATE FUNCTION [dbo].[fn_StateId__State]
(
	@StateId INT
)
RETURNS NVARCHAR(30)
AS
BEGIN
	RETURN CASE
		WHEN @StateId = 0		THEN N'Draft'
		WHEN @StateId = -1		THEN N'Void'
		WHEN @StateId = +1		THEN N'Requested'
		WHEN @StateId = -2		THEN N'Rejected'
		WHEN @StateId = +2		THEN N'Authorized'
		WHEN @StateId = -3		THEN N'Failed'
		WHEN @StateId = +3		THEN N'Completed'
		WHEN @StateId = -4		THEN N'Invalid'
		WHEN @StateId = +4		THEN N'Ready To Post'
	END
END
