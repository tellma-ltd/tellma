INSERT INTO dbo.Agents
([Id], [Name]) VALUES
(1,		N'el-Amin Al-Tayyib');

DECLARE @elAminAgent INT = (SELECT [Id] FROM dbo.[Agents] WHERE [Name] = N'el-Amin Al-Tayyib');

