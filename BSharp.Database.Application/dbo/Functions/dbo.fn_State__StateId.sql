CREATE FUNCTION [dbo].[fn_State__StateId]
(
	@State NVARCHAR(30)
)
RETURNS INT
AS
BEGIN
	RETURN CASE
		WHEN @State = N'Draft'		THEN 0
		WHEN @State = N'Void'		THEN -1
		WHEN @State = N'Requested'	THEN +1
		WHEN @State = N'Rejected'	THEN -2
		WHEN @State = N'Authorized'	THEN +2
		WHEN @State = N'Failed'		THEN -3
		WHEN @State = N'Completed'	THEN +3
		WHEN @State = N'Invalid'	THEN -4
		WHEN @State = N'Reviewed'	THEN +4
	END
END
	--CONSTRAINT [CK_Lines__State] CHECK ([State]	IN (N'Draft', N'Void', N'Requested', N'Rejected', N'Authorized', N'Failed', N'Completed', N'Invalid', N'Reviewed')),
	--CONSTRAINT [CK_Lines__State] CHECK ([State]		IN (0		,	-1,			+1,			-2,				+2,			-3,			+3,			-4,				+4)),
