CREATE TYPE [dbo].[MeasurementUnitList] AS TABLE (
	[Index]			INT	PRIMARY KEY			IDENTITY(0, 1),
	[Id]			INT NOT NULL DEFAULT 0,
	[UnitType]		NVARCHAR (255)	NOT NULL,
	[Name]			NVARCHAR (255)	NOT NULL,
	[Name2]			NVARCHAR (255),
	[Name3]			NVARCHAR (255),
	[Description]	NVARCHAR (255)	NOT NULL,
	[Description2]	NVARCHAR (255),
	[Description3]	NVARCHAR (255),
	[UnitAmount]	FLOAT (53)		NOT NULL,
	[BaseAmount]	FLOAT (53)		NOT NULL,
	[Code]			NVARCHAR (255),
	INDEX IX_MeasurementUnitList__Code ([Code]),
	INDEX IX_MeasurementUnitList__Name ([Name]),
	INDEX IX_MeasurementUnitList__Name2 ([Name2]),
	INDEX IX_MeasurementUnitList__Name3 ([Name3]),
	CHECK ([UnitType] IN (N'Pure', N'Time', N'Distance', N'Count', N'Mass', N'Volume', N'Currency'))
);