CREATE TABLE [dbo].[Units] (
	[Id]			INT					CONSTRAINT [PK_Units] PRIMARY KEY IDENTITY,
	[UnitType]		NVARCHAR (50)		NOT NULL CONSTRAINT [CK_Units__UnitType] CHECK ([UnitType] IN (N'Pure', N'Time', N'Distance', N'Count', N'Mass', N'Volume')),
	[Name]			NVARCHAR (50)		NOT NULL,
	[Name2]			NVARCHAR (50),
	[Name3]			NVARCHAR (50),
	[Description]	NVARCHAR (255)		NOT NULL,
	[Description2]	NVARCHAR (255),
	[Description3]	NVARCHAR (255),
	[UnitAmount]	FLOAT (53)			NOT NULL DEFAULT 1,
	[BaseAmount]	FLOAT (53)			NOT NULL DEFAULT 1,
	[IsActive]		BIT					NOT NULL DEFAULT 1,
	[Code]			NVARCHAR (50),
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL,
	[CreatedById]	INT					NOT NULL,
	[ModifiedAt]	DATETIMEOFFSET(7)	NOT NULL,
	[ModifiedById]	INT					NOT NULL
);
GO