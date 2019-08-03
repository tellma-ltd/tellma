CREATE TABLE [dbo].[MeasurementUnits] (
	[Id]			INT PRIMARY KEY IDENTITY(1,1),
	[UnitType]		NVARCHAR (255)		NOT NULL,
	[Name]			NVARCHAR (255)		NOT NULL,
	[Name2]			NVARCHAR (255),
	[Name3]			NVARCHAR (255),
	[Description]	NVARCHAR (255)		NOT NULL,
	[Description2]	NVARCHAR (255),
	[Description3]	NVARCHAR (255),
	[UnitAmount]	FLOAT (53)			NOT NULL DEFAULT 1,
	[BaseAmount]	FLOAT (53)			NOT NULL DEFAULT 1,
	[IsActive]		BIT					NOT NULL DEFAULT 1,
	[IsDeleted]		BIT					NOT NULL DEFAULT 0,
	[Code]			NVARCHAR (255),
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]	DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	CONSTRAINT [CK_MeasurementUnits__UnitType] CHECK ([UnitType] IN (N'Pure', N'Time', N'Distance', N'Count', N'Mass', N'Volume', N'Money'))
);
GO