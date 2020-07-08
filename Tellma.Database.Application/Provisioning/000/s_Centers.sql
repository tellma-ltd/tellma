DELETE FROM @Centers;
INSERT INTO @Centers([Index], [Code], [Name], [CenterType]) VALUES (0, N'0', N'Business', N'Abstract');

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Business Entity Segment: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DECLARE @BE INT = (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'0');