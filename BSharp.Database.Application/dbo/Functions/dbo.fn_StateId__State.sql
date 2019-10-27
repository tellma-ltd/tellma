CREATE FUNCTION [dbo].[fn_StateId__State]
(
	@StateId INT
)
RETURNS NVARCHAR(30)
AS
BEGIN
	RETURN CASE
		WHEN @StateId = 0		THEN N'Draft'
		WHEN @StateId = -10		THEN N'Void'
		WHEN @StateId = +10		THEN N'Requested'
		WHEN @StateId = -20		THEN N'Rejected'
		WHEN @StateId = +20		THEN N'Authorized'
		WHEN @StateId = -30		THEN N'Failed'
		WHEN @StateId = +30		THEN N'Completed'
		WHEN @StateId = -100	THEN N'Invalid'
		WHEN @StateId = +100	THEN N'Reviewed'
	END
END
