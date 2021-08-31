CREATE TABLE [dbo].[Currencies]
(
	[Id]			NCHAR(3)			NOT NULL CONSTRAINT [PK_Currencies] PRIMARY KEY,
	[Name]			NVARCHAR (50)		NOT NULL CONSTRAINT [UQ_Currencies__Name] UNIQUE,
	[Name2]			NVARCHAR (50),
	[Name3]			NVARCHAR (50),
	[Description]	NVARCHAR (255)		NOT NULL CONSTRAINT [UQ_Currencies__Description] UNIQUE,
	[Description2]	NVARCHAR (255),
	[Description3]	NVARCHAR (255),
	[NumericCode]	SMALLINT			NOT NULL CONSTRAINT [UQ_Currencies__NumericCode] UNIQUE,
	[E]				SMALLINT			NOT NULL DEFAULT 2 CONSTRAINT [CK_Currencies__E] CHECK ([E] IN (-1, 0, 2, 3, 4)),			
	[IsActive]		BIT					NOT NULL DEFAULT 0,
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT					NOT NULL CONSTRAINT [FK_Currencies__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]	DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]	INT					NOT NULL CONSTRAINT [FK_Currencies__ModifiedById] REFERENCES [dbo].[Users] ([Id])
)
