CREATE TYPE [dbo].[NotificationTemplateParameterList] AS TABLE
(
	[Index]			INT				DEFAULT 0,
	[HeaderIndex]	INT				DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id]						INT	NOT NULL DEFAULT 0,
	[Key]						NVARCHAR (255),
	[Label]						NVARCHAR (255),
	[Label2]					NVARCHAR (255),
	[Label3]					NVARCHAR (255),
	[IsRequired]				BIT,
	[Control]					NVARCHAR (50),  -- 'text', 'number', 'decimal', 'date', 'boolean', 'Resource'
	[ControlOptions]			NVARCHAR (1024)
)
