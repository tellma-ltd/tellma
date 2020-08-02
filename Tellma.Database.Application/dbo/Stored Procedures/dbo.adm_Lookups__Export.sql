CREATE PROCEDURE [dbo].[adm_Lookups__Export]
@DefinitionId INT = 10 -- Grain Type
AS
DECLARE @MinIndex INT = (SELECT MIN([Id]) FROM dbo.[Lookups] WHERE [DefinitionId] = @DefinitionId)
SELECT ([Id] - @MinIndex) AS [Index], Code, [Name], [Name2],
		N'(' + CAST(([Id] - @MinIndex) AS NVARCHAR(10)) + ',N''' + 
		[Code] + N''', N''' + [Name] + N''', N''' +  [Name2] + N'''),'
		AS N'INSERT INTO @Lookups([Index],[Code],[Name],[Name2]) VALUES'
 FROM dbo.[Lookups] WHERE [DefinitionId] = @DefinitionId
 ORDER BY [Id]