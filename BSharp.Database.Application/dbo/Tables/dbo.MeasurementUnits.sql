CREATE TABLE [dbo].[MeasurementUnits] (
	[Id]			INT					CONSTRAINT [PK_MeasurementUnits] PRIMARY KEY IDENTITY,
	[UnitType]		NVARCHAR (255)		NOT NULL CONSTRAINT [CK_MeasurementUnits__UnitType] CHECK ([UnitType] IN (N'Pure', N'Time', N'Distance', N'Count', N'Mass', N'Volume')),
	[Name]			NVARCHAR (255)		NOT NULL,
	[Name2]			NVARCHAR (255),
	[Name3]			NVARCHAR (255),
	[Description]	NVARCHAR (255)		NOT NULL,
	[Description2]	NVARCHAR (255),
	[Description3]	NVARCHAR (255),
	[UnitAmount]	FLOAT (53)			NOT NULL DEFAULT 1,
	[BaseAmount]	FLOAT (53)			NOT NULL DEFAULT 1,
	[IsActive]		BIT					NOT NULL DEFAULT 1,
	[Code]			NVARCHAR (255),
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_MeasurementUnits__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]	DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]	INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_MeasurementUnits__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO