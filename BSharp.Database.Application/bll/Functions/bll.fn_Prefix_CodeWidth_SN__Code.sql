CREATE FUNCTION [bll].[fn_Prefix_CodeWidth_SN__Code]
(
	@Prefix NVARCHAR(5),
	@CodeWidth TINYINT,
	@SerialNumber		INT
)
RETURNS NVARCHAR (30)
AS
BEGIN RETURN
			@Prefix + 
			REPLICATE(N'0', @CodeWidth - 1 - FLOOR(LOG10(@SerialNumber))) +
			CAST(@SerialNumber AS NVARCHAR(30))
END
