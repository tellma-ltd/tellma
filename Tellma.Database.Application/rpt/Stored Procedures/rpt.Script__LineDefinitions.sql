CREATE PROCEDURE [rpt].[Script__LineDefinitions]
	@Script NVARCHAR (255)
AS
	SELECT
		[Code], [TitleSingular],
		IIF([GenerateScript] LIKE N'%' + @Script + N'%', 1, 0) AS [Auto Generate],
		IIF([PreprocessScript] LIKE N'%' + @Script + N'%', 1, 0) AS [Preprocess],
		IIF([ValidateScript] LIKE N'%' + @Script + N'%', 1, 0) AS [Validate],
		IIF([SignValidateScript] LIKE N'%' + @Script + N'%', 1, 0) AS [Sign],
		IIF([UnsignValidateScript] LIKE N'%' + @Script + N'%', 1, 0) AS [Unsign]
	FROM dbo.LineDefinitions
	WHERE [GenerateScript] LIKE N'%' + @Script + N'%'
	OR [PreprocessScript] LIKE N'%' + @Script + N'%'
	OR [ValidateScript] LIKE N'%' + @Script + N'%'
	OR [SignValidateScript] LIKE N'%' + @Script + N'%'
	OR [UnsignValidateScript] LIKE N'%' + @Script + N'%';
GO