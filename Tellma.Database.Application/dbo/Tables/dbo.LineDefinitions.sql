	CREATE TABLE [dbo].[LineDefinitions] (
	[Id]						INT 			CONSTRAINT [PK_LineDefinitions] PRIMARY KEY IDENTITY,
	[Code]						NVARCHAR (100)	NOT NULL CONSTRAINT [UQ_LineDefinitions] UNIQUE,
	[LineType]					TINYINT			NOT NULL, -- 20: T for P, 40: Plan, 60:T for T, 80:Template, 100: Event, 120: Regulatory
	[Description]				NVARCHAR (1024),
	[Description2]				NVARCHAR (1024),
	[Description3]				NVARCHAR (1024),
	[TitleSingular]				NVARCHAR (100)	NOT NULL,
	[TitleSingular2]			NVARCHAR (100),
	[TitleSingular3]			NVARCHAR (100),
	[TitlePlural]				NVARCHAR (100)	NOT NULL,
	[TitlePlural2]				NVARCHAR (100),
	[TitlePlural3]				NVARCHAR (100),
	[AllowSelectiveSigning]		BIT				NOT NULL DEFAULT 0,
	[ViewDefaultsToForm]		BIT				NOT NULL DEFAULT 0,

	-- New Barcode Stuff
	[BarcodeColumnIndex]		INT,
	[BarcodeProperty]			NVARCHAR (50),
	[BarcodeExistingItemHandling] NVARCHAR (50) CONSTRAINT [CK_LineDefinitions__BarcodeExistingItemHandling] CHECK ([BarcodeExistingItemHandling] IN (N'AddNewLine', N'IncrementQuantity', N'ThrowError', N'DoNothing')),
	[BarcodeBeepsEnabled]		BIT				NOT NULL DEFAULT 1,

	[GenerateLabel]				NVARCHAR (50),
	[GenerateLabel2]			NVARCHAR (50),
	[GenerateLabel3]			NVARCHAR (50),
	[GenerateScript]			NVARCHAR (MAX), -- to store SQL code that generates the line in the UI
	[PreprocessScript]			NVARCHAR (MAX), -- to store SQL code that preprocesses the line in the save pipeline
	[ValidateScript]			NVARCHAR (MAX), -- to store SQL code that validates the line in the save pipeline
	[SavedById]					INT				NOT NULL CONSTRAINT [FK_LineDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionsHistory]));
GO;