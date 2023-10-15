CREATE FUNCTION [dbo].[fn_ToTitleCase] (
	@InputString NVARCHAR(255)
)
RETURNS NVARCHAR(255)
AS
BEGIN
	DECLARE @Index INT
	DECLARE @Char CHAR(1)
	DECLARE @OutputString NVARCHAR(255)
	SET @OutputString = LOWER(@InputString)
	SET @Index = 2
	SET @OutputString =
	STUFF(@OutputString, 1, 1,UPPER(SUBSTRING(@InputString,1,1)))
	WHILE @Index <= LEN(@InputString)
	BEGIN
	SET @Char = SUBSTRING(@InputString, @Index, 1)
	IF @Char IN (' ', ';', ':', '!', '?', ',', '.', '_', '-', '/', '&','''','(')
	IF @Index + 1 <= LEN(@InputString)
	BEGIN
	IF @Char != ''''
	OR
	UPPER(SUBSTRING(@InputString, @Index + 1, 1)) != 'S'
	SET @OutputString =
	STUFF(@OutputString, @Index + 1, 1,UPPER(SUBSTRING(@InputString, @Index + 1, 1)))
	END
	SET @Index = @Index + 1
	END
	RETURN ISNULL(@OutputString,'')
END