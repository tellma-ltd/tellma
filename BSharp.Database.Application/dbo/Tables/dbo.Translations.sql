CREATE TABLE [dbo].[Translations]
(
	[CultureId]		nvarchar (255),
	[Name]			nvarchar (255),
	[Value]			nvarchar(2048) NOT NULL,
	[Tier]			nvarchar (255),	
	CONSTRAINT [PK_Translations] PRIMARY KEY NONCLUSTERED ([CultureId], [Name]),
	CONSTRAINT [CK_Translations_Tier] CHECK([Tier] IN (N'Client', N'Server', N'Shared'))
);
GO