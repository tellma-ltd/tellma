CREATE TYPE [dbo].[IdKeyValuePairList] AS TABLE (
	[Id] INT,
	[Key] NVARCHAR (50),
	[Value] NVARCHAR (255),
	PRIMARY KEY([Id], [Key])
)
GO